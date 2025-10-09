using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WhoWantsToBeAShillionaire.Models;
using WhoWantsToBeAShillionaire.Services;

namespace WhoWantsToBeAShillionaire.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly AudioService _audioService;
        private readonly GameLogic _gameLogic;

        private Question _currentQuestion;
        private string _gameStatus;
        private bool _isGameActive;
        private string _selectedAnswer;
        private bool _showFinalAnswer;
        private ObservableCollection<string> _prizeLadder;

        public MainViewModel(DatabaseService databaseService, AudioService audioService, GameLogic gameLogic)
        {
            _databaseService = databaseService;
            _audioService = audioService;
            _gameLogic = gameLogic;
            
            _prizeLadder = new ObservableCollection<string>();
            _gameStatus = "Welcome to Who Wants to Be a Shillionaire!";
            
            InitializeCommands();
            UpdatePrizeLadder();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties

        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCurrentQuestion));
            }
        }

        public string GameStatus
        {
            get => _gameStatus;
            set
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsGameActive
        {
            get => _isGameActive;
            set
            {
                _isGameActive = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAnswer
        {
            get => _selectedAnswer;
            set
            {
                _selectedAnswer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedAnswer));
            }
        }

        public bool ShowFinalAnswer
        {
            get => _showFinalAnswer;
            set
            {
                _showFinalAnswer = value;
                OnPropertyChanged();
            }
        }

        public bool HasCurrentQuestion => CurrentQuestion != null;
        public bool HasSelectedAnswer => !string.IsNullOrEmpty(SelectedAnswer);

        public ObservableCollection<string> PrizeLadder
        {
            get => _prizeLadder;
            set
            {
                _prizeLadder = value;
                OnPropertyChanged();
            }
        }

        public bool CanUseFiftyFifty => !_gameLogic.FiftyFiftyUsed && IsGameActive;
        public bool CanUsePhoneAFriend => !_gameLogic.PhoneAFriendUsed && IsGameActive;
        public bool CanUseAskTheAudience => !_gameLogic.AskTheAudienceUsed && IsGameActive;

        public string CurrentPrizeText => $"Current Winnings: ${_gameLogic.CurrentPrizeMoney:N0}";
        public string GuaranteedPrizeText => $"Guaranteed: ${_gameLogic.GetGuaranteedMoney():N0}";

        #endregion

        #region Commands

        public ICommand StartGameCommand { get; private set; }
        public ICommand SelectAnswerCommand { get; private set; }
        public ICommand FinalAnswerCommand { get; private set; }
        public ICommand WalkAwayCommand { get; private set; }
        public ICommand UseFiftyFiftyCommand { get; private set; }
        public ICommand UsePhoneAFriendCommand { get; private set; }
        public ICommand UseAskTheAudienceCommand { get; private set; }
        public ICommand NewGameCommand { get; private set; }

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(StartGame);
            SelectAnswerCommand = new RelayCommand<string>(SelectAnswer);
            FinalAnswerCommand = new RelayCommand(ProcessFinalAnswer, () => HasSelectedAnswer);
            WalkAwayCommand = new RelayCommand(WalkAway);
            UseFiftyFiftyCommand = new RelayCommand(UseFiftyFifty, () => CanUseFiftyFifty);
            UsePhoneAFriendCommand = new RelayCommand(UsePhoneAFriend, () => CanUsePhoneAFriend);
            UseAskTheAudienceCommand = new RelayCommand(UseAskTheAudience, () => CanUseAskTheAudience);
            NewGameCommand = new RelayCommand(StartNewGame);
        }

        #endregion

        #region Game Logic Methods

        public void StartGame()
        {
            _audioService.PlaySound("game_start");
            _gameLogic.StartNewGame();
            IsGameActive = true;
            LoadNextQuestion();
            UpdatePrizeLadder();
            UpdateGameStatus();
        }

        public void StartNewGame()
        {
            StartGame();
        }

        private async void LoadNextQuestion()
        {
            try
            {
                var difficulty = _gameLogic.GetCurrentDifficulty();
                var questions = await _databaseService.GetQuestionsByDifficultyAsync(difficulty);
                
                if (questions.Count > 0)
                {
                    var random = new Random();
                    CurrentQuestion = questions[random.Next(questions.Count)];
                    SelectedAnswer = null;
                    ShowFinalAnswer = false;
                }
                else
                {
                    GameStatus = "No questions available for this difficulty level.";
                    IsGameActive = false;
                }
            }
            catch (Exception ex)
            {
                GameStatus = $"Error loading question: {ex.Message}";
                IsGameActive = false;
            }
        }

        private void SelectAnswer(string answer)
        {
            if (!IsGameActive || string.IsNullOrEmpty(answer))
                return;

            _audioService.PlaySound("answer_select");
            SelectedAnswer = answer;
            OnPropertyChanged(nameof(CanUseFiftyFifty));
            OnPropertyChanged(nameof(CanUsePhoneAFriend));
            OnPropertyChanged(nameof(CanUseAskTheAudience));
        }

        private void ProcessFinalAnswer()
        {
            if (!HasSelectedAnswer || !IsGameActive)
                return;

            _audioService.PlaySound("final_answer");
            ShowFinalAnswer = true;

            if (SelectedAnswer == CurrentQuestion.CorrectAnswer)
            {
                _audioService.PlaySound("correct_answer");
                _gameLogic.AnswerCorrect();
                
                if (_gameLogic.IsGameWon())
                {
                    GameStatus = "Congratulations! You're a Shillionaire! You've won $1,000,000!";
                    _audioService.PlaySound("victory");
                    IsGameActive = false;
                }
                else
                {
                    GameStatus = $"Correct! You've won ${_gameLogic.CurrentPrizeMoney:N0}";
                    System.Threading.Tasks.Task.Delay(3000).ContinueWith(_ =>
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            LoadNextQuestion();
                            UpdateGameStatus();
                        });
                    });
                }
            }
            else
            {
                _audioService.PlaySound("wrong_answer");
                var guaranteedAmount = _gameLogic.GetGuaranteedMoney();
                GameStatus = $"Wrong answer! The correct answer was {CurrentQuestion.CorrectAnswer}. You leave with ${guaranteedAmount:N0}.";
                _audioService.PlaySound("game_over");
                IsGameActive = false;
            }

            UpdatePrizeLadder();
            OnPropertyChanged(nameof(CurrentPrizeText));
            OnPropertyChanged(nameof(GuaranteedPrizeText));
        }

        private void WalkAway()
        {
            if (!IsGameActive)
                return;

            _audioService.PlaySound("walk_away");
            var walkAwayAmount = _gameLogic.CalculateWalkAwayAmount();
            GameStatus = $"You chose to walk away with ${walkAwayAmount:N0}. Thanks for playing!";
            IsGameActive = false;
            OnPropertyChanged(nameof(CurrentPrizeText));
        }

        private void UseFiftyFifty()
        {
            if (!_gameLogic.UseFiftyFifty())
                return;

            _audioService.PlaySound("lifeline_use");
            // Implementation would remove two wrong answers from the UI
            OnPropertyChanged(nameof(CanUseFiftyFifty));
        }

        private void UsePhoneAFriend()
        {
            if (!_gameLogic.UsePhoneAFriend())
                return;

            _audioService.PlaySound("lifeline_use");
            // Implementation would show phone a friend dialog
            OnPropertyChanged(nameof(CanUsePhoneAFriend));
        }

        private void UseAskTheAudience()
        {
            if (!_gameLogic.UseAskTheAudience())
                return;

            _audioService.PlaySound("lifeline_use");
            // Implementation would show audience poll results
            OnPropertyChanged(nameof(CanUseAskTheAudience));
        }

        private void UpdateGameStatus()
        {
            if (IsGameActive)
            {
                GameStatus = _gameLogic.GetGameProgressText();
            }
        }

        private void UpdatePrizeLadder()
        {
            var ladder = _gameLogic.GetPrizeLadder();
            PrizeLadder.Clear();
            foreach (var item in ladder)
            {
                PrizeLadder.Add(item);
            }
        }

        #endregion

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
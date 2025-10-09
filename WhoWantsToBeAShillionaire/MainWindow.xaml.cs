using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Threading.Tasks;
using WhoWantsToBeAShillionaire.Models;
using WhoWantsToBeAShillionaire.Services;
using WhoWantsToBeAShillionaire.Views;

namespace WhoWantsToBeAShillionaire
{
    public partial class MainWindow : Window
    {
        private DatabaseService _databaseService;
        private AudioService _audioService;
        private GameLogic _gameLogic;
        private SettingsService _settingsService;
        private AIService _aiService;
        private AppSettings _settings;
        private Question _currentQuestion;
        private string _selectedAnswer;
        private readonly DispatcherTimer _answerTimer;

        public MainWindow()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _audioService = new AudioService();
            _gameLogic = new GameLogic();
            _settingsService = new SettingsService();
            _aiService = new AIService();
            _answerTimer = new DispatcherTimer();
            _answerTimer.Interval = TimeSpan.FromSeconds(2);
            _answerTimer.Tick += AnswerTimer_Tick;
            
            LoadSettings();
            _aiService.UpdateSettings(_settings);
            this.Loaded += MainWindow_Loaded;
            LoadQuestions();
        }

        private async Task ApplyDynamicBackgroundAsync(string? overridePrompt = null)
        {
            try
            {
                var prompt = overridePrompt ?? _settings?.AISettings?.ImagePrompt ?? "Create a polished Gemini-themed sci‑fi trivia game UI background: luminous blues and purples, subtle grid and particles, widescreen composition";
                var (imagePath, caption) = await _aiService.GenerateImageWithGeminiAsync(prompt, "app_background.png");

                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath);
                bitmap.EndInit();
                bitmap.Freeze();

                var brush = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0.35
                };

                this.Background = brush;
                try { MainMenuScreen.Background = brush; } catch { }
                try { GameScreen.Background = brush; } catch { }
                try { AdminScreen.Background = brush; } catch { }
                try { this.Icon = bitmap; } catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dynamic background generation failed: {ex.Message}");
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ApplyDynamicBackgroundAsync();
        }

        private void LoadSettings()
        {
            try
            {
                _settings = _settingsService.LoadSettings();
                _audioService.SetVolume(_settings.AudioSettings.Volume);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Settings Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                _settings = new AppSettings(); // Use default settings
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_settings);
            if (settingsWindow.ShowDialog() == true)
            {
                _settings = settingsWindow.Settings;
                _settingsService.SaveSettings(_settings);
                _audioService.SetVolume(_settings.AudioSettings.Volume);
                // Ensure AI service uses the latest settings (timeouts, model, etc.)
                _aiService.UpdateSettings(_settings);
            }
        }

        private async void GenerateQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_settings.AISettings.ApiKey))
            {
                MessageBox.Show("Please configure your AI API key in Settings first.", "AI Configuration Required", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                GenerateQuestionsButton.IsEnabled = false;
                GenerateQuestionsButton.Content = "Generating...";

                var difficulty = GetSelectedDifficulty();
                var category = GetSelectedCategory();
                var count = _settings.AISettings.QuestionsPerDifficulty;

                var questions = await _aiService.GenerateQuestionsAsync(difficulty, count, new List<string> { category });

                foreach (var question in questions)
                {
                    await _databaseService.AddQuestionAsync(question);
                }

                LoadQuestions();
                MessageBox.Show($"Successfully generated and saved {questions.Count} questions!", 
                    "AI Generation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating questions: {ex.Message}", "AI Generation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GenerateQuestionsButton.IsEnabled = true;
                GenerateQuestionsButton.Content = "Generate AI Questions";
            }
        }

        private string GetSelectedDifficulty()
        {
            if (EasyRadio.IsChecked == true) return "Easy";
            if (MediumRadio.IsChecked == true) return "Medium";
            if (HardRadio.IsChecked == true) return "Hard";
            return "Easy";
        }

        private string GetSelectedCategory()
        {
            var categoryText = CategoryTextBox.Text?.Trim();
            return string.IsNullOrEmpty(categoryText) ? "General Knowledge" : categoryText;
        }

        private async void LoadQuestions()
        {
            await _databaseService.InitializeAsync();
            RefreshQuestionsList();
        }

        #region Main Menu Events
        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            _audioService.PlaySound("button_click");
            StartNewGame();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            _audioService.PlaySound("button_click");
            ShowAdminPanel();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            _audioService.PlaySound("button_click");
            Application.Current.Shutdown();
        }
        #endregion

        #region Game Logic
        private void StartNewGame()
        {
            _gameLogic.StartNewGame();
            ShowGameScreen();
            LoadNextQuestion();
            _audioService.PlaySound("game_start");
        }

        private async void LoadNextQuestion()
        {
            var questions = await _databaseService.GetQuestionsByDifficultyAsync(_gameLogic.GetCurrentDifficulty());
            
            if (questions.Count == 0)
            {
                MessageBox.Show("No questions available for this difficulty level!", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                ShowMainMenu();
                return;
            }

            _currentQuestion = questions[new Random().Next(questions.Count)];
            DisplayQuestion();
            UpdateGameUI();
        }

        private void DisplayQuestion()
        {
            QuestionText.Text = _currentQuestion.QuestionText;
            AnswerAText.Text = _currentQuestion.AnswerA;
            AnswerBText.Text = _currentQuestion.AnswerB;
            AnswerCText.Text = _currentQuestion.AnswerC;
            AnswerDText.Text = _currentQuestion.AnswerD;

            // Reset answer button styles
            ResetAnswerButtons();
            
            // Reset lifeline availability for new question
            FinalAnswerButton.IsEnabled = false;
            _selectedAnswer = null;
        }

        private void UpdateGameUI()
        {
            QuestionCounterText.Text = $"Question {_gameLogic.CurrentQuestionNumber} of 15";
            PrizeMoneyText.Text = $"${_gameLogic.CurrentPrizeMoney:N0}";
        }

        private void ResetAnswerButtons()
        {
            var buttons = new[] { AnswerA, AnswerB, AnswerC, AnswerD };
            foreach (var button in buttons)
            {
                button.Style = (Style)FindResource("AnswerButtonStyle");
                button.IsEnabled = true;
            }
        }
        #endregion

        #region Answer Events
        private void AnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _audioService.PlaySound("answer_select");
                
                // Reset all buttons first
                ResetAnswerButtons();
                
                // Highlight selected answer
                button.Style = (Style)FindResource("SelectedAnswerStyle");
                _selectedAnswer = button.Tag.ToString();
                FinalAnswerButton.IsEnabled = true;
            }
        }

        private void FinalAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedAnswer))
                return;

            _audioService.PlaySound("final_answer");
            
            // Disable all answer buttons
            var buttons = new[] { AnswerA, AnswerB, AnswerC, AnswerD };
            foreach (var button in buttons)
            {
                button.IsEnabled = false;
            }
            
            FinalAnswerButton.IsEnabled = false;
            
            // Show correct answer after delay
            _answerTimer.Start();
        }

        private void AnswerTimer_Tick(object sender, EventArgs e)
        {
            _answerTimer.Stop();
            ProcessAnswer();
        }

        private void ProcessAnswer()
        {
            bool isCorrect = _selectedAnswer == _currentQuestion.CorrectAnswer;
            
            // Highlight correct and wrong answers
            HighlightAnswers(isCorrect);
            
            if (isCorrect)
            {
                _audioService.PlaySound("correct_answer");
                _gameLogic.AnswerCorrect();
                
                if (_gameLogic.IsGameWon())
                {
                    ShowGameOver("Congratulations! You're a Shillionaire!");
                }
                else
                {
                    // Continue to next question after delay
                    var continueTimer = new DispatcherTimer();
                    continueTimer.Interval = TimeSpan.FromSeconds(3);
                    continueTimer.Tick += (s, e) =>
                    {
                        continueTimer.Stop();
                        LoadNextQuestion();
                    };
                    continueTimer.Start();
                }
            }
            else
            {
                _audioService.PlaySound("wrong_answer");
                ShowGameOver("Wrong Answer!");
            }
        }

        private void HighlightAnswers(bool selectedIsCorrect)
        {
            var buttons = new[] { AnswerA, AnswerB, AnswerC, AnswerD };
            
            foreach (var button in buttons)
            {
                string answer = button.Tag.ToString();
                
                if (answer == _currentQuestion.CorrectAnswer)
                {
                    button.Style = (Style)FindResource("CorrectAnswerStyle");
                }
                else if (answer == _selectedAnswer && !selectedIsCorrect)
                {
                    button.Style = (Style)FindResource("WrongAnswerStyle");
                }
            }
        }

        private void WalkAwayButton_Click(object sender, RoutedEventArgs e)
        {
            _audioService.PlaySound("walk_away");
            ShowGameOver("You walked away with your winnings!");
        }
        #endregion

        #region Lifelines
        private void FiftyFiftyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameLogic.UseFiftyFifty())
            {
                _audioService.PlaySound("lifeline_use");
                FiftyFiftyButton.IsEnabled = false;
                
                // Remove two wrong answers
                var wrongAnswers = new List<string> { "A", "B", "C", "D" }
                    .Where(a => a != _currentQuestion.CorrectAnswer)
                    .OrderBy(x => Guid.NewGuid())
                    .Take(2)
                    .ToList();

                var buttons = new[] { AnswerA, AnswerB, AnswerC, AnswerD };
                foreach (var button in buttons)
                {
                    if (wrongAnswers.Contains(button.Tag.ToString()))
                    {
                        button.IsEnabled = false;
                        button.Opacity = 0.3;
                    }
                }
            }
        }

        private void PhoneFriendButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameLogic.UsePhoneAFriend())
            {
                _audioService.PlaySound("lifeline_use");
                PhoneFriendButton.IsEnabled = false;
                
                // Simulate friend's advice (80% chance of correct answer)
                var random = new Random();
                string friendAnswer = random.NextDouble() < 0.8 
                    ? _currentQuestion.CorrectAnswer 
                    : new[] { "A", "B", "C", "D" }[random.Next(4)];
                
                MessageBox.Show($"Your friend thinks the answer is {friendAnswer}.", 
                    "Phone a Friend", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AskAudienceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_gameLogic.UseAskTheAudience())
            {
                _audioService.PlaySound("lifeline_use");
                AskAudienceButton.IsEnabled = false;
                
                // Simulate audience poll (correct answer gets highest percentage)
                var random = new Random();
                var percentages = new Dictionary<string, int>();
                
                // Give correct answer 40-70% of votes
                percentages[_currentQuestion.CorrectAnswer] = random.Next(40, 71);
                
                // Distribute remaining percentage among other answers
                var remaining = 100 - percentages[_currentQuestion.CorrectAnswer];
                var otherAnswers = new[] { "A", "B", "C", "D" }
                    .Where(a => a != _currentQuestion.CorrectAnswer)
                    .ToArray();
                
                for (int i = 0; i < otherAnswers.Length; i++)
                {
                    if (i == otherAnswers.Length - 1)
                    {
                        percentages[otherAnswers[i]] = remaining;
                    }
                    else
                    {
                        int percent = random.Next(0, remaining / 2);
                        percentages[otherAnswers[i]] = percent;
                        remaining -= percent;
                    }
                }
                
                string pollResults = string.Join("\n", 
                    percentages.OrderBy(kvp => kvp.Key)
                              .Select(kvp => $"{kvp.Key}: {kvp.Value}%"));
                
                MessageBox.Show($"Audience Poll Results:\n\n{pollResults}", 
                    "Ask the Audience", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region Screen Navigation
        private void ShowMainMenu()
        {
            MainMenuScreen.Visibility = Visibility.Visible;
            GameScreen.Visibility = Visibility.Collapsed;
            AdminScreen.Visibility = Visibility.Collapsed;
            GameOverScreen.Visibility = Visibility.Collapsed;
        }

        private void ShowGameScreen()
        {
            MainMenuScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Visible;
            AdminScreen.Visibility = Visibility.Collapsed;
            GameOverScreen.Visibility = Visibility.Collapsed;
            
            // Reset lifeline buttons
            FiftyFiftyButton.IsEnabled = true;
            PhoneFriendButton.IsEnabled = true;
            AskAudienceButton.IsEnabled = true;
        }

        private void ShowAdminPanel()
        {
            MainMenuScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Collapsed;
            AdminScreen.Visibility = Visibility.Visible;
            GameOverScreen.Visibility = Visibility.Collapsed;
            
            RefreshQuestionsList();
        }

        private void ShowGameOver(string title)
        {
            MainMenuScreen.Visibility = Visibility.Collapsed;
            GameScreen.Visibility = Visibility.Collapsed;
            AdminScreen.Visibility = Visibility.Collapsed;
            GameOverScreen.Visibility = Visibility.Visible;
            
            GameOverTitle.Text = title;
            FinalPrizeText.Text = $"You won: ${_gameLogic.CurrentPrizeMoney:N0}";
            QuestionsAnsweredText.Text = $"Questions answered: {_gameLogic.CurrentQuestionNumber - 1}";
        }
        #endregion

        #region Admin Panel Events
        private async void SaveQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateQuestionForm())
            {
                var question = new Question
                {
                    QuestionText = QuestionTextBox.Text.Trim(),
                    AnswerA = AnswerATextBox.Text.Trim(),
                    AnswerB = AnswerBTextBox.Text.Trim(),
                    AnswerC = AnswerCTextBox.Text.Trim(),
                    AnswerD = AnswerDTextBox.Text.Trim(),
                    CorrectAnswer = ((ComboBoxItem)CorrectAnswerComboBox.SelectedItem).Tag.ToString(),
                    Difficulty = int.Parse(((ComboBoxItem)DifficultyComboBox.SelectedItem).Tag.ToString())
                };

                await _databaseService.AddQuestionAsync(question);
                ClearQuestionForm();
                RefreshQuestionsList();
                
                MessageBox.Show("Question saved successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearFormButton_Click(object sender, RoutedEventArgs e)
        {
            ClearQuestionForm();
        }

        private void QuestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (QuestionsListBox.SelectedItem is Question question)
            {
                LoadQuestionToForm(question);
            }
        }

        private void ImportQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Import Questions"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _databaseService.ImportQuestionsFromJson(openFileDialog.FileName);
                    RefreshQuestionsList();
                    MessageBox.Show("Questions imported successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing questions: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportQuestionsButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Export Questions",
                FileName = "questions.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _databaseService.ExportQuestionsToJson(saveFileDialog.FileName);
                    MessageBox.Show("Questions exported successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting questions: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
        }
        #endregion

        #region Game Over Events
        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            ShowMainMenu();
        }
        #endregion

        #region Helper Methods
        private bool ValidateQuestionForm()
        {
            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text) ||
                string.IsNullOrWhiteSpace(AnswerATextBox.Text) ||
                string.IsNullOrWhiteSpace(AnswerBTextBox.Text) ||
                string.IsNullOrWhiteSpace(AnswerCTextBox.Text) ||
                string.IsNullOrWhiteSpace(AnswerDTextBox.Text) ||
                DifficultyComboBox.SelectedItem == null ||
                CorrectAnswerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void ClearQuestionForm()
        {
            QuestionTextBox.Clear();
            AnswerATextBox.Clear();
            AnswerBTextBox.Clear();
            AnswerCTextBox.Clear();
            AnswerDTextBox.Clear();
            DifficultyComboBox.SelectedIndex = -1;
            CorrectAnswerComboBox.SelectedIndex = -1;
        }

        private void LoadQuestionToForm(Question question)
        {
            QuestionTextBox.Text = question.QuestionText;
            AnswerATextBox.Text = question.AnswerA;
            AnswerBTextBox.Text = question.AnswerB;
            AnswerCTextBox.Text = question.AnswerC;
            AnswerDTextBox.Text = question.AnswerD;
            
            DifficultyComboBox.SelectedIndex = question.Difficulty - 1;
            CorrectAnswerComboBox.SelectedIndex = question.CorrectAnswer switch
            {
                "A" => 0,
                "B" => 1,
                "C" => 2,
                "D" => 3,
                _ => -1
            };
        }

        private async void RefreshQuestionsList()
        {
            var questions = await _databaseService.GetAllQuestionsAsync();
            QuestionsListBox.ItemsSource = questions;
        }
        #endregion
    }
}
using System;

namespace WhoWantsToBeAShillionaire.Services
{
    public class GameLogic
    {
        private readonly int[] _prizeAmounts = 
        {
            100, 200, 300, 500, 1000,           // Questions 1-5 (Easy)
            2000, 4000, 8000, 16000, 32000,     // Questions 6-10 (Medium)
            64000, 125000, 250000, 500000, 1000000  // Questions 11-15 (Hard)
        };

        private readonly int[] _safeHavens = { 1000, 32000, 1000000 }; // Questions 5, 10, 15

        public int CurrentQuestionNumber { get; private set; }
        public int CurrentPrizeMoney { get; private set; }
        public bool FiftyFiftyUsed { get; private set; }
        public bool PhoneAFriendUsed { get; private set; }
        public bool AskTheAudienceUsed { get; private set; }

        public GameLogic()
        {
            StartNewGame();
        }

        public void StartNewGame()
        {
            CurrentQuestionNumber = 1;
            CurrentPrizeMoney = 0;
            FiftyFiftyUsed = false;
            PhoneAFriendUsed = false;
            AskTheAudienceUsed = false;
        }

        public int GetCurrentDifficulty()
        {
            return CurrentQuestionNumber switch
            {
                <= 5 => 1,   // Easy
                <= 10 => 2,  // Medium
                _ => 3       // Hard
            };
        }

        public void AnswerCorrect()
        {
            if (CurrentQuestionNumber <= _prizeAmounts.Length)
            {
                CurrentPrizeMoney = _prizeAmounts[CurrentQuestionNumber - 1];
                CurrentQuestionNumber++;
            }
        }

        public bool IsGameWon()
        {
            return CurrentQuestionNumber > 15;
        }

        public int GetGuaranteedMoney()
        {
            // Return the highest safe haven amount that has been reached
            if (CurrentQuestionNumber > 10)
                return 32000;
            else if (CurrentQuestionNumber > 5)
                return 1000;
            else
                return 0;
        }

        public int GetNextPrizeAmount()
        {
            if (CurrentQuestionNumber <= _prizeAmounts.Length)
                return _prizeAmounts[CurrentQuestionNumber - 1];
            return 1000000; // Maximum prize
        }

        public bool UseFiftyFifty()
        {
            if (!FiftyFiftyUsed)
            {
                FiftyFiftyUsed = true;
                return true;
            }
            return false;
        }

        public bool UsePhoneAFriend()
        {
            if (!PhoneAFriendUsed)
            {
                PhoneAFriendUsed = true;
                return true;
            }
            return false;
        }

        public bool UseAskTheAudience()
        {
            if (!AskTheAudienceUsed)
            {
                AskTheAudienceUsed = true;
                return true;
            }
            return false;
        }

        public string GetQuestionDifficultyText()
        {
            return GetCurrentDifficulty() switch
            {
                1 => "Easy",
                2 => "Medium",
                3 => "Hard",
                _ => "Unknown"
            };
        }

        public string GetPrizeText(int questionNumber)
        {
            if (questionNumber <= 0 || questionNumber > _prizeAmounts.Length)
                return "$0";
            
            return $"${_prizeAmounts[questionNumber - 1]:N0}";
        }

        public bool IsSafeHaven(int questionNumber)
        {
            return Array.Exists(_safeHavens, amount => 
                questionNumber > 0 && 
                questionNumber <= _prizeAmounts.Length && 
                _prizeAmounts[questionNumber - 1] == amount);
        }

        public string GetGameProgressText()
        {
            if (CurrentQuestionNumber > 15)
                return "Congratulations! You're a Shillionaire!";
            
            var difficulty = GetQuestionDifficultyText();
            var nextPrize = GetPrizeText(CurrentQuestionNumber);
            
            return $"Question {CurrentQuestionNumber} ({difficulty}) - Playing for {nextPrize}";
        }

        public double GetProgressPercentage()
        {
            return Math.Min(100.0, (CurrentQuestionNumber - 1) / 15.0 * 100.0);
        }

        public string[] GetPrizeLadder()
        {
            var ladder = new string[15];
            for (int i = 0; i < _prizeAmounts.Length; i++)
            {
                var questionNum = i + 1;
                var amount = $"${_prizeAmounts[i]:N0}";
                var marker = "";
                
                if (questionNum == CurrentQuestionNumber)
                    marker = " ◄ CURRENT";
                else if (questionNum < CurrentQuestionNumber)
                    marker = " ✓";
                else if (IsSafeHaven(questionNum))
                    marker = " ♦ SAFE";
                
                ladder[14 - i] = $"{questionNum:D2}. {amount}{marker}";
            }
            return ladder;
        }

        public int CalculateWalkAwayAmount()
        {
            // If they haven't answered any questions correctly, they get nothing
            if (CurrentQuestionNumber <= 1)
                return 0;
            
            // Otherwise, they get the amount from the previous question
            return _prizeAmounts[CurrentQuestionNumber - 2];
        }
    }
}
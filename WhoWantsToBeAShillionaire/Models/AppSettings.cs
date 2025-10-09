using System;
using System.Linq;
using System.Collections.Generic;

namespace WhoWantsToBeAShillionaire.Models
{
    public partial class AppSettings
    {
        public AISettings AISettings { get; set; } = new AISettings();
        public AudioSettings AudioSettings { get; set; } = new AudioSettings();
        public GameSettings GameSettings { get; set; } = new GameSettings();

        // Legacy flat properties used by AIService validation
        public int MinQuestionLength { get; set; } = 10;
        public int MaxQuestionLength { get; set; } = 300;
        public int MinAnswerLength { get; set; } = 1;
        public int MaxAnswerLength { get; set; } = 100;
        public bool AllowDuplicateQuestions { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 30;

        // Convenience passthroughs for legacy direct property access
        public string AiProvider => AISettings.Provider;
        public string ApiKey => AISettings.ApiKey;
        public string CustomApiUrl => AISettings.CustomApiUrl;
        public double Temperature => AISettings.Temperature;
        public int MaxTokens => AISettings.MaxTokens;
        public string Model => AISettings.Model;
        public string[] PreferredCategories => AISettings.PreferredCategories;

        public AppSettings Clone()
        {
            return new AppSettings
            {
                AISettings = new AISettings
                {
                    Provider = this.AISettings.Provider,
                    ApiKey = this.AISettings.ApiKey,
                    GeminiApiKey = this.AISettings.GeminiApiKey,
                    CustomApiUrl = this.AISettings.CustomApiUrl,
                    QuestionsPerDifficulty = this.AISettings.QuestionsPerDifficulty,
                    Temperature = this.AISettings.Temperature,
                    Model = this.AISettings.Model,
                    MaxTokens = this.AISettings.MaxTokens,
                    PreferredCategories = this.AISettings.PreferredCategories?.ToArray() ?? new string[0],
                    ImagePrompt = this.AISettings.ImagePrompt
                },
                AudioSettings = new AudioSettings
                {
                    EnableSoundEffects = this.AudioSettings.EnableSoundEffects,
                    Volume = this.AudioSettings.Volume
                },
                GameSettings = new GameSettings
                {
                    AutoSave = this.GameSettings.AutoSave,
                    EnableQuestionTimer = this.GameSettings.EnableQuestionTimer,
                    Theme = this.GameSettings.Theme,
                    NumberOfTiers = this.GameSettings.NumberOfTiers,
                    SafeHavenTiers = this.GameSettings.SafeHavenTiers?.ToList() ?? new List<int>(),
                    UseCustomPrizes = this.GameSettings.UseCustomPrizes,
                    PrizeTiers = this.GameSettings.PrizeTiers?.Select(p => p?.Clone()).Where(p => p != null).ToList() ?? new List<PrizeTierConfig>(),
                    CustomQuestions = this.GameSettings.CustomQuestions?.Select(q => q?.Clone()).Where(q => q != null).ToList() ?? new List<CustomTriviaQuestion>()
                },
                MinQuestionLength = this.MinQuestionLength,
                MaxQuestionLength = this.MaxQuestionLength,
                MinAnswerLength = this.MinAnswerLength,
                MaxAnswerLength = this.MaxAnswerLength,
                AllowDuplicateQuestions = this.AllowDuplicateQuestions,
                TimeoutSeconds = this.TimeoutSeconds,
                EnableDifficultyScaling = this.EnableDifficultyScaling,
                EasyDifficultyMultiplier = this.EasyDifficultyMultiplier,
                MediumDifficultyMultiplier = this.MediumDifficultyMultiplier,
                HardDifficultyMultiplier = this.HardDifficultyMultiplier
            };
        }

        public bool IsValid()
        {
            if (AISettings == null || AudioSettings == null || GameSettings == null)
                return false;

            if (MinQuestionLength <= 0 || MaxQuestionLength < MinQuestionLength)
                return false;

            if (MinAnswerLength <= 0 || MaxAnswerLength < MinAnswerLength)
                return false;

            if (TimeoutSeconds <= 0 || TimeoutSeconds > 300)
                return false;

            // Basic AI settings validation
            if (string.IsNullOrWhiteSpace(AISettings.Provider))
                return false;

            // Require Custom API URL when provider is custom (case-insensitive)
            if (string.Equals(AISettings.Provider, "custom", StringComparison.OrdinalIgnoreCase) && 
                string.IsNullOrWhiteSpace(AISettings.CustomApiUrl))
                return false;

            if (AISettings.QuestionsPerDifficulty <= 0 || AISettings.QuestionsPerDifficulty > 100)
                return false;

            if (AISettings.Temperature < 0 || AISettings.Temperature > 2)
                return false;

            // Game settings minimal validation
            if (GameSettings.NumberOfTiers <= 0 || GameSettings.NumberOfTiers > 50)
                return false;

            return true;
        }
    }

    public class AISettings
    {
        public string Provider { get; set; } = "OpenAI";
        public string ApiKey { get; set; } = "";
        public string GeminiApiKey { get; set; } = ""; // Gemini-specific key for validation/fetching
        public string CustomApiUrl { get; set; } = "";
        public int QuestionsPerDifficulty { get; set; } = 10;
        public double Temperature { get; set; } = 0.7;
        public string Model { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 256;
        public string[] PreferredCategories { get; set; } = new[] { "General Knowledge", "Science", "History", "Geography", "Sports", "Entertainment", "Literature" };
        public string ImagePrompt { get; set; } = "Create a polished Gemini-themed sci‑fi trivia game UI background: luminous blues and purples, subtle grid and particles, widescreen composition";
    }

    public class AudioSettings
    {
        public bool EnableSoundEffects { get; set; } = true;
        public int Volume { get; set; } = 50;
    }

    public class GameSettings
    {
        public bool AutoSave { get; set; } = true;
        public bool EnableQuestionTimer { get; set; } = false;
        public string Theme { get; set; } = "Default";

        // Trivia game ladder configuration
        public int NumberOfTiers { get; set; } = 15;
        public List<int> SafeHavenTiers { get; set; } = new List<int>();

        // Prize configuration
        public bool UseCustomPrizes { get; set; } = false;
        public List<PrizeTierConfig> PrizeTiers { get; set; } = new List<PrizeTierConfig>();

        // Custom trivia questions authored by user
        public List<CustomTriviaQuestion> CustomQuestions { get; set; } = new List<CustomTriviaQuestion>();
    }

    public class PrizeTierConfig
    {
        public int TierIndex { get; set; } // 0-based tier index
        public string PrizeType { get; set; } = "USD"; // USD | Text | Image
        public decimal AmountUSD { get; set; } = 0m; // Only used when PrizeType == USD
        public string TextDescription { get; set; } = string.Empty; // Only used when PrizeType == Text
        public string ImagePath { get; set; } = string.Empty; // Only used when PrizeType == Image

        public PrizeTierConfig Clone()
        {
            return new PrizeTierConfig
            {
                TierIndex = this.TierIndex,
                PrizeType = this.PrizeType,
                AmountUSD = this.AmountUSD,
                TextDescription = this.TextDescription,
                ImagePath = this.ImagePath
            };
        }
    }

    public class CustomTriviaQuestion
    {
        public int TierIndex { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Answers { get; set; } = new List<string>(); // 2-4 options
        public int CorrectAnswerIndex { get; set; } = 0; // index into Answers
        public string Hint { get; set; } = string.Empty;

        public CustomTriviaQuestion Clone()
        {
            return new CustomTriviaQuestion
            {
                TierIndex = this.TierIndex,
                QuestionText = this.QuestionText,
                Answers = this.Answers?.ToList() ?? new List<string>(),
                CorrectAnswerIndex = this.CorrectAnswerIndex,
                Hint = this.Hint
            };
        }
    }

    // Additional tuning and scaling options
    public partial class AppSettings
    {
        public bool EnableDifficultyScaling { get; set; } = true;
        public double EasyDifficultyMultiplier { get; set; } = 1.0;
        public double MediumDifficultyMultiplier { get; set; } = 1.3;
        public double HardDifficultyMultiplier { get; set; } = 1.6;
    }
}
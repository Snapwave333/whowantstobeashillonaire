using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Navigation;
using WhoWantsToBeAShillionaire.Models;
using WhoWantsToBeAShillionaire.Services;

namespace WhoWantsToBeAShillionaire.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly AIService _aiService;
        private bool _isApiKeyVisible = false;
        private bool _isGeminiKeyVisible = false;
        private AppSettings _settings;
        private readonly Dictionary<string, string> _providerLinks = new()
        {
            { "openai", "https://platform.openai.com/api-keys" },
            { "anthropic", "https://console.anthropic.com/settings/keys" },
            { "google", "https://aistudio.google.com/app/apikey" },
            { "huggingface", "https://huggingface.co/settings/tokens" },
            { "cohere", "https://dashboard.cohere.com/api-keys" },
            { "ai21", "https://studio.ai21.com/account/api-key" },
            { "mistral", "https://console.mistral.ai/api-keys" },
            { "deepseek", "https://platform.deepseek.com/api_keys" },
            { "custom", "" }
        };

        // Track dynamic prize tier UI rows
        private readonly List<PrizeRowData> _prizeRows = new();

        // Helper class to track dynamic prize tier UI controls
        private class PrizeRowData
        {
            public int TierIndex;
            public ComboBox TypeComboBox;
            public TextBox AmountTextBox;
            public TextBox TextDescriptionTextBox;
            public TextBox ImagePathTextBox;
        }

        public AppSettings Settings => _settings;

        public SettingsWindow(AppSettings currentSettings)
        {
            InitializeComponent();
            _aiService = new AIService();
            _settings = currentSettings.Clone();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // AI Settings
                SelectProviderByTag(_settings.AISettings.Provider);
                APIKeyPasswordBox.Password = _settings.AISettings.ApiKey ?? "";
                APIKeyTextBox.Text = _settings.AISettings.ApiKey ?? "";
                CustomAPIURLTextBox.Text = _settings.AISettings.CustomApiUrl ?? "";
                QuestionsPerDifficultyTextBox.Text = _settings.AISettings.QuestionsPerDifficulty.ToString();
                AICreativityTextBox.Text = _settings.AISettings.Temperature.ToString("F1");
                PreferredCategoriesTextBox.Text = string.Join(", ", _settings.AISettings.PreferredCategories);
                PopulateModelOptionsForSelectedProvider();
                ModelComboBox.Text = _settings.AISettings.Model ?? string.Empty;

                // Audio Settings
                EnableSoundEffectsCheckBox.IsChecked = _settings.AudioSettings.EnableSoundEffects;
                VolumeSlider.Value = _settings.AudioSettings.Volume;
                VolumeLabel.Text = $"{_settings.AudioSettings.Volume}%";

                // Game Settings
                AutoSaveCheckBox.IsChecked = _settings.GameSettings.AutoSave;
                QuestionTimerCheckBox.IsChecked = _settings.GameSettings.EnableQuestionTimer;

                // Theme
                foreach (ComboBoxItem item in ThemeComboBox.Items)
                {
                    if (string.Equals(item.Tag?.ToString(), _settings.GameSettings.Theme ?? "Default", StringComparison.OrdinalIgnoreCase))
                    {
                        ThemeComboBox.SelectedItem = item;
                        break;
                    }
                }

                // New: Initialize Game Setup and Custom sections
                NumberOfTiersTextBox.Text = Math.Max(1, Math.Min(50, _settings.GameSettings.NumberOfTiers)).ToString();
                UseCustomPrizesCheckBox.IsChecked = _settings.GameSettings.UseCustomPrizes;

                // New: Gemini key toggle defaults to hidden
                GeminiKeyPasswordBox.Password = _settings.AISettings.GeminiApiKey ?? string.Empty;
                GeminiKeyTextBox.Text = _settings.AISettings.GeminiApiKey ?? string.Empty;
                GeminiKeyTextBox.Visibility = Visibility.Collapsed;
                GeminiKeyPasswordBox.Visibility = Visibility.Visible;

                // Initialize Image Prompt
                if (ImagePromptTextBox != null)
                {
                    ImagePromptTextBox.Text = _settings.AISettings.ImagePrompt ?? string.Empty;
                }

                // Ensure lists are initialized
                _settings.GameSettings.SafeHavenTiers ??= new List<int>();
                _settings.GameSettings.PrizeTiers ??= new List<PrizeTierConfig>();
                _settings.GameSettings.CustomQuestions ??= new List<CustomTriviaQuestion>();

                // Populate dynamic UI controls
                PopulateSafeHavenCheckboxes();
                PopulatePrizeTiersUI();
                PopulateQuestionTierComboBox();
                RefreshQuestionsListBox();

                // Update UI based on provider
                UpdateProviderDetails();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectProviderByTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) tag = "openai";
            foreach (ListBoxItem item in ProviderListBox.Items)
            {
                if (string.Equals(item.Tag?.ToString(), tag, StringComparison.OrdinalIgnoreCase))
                {
                    ProviderListBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SaveSettings()
        {
            try
            {
                // AI Settings
                _settings.AISettings.Provider = ((ListBoxItem)ProviderListBox.SelectedItem)?.Tag?.ToString() ?? "openai";
                _settings.AISettings.ApiKey = _isApiKeyVisible ? APIKeyTextBox.Text : APIKeyPasswordBox.Password;
                _settings.AISettings.GeminiApiKey = _isGeminiKeyVisible ? GeminiKeyTextBox.Text : GeminiKeyPasswordBox.Password;
                _settings.AISettings.CustomApiUrl = CustomAPIURLTextBox.Text;
                _settings.AISettings.Model = string.IsNullOrWhiteSpace(ModelComboBox.Text) 
                    ? ModelDefaults.GetDefault(_settings.AISettings.Provider) 
                    : ModelComboBox.Text;
                
                if (int.TryParse(QuestionsPerDifficultyTextBox.Text, out int qpd))
                    _settings.AISettings.QuestionsPerDifficulty = qpd;
                
                if (double.TryParse(AICreativityTextBox.Text, out double temp))
                    _settings.AISettings.Temperature = temp;
                
                _settings.AISettings.PreferredCategories = PreferredCategoriesTextBox.Text
                    .Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToArray();

                // Audio Settings
                _settings.AudioSettings.EnableSoundEffects = EnableSoundEffectsCheckBox.IsChecked ?? true;
                _settings.AudioSettings.Volume = (int)VolumeSlider.Value;

                // Game Settings
                _settings.GameSettings.AutoSave = AutoSaveCheckBox.IsChecked ?? true;
                _settings.GameSettings.EnableQuestionTimer = QuestionTimerCheckBox.IsChecked ?? false;

                if (int.TryParse(NumberOfTiersTextBox.Text, out int tiers))
                    _settings.GameSettings.NumberOfTiers = Math.Max(1, Math.Min(50, tiers));
                _settings.GameSettings.UseCustomPrizes = UseCustomPrizesCheckBox.IsChecked ?? false;

                // Persist prize tiers from UI rows if custom prizes enabled
                if (_settings.GameSettings.UseCustomPrizes)
                {
                    var prizeList = new List<PrizeTierConfig>();
                    foreach (var row in _prizeRows)
                    {
                        var type = (row.TypeComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "USD";
                        var cfg = new PrizeTierConfig
                        {
                            TierIndex = row.TierIndex,
                            PrizeType = type,
                            AmountUSD = 0m,
                            TextDescription = string.Empty,
                            ImagePath = string.Empty
                        };
                        if (type == "USD" && decimal.TryParse(row.AmountTextBox.Text, out var usd))
                            cfg.AmountUSD = usd;
                        else if (type == "Text")
                            cfg.TextDescription = row.TextDescriptionTextBox.Text ?? string.Empty;
                        else if (type == "Image")
                            cfg.ImagePath = row.ImagePathTextBox.Text ?? string.Empty;
                        prizeList.Add(cfg);
                    }
                    _settings.GameSettings.PrizeTiers = prizeList;
                }
                else
                {
                    _settings.GameSettings.PrizeTiers = new List<PrizeTierConfig>();
                }

                // Theme selection
                var selectedTheme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Default";
                _settings.GameSettings.Theme = selectedTheme;

                // Apply theme immediately
                var app = (App)Application.Current;
                app.ApplyTheme(string.Equals(selectedTheme, "LiquidGlass", StringComparison.OrdinalIgnoreCase) ? App.AppTheme.LiquidGlass : App.AppTheme.Default);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProviderDetails()
        {
            var selectedProviderTag = ((ListBoxItem)ProviderListBox.SelectedItem)?.Tag?.ToString() ?? "openai";
            var displayName = ((ListBoxItem)ProviderListBox.SelectedItem)?.Content?.ToString() ?? "OpenAI";
            SelectedProviderLabel.Text = $"Provider: {displayName}";

            if (_providerLinks.TryGetValue(selectedProviderTag.ToLower(), out var url) && !string.IsNullOrEmpty(url))
            {
                ProviderLink.NavigateUri = new Uri(url);
            }
            else
            {
                ProviderLink.NavigateUri = null;
            }

            bool isCustom = string.Equals(selectedProviderTag, "custom", StringComparison.OrdinalIgnoreCase);
            CustomAPIURLLabel.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;
            CustomAPIURLTextBox.Visibility = isCustom ? Visibility.Visible : Visibility.Collapsed;

            PopulateModelOptionsForSelectedProvider();
            ValidateModelWarning();
        }

        // Event Handlers
        private void ProviderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProviderDetails();
        }

        private void PopulateModelOptionsForSelectedProvider()
        {
            if (ModelComboBox == null) return;
            var provider = ((ListBoxItem)ProviderListBox.SelectedItem)?.Tag?.ToString() ?? "openai";
            ModelComboBox.ItemsSource = ModelDefaults.GetModels(provider);

            // Default selection if none
            if (string.IsNullOrWhiteSpace(ModelComboBox.Text))
            {
                var defaultModel = ModelDefaults.GetDefault(provider);
                ModelComboBox.Text = defaultModel;
            }
            ValidateModelWarning();
        }


        private void ShowHideAPIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            _isApiKeyVisible = !_isApiKeyVisible;

            if (_isApiKeyVisible)
            {
                APIKeyTextBox.Text = APIKeyPasswordBox.Password;
                APIKeyPasswordBox.Visibility = Visibility.Collapsed;
                APIKeyTextBox.Visibility = Visibility.Visible;
                ShowHideAPIKeyButton.Content = "Hide";
            }
            else
            {
                APIKeyPasswordBox.Password = APIKeyTextBox.Text;
                APIKeyTextBox.Visibility = Visibility.Collapsed;
                APIKeyPasswordBox.Visibility = Visibility.Visible;
                ShowHideAPIKeyButton.Content = "Show";
            }
        }

        private void DeleteAPIKeyButton_Click(object sender, RoutedEventArgs e)
        {
            APIKeyPasswordBox.Password = string.Empty;
            APIKeyTextBox.Text = string.Empty;
            _settings.AISettings.ApiKey = string.Empty;
        }

        private void ShowHideGeminiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            _isGeminiKeyVisible = !_isGeminiKeyVisible;
            if (_isGeminiKeyVisible)
            {
                GeminiKeyTextBox.Text = GeminiKeyPasswordBox.Password;
                GeminiKeyPasswordBox.Visibility = Visibility.Collapsed;
                GeminiKeyTextBox.Visibility = Visibility.Visible;
                ShowHideGeminiKeyButton.Content = "Hide";
            }
            else
            {
                GeminiKeyPasswordBox.Password = GeminiKeyTextBox.Text;
                GeminiKeyTextBox.Visibility = Visibility.Collapsed;
                GeminiKeyPasswordBox.Visibility = Visibility.Visible;
                ShowHideGeminiKeyButton.Content = "Show";
            }
        }

        private void DeleteGeminiKeyButton_Click(object sender, RoutedEventArgs e)
        {
            GeminiKeyPasswordBox.Password = string.Empty;
            GeminiKeyTextBox.Text = string.Empty;
            _settings.AISettings.GeminiApiKey = string.Empty;
        }

        private void GeminiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isGeminiKeyVisible)
            {
                _settings.AISettings.GeminiApiKey = GeminiKeyPasswordBox.Password;
            }
        }

        private void GeminiKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isGeminiKeyVisible)
            {
                _settings.AISettings.GeminiApiKey = GeminiKeyTextBox.Text;
            }
        }

        // Test connection handled below with inline status updates

        // Additional Event Handlers
        private void ImagePromptTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.AISettings.ImagePrompt = ImagePromptTextBox.Text;
        }

        private async void RegenerateBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegenerateBackgroundButton.IsEnabled = false;
                BackgroundStatusText.Text = "Generating background...";
                _settings.AISettings.ImagePrompt = ImagePromptTextBox.Text;
                if (Application.Current?.MainWindow is MainWindow mw)
                {
                    var method = typeof(MainWindow).GetMethod("ApplyDynamicBackgroundAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    if (method != null)
                    {
                        var task = (System.Threading.Tasks.Task)method.Invoke(mw, new object?[] { _settings.AISettings.ImagePrompt });
                        await task;
                        BackgroundStatusText.Text = "Background updated";
                    }
                    else
                    {
                        BackgroundStatusText.Text = "Method not found";
                    }
                }
                else
                {
                    BackgroundStatusText.Text = "Main window not available";
                }
            }
            catch (Exception ex)
            {
                BackgroundStatusText.Text = "Failed to update";
                MessageBox.Show($"Failed to regenerate background: {ex.Message}", "Background Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                RegenerateBackgroundButton.IsEnabled = true;
            }
        }

        private void APIKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isApiKeyVisible)
            {
                _settings.AISettings.ApiKey = APIKeyPasswordBox.Password;
            }
        }

        private void APIKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isApiKeyVisible)
            {
                _settings.AISettings.ApiKey = APIKeyTextBox.Text;
            }
        }

        private void CustomAPIURLTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _settings.AISettings.CustomApiUrl = CustomAPIURLTextBox.Text;
        }

        private void QuestionsPerDifficultyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(QuestionsPerDifficultyTextBox.Text, out int value))
            {
                if (value < 1) QuestionsPerDifficultyTextBox.Text = "1";
                if (value > 50) QuestionsPerDifficultyTextBox.Text = "50";
            }
        }

        private void AICreativityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(AICreativityTextBox.Text, out double value))
            {
                if (value < 0.0) AICreativityTextBox.Text = "0.0";
                if (value > 1.0) AICreativityTextBox.Text = "1.0";
            }
        }

        private void PreferredCategoriesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // No validation needed for categories
        }

        private void EnableSoundEffectsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            VolumeSlider.IsEnabled = true;
        }

        private void EnableSoundEffectsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            VolumeSlider.IsEnabled = false;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeLabel != null)
            {
                VolumeLabel.Text = $"{(int)e.NewValue}%";
            }
        }

        private void AutoSaveCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Auto-save enabled
        }

        private void AutoSaveCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Auto-save disabled
        }

        private void QuestionTimerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Question timer enabled
        }

        private void QuestionTimerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Question timer disabled
        }

        private void NumberOfTiersTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!int.TryParse(NumberOfTiersTextBox.Text, out int value))
            {
                NumberOfTiersTextBox.Text = _settings.GameSettings.NumberOfTiers.ToString();
                return;
            }
            if (value < 1) value = 1;
            if (value > 50) value = 50;
            NumberOfTiersTextBox.Text = value.ToString();
            _settings.GameSettings.NumberOfTiers = value;
            // Clamp any existing safe haven selections to the new tier count
            _settings.GameSettings.SafeHavenTiers = (_settings.GameSettings.SafeHavenTiers ?? new List<int>())
                .Where(i => i >= 0 && i < value)
                .Distinct()
                .ToList();
            PopulateSafeHavenCheckboxes();
            PopulatePrizeTiersUI();
            PopulateQuestionTierComboBox();
        }

        private void UseCustomPrizesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _settings.GameSettings.UseCustomPrizes = true;
            PopulatePrizeTiersUI();
        }

        private void UseCustomPrizesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _settings.GameSettings.UseCustomPrizes = false;
            PopulatePrizeTiersUI();
        }

        private void PopulateSafeHavenCheckboxes()
        {
            if (SafeHavenWrapPanel == null) return;
            SafeHavenWrapPanel.Children.Clear();
            var total = Math.Max(1, Math.Min(50, _settings.GameSettings.NumberOfTiers));
            var selected = _settings.GameSettings.SafeHavenTiers ?? new List<int>();
            for (int i = 0; i < total; i++)
            {
                var cb = new CheckBox
                {
                    Content = $"Tier {i + 1}",
                    Tag = i,
                    Margin = new Thickness(5, 2, 5, 2),
                    IsChecked = selected.Contains(i)
                };
                cb.Checked += SafeHavenCheckbox_Checked;
                cb.Unchecked += SafeHavenCheckbox_Unchecked;
                SafeHavenWrapPanel.Children.Add(cb);
            }
        }

        private void SafeHavenCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int idx)
            {
                _settings.GameSettings.SafeHavenTiers ??= new List<int>();
                if (!_settings.GameSettings.SafeHavenTiers.Contains(idx))
                    _settings.GameSettings.SafeHavenTiers.Add(idx);
            }
        }

        private void SafeHavenCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is int idx)
            {
                _settings.GameSettings.SafeHavenTiers ??= new List<int>();
                _settings.GameSettings.SafeHavenTiers.Remove(idx);
            }
        }

        private void PopulatePrizeTiersUI()
        {
            if (PrizeTiersItemsControl == null) return;
            PrizeTiersItemsControl.Items.Clear();
            _prizeRows.Clear();

            if (!(_settings.GameSettings.UseCustomPrizes))
            {
                PrizeTiersItemsControl.IsEnabled = false;
                return;
            }
            PrizeTiersItemsControl.IsEnabled = true;

            var total = Math.Max(1, Math.Min(50, _settings.GameSettings.NumberOfTiers));
            for (int i = 0; i < total; i++)
            {
                var rowPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
                var tierLabel = new TextBlock { Text = $"Tier {i + 1}", Width = 70, VerticalAlignment = VerticalAlignment.Center };
                var typeCombo = new ComboBox { Width = 90, Margin = new Thickness(6,0,6,0) };
                typeCombo.Items.Add(new ComboBoxItem { Content = "USD" });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Text" });
                typeCombo.Items.Add(new ComboBoxItem { Content = "Image" });
                var amountBox = new TextBox { Width = 90, Margin = new Thickness(6,0,6,0), ToolTip = "USD amount" };
                var textDescBox = new TextBox { Width = 160, Margin = new Thickness(6,0,6,0), ToolTip = "Text description" };
                var imagePathBox = new TextBox { Width = 160, Margin = new Thickness(6,0,6,0), ToolTip = "Image path" };

                var existing = _settings.GameSettings.PrizeTiers?.FirstOrDefault(p => p.TierIndex == i);
                var selectedType = existing?.PrizeType ?? "USD";
                typeCombo.SelectedItem = typeCombo.Items.Cast<ComboBoxItem>().FirstOrDefault(ci => (string)ci.Content == selectedType) ?? typeCombo.Items[0];
                if (existing != null)
                {
                    amountBox.Text = existing.AmountUSD.ToString();
                    textDescBox.Text = existing.TextDescription ?? string.Empty;
                    imagePathBox.Text = existing.ImagePath ?? string.Empty;
                }

                void updateVisibility()
                {
                    var t = (typeCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
                    amountBox.Visibility = t == "USD" ? Visibility.Visible : Visibility.Collapsed;
                    textDescBox.Visibility = t == "Text" ? Visibility.Visible : Visibility.Collapsed;
                    imagePathBox.Visibility = t == "Image" ? Visibility.Visible : Visibility.Collapsed;
                }
                updateVisibility();
                typeCombo.SelectionChanged += (s, e) => updateVisibility();

                var rowData = new PrizeRowData
                {
                    TierIndex = i,
                    TypeComboBox = typeCombo,
                    AmountTextBox = amountBox,
                    TextDescriptionTextBox = textDescBox,
                    ImagePathTextBox = imagePathBox
                };
                _prizeRows.Add(rowData);

                rowPanel.Children.Add(tierLabel);
                rowPanel.Children.Add(typeCombo);
                rowPanel.Children.Add(amountBox);
                rowPanel.Children.Add(textDescBox);
                rowPanel.Children.Add(imagePathBox);

                PrizeTiersItemsControl.Items.Add(rowPanel);
            }
        }

        private void PopulateQuestionTierComboBox()
        {
            if (QuestionTierComboBox == null) return;
            QuestionTierComboBox.Items.Clear();
            var total = Math.Max(1, Math.Min(50, _settings.GameSettings.NumberOfTiers));
            for (int i = 0; i < total; i++)
            {
                var item = new ComboBoxItem { Content = $"Tier {i + 1}", Tag = i };
                QuestionTierComboBox.Items.Add(item);
            }
            if (QuestionTierComboBox.Items.Count > 0 && QuestionTierComboBox.SelectedIndex < 0)
                QuestionTierComboBox.SelectedIndex = 0;
        }

        private void RefreshQuestionsListBox()
        {
            if (QuestionsListBox == null) return;
            QuestionsListBox.Items.Clear();
            foreach (var q in _settings.GameSettings.CustomQuestions ?? new List<CustomTriviaQuestion>())
            {
                var preview = q.QuestionText ?? string.Empty;
                if (preview.Length > 80) preview = preview.Substring(0, 77) + "...";
                var content = $"Tier {q.TierIndex + 1}: {preview}";
                var item = new ListBoxItem { Content = content, Tag = q };
                QuestionsListBox.Items.Add(item);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to reset all settings to defaults?", 
                "Reset Settings", MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _settings = new AppSettings();
                LoadSettings();
            }
        }

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Save any changes and close dialog positively
                SaveSettings();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tierIdx = 0;
                if (QuestionTierComboBox.SelectedItem is ComboBoxItem cbi && cbi.Tag is int t) tierIdx = t;
                var questionText = (QuestionTextBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(questionText))
                {
                    MessageBox.Show("Please enter a question.");
                    return;
                }
                var answersRaw = new[] { AnswerATextBox.Text, AnswerBTextBox.Text, AnswerCTextBox.Text, AnswerDTextBox.Text };
                var answers = new List<(string text, int origIdx)>();
                for (int i = 0; i < answersRaw.Length; i++)
                {
                    var txt = (answersRaw[i] ?? string.Empty).Trim();
                    if (!string.IsNullOrEmpty(txt)) answers.Add((txt, i));
                }
                if (answers.Count < 2)
                {
                    MessageBox.Show("Please provide at least two answer options.");
                    return;
                }
                int selectedOrig = (CorrectA.IsChecked == true) ? 0 : (CorrectB.IsChecked == true) ? 1 : (CorrectC.IsChecked == true) ? 2 : (CorrectD.IsChecked == true) ? 3 : -1;
                if (selectedOrig < 0)
                {
                    MessageBox.Show("Please mark one answer as correct.");
                    return;
                }
                int correctIndex = answers.FindIndex(a => a.origIdx == selectedOrig);
                if (correctIndex < 0)
                {
                    MessageBox.Show("The marked correct answer is empty. Please mark a non-empty answer as correct.");
                    return;
                }
                var hint = (HintTextBox.Text ?? string.Empty).Trim();
                var newQ = new CustomTriviaQuestion
                {
                    TierIndex = tierIdx,
                    QuestionText = questionText,
                    Answers = answers.Select(a => a.text).ToList(),
                    CorrectAnswerIndex = correctIndex,
                    Hint = hint
                };
                _settings.GameSettings.CustomQuestions ??= new List<CustomTriviaQuestion>();
                _settings.GameSettings.CustomQuestions.Add(newQ);
                RefreshQuestionsListBox();
                QuestionTextBox.Text = string.Empty;
                AnswerATextBox.Text = string.Empty;
                AnswerBTextBox.Text = string.Empty;
                AnswerCTextBox.Text = string.Empty;
                AnswerDTextBox.Text = string.Empty;
                CorrectA.IsChecked = false; CorrectB.IsChecked = false; CorrectC.IsChecked = false; CorrectD.IsChecked = false;
                HintTextBox.Text = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add question: {ex.Message}");
            }
        }

        private void RemoveSelectedQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QuestionsListBox.SelectedItem is ListBoxItem li && li.Tag is CustomTriviaQuestion q)
                {
                    _settings.GameSettings.CustomQuestions.Remove(q);
                    RefreshQuestionsListBox();
                    return;
                }
                MessageBox.Show("Please select a question to remove.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove question: {ex.Message}");
            }
        }

        private void ValidateModelWarning()
        {
            if (ModelComboBox == null || ModelWarningPanel == null) return;
            var provider = ((ListBoxItem)ProviderListBox.SelectedItem)?.Tag?.ToString() ?? "openai";
            var model = ModelComboBox.Text;
            var curated = ModelDefaults.GetModels(provider);
            if (string.Equals(provider, "custom", StringComparison.OrdinalIgnoreCase) || curated.Count == 0)
            {
                ModelWarningPanel.Visibility = Visibility.Collapsed;
                return;
            }
            bool known = ModelDefaults.IsKnownModel(provider, model);
            ModelWarningPanel.Visibility = known ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateModelWarning();
        }

        private void ModelComboBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ValidateModelWarning();
        }

        private void QuestionTimerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(QuestionTimerTextBox.Text, out int value))
            {
                if (value < 0) QuestionTimerTextBox.Text = "0";
                if (value > 300) QuestionTimerTextBox.Text = "300";
            }
        }

        private async void TestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TestConnectionButton.IsEnabled = false;
                ConnectionStatusTextBlock.Text = "Testing connection...";
                var provider = ((ListBoxItem)ProviderListBox.SelectedItem)?.Tag?.ToString() ?? "openai";
                var apiKey = _isApiKeyVisible ? APIKeyTextBox.Text : APIKeyPasswordBox.Password;
                var customUrl = CustomAPIURLTextBox.Text;

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    ConnectionStatusTextBlock.Text = "Connection Status: API key is required";
                    ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                var aiService = new AIService();
                var success = await aiService.TestConnectionAsync(provider, apiKey, customUrl);

                if (success)
                {
                    ConnectionStatusTextBlock.Text = "Connection Status: ✓ Connected successfully";
                    ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    ConnectionStatusTextBlock.Text = "Connection Status: ✗ Connection failed";
                    ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                ConnectionStatusTextBlock.Text = $"Connection Status: Error - {ex.Message}";
                ConnectionStatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
            finally
            {
                TestConnectionButton.IsEnabled = true;
            }
        }

        private void ProviderLink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                if (e.Uri != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = e.Uri.AbsoluteUri,
                        UseShellExecute = true
                    });
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
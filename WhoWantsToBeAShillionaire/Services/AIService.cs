using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using WhoWantsToBeAShillionaire.Models;

namespace WhoWantsToBeAShillionaire.Services
{
    public class AIService
    {
        private readonly HttpClient _httpClient;
        private readonly SettingsService _settingsService;
        private AppSettings _settings;

        public AIService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _settingsService = new SettingsService();
            _settings = _settingsService.LoadSettings();
        }

        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings;
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public async Task<bool> TestConnectionAsync(string provider, string apiKey, string customUrl = null)
        {
            try
            {
                var testPrompt = "Respond with 'OK' if you can understand this message.";
                var response = await GenerateResponseAsync(provider, apiKey, testPrompt, customUrl);
                return !string.IsNullOrWhiteSpace(response) && response.ToUpper().Contains("OK");
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Question>> GenerateQuestionsAsync(string difficulty, int count, List<string> categories = null)
        {
            var questions = new List<Question>();
            
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                throw new Exception("API key is not configured");

            var difficultyMultiplier = GetDifficultyMultiplier(difficulty);
            var categoryList = categories ?? (_settings.PreferredCategories?.ToList() ?? new List<string>());

            for (int i = 0; i < count; i++)
            {
                try
                {
                    var question = await GenerateSingleQuestionAsync(difficulty, categoryList, difficultyMultiplier);
                    if (question != null && IsValidQuestion(question))
                    {
                        questions.Add(question);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to generate question {i + 1}: {ex.Message}");
                    // Continue with next question
                }
            }

            return questions;
        }

        private async Task<Question> GenerateSingleQuestionAsync(string difficulty, List<string> categories, double difficultyMultiplier)
        {
            var prompt = BuildQuestionPrompt(difficulty, categories, difficultyMultiplier);
            var response = await GenerateResponseAsync(_settings.AiProvider, _settings.ApiKey, prompt, _settings.CustomApiUrl);
            
            if (string.IsNullOrWhiteSpace(response))
                return null;

            return ParseQuestionFromResponse(response, difficulty);
        }

        private string BuildQuestionPrompt(string difficulty, List<string> categories, double difficultyMultiplier)
        {
            var categoryText = categories.Any() ? string.Join(", ", categories) : "General Knowledge";
            
            var basePrompt = $@"Generate a multiple-choice trivia question for a game show like 'Who Wants to Be a Millionaire'.

Requirements:
- Difficulty: {difficulty} (complexity multiplier: {difficultyMultiplier:F1})
- Categories: {categoryText}
- Format: JSON with fields: question, answerA, answerB, answerC, answerD, correctAnswer
- correctAnswer should be 'A', 'B', 'C', or 'D'
- Question should be {GetQuestionComplexityDescription(difficulty)}
- All answers should be plausible but only one correct
- Avoid overly obscure or trick questions
- Keep answers concise (2-50 characters each)
- Question length: 10-200 characters

Difficulty Guidelines:
- Easy: Common knowledge, basic facts, popular culture
- Medium: Requires some education or general awareness
- Hard: Specialized knowledge, complex concepts, historical details

Example format:
{{
  ""question"": ""What is the capital of France?"",
  ""answerA"": ""London"",
  ""answerB"": ""Berlin"",
  ""answerC"": ""Paris"",
  ""answerD"": ""Madrid"",
  ""correctAnswer"": ""C""
}}

Generate ONE question now:";

            return basePrompt;
        }

        private string GetQuestionComplexityDescription(string difficulty)
        {
            return difficulty.ToLower() switch
            {
                "easy" => "straightforward and accessible to most people",
                "medium" => "moderately challenging, requiring some knowledge",
                "hard" => "challenging and requiring specialized or detailed knowledge",
                _ => "appropriately challenging"
            };
        }

        private double GetDifficultyMultiplier(string difficulty)
        {
            if (!_settings.EnableDifficultyScaling)
                return 1.0;

            return difficulty.ToLower() switch
            {
                "easy" => _settings.EasyDifficultyMultiplier,
                "medium" => _settings.MediumDifficultyMultiplier,
                "hard" => _settings.HardDifficultyMultiplier,
                _ => 1.0
            };
        }

        private async Task<string> GenerateResponseAsync(string provider, string apiKey, string prompt, string customUrl = null)
        {
            switch (provider?.ToLower())
            {
                case "openai":
                    return await CallOpenAIAsync(apiKey, prompt);
                case "anthropic":
                    return await CallAnthropicAsync(apiKey, prompt);
                case "google":
                    return await CallGoogleAsync(apiKey, prompt);
                case "huggingface":
                    return await CallHuggingFaceAsync(apiKey, prompt);
                case "cohere":
                    return await CallCohereAsync(apiKey, prompt);
                case "ai21":
                    return await CallAI21Async(apiKey, prompt);
                case "mistral":
                    return await CallMistralAsync(apiKey, prompt);
                case "deepseek":
                    return await CallDeepSeekAsync(apiKey, prompt);
                case "custom":
                    return await CallCustomAPIAsync(customUrl, apiKey, prompt);
                default:
                    throw new NotSupportedException($"AI provider '{provider}' is not supported");
            }
        }

        private async Task<string> CallOpenAIAsync(string apiKey, string prompt)
        {
            var url = "https://api.openai.com/v1/chat/completions";
            
            var requestBody = new
            {
                model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("openai") : _settings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a trivia question generator. Always respond with valid JSON." },
                    new { role = "user", content = prompt }
                },
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return responseObj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }

        private async Task<string> CallAnthropicAsync(string apiKey, string prompt)
        {
            var url = "https://api.anthropic.com/v1/messages";
            
            var requestBody = new
            {
                model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("anthropic") : _settings.Model,
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return responseObj.GetProperty("content")[0].GetProperty("text").GetString();
        }

        private async Task<string> CallGoogleAsync(string apiKey, string prompt)
        {
            var model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("google") : _settings.Model;
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = _settings.Temperature,
                    maxOutputTokens = _settings.MaxTokens
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return responseObj.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        }

        private async Task<string> CallHuggingFaceAsync(string apiKey, string prompt)
        {
            // Uses Hugging Face Inference API
            var model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("huggingface") : _settings.Model;
            var url = $"https://api-inference.huggingface.co/models/{model}";

            var requestBody = new
            {
                inputs = prompt,
                parameters = new
                {
                    temperature = _settings.Temperature,
                    max_new_tokens = _settings.MaxTokens
                }
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var arr = JsonSerializer.Deserialize<JsonElement>(responseJson);
            // Typical HF text generation returns an array with generated_text
            if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
            {
                var first = arr[0];
                if (first.TryGetProperty("generated_text", out var gt))
                    return gt.GetString();
            }
            return responseJson;
        }

        private async Task<string> CallCohereAsync(string apiKey, string prompt)
        {
            var url = "https://api.cohere.ai/v1/generate";
            var model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("cohere") : _settings.Model;

            var requestBody = new
            {
                model = model,
                prompt = prompt,
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            if (obj.TryGetProperty("generations", out var gens) && gens.ValueKind == JsonValueKind.Array && gens.GetArrayLength() > 0)
            {
                var text = gens[0].GetProperty("text").GetString();
                return text;
            }
            return responseJson;
        }

        private async Task<string> CallAI21Async(string apiKey, string prompt)
        {
            var model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("ai21") : _settings.Model;
            var url = $"https://api.ai21.com/studio/v1/{model}/complete";

            var requestBody = new
            {
                prompt = prompt,
                maxTokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            if (obj.TryGetProperty("completions", out var comps) && comps.ValueKind == JsonValueKind.Array && comps.GetArrayLength() > 0)
            {
                var data = comps[0].GetProperty("data");
                if (data.TryGetProperty("text", out var txt))
                    return txt.GetString();
            }
            return responseJson;
        }

        private async Task<string> CallMistralAsync(string apiKey, string prompt)
        {
            var url = "https://api.mistral.ai/v1/chat/completions";
            var model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("mistral") : _settings.Model;

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are a trivia question generator. Always respond with valid JSON." },
                    new { role = "user", content = prompt }
                },
                temperature = _settings.Temperature,
                max_tokens = _settings.MaxTokens
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            if (obj.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var msg = choices[0].GetProperty("message");
                if (msg.TryGetProperty("content", out var contentProp))
                    return contentProp.GetString();
            }
            return responseJson;
        }

        private async Task<string> CallDeepSeekAsync(string apiKey, string prompt)
        {
            // DeepSeek follows OpenAI-like API
            var url = "https://api.deepseek.com/v1/chat/completions";
            var model = string.IsNullOrWhiteSpace(_settings.Model) ? ModelDefaults.GetDefault("deepseek") : _settings.Model;

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "You are a trivia question generator. Always respond with valid JSON." },
                    new { role = "user", content = prompt }
                },
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var obj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            if (obj.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var msg = choices[0].GetProperty("message");
                if (msg.TryGetProperty("content", out var contentProp))
                    return contentProp.GetString();
            }
            return responseJson;
        }

        private async Task<string> CallCustomAPIAsync(string url, string apiKey, string prompt)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Custom API URL is required");

            var requestBody = new
            {
                prompt = prompt,
                max_tokens = _settings.MaxTokens,
                temperature = _settings.Temperature
            };

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WhoWantsToBeAShillionaire/1.0");

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            
            // Try to parse as standard format first
            try
            {
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                if (responseObj.TryGetProperty("response", out var responseProperty))
                    return responseProperty.GetString();
                if (responseObj.TryGetProperty("text", out var textProperty))
                    return textProperty.GetString();
                if (responseObj.TryGetProperty("content", out var contentProperty))
                    return contentProperty.GetString();
            }
            catch
            {
                // If JSON parsing fails, return raw response
            }

            return responseJson;
        }

        private Question ParseQuestionFromResponse(string response, string difficulty)
        {
            try
            {
                // Clean up the response - remove markdown code blocks if present
                var cleanResponse = response.Trim();
                if (cleanResponse.StartsWith("```json"))
                {
                    cleanResponse = cleanResponse.Substring(7);
                }
                if (cleanResponse.StartsWith("```"))
                {
                    cleanResponse = cleanResponse.Substring(3);
                }
                if (cleanResponse.EndsWith("```"))
                {
                    cleanResponse = cleanResponse.Substring(0, cleanResponse.Length - 3);
                }
                cleanResponse = cleanResponse.Trim();

                // Try to find JSON in the response
                var startIndex = cleanResponse.IndexOf('{');
                var endIndex = cleanResponse.LastIndexOf('}');
                
                if (startIndex >= 0 && endIndex > startIndex)
                {
                    cleanResponse = cleanResponse.Substring(startIndex, endIndex - startIndex + 1);
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var questionData = JsonSerializer.Deserialize<JsonElement>(cleanResponse, options);

                var question = new Question
                {
                    QuestionText = questionData.GetProperty("question").GetString(),
                    AnswerA = questionData.GetProperty("answerA").GetString(),
                    AnswerB = questionData.GetProperty("answerB").GetString(),
                    AnswerC = questionData.GetProperty("answerC").GetString(),
                    AnswerD = questionData.GetProperty("answerD").GetString(),
                    CorrectAnswer = questionData.GetProperty("correctAnswer").GetString().ToUpper(),
                    Difficulty = MapDifficultyToInt(difficulty)
                };

                return question;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse AI response: {ex.Message}");
                Console.WriteLine($"Response: {response}");
                return null;
            }
        }

        private int MapDifficultyToInt(string difficulty)
        {
            switch (difficulty?.ToLower())
            {
                case "easy":
                    return 1;
                case "medium":
                    return 2;
                case "hard":
                    return 3;
                default:
                    return 1;
            }
        }

        private bool IsValidQuestion(Question question)
        {
            if (question == null) return false;
            
            // Check required fields
            if (string.IsNullOrWhiteSpace(question.QuestionText) ||
                string.IsNullOrWhiteSpace(question.AnswerA) ||
                string.IsNullOrWhiteSpace(question.AnswerB) ||
                string.IsNullOrWhiteSpace(question.AnswerC) ||
                string.IsNullOrWhiteSpace(question.AnswerD) ||
                string.IsNullOrWhiteSpace(question.CorrectAnswer))
            {
                return false;
            }

            // Check correct answer format
            if (!new[] { "A", "B", "C", "D" }.Contains(question.CorrectAnswer.ToUpper()))
            {
                return false;
            }

            // Check length constraints
            if (question.QuestionText.Length < _settings.MinQuestionLength ||
                question.QuestionText.Length > _settings.MaxQuestionLength)
            {
                return false;
            }

            var answers = new[] { question.AnswerA, question.AnswerB, question.AnswerC, question.AnswerD };
            foreach (var answer in answers)
            {
                if (answer.Length < _settings.MinAnswerLength ||
                    answer.Length > _settings.MaxAnswerLength)
                {
                    return false;
                }
            }

            // Check for duplicate answers (if not allowed)
            if (!_settings.AllowDuplicateQuestions)
            {
                var uniqueAnswers = answers.Distinct().Count();
                if (uniqueAnswers < 4)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<(string imagePath, string caption)> GenerateImageWithGeminiAsync(string prompt, string fileName = "generated_background.png", string model = "gemini-2.5-flash-image-preview")
        {
            var apiKey = _settings?.AISettings?.GeminiApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Gemini API key is not configured in Settings.");

            var outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated");
            Directory.CreateDirectory(outputDir);
            var outPath = Path.Combine(outputDir, fileName);

            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var requestObj = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };
            var json = JsonSerializer.Serialize(requestObj);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            using var response = await _httpClient.SendAsync(httpRequest);
            var responseText = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini image generation failed: {response.StatusCode} - {responseText}");

            string caption = null;
            try
            {
                using var doc = JsonDocument.Parse(responseText);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array)
                {
                    foreach (var candidate in candidates.EnumerateArray())
                    {
                        if (candidate.TryGetProperty("content", out var content))
                        {
                            if (content.TryGetProperty("parts", out var parts) && parts.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var part in parts.EnumerateArray())
                                {
                                    if (part.TryGetProperty("text", out var textProp) && textProp.ValueKind == JsonValueKind.String)
                                    {
                                        caption = textProp.GetString();
                                    }
                                    if (part.TryGetProperty("inlineData", out var inlineData) && inlineData.ValueKind == JsonValueKind.Object)
                                    {
                                        if (inlineData.TryGetProperty("data", out var dataProp) && dataProp.ValueKind == JsonValueKind.String)
                                        {
                                            var b64 = dataProp.GetString();
                                            var bytes = Convert.FromBase64String(b64);
                                            await File.WriteAllBytesAsync(outPath, bytes);
                                            return (outPath, caption);
                                        }
                                    }
                                    if (part.TryGetProperty("inline_data", out var inlineDataSnake) && inlineDataSnake.ValueKind == JsonValueKind.Object)
                                    {
                                        if (inlineDataSnake.TryGetProperty("data", out var dataProp2) && dataProp2.ValueKind == JsonValueKind.String)
                                        {
                                            var b64 = dataProp2.GetString();
                                            var bytes = Convert.FromBase64String(b64);
                                            await File.WriteAllBytesAsync(outPath, bytes);
                                            return (outPath, caption);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse Gemini image response: {ex.Message}");
            }

            throw new Exception("Gemini response did not include inline image data.");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
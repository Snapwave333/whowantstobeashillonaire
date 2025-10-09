using System;
using System.Collections.Generic;
using System.Linq;

namespace WhoWantsToBeAShillionaire.Services
{
    public static class ModelDefaults
    {
        private static readonly Dictionary<string, string[]> ProviderModels = new(StringComparer.OrdinalIgnoreCase)
        {
            { "openai", new[] { "gpt-4o-mini", "gpt-4o", "gpt-4-turbo", "gpt-3.5-turbo" } },
            { "anthropic", new[] { "claude-3-5-sonnet-20240620", "claude-3-opus-20240229", "claude-3-haiku-20240307" } },
            { "google", new[] { "gemini-1.5-flash", "gemini-1.5-pro", "gemini-pro" } },
            { "huggingface", new[] { "gpt2", "tiiuae/falcon-7b-instruct", "meta-llama/Llama-3.1-8B-Instruct" } },
            { "cohere", new[] { "command-r-plus", "command-r", "command" } },
            { "ai21", new[] { "j2-ultra", "j2-mid" } },
            { "mistral", new[] { "mistral-large-latest", "mistral-medium-latest", "mistral-small-latest" } },
            { "deepseek", new[] { "deepseek-chat", "deepseek-coder" } },
            { "custom", Array.Empty<string>() }
        };

        public static IReadOnlyList<string> GetModels(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider)) provider = "openai";
            return ProviderModels.TryGetValue(provider, out var list) ? list : Array.Empty<string>();
        }

        public static string GetDefault(string provider)
        {
            var p = provider?.ToLower() ?? "openai";
            return p switch
            {
                "openai" => "gpt-3.5-turbo",
                "anthropic" => "claude-3-haiku-20240307",
                "google" => "gemini-pro",
                "huggingface" => "gpt2",
                "cohere" => "command",
                "ai21" => "j2-mid",
                "mistral" => "mistral-small-latest",
                "deepseek" => "deepseek-chat",
                _ => "gpt-3.5-turbo"
            };
        }

        public static bool IsKnownModel(string provider, string model)
        {
            if (string.IsNullOrWhiteSpace(model)) return false;
            var list = GetModels(provider);
            return list.Any(m => string.Equals(m, model, StringComparison.OrdinalIgnoreCase));
        }
    }
}
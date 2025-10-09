using System.Linq;
using WhoWantsToBeAShillionaire.Services;

namespace WhoWantsToBeAShillionaire.Tests;

public class ModelDefaultsTests
{
    [Fact]
    public void GetDefault_ReturnsExpected_ForKnownProviders()
    {
        Assert.Equal("gpt-3.5-turbo", ModelDefaults.GetDefault("openai"));
        Assert.Equal("claude-3-haiku-20240307", ModelDefaults.GetDefault("anthropic"));
        Assert.Equal("gemini-pro", ModelDefaults.GetDefault("google"));
        Assert.Equal("gpt2", ModelDefaults.GetDefault("huggingface"));
        Assert.Equal("command", ModelDefaults.GetDefault("cohere"));
        Assert.Equal("j2-mid", ModelDefaults.GetDefault("ai21"));
        Assert.Equal("mistral-small-latest", ModelDefaults.GetDefault("mistral"));
        Assert.Equal("deepseek-chat", ModelDefaults.GetDefault("deepseek"));
    }

    [Fact]
    public void GetDefault_FallsBack_ToOpenAI_WhenUnknownOrNull()
    {
        Assert.Equal("gpt-3.5-turbo", ModelDefaults.GetDefault(null));
        Assert.Equal("gpt-3.5-turbo", ModelDefaults.GetDefault(""));
        Assert.Equal("gpt-3.5-turbo", ModelDefaults.GetDefault("unknown-provider"));
    }

    [Fact]
    public void GetModels_ReturnsList_ForKnownProviders()
    {
        var openaiModels = ModelDefaults.GetModels("openai");
        Assert.Contains("gpt-3.5-turbo", openaiModels);
        Assert.Contains("gpt-4o", openaiModels);

        var anthropicModels = ModelDefaults.GetModels("anthropic");
        Assert.Contains("claude-3-haiku-20240307", anthropicModels);
    }

    [Fact]
    public void GetModels_ReturnsEmpty_ForUnknownOrCustom()
    {
        var unknown = ModelDefaults.GetModels("unknown");
        Assert.Empty(unknown);

        var custom = ModelDefaults.GetModels("custom");
        Assert.Empty(custom);
    }

    [Fact]
    public void IsKnownModel_Works_ForCuratedLists()
    {
        Assert.True(ModelDefaults.IsKnownModel("openai", "gpt-3.5-turbo"));
        Assert.False(ModelDefaults.IsKnownModel("openai", "nonexistent-model"));

        Assert.True(ModelDefaults.IsKnownModel("anthropic", "claude-3-haiku-20240307"));
        Assert.False(ModelDefaults.IsKnownModel("anthropic", "claude-2"));
    }
}
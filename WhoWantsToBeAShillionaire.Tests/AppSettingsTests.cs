using WhoWantsToBeAShillionaire.Models;

namespace WhoWantsToBeAShillionaire.Tests;

public class AppSettingsTests
{
    [Fact]
    public void IsValid_DefaultSettings_ReturnsTrue()
    {
        var s = new AppSettings();
        Assert.True(s.IsValid());
    }

    [Fact]
    public void IsValid_CustomProviderWithoutUrl_ReturnsFalse()
    {
        var s = new AppSettings();
        s.AISettings.Provider = "custom";
        s.AISettings.CustomApiUrl = "";
        Assert.False(s.IsValid());
    }

    [Fact]
    public void IsValid_InvalidTimeout_ReturnsFalse()
    {
        var s = new AppSettings();
        s.TimeoutSeconds = 0;
        Assert.False(s.IsValid());
    }
}
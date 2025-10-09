using System.IO;
using System.Text.Json;
using WhoWantsToBeAShillionaire.Models;
using WhoWantsToBeAShillionaire.Services;

namespace WhoWantsToBeAShillionaire.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void ExportSettings_RedactsApiKey_WhenIncludeApiKeyFalse()
    {
        var svc = new SettingsService();
        var s = new AppSettings();
        s.AISettings.ApiKey = "secret-key";

        var tmp = Path.GetTempFileName();
        try
        {
            svc.ExportSettings(tmp, includeApiKey: false);
            var json = File.ReadAllText(tmp);
            var exported = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Assert.NotNull(exported);
            Assert.Equal("[REDACTED]", exported!.AISettings.ApiKey);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }
}
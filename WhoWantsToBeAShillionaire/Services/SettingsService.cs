using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WhoWantsToBeAShillionaire.Models;

namespace WhoWantsToBeAShillionaire.Services
{
    public class SettingsService
    {
        private const string SettingsFileName = "settings.json";
        private const string EncryptedSettingsFileName = "settings.enc";
        private readonly string _settingsPath;
        private readonly string _encryptedSettingsPath;
        private readonly byte[] _entropy;

        public SettingsService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WhoWantsToBeAShillionaire");
            Directory.CreateDirectory(appDataPath);
            
            _settingsPath = Path.Combine(appDataPath, SettingsFileName);
            _encryptedSettingsPath = Path.Combine(appDataPath, EncryptedSettingsFileName);
            
            // Create entropy for encryption (unique per machine)
            _entropy = Encoding.UTF8.GetBytes(Environment.MachineName + Environment.UserName);
        }

        public AppSettings LoadSettings()
        {
            try
            {
                // Try to load encrypted settings first
                if (File.Exists(_encryptedSettingsPath))
                {
                    return LoadEncryptedSettings();
                }
                
                // Fallback to plain text settings
                if (File.Exists(_settingsPath))
                {
                    return LoadPlainTextSettings();
                }
                
                // Return default settings if no file exists
                return new AppSettings();
            }
            catch (Exception ex)
            {
                // Log error and return default settings
                Console.WriteLine($"Error loading settings: {ex.Message}");
                return new AppSettings();
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            try
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                if (!settings.IsValid())
                    throw new ArgumentException("Settings validation failed");

                // Save encrypted settings
                SaveEncryptedSettings(settings);
                
                // Remove plain text settings file if it exists
                if (File.Exists(_settingsPath))
                {
                    File.Delete(_settingsPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}", ex);
            }
        }

        private AppSettings LoadEncryptedSettings()
        {
            try
            {
                var encryptedData = File.ReadAllBytes(_encryptedSettingsPath);
                var decryptedData = ProtectedData.Unprotect(encryptedData, _entropy, DataProtectionScope.CurrentUser);
                var json = Encoding.UTF8.GetString(decryptedData);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                return JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load encrypted settings: {ex.Message}", ex);
            }
        }

        private AppSettings LoadPlainTextSettings()
        {
            try
            {
                var json = File.ReadAllText(_settingsPath);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                return JsonSerializer.Deserialize<AppSettings>(json, options) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load plain text settings: {ex.Message}", ex);
            }
        }

        private void SaveEncryptedSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(settings, options);
                var data = Encoding.UTF8.GetBytes(json);
                var encryptedData = ProtectedData.Protect(data, _entropy, DataProtectionScope.CurrentUser);
                
                File.WriteAllBytes(_encryptedSettingsPath, encryptedData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save encrypted settings: {ex.Message}", ex);
            }
        }

        public void BackupSettings()
        {
            try
            {
                if (File.Exists(_encryptedSettingsPath))
                {
                    var backupPath = _encryptedSettingsPath + $".backup.{DateTime.Now:yyyyMMdd_HHmmss}";
                    File.Copy(_encryptedSettingsPath, backupPath);
                    
                    // Clean up old backups (keep only the last 5)
                    CleanupOldBackups();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to backup settings: {ex.Message}");
            }
        }

        private void CleanupOldBackups()
        {
            try
            {
                var directory = Path.GetDirectoryName(_encryptedSettingsPath);
                var backupFiles = Directory.GetFiles(directory, "settings.enc.backup.*");
                
                if (backupFiles.Length > 5)
                {
                    Array.Sort(backupFiles);
                    for (int i = 0; i < backupFiles.Length - 5; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to cleanup old backups: {ex.Message}");
            }
        }

        public void ResetSettings()
        {
            try
            {
                if (File.Exists(_encryptedSettingsPath))
                {
                    BackupSettings();
                    File.Delete(_encryptedSettingsPath);
                }
                
                if (File.Exists(_settingsPath))
                {
                    File.Delete(_settingsPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reset settings: {ex.Message}", ex);
            }
        }

        public bool SettingsExist()
        {
            return File.Exists(_encryptedSettingsPath) || File.Exists(_settingsPath);
        }

        public string GetSettingsPath()
        {
            return File.Exists(_encryptedSettingsPath) ? _encryptedSettingsPath : _settingsPath;
        }

        public void ExportSettings(string filePath, bool includeApiKey = false)
        {
            try
            {
                var settings = LoadSettings();
                
                if (!includeApiKey)
                {
                    // Create a copy without the API key for export
                    var exportSettings = settings.Clone();
                    exportSettings.AISettings.ApiKey = "[REDACTED]";
                    settings = exportSettings;
                }
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export settings: {ex.Message}", ex);
            }
        }

        public void ImportSettings(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("Settings file not found");
                
                var json = File.ReadAllText(filePath);
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var settings = JsonSerializer.Deserialize<AppSettings>(json, options);
                
                if (settings == null)
                    throw new Exception("Failed to deserialize settings");
                
                // Validate imported settings
                if (!settings.IsValid())
                    throw new Exception("Imported settings are invalid");
                
                // Backup current settings before importing
                BackupSettings();
                
                // Save imported settings
                SaveSettings(settings);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to import settings: {ex.Message}", ex);
            }
        }
    }
}
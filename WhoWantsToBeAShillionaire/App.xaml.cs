using System.Windows;
using WhoWantsToBeAShillionaire.Services;

namespace WhoWantsToBeAShillionaire
{
    public partial class App : Application
    {
        public enum AppTheme
        {
            Default,
            LiquidGlass
        }

        public void ApplyTheme(AppTheme theme)
        {
            // Remove existing merged theme dictionaries first
            var toRemove = new System.Collections.Generic.List<ResourceDictionary>();
            foreach (var rd in Resources.MergedDictionaries)
            {
                if (rd.Source != null && rd.Source.OriginalString.Contains("Themes/"))
                {
                    toRemove.Add(rd);
                }
            }
            foreach (var rd in toRemove)
            {
                Resources.MergedDictionaries.Remove(rd);
            }

            if (theme == AppTheme.LiquidGlass)
            {
                var glass = new ResourceDictionary
                {
                    Source = new System.Uri("/WhoWantsToBeAShillionaire;component/Themes/LiquidGlass.xaml", System.UriKind.Relative)
                };
                Resources.MergedDictionaries.Add(glass);
            }
            // Default theme relies on App.xaml base brushes/styles
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                // Initialize database
                var db = new Services.DatabaseService();
                db.InitializeAsync().GetAwaiter().GetResult();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to initialize app: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Load settings & theme
            Models.AppSettings settings = null;
            SettingsService svc = null;
            try
            {
                svc = new Services.SettingsService();
                settings = svc.LoadSettings() ?? new Models.AppSettings();
                var themeName = settings?.GameSettings?.Theme ?? "Default";
                ApplyTheme(string.Equals(themeName, "LiquidGlass", System.StringComparison.OrdinalIgnoreCase) ? AppTheme.LiquidGlass : AppTheme.Default);
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load/apply settings: {ex.Message}", "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                settings = new Models.AppSettings();
            }

            // Try to show SettingsWindow (non-blocking failure)
            try
            {
                var settingsWindow = new WhoWantsToBeAShillionaire.Views.SettingsWindow(settings);
                var dialogResult = settingsWindow.ShowDialog();

                if (dialogResult == true && svc != null)
                {
                    svc.SaveSettings(settingsWindow.Settings);
                    settings = settingsWindow.Settings;
                }
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Settings window failed to open: {ex.Message}", "Non-fatal", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Always show the main window
            var mainWindow = new WhoWantsToBeAShillionaire.MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
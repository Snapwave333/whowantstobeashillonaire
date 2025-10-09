using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using System.Windows.Forms;

namespace ShillionaireInstaller
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                using (var splash = new SplashForm())
                {
                    splash.Show();
                    Application.DoEvents();
                }

                if (args.Length > 0 && string.Equals(args[0], "--uninstall", StringComparison.OrdinalIgnoreCase))
                {
                    PerformUninstall();
                    return;
                }

                using var form = new InstallerForm();
                if (form.ShowDialog() != DialogResult.OK)
                    return;

                string installDir = form.InstallDirectory;
                Directory.CreateDirectory(installDir);

                string temp = Path.Combine(Path.GetTempPath(), "shillionaire_installer");
                if (Directory.Exists(temp)) Directory.Delete(temp, true);
                Directory.CreateDirectory(temp);

                var asm = Assembly.GetExecutingAssembly();
                var resName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("app.zip", StringComparison.OrdinalIgnoreCase));
                if (resName == null)
                    throw new InvalidOperationException("Embedded payload 'app.zip' not found.");

                string zipPath = Path.Combine(temp, "app.zip");
                using (var stream = asm.GetManifestResourceStream(resName))
                using (var fs = File.Create(zipPath))
                {
                    stream!.CopyTo(fs);
                }

                using (var progress = new ProgressForm())
                {
                    progress.Show();
                    progress.SetStatus("Extracting application files...");
                    // We don't have per-file progress from ZipFile, so approximate:
                    ZipFile.ExtractToDirectory(zipPath, installDir, true);
                    progress.SetProgress(80);

                    progress.SetStatus("Creating shortcuts...");
                    progress.SetProgress(90);
                    // Create shortcuts below
                    progress.SetStatus("Finalizing...");
                    progress.SetProgress(100);
                    progress.Close();
                }

                // Create shortcuts (optional)
                string exePath = Path.Combine(installDir, "WhoWantsToBeAShillionaire.exe");
                if (form.CreateDesktopShortcut)
                    CreateShortcutOnDesktop("Who Wants to Be a Shillionaire", exePath);
                if (form.CreateStartMenuShortcut)
                    CreateShortcutInStartMenu("Who Wants to Be a Shillionaire", exePath);

                // Register uninstall entry
                RegisterUninstall("Who Wants to Be a Shillionaire", installDir, exePath);

                // Launch app after install (optional)
                if (form.LaunchAfterInstall)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        // This function is smoother than a fresh Windows install
        private static void CreateShortcutOnDesktop(string shortcutName, string targetPath)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string shortcutPath = Path.Combine(desktop, shortcutName + ".lnk");

            Type? wshShellType = Type.GetTypeFromProgID("WScript.Shell");
            if (wshShellType == null)
                return;

            dynamic shell = Activator.CreateInstance(wshShellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.IconLocation = targetPath + ",0";
            shortcut.Save();
        }

        private static void CreateShortcutInStartMenu(string shortcutName, string targetPath)
        {
            string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string appFolder = Path.Combine(programs, "Shillionaire Studios");
            Directory.CreateDirectory(appFolder);
            string shortcutPath = Path.Combine(appFolder, shortcutName + ".lnk");

            Type? wshShellType = Type.GetTypeFromProgID("WScript.Shell");
            if (wshShellType == null)
                return;

            dynamic shell = Activator.CreateInstance(wshShellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
            shortcut.IconLocation = targetPath + ",0";
            shortcut.Save();
        }

        private static void RegisterUninstall(string displayName, string installDir, string exePath)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Shillionaire");
                if (key == null) return;
                key.SetValue("DisplayName", displayName);
                key.SetValue("InstallLocation", installDir);
                key.SetValue("DisplayIcon", exePath);
                key.SetValue("Publisher", "Shillionaire Studios");
                key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                // Uninstall via running the application with --uninstall, launched silently
                var cmd = "mshta vbscript:CreateObject(\"WScript.Shell\").Run(\"\"\"" + exePath.Replace("\\", "\\\\") + "\"\" --uninstall\",0,false)(close)";
                key.SetValue("UninstallString", cmd);
            }
            catch { /* best-effort */ }
        }
        private static void PerformUninstall()
        {
            try
            {
                string productName = "Who Wants to Be a Shillionaire";
                string companyName = "Shillionaire Studios";
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string installDir = Path.Combine(localAppData, companyName, productName);
                if (Directory.Exists(installDir))
                {
                    Directory.Delete(installDir, true);
                }

                // Remove desktop shortcut
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktop, productName + ".lnk");
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                // Remove Start Menu shortcut
                string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string appFolder = Path.Combine(programs, "Shillionaire Studios");
                string startShortcut = Path.Combine(appFolder, productName + ".lnk");
                if (File.Exists(startShortcut))
                {
                    File.Delete(startShortcut);
                }
                if (Directory.Exists(appFolder))
                {
                    // Try remove folder if empty
                    try { if (!Directory.EnumerateFileSystemEntries(appFolder).Any()) Directory.Delete(appFolder); } catch { }
                }
            }
            catch { }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StarshipStarfield
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                SetProcessDPIAware();
            }
            catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConfigManager config = new ConfigManager();
            config.Load();

            if (args.Length > 0)
            {
                string firstArg = args[0].ToLower().Trim();
                string secondArg = args.Length > 1 ? args[1].Trim() : null;

                // Preview mode: /p <HWND> or /p:<HWND>
                if (firstArg.StartsWith("/p") || firstArg.StartsWith("-p"))
                {
                    IntPtr previewHwnd = IntPtr.Zero;
                    if (firstArg.Length > 2 && (firstArg[2] == ':' || firstArg[2] == ' '))
                    {
                        previewHwnd = new IntPtr(long.Parse(firstArg.Substring(3).Trim()));
                    }
                    else if (!string.IsNullOrEmpty(secondArg))
                    {
                        previewHwnd = new IntPtr(long.Parse(secondArg));
                    }

                    if (previewHwnd != IntPtr.Zero)
                    {
                        Application.Run(new ScreenSaverForm(previewHwnd, config));
                        return;
                    }
                }
                // Configuration mode: /c or /c:<HWND>
                else if (firstArg.StartsWith("/c") || firstArg.StartsWith("-c"))
                {
                    Application.Run(new SettingsForm(config));
                    return;
                }
                // Fullscreen screensaver: /s
                else if (firstArg.StartsWith("/s") || firstArg.StartsWith("-s"))
                {
                    RunScreensaver(config);
                    return;
                }
            }

            // Default execution (no arguments or double-clicked): run full screen
            RunScreensaver(config);
        }

        private static void RunScreensaver(ConfigManager config)
        {
            if (config.MultiMonitor && Screen.AllScreens.Length > 1)
            {
                List<ScreenSaverForm> forms = new List<ScreenSaverForm>();
                foreach (Screen screen in Screen.AllScreens)
                {
                    ScreenSaverForm f = new ScreenSaverForm(screen.Bounds, config);
                    forms.Add(f);
                    f.Show();
                }
                Application.Run();
            }
            else
            {
                Application.Run(new ScreenSaverForm(Screen.PrimaryScreen.Bounds, config));
            }
        }
    }
}

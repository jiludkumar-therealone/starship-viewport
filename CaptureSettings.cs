using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace StarshipStarfield
{
    class CaptureSettings
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                ConfigManager cfg = new ConfigManager();
                cfg.Load();
                using (SettingsForm form = new SettingsForm(cfg))
                {
                    form.Show();
                    Application.DoEvents();
                    using (Bitmap bmp = new Bitmap(form.Width, form.Height))
                    {
                        form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
                        bmp.Save("screensaver_controls_ui.png", ImageFormat.Png);
                    }
                    Console.WriteLine("SUCCESS_SETTINGS_IMAGE");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
            }
        }
    }
}

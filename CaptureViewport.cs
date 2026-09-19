using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace StarshipStarfield
{
    class CaptureViewport
    {
        static void Main()
        {
            int width = 1920;
            int height = 1080;

            ConfigManager cfg = new ConfigManager();
            cfg.Load();
            cfg.WarpFactor = 5.0f;
            cfg.StreakLength = 1.3f;
            cfg.SpectralVariance = true;

            StarfieldEngine engine = new StarfieldEngine(
                cfg.StarCount,
                cfg.WarpFactor * 4.5f,
                cfg.StreakLength,
                cfg.SpectralVariance
            );
            engine.Resize(width, height);

            TelemetryOverlay telemetry = new TelemetryOverlay(cfg);

            // Simulate 60 frames (~1 sec of flight)
            float dt = 1.0f / 60.0f;
            for (int i = 0; i < 60; i++)
            {
                engine.Update(dt);
                telemetry.Update(dt);
            }

            using (Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Black);
                engine.Render(g);
                telemetry.Render(g, width, height);
                bmp.Save("test_viewport_preview.png", ImageFormat.Png);
            }

            Console.WriteLine("Rendered viewport telemetry to test_viewport_preview.png");
        }
    }
}

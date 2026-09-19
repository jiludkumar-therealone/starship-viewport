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
            // Hero screenshot settings: Deep Warp Cruise for dramatic warp strikes & nebulae
            cfg.WarpFactor = 5.0f;
            cfg.StreakLength = 1.4f;
            cfg.EnableNebula = true;
            cfg.EnableCosmicDust = true;
            cfg.SpectralVariance = true;

            StarfieldEngine engine = new StarfieldEngine(
                cfg.StarCount,
                cfg.WarpFactor * 8.5f,
                cfg.StreakLength,
                cfg.SpectralVariance,
                cfg.EnableNebula,
                cfg.EnableCosmicDust
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
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                int benchFrames = 20;
                for (int f = 0; f < benchFrames; f++)
                {
                    if (!engine.HasBackdrop) g.Clear(Color.Black);
                    engine.Render(g);
                }
                sw.Stop();
                Console.WriteLine("Average Render Frame Time: " + (sw.ElapsedMilliseconds / (double)benchFrames) + " ms (" + (1000.0 / (sw.ElapsedMilliseconds / (double)benchFrames)) + " FPS)");

                telemetry.Render(g, width, height);
                bmp.Save("test_viewport_preview.png", ImageFormat.Png);
            }

            Console.WriteLine("Rendered viewport telemetry to test_viewport_preview.png");
        }
    }
}

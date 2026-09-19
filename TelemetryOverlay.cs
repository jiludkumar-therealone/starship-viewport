using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace StarshipStarfield
{
    public class TelemetryOverlay
    {
        private ConfigManager config;
        private Random rand = new Random();
        private double currentKmPerSec;
        private double currentLyPerHour;
        private double jitterKmOffset = 0.0;
        private double jitterLyOffset = 0.0;
        private float jitterTimer = 0f;

        // Fonts
        private Font headerFont;
        private Font dataFont;
        private Font labelFont;
        private Font subFont;

        public TelemetryOverlay(ConfigManager cfg)
        {
            this.config = cfg;
            this.currentKmPerSec = cfg.GetEffectiveKmPerSec();
            this.currentLyPerHour = cfg.GetEffectiveLyPerHour();

            headerFont = new Font("Consolas", 8.5f, FontStyle.Bold);
            dataFont = new Font("Consolas", 11.5f, FontStyle.Bold);
            labelFont = new Font("Consolas", 8.5f, FontStyle.Regular);
            subFont = new Font("Consolas", 7.5f, FontStyle.Regular);
        }

        public void Update(float deltaTime)
        {
            currentKmPerSec = config.GetEffectiveKmPerSec();
            currentLyPerHour = config.GetEffectiveLyPerHour();

            if (config.EnableSensorJitter)
            {
                jitterTimer += deltaTime;
                if (jitterTimer > 0.12f) // update micro-fluctuation ~8 times/sec
                {
                    jitterTimer = 0f;
                    // Jitter by ~0.02%
                    double deltaPct = (rand.NextDouble() * 0.0004) - 0.0002;
                    jitterKmOffset = currentKmPerSec * deltaPct;
                    jitterLyOffset = currentLyPerHour * deltaPct;
                }
            }
            else
            {
                jitterKmOffset = 0.0;
                jitterLyOffset = 0.0;
            }
        }

        public void Render(Graphics g, int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Resolve theme colors
            Color mainColor;
            Color dimColor;
            Color accentColor;

            switch (config.ColorTheme)
            {
                case "Amber":
                    mainColor = Color.FromArgb(255, 175, 40);
                    dimColor = Color.FromArgb(170, 110, 25);
                    accentColor = Color.FromArgb(255, 210, 100);
                    break;
                case "White":
                    mainColor = Color.FromArgb(240, 245, 250);
                    dimColor = Color.FromArgb(140, 155, 170);
                    accentColor = Color.FromArgb(255, 255, 255);
                    break;
                case "Green":
                    mainColor = Color.FromArgb(50, 255, 130);
                    dimColor = Color.FromArgb(25, 150, 75);
                    accentColor = Color.FromArgb(120, 255, 170);
                    break;
                case "Crimson":
                    mainColor = Color.FromArgb(255, 75, 95);
                    dimColor = Color.FromArgb(160, 40, 55);
                    accentColor = Color.FromArgb(255, 130, 145);
                    break;
                case "Cyan":
                default:
                    mainColor = Color.FromArgb(0, 230, 255);
                    dimColor = Color.FromArgb(0, 135, 160);
                    accentColor = Color.FromArgb(128, 245, 255);
                    break;
            }

            // Calculate display strings
            double dispKm = Math.Max(0.0, currentKmPerSec + jitterKmOffset);
            double dispLy = Math.Max(0.0, currentLyPerHour + jitterLyOffset);

            string kmText;
            if (dispKm >= 1000000.0)
            {
                kmText = string.Format("{0:N0} km/s", dispKm);
            }
            else
            {
                kmText = string.Format("{0:N1} km/s", dispKm);
            }

            string lyText;
            if (dispLy < 0.001)
            {
                lyText = string.Format("{0:F6} ly/h", dispLy);
            }
            else if (dispLy < 1.0)
            {
                lyText = string.Format("{0:F4} ly/h", dispLy);
            }
            else
            {
                lyText = string.Format("{0:F3} ly/h", dispLy);
            }

            // HUD geometry in bottom right
            float panelWidth = 295f;
            float panelHeight = 86f;
            float margin = 32f;
            float panelX = screenWidth - panelWidth - margin;
            float panelY = screenHeight - panelHeight - margin;

            // 1. Dark translucent backdrop
            using (Brush bgBrush = new SolidBrush(Color.FromArgb(170, 6, 10, 16)))
            {
                g.FillRectangle(bgBrush, panelX, panelY, panelWidth, panelHeight);
            }

            // 2. Tactical border / frame
            if (config.ShowTelemetryFrame)
            {
                using (Pen borderPen = new Pen(Color.FromArgb(50, mainColor.R, mainColor.G, mainColor.B), 1f))
                {
                    g.DrawRectangle(borderPen, panelX, panelY, panelWidth, panelHeight);
                }

                // Corner bracket accents
                float bracketLen = 9f;
                using (Pen cornerPen = new Pen(mainColor, 1.8f))
                {
                    // Top-Left
                    g.DrawLine(cornerPen, panelX, panelY + bracketLen, panelX, panelY);
                    g.DrawLine(cornerPen, panelX, panelY, panelX + bracketLen, panelY);
                    // Top-Right
                    g.DrawLine(cornerPen, panelX + panelWidth - bracketLen, panelY, panelX + panelWidth, panelY);
                    g.DrawLine(cornerPen, panelX + panelWidth, panelY, panelX + panelWidth, panelY + bracketLen);
                    // Bottom-Left
                    g.DrawLine(cornerPen, panelX, panelY + panelHeight - bracketLen, panelX, panelY + panelHeight);
                    g.DrawLine(cornerPen, panelX, panelY + panelHeight, panelX + bracketLen, panelY + panelHeight);
                    // Bottom-Right
                    g.DrawLine(cornerPen, panelX + panelWidth - bracketLen, panelY + panelHeight, panelX + panelWidth, panelY + panelHeight);
                    g.DrawLine(cornerPen, panelX + panelWidth, panelY + panelHeight - bracketLen, panelX + panelWidth, panelY + panelHeight);
                }
            }

            // 3. Header bar: status indicator + title
            float contentX = panelX + 12f;
            float currentY = panelY + 8f;

            // Sensor status pulse
            using (Brush dotBrush = new SolidBrush(accentColor))
            {
                g.FillEllipse(dotBrush, contentX, currentY + 3f, 5f, 5f);
            }

            using (Brush headerBrush = new SolidBrush(dimColor))
            {
                g.DrawString("NAV TELEMETRY // FWD SENSORS", headerFont, headerBrush, contentX + 10f, currentY);
            }

            currentY += 18f;

            // 4. Primary Velocity Readout (km/s)
            using (Brush labelBrush = new SolidBrush(dimColor))
            {
                g.DrawString("VELOCITY", labelFont, labelBrush, contentX, currentY + 2f);
            }
            using (Brush valBrush = new SolidBrush(mainColor))
            {
                g.DrawString(kmText, dataFont, valBrush, contentX + 70f, currentY);
            }

            currentY += 21f;

            // 5. Interstellar Warp Rate Readout (ly/h)
            using (Brush labelBrush = new SolidBrush(dimColor))
            {
                g.DrawString("RATE", labelFont, labelBrush, contentX, currentY + 2f);
            }
            using (Brush valBrush = new SolidBrush(mainColor))
            {
                g.DrawString(lyText, dataFont, valBrush, contentX + 70f, currentY);
            }

            currentY += 20f;

            // 6. Sub-status indicator line
            string warpMode = config.WarpFactor > 1.0f ? "WARP CRUISE ENGAGED" : "SUB-LIGHT IMPULSE";
            using (Brush subBrush = new SolidBrush(Color.FromArgb(120, dimColor.R, dimColor.G, dimColor.B)))
            {
                g.DrawString(string.Format("SYS: ONLINE | {0}", warpMode), subFont, subBrush, contentX, currentY);
            }
        }
    }
}

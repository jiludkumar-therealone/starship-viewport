using System;
using Microsoft.Win32;

namespace StarshipStarfield
{
    public class ConfigManager
    {
        private const string REG_KEY = @"Software\StarshipStarfieldScreensaver";

        // Configuration fields with defaults
        public int StarCount { get; set; }
        public float WarpFactor { get; set; }        // Scales travel speed & telemetry
        public int PresetIndex { get; set; }         // Selected preset index (0-7)
        public float StreakLength { get; set; }       // Length multiplier for star streaks
        public string ColorTheme { get; set; }        // Cyan, Amber, White, Green, Crimson
        public bool MultiMonitor { get; set; }        // Cover all displays
        public bool ShowTelemetryFrame { get; set; }  // Border / tactical frame around HUD
        public bool EnableSensorJitter { get; set; }  // Subtle micro-fluctuations in readout
        public bool SpectralVariance { get; set; }    // Realistic star spectral colors vs monochrome
        public bool EnableNebula { get; set; }        // Procedural 3D cosmic gas clouds
        public bool EnableCosmicDust { get; set; }    // Interstellar dust motes / micro-debris
        public double CustomKmPerSec { get; set; }    // Speed in km/s
        public double CustomLyPerHour { get; set; }   // Speed in ly/h
        public bool UseCustomSpeed { get; set; }      // Manual override toggle

        public ConfigManager()
        {
            // Default configuration: Deep Warp Cruise
            StarCount = 1800;
            WarpFactor = 5.0f; // Mid-high warp
            PresetIndex = 5;   // Deep Warp Cruise
            StreakLength = 1.2f;
            ColorTheme = "Cyan";
            MultiMonitor = true;
            ShowTelemetryFrame = true;
            EnableSensorJitter = true;
            SpectralVariance = true;
            EnableNebula = true;
            EnableCosmicDust = true;
            UseCustomSpeed = false;
            CustomKmPerSec = 2997924580.0; // ~10,000 c
            CustomLyPerHour = 1.1408;
        }

        public static int InferPresetIndex(float warpFactor)
        {
            if (warpFactor <= 0.6f) return 0;       // Sub-light Impulse
            if (warpFactor <= 1.5f) return 1;       // Light Speed
            if (warpFactor <= 2.8f) return 2;       // Warp 3 Cruise
            if (warpFactor <= 4.0f) return 3;       // Warp 5 Standard
            if (warpFactor <= 4.8f) return 4;       // Warp 8 Maximum
            if (warpFactor <= 6.5f) return 5;       // Deep Warp Cruise
            if (warpFactor <= 9.0f) return 6;       // Transwarp / Slipstream
            return 7;                               // Custom
        }

        public void Load()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(REG_KEY))
                {
                    if (key == null) return;

                    object val = key.GetValue("StarCount");
                    if (val != null) StarCount = Math.Max(200, Math.Min(5000, Convert.ToInt32(val)));

                    val = key.GetValue("WarpFactor");
                    if (val != null) WarpFactor = (float)Convert.ToDouble(val);

                    val = key.GetValue("PresetIndex");
                    if (val != null)
                    {
                        PresetIndex = Convert.ToInt32(val);
                    }
                    else
                    {
                        PresetIndex = InferPresetIndex(WarpFactor);
                    }

                    val = key.GetValue("StreakLength");
                    if (val != null) StreakLength = (float)Convert.ToDouble(val);

                    val = key.GetValue("ColorTheme");
                    if (val != null) ColorTheme = val.ToString();

                    val = key.GetValue("MultiMonitor");
                    if (val != null) MultiMonitor = Convert.ToBoolean(val);

                    val = key.GetValue("ShowTelemetryFrame");
                    if (val != null) ShowTelemetryFrame = Convert.ToBoolean(val);

                    val = key.GetValue("EnableSensorJitter");
                    if (val != null) EnableSensorJitter = Convert.ToBoolean(val);

                    val = key.GetValue("SpectralVariance");
                    if (val != null) SpectralVariance = Convert.ToBoolean(val);

                    val = key.GetValue("EnableNebula");
                    if (val != null) EnableNebula = Convert.ToBoolean(val);

                    val = key.GetValue("EnableCosmicDust");
                    if (val != null) EnableCosmicDust = Convert.ToBoolean(val);

                    val = key.GetValue("UseCustomSpeed");
                    if (val != null) UseCustomSpeed = Convert.ToBoolean(val);

                    val = key.GetValue("CustomKmPerSec");
                    if (val != null) CustomKmPerSec = Convert.ToDouble(val);

                    val = key.GetValue("CustomLyPerHour");
                    if (val != null) CustomLyPerHour = Convert.ToDouble(val);
                }
            }
            catch
            {
                // Fallback to defaults if registry read fails
            }
        }

        public void Save()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(REG_KEY))
                {
                    if (key == null) return;

                    key.SetValue("StarCount", StarCount);
                    key.SetValue("WarpFactor", WarpFactor);
                    key.SetValue("PresetIndex", PresetIndex);
                    key.SetValue("StreakLength", StreakLength);
                    key.SetValue("ColorTheme", ColorTheme ?? "Cyan");
                    key.SetValue("MultiMonitor", MultiMonitor);
                    key.SetValue("ShowTelemetryFrame", ShowTelemetryFrame);
                    key.SetValue("EnableSensorJitter", EnableSensorJitter);
                    key.SetValue("SpectralVariance", SpectralVariance);
                    key.SetValue("EnableNebula", EnableNebula);
                    key.SetValue("EnableCosmicDust", EnableCosmicDust);
                    key.SetValue("UseCustomSpeed", UseCustomSpeed);
                    key.SetValue("CustomKmPerSec", CustomKmPerSec);
                    key.SetValue("CustomLyPerHour", CustomLyPerHour);
                }
            }
            catch
            {
                // Ignore save errors
            }
        }

        // Helper calculations for velocity display
        public double GetEffectiveKmPerSec()
        {
            if (UseCustomSpeed && CustomKmPerSec > 0)
                return CustomKmPerSec;

            // Speed of light: 299,792 km/s
            // Warp 1.0 = 1c
            // Warp factor 1 -> 0.5c (149,896 km/s)
            // Warp factor 5 -> 10,000c (2,997,924,580 km/s)
            // Warp factor 10 -> 87,660c (26,279,807,000 km/s ~ 10 ly/h)
            double c = 299792.458;
            if (WarpFactor <= 1.0f)
            {
                return c * WarpFactor;
            }
            // Exponential warp scaling: c * (WarpFactor^3.3)
            double multiplier = Math.Pow(WarpFactor, 3.3) * 50.0;
            return c * multiplier;
        }

        public double GetEffectiveLyPerHour()
        {
            if (UseCustomSpeed && CustomLyPerHour > 0)
                return CustomLyPerHour;

            double kmPerSec = GetEffectiveKmPerSec();
            // 1 ly = 9.460730472e12 km
            // 1 hour = 3600 seconds
            // ly/h = (kmPerSec * 3600) / 9.460730472e12
            return (kmPerSec * 3600.0) / 9.460730472e12;
        }
    }
}

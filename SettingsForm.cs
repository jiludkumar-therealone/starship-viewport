using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace StarshipStarfield
{
    public class SettingsForm : Form
    {
        private ConfigManager config;
        private bool isUpdatingUI = false;

        // UI Controls
        private TrackBar tbWarp;
        private Label lblWarpVal;
        private Label lblCalculatedKm;
        private Label lblCalculatedLy;
        private ComboBox cmbPresets;

        private TrackBar tbStarCount;
        private Label lblStarCountVal;

        private TrackBar tbStreak;
        private Label lblStreakVal;

        private ComboBox cmbTheme;
        private CheckBox chkSpectral;
        private CheckBox chkTelemetryFrame;
        private CheckBox chkJitter;
        private CheckBox chkMultiMonitor;

        private Button btnTest;
        private Button btnSave;
        private Button btnCancel;
        private Button btnSetDefault;

        public SettingsForm(ConfigManager cfg)
        {
            this.config = cfg;
            InitializeComponent();
            LoadConfigToUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Starship Viewport Screensaver - Helm Controls";
            this.Size = new Size(540, 740);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 24, 30);
            this.ForeColor = Color.FromArgb(220, 230, 240);
            this.Font = new Font("Segoe UI", 9f);

            int left = 24;
            int rightWidth = 475;
            int top = 16;

            // Title Banner
            Label lblTitle = new Label();
            lblTitle.Text = "STARSHIP FORWARD VIEWPORT // CONFIGURATION";
            lblTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 210, 255);
            lblTitle.Location = new Point(left, top);
            lblTitle.Size = new Size(rightWidth, 26);
            this.Controls.Add(lblTitle);

            top += 34;

            // Group: Flight Speed & Warp Factor
            GroupBox grpSpeed = CreateGroupBox("WARP VELOCITY // PROPULSION RATE", left, top, rightWidth, 175);
            this.Controls.Add(grpSpeed);

            Label lblPreset = new Label { Text = "Propulsion Preset:", Location = new Point(16, 26), Size = new Size(130, 22) };
            grpSpeed.Controls.Add(lblPreset);

            cmbPresets = new ComboBox
            {
                Location = new Point(150, 23),
                Size = new Size(295, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(32, 38, 48),
                ForeColor = Color.White
            };
            cmbPresets.Items.AddRange(new object[] {
                "Sub-light Impulse (0.3c - 89,938 km/s)",
                "Light Speed (1.0c - 299,792 km/s | 0.0001 ly/h)",
                "Warp 3 Cruise (39c - 11,691,900 km/s | 0.0044 ly/h)",
                "Warp 5 Standard (214c - 64,155,500 km/s | 0.0244 ly/h)",
                "Warp 8 Maximum (1,024c - 306,987,400 km/s | 0.1168 ly/h)",
                "Deep Warp Cruise (10,000c - 2,997,925,000 km/s | 1.1408 ly/h)",
                "Transwarp / Slipstream (50,000c - 14,989,620,000 km/s | 5.7040 ly/h)",
                "Custom Velocity Profile"
            });
            cmbPresets.SelectedIndexChanged += OnPresetChanged;
            grpSpeed.Controls.Add(cmbPresets);

            Label lblWarp = new Label { Text = "Warp Scale Slider:", Location = new Point(16, 58), Size = new Size(130, 22) };
            grpSpeed.Controls.Add(lblWarp);

            tbWarp = new TrackBar
            {
                Location = new Point(150, 54),
                Size = new Size(240, 42),
                Minimum = 2,
                Maximum = 100, // 0.2 to 10.0
                TickFrequency = 10,
                SmallChange = 1,
                LargeChange = 5
            };
            tbWarp.ValueChanged += OnWarpSliderChanged;
            grpSpeed.Controls.Add(tbWarp);

            lblWarpVal = new Label
            {
                Text = "5.0x",
                Location = new Point(400, 58),
                Size = new Size(60, 22),
                ForeColor = Color.FromArgb(0, 230, 255),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            grpSpeed.Controls.Add(lblWarpVal);

            // Telemetry Preview Box inside group
            Panel pnlSpeedPreview = new Panel
            {
                Location = new Point(16, 100),
                Size = new Size(440, 60),
                BackColor = Color.FromArgb(12, 16, 22)
            };
            grpSpeed.Controls.Add(pnlSpeedPreview);

            Label lblNavTelemetryTag = new Label
            {
                Text = "NAV TELEMETRY OUTPUT:",
                Font = new Font("Consolas", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 130, 160),
                Location = new Point(10, 6),
                Size = new Size(200, 16)
            };
            pnlSpeedPreview.Controls.Add(lblNavTelemetryTag);

            lblCalculatedKm = new Label
            {
                Text = "VELOCITY: 2,997,924,580 km/s",
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 230, 255),
                Location = new Point(10, 24),
                Size = new Size(240, 24)
            };
            pnlSpeedPreview.Controls.Add(lblCalculatedKm);

            lblCalculatedLy = new Label
            {
                Text = "RATE: 1.1408 ly/h",
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 230, 255),
                Location = new Point(255, 24),
                Size = new Size(180, 24)
            };
            pnlSpeedPreview.Controls.Add(lblCalculatedLy);

            top += 185;

            // Group: Starfield Visual Dynamics
            GroupBox grpStarfield = CreateGroupBox("STARFIELD PARTICLES // MOTION STREAKS", left, top, rightWidth, 125);
            this.Controls.Add(grpStarfield);

            // Star Count
            Label lblStars = new Label { Text = "Star Density:", Location = new Point(16, 26), Size = new Size(130, 22) };
            grpStarfield.Controls.Add(lblStars);

            tbStarCount = new TrackBar
            {
                Location = new Point(150, 23),
                Size = new Size(240, 42),
                Minimum = 400,
                Maximum = 4500,
                TickFrequency = 500,
                SmallChange = 100,
                LargeChange = 500
            };
            tbStarCount.ValueChanged += (s, e) => { lblStarCountVal.Text = tbStarCount.Value.ToString("N0") + " Stars"; };
            grpStarfield.Controls.Add(tbStarCount);

            lblStarCountVal = new Label
            {
                Text = "1,800 Stars",
                Location = new Point(400, 26),
                Size = new Size(70, 22),
                ForeColor = Color.White
            };
            grpStarfield.Controls.Add(lblStarCountVal);

            // Relativistic Streak
            Label lblStreak = new Label { Text = "Relativistic Streak:", Location = new Point(16, 72), Size = new Size(130, 22) };
            grpStarfield.Controls.Add(lblStreak);

            tbStreak = new TrackBar
            {
                Location = new Point(150, 68),
                Size = new Size(240, 42),
                Minimum = 0,
                Maximum = 30, // 0.0 to 3.0
                TickFrequency = 5,
                SmallChange = 1,
                LargeChange = 5
            };
            tbStreak.ValueChanged += (s, e) => { lblStreakVal.Text = (tbStreak.Value / 10.0f).ToString("F1") + "x"; };
            grpStarfield.Controls.Add(tbStreak);

            lblStreakVal = new Label
            {
                Text = "1.2x",
                Location = new Point(400, 72),
                Size = new Size(70, 22),
                ForeColor = Color.White
            };
            grpStarfield.Controls.Add(lblStreakVal);

            top += 135;

            // Group: HUD & Visual Styling
            GroupBox grpHud = CreateGroupBox("HUD THEME // SYSTEM OPTIONS", left, top, rightWidth, 150);
            this.Controls.Add(grpHud);

            Label lblTheme = new Label { Text = "Telemetry Color Theme:", Location = new Point(16, 26), Size = new Size(150, 22) };
            grpHud.Controls.Add(lblTheme);

            cmbTheme = new ComboBox
            {
                Location = new Point(175, 23),
                Size = new Size(200, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(32, 38, 48),
                ForeColor = Color.White
            };
            cmbTheme.Items.AddRange(new object[] { "Cyan", "Amber", "White", "Green", "Crimson" });
            grpHud.Controls.Add(cmbTheme);

            chkSpectral = new CheckBox
            {
                Text = "Multi-spectral stellar classes (Blue-white, Solar gold, Warm amber)",
                Location = new Point(16, 56),
                Size = new Size(440, 22),
                ForeColor = Color.FromArgb(210, 225, 240)
            };
            grpHud.Controls.Add(chkSpectral);

            chkTelemetryFrame = new CheckBox
            {
                Text = "Display tactical HUD frame and corner brackets",
                Location = new Point(16, 80),
                Size = new Size(320, 22),
                ForeColor = Color.FromArgb(210, 225, 240)
            };
            grpHud.Controls.Add(chkTelemetryFrame);

            chkJitter = new CheckBox
            {
                Text = "Live telemetry sensor micro-jitter",
                Location = new Point(16, 104),
                Size = new Size(240, 22),
                ForeColor = Color.FromArgb(210, 225, 240)
            };
            grpHud.Controls.Add(chkJitter);

            chkMultiMonitor = new CheckBox
            {
                Text = "Engage across all monitors",
                Location = new Point(275, 104),
                Size = new Size(180, 22),
                ForeColor = Color.FromArgb(210, 225, 240)
            };
            grpHud.Controls.Add(chkMultiMonitor);

            top += 160;

            // Action Buttons
            btnTest = new Button
            {
                Text = "Preview Viewport",
                Location = new Point(left, top),
                Size = new Size(130, 32),
                BackColor = Color.FromArgb(40, 50, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.Click += OnTestClicked;
            this.Controls.Add(btnTest);

            btnSetDefault = new Button
            {
                Text = "Set Active Screensaver",
                Location = new Point(left + 140, top),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(25, 75, 55),
                ForeColor = Color.FromArgb(180, 255, 210),
                FlatStyle = FlatStyle.Flat
            };
            btnSetDefault.Click += OnSetDefaultClicked;
            this.Controls.Add(btnSetDefault);

            btnSave = new Button
            {
                Text = "Save && Apply",
                Location = new Point(left + 300, top),
                Size = new Size(95, 32),
                BackColor = Color.FromArgb(0, 120, 170),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            btnSave.Click += OnSaveClicked;
            this.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(left + 405, top),
                Size = new Size(70, 32),
                BackColor = Color.FromArgb(50, 55, 65),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private GroupBox CreateGroupBox(string title, int x, int y, int w, int h)
        {
            GroupBox grp = new GroupBox
            {
                Text = "  " + title + "  ",
                Location = new Point(x, y),
                Size = new Size(w, h),
                ForeColor = Color.FromArgb(0, 185, 230),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            return grp;
        }

        private void LoadConfigToUI()
        {
            isUpdatingUI = true;

            // Load presets correctly without defaulting back to index 5
            int targetPreset = (config.PresetIndex >= 0 && config.PresetIndex < cmbPresets.Items.Count)
                ? config.PresetIndex
                : ConfigManager.InferPresetIndex(config.WarpFactor);
            cmbPresets.SelectedIndex = targetPreset;

            int warpSliderVal = Math.Max(2, Math.Min(100, (int)Math.Round(config.WarpFactor * 10f)));
            tbWarp.Value = warpSliderVal;

            tbStarCount.Value = Math.Max(400, Math.Min(4500, config.StarCount));
            lblStarCountVal.Text = tbStarCount.Value.ToString("N0") + " Stars";

            tbStreak.Value = Math.Max(0, Math.Min(30, (int)Math.Round(config.StreakLength * 10f)));
            lblStreakVal.Text = (tbStreak.Value / 10.0f).ToString("F1") + "x";

            if (cmbTheme.Items.Contains(config.ColorTheme))
                cmbTheme.SelectedItem = config.ColorTheme;
            else
                cmbTheme.SelectedIndex = 0;

            chkSpectral.Checked = config.SpectralVariance;
            chkTelemetryFrame.Checked = config.ShowTelemetryFrame;
            chkJitter.Checked = config.EnableSensorJitter;
            chkMultiMonitor.Checked = config.MultiMonitor;

            isUpdatingUI = false;
            UpdateCalculatedSpeedUI();
        }

        private void OnPresetChanged(object sender, EventArgs e)
        {
            if (isUpdatingUI) return;

            isUpdatingUI = true;
            switch (cmbPresets.SelectedIndex)
            {
                case 0: // Impulse 0.3c
                    tbWarp.Value = 3; // 0.3x
                    break;
                case 1: // 1.0c
                    tbWarp.Value = 10; // 1.0x
                    break;
                case 2: // Warp 3
                    tbWarp.Value = 20; // 2.0x
                    break;
                case 3: // Warp 5
                    tbWarp.Value = 35; // 3.5x
                    break;
                case 4: // Warp 8
                    tbWarp.Value = 45; // 4.5x
                    break;
                case 5: // Deep Warp Cruise
                    tbWarp.Value = 50; // 5.0x
                    break;
                case 6: // Transwarp / Slipstream
                    tbWarp.Value = 85; // 8.5x
                    break;
                case 7: // Custom
                    break;
                default:
                    break;
            }
            isUpdatingUI = false;
            UpdateCalculatedSpeedUI();
        }

        private void OnWarpSliderChanged(object sender, EventArgs e)
        {
            if (!isUpdatingUI)
            {
                int val = tbWarp.Value;
                int matched = 7; // Custom
                if (val == 3) matched = 0;
                else if (val == 10) matched = 1;
                else if (val == 20) matched = 2;
                else if (val == 35) matched = 3;
                else if (val == 45) matched = 4;
                else if (val == 50) matched = 5;
                else if (val == 85) matched = 6;

                if (cmbPresets.SelectedIndex != matched)
                {
                    isUpdatingUI = true;
                    cmbPresets.SelectedIndex = matched;
                    isUpdatingUI = false;
                }
            }
            UpdateCalculatedSpeedUI();
        }

        private void UpdateCalculatedSpeedUI()
        {
            float warp = tbWarp.Value / 10.0f;
            lblWarpVal.Text = warp.ToString("F1") + "x";

            // Calculate speeds
            double c = 299792.458;
            double kmPerSec;
            if (warp <= 1.0f)
            {
                kmPerSec = c * warp;
            }
            else
            {
                double multiplier = Math.Pow(warp, 3.3) * 50.0;
                kmPerSec = c * multiplier;
            }

            double lyPerHour = (kmPerSec * 3600.0) / 9.460730472e12;

            if (kmPerSec >= 1000000.0)
                lblCalculatedKm.Text = string.Format("VELOCITY: {0:N0} km/s", kmPerSec);
            else
                lblCalculatedKm.Text = string.Format("VELOCITY: {0:N1} km/s", kmPerSec);

            if (lyPerHour < 0.001)
                lblCalculatedLy.Text = string.Format("RATE: {0:F6} ly/h", lyPerHour);
            else if (lyPerHour < 1.0)
                lblCalculatedLy.Text = string.Format("RATE: {0:F4} ly/h", lyPerHour);
            else
                lblCalculatedLy.Text = string.Format("RATE: {0:F3} ly/h", lyPerHour);
        }

        private void OnTestClicked(object sender, EventArgs e)
        {
            ConfigManager tempCfg = BuildCurrentConfig();
            using (ScreenSaverForm previewForm = new ScreenSaverForm(Screen.PrimaryScreen.Bounds, tempCfg))
            {
                previewForm.ShowDialog();
            }
        }

        private ConfigManager BuildCurrentConfig()
        {
            ConfigManager cfg = new ConfigManager();
            cfg.WarpFactor = tbWarp.Value / 10.0f;
            cfg.PresetIndex = cmbPresets.SelectedIndex;
            cfg.StarCount = tbStarCount.Value;
            cfg.StreakLength = tbStreak.Value / 10.0f;
            cfg.ColorTheme = cmbTheme.SelectedItem != null ? cmbTheme.SelectedItem.ToString() : "Cyan";
            cfg.SpectralVariance = chkSpectral.Checked;
            cfg.ShowTelemetryFrame = chkTelemetryFrame.Checked;
            cfg.EnableSensorJitter = chkJitter.Checked;
            cfg.MultiMonitor = chkMultiMonitor.Checked;
            return cfg;
        }

        private void OnSaveClicked(object sender, EventArgs e)
        {
            config.WarpFactor = tbWarp.Value / 10.0f;
            config.PresetIndex = cmbPresets.SelectedIndex;
            config.StarCount = tbStarCount.Value;
            config.StreakLength = tbStreak.Value / 10.0f;
            config.ColorTheme = cmbTheme.SelectedItem != null ? cmbTheme.SelectedItem.ToString() : "Cyan";
            config.SpectralVariance = chkSpectral.Checked;
            config.ShowTelemetryFrame = chkTelemetryFrame.Checked;
            config.EnableSensorJitter = chkJitter.Checked;
            config.MultiMonitor = chkMultiMonitor.Checked;

            config.Save();
            this.Close();
        }

        private void OnSetDefaultClicked(object sender, EventArgs e)
        {
            try
            {
                // Save current configuration first
                config.WarpFactor = tbWarp.Value / 10.0f;
                config.PresetIndex = cmbPresets.SelectedIndex;
                config.StarCount = tbStarCount.Value;
                config.StreakLength = tbStreak.Value / 10.0f;
                config.ColorTheme = cmbTheme.SelectedItem != null ? cmbTheme.SelectedItem.ToString() : "Cyan";
                config.SpectralVariance = chkSpectral.Checked;
                config.ShowTelemetryFrame = chkTelemetryFrame.Checked;
                config.EnableSensorJitter = chkJitter.Checked;
                config.MultiMonitor = chkMultiMonitor.Checked;
                config.Save();

                string currentExePath = Application.ExecutablePath;
                using (RegistryKey desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true))
                {
                    if (desktopKey != null)
                    {
                        desktopKey.SetValue("SCRNSAVE.EXE", currentExePath);
                        desktopKey.SetValue("ScreenSaveActive", "1");
                    }
                }

                MessageBox.Show(
                    "Starship Viewport is now registered as your active Windows Screensaver!\n\nLocation: " + currentExePath,
                    "Screensaver Installed Successfully",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not set screensaver: " + ex.Message, "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

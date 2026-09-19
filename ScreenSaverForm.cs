using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StarshipStarfield
{
    public class ScreenSaverForm : Form
    {
        #region Win32 API Interop
        [DllImport("user32.dll")]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        private const int GWL_STYLE = -16;
        private const int WS_CHILD = 0x40000000;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        #endregion

        private ConfigManager config;
        private StarfieldEngine engine;
        private TelemetryOverlay telemetry;
        private Timer renderTimer;
        private Stopwatch stopwatch;
        private double lastTimestamp = 0.0;

        private bool isPreviewMode = false;
        private Point initialMousePos;
        private bool hasInitialMousePos = false;

        // Constructor for full screen mode
        public ScreenSaverForm(Rectangle bounds, ConfigManager cfg)
        {
            this.config = cfg;
            this.isPreviewMode = false;

            InitializeStyles();

            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            this.Bounds = bounds;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            InitEngineAndTelemetry();
        }

        // Constructor for preview mode
        public ScreenSaverForm(IntPtr previewHwnd, ConfigManager cfg)
        {
            this.config = cfg;
            this.isPreviewMode = true;

            InitializeStyles();

            // Set parent window to preview box
            SetParent(this.Handle, previewHwnd);

            // Set child window style
            int style = GetWindowLong(this.Handle, GWL_STYLE);
            SetWindowLong(this.Handle, GWL_STYLE, new IntPtr(style | WS_CHILD));

            // Match parent preview size
            RECT rect;
            GetClientRect(previewHwnd, out rect);
            this.Size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            this.Location = new Point(0, 0);

            InitEngineAndTelemetry();
        }

        private void InitializeStyles()
        {
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque,
                true);
            this.UpdateStyles();
            this.BackColor = Color.Black;
        }

        private void InitEngineAndTelemetry()
        {
            // Map WarpFactor (0.1 to 10.0) to starfield speed (3.5 to 85.0) for high-velocity warp travel
            float visualSpeed = Math.Max(3.5f, config.WarpFactor * 8.5f);
            engine = new StarfieldEngine(
                config.StarCount,
                visualSpeed,
                config.StreakLength,
                config.SpectralVariance,
                config.EnableNebula,
                config.EnableCosmicDust
            );
            engine.Resize(this.ClientSize.Width, this.ClientSize.Height);

            telemetry = new TelemetryOverlay(config);

            stopwatch = Stopwatch.StartNew();
            lastTimestamp = stopwatch.Elapsed.TotalSeconds;

            renderTimer = new Timer();
            renderTimer.Interval = 16; // ~60 FPS
            renderTimer.Tick += OnRenderTick;
            renderTimer.Start();
        }

        private void OnRenderTick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (engine != null)
            {
                engine.Resize(this.ClientSize.Width, this.ClientSize.Height);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            double now = stopwatch.Elapsed.TotalSeconds;
            float dt = (float)(now - lastTimestamp);
            lastTimestamp = now;

            // Clamp delta time to avoid large jumps during pauses
            if (dt > 0.1f) dt = 0.1f;
            if (dt < 0.001f) dt = 0.001f;

            Graphics g = e.Graphics;

            // Clear to deep cosmos only if engine does not have an opaque backdrop covering the frame
            if (engine == null || !engine.HasBackdrop)
            {
                g.Clear(Color.Black);
            }

            // Update & render starfield
            if (engine != null)
            {
                engine.Update(dt);
                engine.Render(g);
            }

            // Update & render telemetry overlay (if view is wide enough, e.g. > 300px)
            if (telemetry != null && this.ClientSize.Width > 320 && this.ClientSize.Height > 200)
            {
                telemetry.Update(dt);
                telemetry.Render(g, this.ClientSize.Width, this.ClientSize.Height);
            }
        }

        #region Input Handling (Screensaver Exit Triggers)
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!isPreviewMode)
            {
                Cursor.Hide();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isPreviewMode) return;

            if (!hasInitialMousePos)
            {
                initialMousePos = e.Location;
                hasInitialMousePos = true;
                return;
            }

            // Exit only if mouse moved significantly (prevents vibration accidental exits)
            if (Math.Abs(e.X - initialMousePos.X) > 10 || Math.Abs(e.Y - initialMousePos.Y) > 10)
            {
                CloseScreensaver();
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!isPreviewMode)
            {
                CloseScreensaver();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!isPreviewMode)
            {
                CloseScreensaver();
            }
        }

        private void CloseScreensaver()
        {
            if (renderTimer != null)
            {
                renderTimer.Stop();
            }
            Cursor.Show();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (renderTimer != null)
                {
                    renderTimer.Stop();
                    renderTimer.Dispose();
                    renderTimer = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}

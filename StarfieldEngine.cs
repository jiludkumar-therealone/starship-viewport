using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace StarshipStarfield
{
    public struct Star
    {
        public float X;
        public float Y;
        public float Z;
        public float PrevZ;
        public Color BaseColor;
        public float Size;
    }

    public class StarfieldEngine
    {
        private Star[] stars;
        private Random rand = new Random();
        private int width;
        private int height;
        private float centerX;
        private float centerY;
        private float fov = 450.0f;
        private float maxZ = 2000.0f;

        // Configuration
        public float Speed { get; set; }
        public float StreakMultiplier { get; set; }
        public bool SpectralColors { get; set; }

        // Color palettes for star classes
        private static readonly Color[] SpectralPalette = new Color[]
        {
            Color.FromArgb(215, 235, 255), // Class O/B (Deep Space Ice Blue)
            Color.FromArgb(255, 255, 255), // Class A (Brilliant White)
            Color.FromArgb(255, 250, 235), // Class F (Warm Ivory)
            Color.FromArgb(255, 225, 175), // Class G (Solar Golden)
            Color.FromArgb(255, 195, 140), // Class K (Amber)
            Color.FromArgb(170, 205, 255)  // Class O (Subtle Cobalt Blue)
        };

        public StarfieldEngine(int count, float speed, float streakMult, bool spectralColors)
        {
            this.Speed = speed;
            this.StreakMultiplier = streakMult;
            this.SpectralColors = spectralColors;
            InitStars(count);
        }

        public void Resize(int w, int h)
        {
            this.width = Math.Max(100, w);
            this.height = Math.Max(100, h);
            this.centerX = this.width / 2.0f;
            this.centerY = this.height / 2.0f;
            this.fov = Math.Min(this.width, this.height) * 0.55f;
        }

        public void SetStarCount(int count)
        {
            if (stars == null || stars.Length != count)
            {
                InitStars(count);
            }
        }

        private void InitStars(int count)
        {
            stars = new Star[count];
            for (int i = 0; i < count; i++)
            {
                ResetStar(ref stars[i], true);
            }
        }

        private void ResetStar(ref Star star, bool randomizeZ)
        {
            // Spread stars across field relative to screen aspect
            float spreadX = width > 0 ? width * 1.5f : 2000f;
            float spreadY = height > 0 ? height * 1.5f : 1500f;

            star.X = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadX;
            star.Y = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadY;

            if (randomizeZ)
            {
                star.Z = (float)rand.NextDouble() * (maxZ - 10.0f) + 10.0f;
            }
            else
            {
                star.Z = maxZ;
            }

            star.PrevZ = star.Z;

            if (SpectralColors)
            {
                int cIdx = rand.Next(SpectralPalette.Length);
                star.BaseColor = SpectralPalette[cIdx];
            }
            else
            {
                star.BaseColor = Color.White;
            }

            star.Size = (float)(rand.NextDouble() * 1.8 + 0.8);
        }

        public void Update(float deltaTime)
        {
            if (stars == null) return;

            // Travel speed in Z units per second
            // deltaTime is in seconds (e.g. ~0.016s at 60fps)
            float zStep = Speed * deltaTime * 60.0f;

            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].PrevZ = stars[i].Z;
                stars[i].Z -= zStep;

                if (stars[i].Z <= 1.0f)
                {
                    ResetStar(ref stars[i], false);
                }
            }
        }

        public void Render(Graphics g)
        {
            if (stars == null || width <= 0 || height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i < stars.Length; i++)
            {
                float z = stars[i].Z;
                if (z <= 1.0f) continue;

                // Project current position
                float k = fov / z;
                float sx = stars[i].X * k + centerX;
                float sy = stars[i].Y * k + centerY;

                // Quick bounds discard
                if (sx < -100 || sx > width + 100 || sy < -100 || sy > height + 100)
                {
                    // If far off screen and moving past, recycle
                    if (z < maxZ * 0.3f)
                    {
                        ResetStar(ref stars[i], false);
                    }
                    continue;
                }

                // Alpha based on depth: fades in as it approaches, distant stars are faint
                float depthNorm = 1.0f - (z / maxZ);
                if (depthNorm < 0f) depthNorm = 0f;
                if (depthNorm > 1f) depthNorm = 1f;

                int alpha = (int)(depthNorm * 255.0f);
                if (alpha < 8) continue;

                Color col = stars[i].BaseColor;
                Color renderColor = Color.FromArgb(alpha, col.R, col.G, col.B);

                // Project previous position for relativistic streak
                float pz = stars[i].PrevZ;
                if (StreakMultiplier > 0.05f && pz > z)
                {
                    // Stretch factor
                    float pzEffective = z + (pz - z) * StreakMultiplier;
                    float pk = fov / pzEffective;
                    float psx = stars[i].X * pk + centerX;
                    float psy = stars[i].Y * pk + centerY;

                    float distSq = (sx - psx) * (sx - psx) + (sy - psy) * (sy - psy);

                    // If there is visible motion streak
                    if (distSq > 1.0f)
                    {
                        float strokeWidth = Math.Max(1.0f, (1.0f - (z / maxZ)) * 2.8f);
                        using (Pen pen = new Pen(renderColor, strokeWidth))
                        {
                            pen.StartCap = LineCap.Round;
                            pen.EndCap = LineCap.Round;
                            g.DrawLine(pen, psx, psy, sx, sy);
                        }
                    }
                }

                // Render star head point
                float starRadius = Math.Max(0.8f, (1.0f - (z / maxZ)) * stars[i].Size * 2.2f);
                if (starRadius > 2.5f)
                {
                    // Close star glow
                    int glowAlpha = Math.Min(180, alpha / 2);
                    using (Brush glowBrush = new SolidBrush(Color.FromArgb(glowAlpha, col.R, col.G, col.B)))
                    {
                        float glowR = starRadius * 1.8f;
                        g.FillEllipse(glowBrush, sx - glowR, sy - glowR, glowR * 2.0f, glowR * 2.0f);
                    }
                }

                using (Brush brush = new SolidBrush(renderColor))
                {
                    g.FillEllipse(brush, sx - starRadius, sy - starRadius, starRadius * 2.0f, starRadius * 2.0f);
                }
            }
        }
    }
}

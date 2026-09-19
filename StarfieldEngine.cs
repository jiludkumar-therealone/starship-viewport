using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

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

    public struct NebulaLobe
    {
        public float OffsetX;
        public float OffsetY;
        public float OffsetZ;
        public float Radius;
        public Color Color;
        public float Alpha;
    }

    public class NebulaCluster
    {
        public float X;
        public float Y;
        public float Z;
        public NebulaLobe[] Lobes;
        public float BaseScale;
    }

    public struct CosmicDustMote
    {
        public float X;
        public float Y;
        public float Z;
        public float DriftX;
        public float DriftY;
        public Color Tint;
        public float Size;
    }

    public class StarfieldEngine : IDisposable
    {
        private Star[] stars;
        private NebulaCluster[] nebulae;
        private CosmicDustMote[] dustMotes;
        private Bitmap cosmicDustBackdrop;
        private Random rand = new Random(1701);

        private int width;
        private int height;
        private float centerX;
        private float centerY;
        private float fov = 450.0f;
        private float maxZ = 2000.0f;
        private float maxNebulaZ = 3600.0f;

        // Background cosmic drift offset
        private float backdropOffsetX = 0f;
        private float backdropOffsetY = 0f;

        // Configuration
        public float Speed { get; set; }
        public float StreakMultiplier { get; set; }
        public bool SpectralColors { get; set; }
        public bool EnableNebula { get; set; }
        public bool EnableCosmicDust { get; set; }

        private static readonly Color[] SpectralPalette = new Color[]
        {
            Color.FromArgb(215, 235, 255), // Class O/B (Deep Space Ice Blue)
            Color.FromArgb(255, 255, 255), // Class A (Brilliant White)
            Color.FromArgb(255, 250, 235), // Class F (Warm Ivory)
            Color.FromArgb(255, 225, 175), // Class G (Solar Golden)
            Color.FromArgb(255, 195, 140), // Class K (Amber)
            Color.FromArgb(170, 205, 255)  // Class O (Subtle Cobalt Blue)
        };

        // Astronomical gas emission palettes (JWST / Hubble narrowband filter colors)
        private static readonly Color[][] NebulaThemes = new Color[][]
        {
            // Theme 0: Orion / Carina (Deep Hydrogen-Alpha Rose & Oxygen-III Cyan)
            new Color[] {
                Color.FromArgb(190, 35, 95),   // H-alpha deep rose
                Color.FromArgb(25, 155, 210),  // [O III] vivid cyan
                Color.FromArgb(120, 25, 160),  // Deep space violet
                Color.FromArgb(220, 90, 140)   // Ionized pink rim
            },
            // Theme 1: Cygnus Veil / Pelican (Celestial Emerald & Cobalt Indigo)
            new Color[] {
                Color.FromArgb(30, 165, 135),  // Ionized oxygen teal
                Color.FromArgb(60, 50, 210),   // Deep cobalt
                Color.FromArgb(20, 120, 180),  // Electric cyan
                Color.FromArgb(85, 190, 160)   // Soft mint glow
            },
            // Theme 2: Eagle / Pillars of Creation (Sulfur-II Amber, Dust Brown, & Cyan Veil)
            new Color[] {
                Color.FromArgb(210, 125, 35),  // [S II] sulfur gold
                Color.FromArgb(175, 60, 25),   // Cosmic dust copper
                Color.FromArgb(35, 140, 195),  // Background [O III] cyan
                Color.FromArgb(240, 185, 80)   // Stellar amber filament
            },
            // Theme 3: Deep Space Ultraviolet Void (Ethereal Purple & Magenta)
            new Color[] {
                Color.FromArgb(135, 40, 190),  // Ultraviolet violet
                Color.FromArgb(195, 45, 135),  // Hot magenta
                Color.FromArgb(70, 30, 160),   // Indigo core
                Color.FromArgb(170, 90, 230)   // Soft purple wisp
            }
        };

        private static readonly Color[] DustPalette = new Color[]
        {
            Color.FromArgb(210, 225, 245), // Silvery starlight
            Color.FromArgb(245, 220, 180), // Solar gold grain
            Color.FromArgb(180, 235, 230)  // Faint ionization cyan
        };

        public StarfieldEngine(int count, float speed, float streakMult, bool spectralColors, bool enableNebula = true, bool enableDust = true)
        {
            this.Speed = speed;
            this.StreakMultiplier = streakMult;
            this.SpectralColors = spectralColors;
            this.EnableNebula = enableNebula;
            this.EnableCosmicDust = enableDust;

            InitStars(count);
            InitNebulae(14);
            InitCosmicDust(350);
        }

        public void Resize(int w, int h)
        {
            this.width = Math.Max(100, w);
            this.height = Math.Max(100, h);
            this.centerX = this.width / 2.0f;
            this.centerY = this.height / 2.0f;
            this.fov = Math.Min(this.width, this.height) * 0.58f;

            GenerateCosmicDustBackdrop();
        }

        private void GenerateCosmicDustBackdrop()
        {
            if (cosmicDustBackdrop != null)
            {
                cosmicDustBackdrop.Dispose();
                cosmicDustBackdrop = null;
            }

            if (!EnableNebula || width <= 0 || height <= 0) return;

            // Generate multi-layer procedural fractal cosmic gas backdrop
            int texW = 1024;
            int texH = 768;
            cosmicDustBackdrop = new Bitmap(texW, texH, PixelFormat.Format32bppArgb);

            // Generate 3 octaves of smooth Perlin-like value noise
            int gridStep = 64;
            int gw = (texW / gridStep) + 3;
            int gh = (texH / gridStep) + 3;
            float[,] nGrid1 = new float[gw, gh];
            float[,] nGrid2 = new float[gw * 2, gh * 2];

            for (int x = 0; x < gw; x++)
                for (int y = 0; y < gh; y++)
                    nGrid1[x, y] = (float)rand.NextDouble();

            for (int x = 0; x < gw * 2; x++)
                for (int y = 0; y < gh * 2; y++)
                    nGrid2[x, y] = (float)rand.NextDouble();

            BitmapData data = cosmicDustBackdrop.LockBits(
                new Rectangle(0, 0, texW, texH),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* ptr = (byte*)data.Scan0;
                int stride = data.Stride;

                for (int y = 0; y < texH; y++)
                {
                    byte* row = ptr + (y * stride);

                    float gy1 = (float)y / gridStep;
                    int y0 = (int)gy1;
                    int y1 = y0 + 1;
                    float ty1 = gy1 - y0;
                    float sy1 = ty1 * ty1 * (3.0f - 2.0f * ty1);

                    float gy2 = (float)y / (gridStep / 2f);
                    int y20 = (int)gy2;
                    int y21 = y20 + 1;
                    float ty2 = gy2 - y20;
                    float sy2 = ty2 * ty2 * (3.0f - 2.0f * ty2);

                    for (int x = 0; x < texW; x++)
                    {
                        float gx1 = (float)x / gridStep;
                        int x0 = (int)gx1;
                        int x1 = x0 + 1;
                        float tx1 = gx1 - x0;
                        float sx1 = tx1 * tx1 * (3.0f - 2.0f * tx1);

                        float top1 = nGrid1[x0, y0] * (1.0f - sx1) + nGrid1[x1, y0] * sx1;
                        float bot1 = nGrid1[x0, y1] * (1.0f - sx1) + nGrid1[x1, y1] * sx1;
                        float n1 = top1 * (1.0f - sy1) + bot1 * sy1;

                        float gx2 = (float)x / (gridStep / 2f);
                        int x20 = (int)gx2;
                        int x21 = x20 + 1;
                        float tx2 = gx2 - x20;
                        float sx2 = tx2 * tx2 * (3.0f - 2.0f * tx2);

                        float top2 = nGrid2[x20, y20] * (1.0f - sx2) + nGrid2[x21, y20] * sx2;
                        float bot2 = nGrid2[x20, y21] * (1.0f - sx2) + nGrid2[x21, y21] * sx2;
                        float n2 = top2 * (1.0f - sy2) + bot2 * sy2;

                        // Combined turbulent noise with dark dust absorption rifts
                        float gasDensity = (n1 * 0.7f + n2 * 0.3f);
                        gasDensity = (float)Math.Pow(gasDensity, 1.8); // High contrast wisps

                        // Dark dust lane filter (Bok globules)
                        float dustRift = (float)Math.Sin(x * 0.008f + y * 0.006f + n1 * 3.5f);
                        if (dustRift > 0.4f)
                        {
                            gasDensity *= Math.Max(0.2f, 1.0f - (dustRift - 0.4f) * 1.5f);
                        }

                        // Colors: Deep space cosmic gas curtain (deep violet to hydrogen-alpha rose & teal)
                        int alpha = (int)(gasDensity * 80.0f); // Rich, ethereal deep background opacity
                        int r = (int)(gasDensity * 140.0f);
                        int g = (int)(gasDensity * 65.0f);
                        int b = (int)(gasDensity * 205.0f);

                        int idx = x * 4;
                        row[idx + 0] = (byte)Math.Min(255, b);
                        row[idx + 1] = (byte)Math.Min(255, g);
                        row[idx + 2] = (byte)Math.Min(255, r);
                        row[idx + 3] = (byte)Math.Min(255, alpha);
                    }
                }
            }

            cosmicDustBackdrop.UnlockBits(data);
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
            float spreadX = width > 0 ? width * 1.6f : 2400f;
            float spreadY = height > 0 ? height * 1.6f : 1800f;

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

        private void InitNebulae(int count)
        {
            nebulae = new NebulaCluster[count];
            for (int i = 0; i < count; i++)
            {
                nebulae[i] = CreateNebulaCluster(true);
            }
        }

        private NebulaCluster CreateNebulaCluster(bool randomizeZ)
        {
            NebulaCluster cluster = new NebulaCluster();
            float spreadX = width > 0 ? width * 2.2f : 3200f;
            float spreadY = height > 0 ? height * 2.2f : 2400f;

            cluster.X = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadX;
            cluster.Y = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadY;
            cluster.Z = randomizeZ ? ((float)rand.NextDouble() * (maxNebulaZ - 500.0f) + 500.0f) : maxNebulaZ;
            cluster.BaseScale = (float)(rand.NextDouble() * 380.0 + 400.0);

            // Select an astronomical emission theme
            Color[] theme = NebulaThemes[rand.Next(NebulaThemes.Length)];

            // Generate 6 to 9 interconnected organic fractal lobes per cluster
            int numLobes = rand.Next(6, 10);
            cluster.Lobes = new NebulaLobe[numLobes];

            for (int j = 0; j < numLobes; j++)
            {
                float angle = (float)(rand.NextDouble() * Math.PI * 2.0);
                float dist = (float)(rand.NextDouble() * cluster.BaseScale * 0.65f);

                cluster.Lobes[j] = new NebulaLobe
                {
                    OffsetX = (float)Math.Cos(angle) * dist,
                    OffsetY = (float)Math.Sin(angle) * dist,
                    OffsetZ = (float)((rand.NextDouble() - 0.5) * 120.0),
                    Radius = (float)(rand.NextDouble() * cluster.BaseScale * 0.75f + cluster.BaseScale * 0.35f),
                    Color = theme[rand.Next(theme.Length)],
                    Alpha = (float)(rand.NextDouble() * 38.0 + 36.0) // 36 to 74 alpha per overlapping lobe
                };
            }

            return cluster;
        }

        private void InitCosmicDust(int count)
        {
            dustMotes = new CosmicDustMote[count];
            for (int i = 0; i < count; i++)
            {
                ResetDust(ref dustMotes[i], true);
            }
        }

        private void ResetDust(ref CosmicDustMote dust, bool randomizeZ)
        {
            float spreadX = width > 0 ? width * 1.3f : 1800f;
            float spreadY = height > 0 ? height * 1.3f : 1400f;

            dust.X = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadX;
            dust.Y = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadY;
            dust.Z = randomizeZ ? ((float)rand.NextDouble() * (maxZ - 10.0f) + 10.0f) : maxZ;
            dust.DriftX = (float)((rand.NextDouble() - 0.5) * 8.0);
            dust.DriftY = (float)((rand.NextDouble() - 0.5) * 8.0);
            dust.Tint = DustPalette[rand.Next(DustPalette.Length)];
            dust.Size = (float)(rand.NextDouble() * 1.1 + 0.5);
        }

        public void Update(float deltaTime)
        {
            float zStep = Speed * deltaTime * 60.0f;

            // Subtle parallax backdrop drift
            backdropOffsetX += Speed * deltaTime * 1.5f;
            backdropOffsetY += (float)Math.Sin(backdropOffsetX * 0.01) * 0.2f;

            // 1. Advance Stars
            if (stars != null)
            {
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

            // 2. Advance 3D Cosmic Nebulae
            if (EnableNebula && nebulae != null)
            {
                float nebulaZStep = zStep * 0.32f;
                for (int i = 0; i < nebulae.Length; i++)
                {
                    nebulae[i].Z -= nebulaZStep;
                    if (nebulae[i].Z <= 220.0f)
                    {
                        nebulae[i] = CreateNebulaCluster(false);
                    }
                }
            }

            // 3. Advance Interstellar Cosmic Dust
            if (EnableCosmicDust && dustMotes != null)
            {
                for (int i = 0; i < dustMotes.Length; i++)
                {
                    dustMotes[i].Z -= zStep;
                    dustMotes[i].X += dustMotes[i].DriftX * deltaTime * 15.0f;
                    dustMotes[i].Y += dustMotes[i].DriftY * deltaTime * 15.0f;

                    if (dustMotes[i].Z <= 2.0f)
                    {
                        ResetDust(ref dustMotes[i], false);
                    }
                }
            }
        }

        public void Render(Graphics g)
        {
            if (width <= 0 || height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Deep Space Cosmic Dust Backdrop (JWST Deep Field Gas Curtain)
            if (EnableNebula && cosmicDustBackdrop != null)
            {
                RenderCosmicBackdrop(g);
            }

            // 2. Procedural 3D Multi-Lobe Volumetric Nebulae
            if (EnableNebula && nebulae != null)
            {
                RenderNebulae(g);
            }

            // 3. Interstellar Cosmic Dust Motes
            if (EnableCosmicDust && dustMotes != null)
            {
                RenderCosmicDust(g);
            }

            // 4. Stars & Blazing Relativistic Warp Strikes
            if (stars != null)
            {
                RenderStars(g);
            }
        }

        private void RenderCosmicBackdrop(Graphics g)
        {
            // Draw expansive procedural nebula gas curtain across the deep field
            int srcW = cosmicDustBackdrop.Width;
            int srcH = cosmicDustBackdrop.Height;

            int ox = (int)backdropOffsetX % srcW;
            int oy = (int)backdropOffsetY % srcH;

            // Scale to fill viewport
            Rectangle destRect = new Rectangle(0, 0, width, height);
            g.DrawImage(cosmicDustBackdrop, destRect, 0, 0, srcW, srcH, GraphicsUnit.Pixel);
        }

        private void RenderNebulae(Graphics g)
        {
            for (int i = 0; i < nebulae.Length; i++)
            {
                NebulaCluster cluster = nebulae[i];
                float cz = cluster.Z;
                if (cz <= 220.0f) continue;

                float kCluster = fov / cz;
                float csx = cluster.X * kCluster + centerX;
                float csy = cluster.Y * kCluster + centerY;

                // Overall cluster volumetric fade
                float clusterAlphaFactor = 1.0f;
                if (cz > 2400.0f)
                {
                    clusterAlphaFactor = Math.Max(0f, (maxNebulaZ - cz) / (maxNebulaZ - 2400.0f));
                }
                else if (cz < 600.0f)
                {
                    clusterAlphaFactor = Math.Max(0f, (cz - 220.0f) / 380.0f);
                }

                if (clusterAlphaFactor <= 0.01f) continue;

                // Render each organic fractal lobe
                for (int l = 0; l < cluster.Lobes.Length; l++)
                {
                    NebulaLobe lobe = cluster.Lobes[l];
                    float lz = cz + lobe.OffsetZ;
                    if (lz <= 180.0f) continue;

                    float lk = fov / lz;
                    float lsx = (cluster.X + lobe.OffsetX) * lk + centerX;
                    float lsy = (cluster.Y + lobe.OffsetY) * lk + centerY;
                    float lsRadius = lobe.Radius * lk;

                    // Bounds check
                    if (lsx < -lsRadius || lsx > width + lsRadius || lsy < -lsRadius || lsy > height + lsRadius)
                        continue;

                    int lobeAlpha = (int)(lobe.Alpha * clusterAlphaFactor);
                    if (lobeAlpha < 3) continue;

                    Color c = lobe.Color;
                    RectangleF lobeBounds = new RectangleF(lsx - lsRadius, lsy - lsRadius, lsRadius * 2.0f, lsRadius * 2.0f);

                    using (GraphicsPath lobePath = new GraphicsPath())
                    {
                        lobePath.AddEllipse(lobeBounds);
                        using (PathGradientBrush pgb = new PathGradientBrush(lobePath))
                        {
                            pgb.CenterColor = Color.FromArgb(lobeAlpha, c.R, c.G, c.B);
                            pgb.SurroundColors = new Color[] { Color.FromArgb(0, c.R, c.G, c.B) };
                            pgb.FocusScales = new PointF(0.25f, 0.25f); // Soft organic plume center
                            g.FillPath(pgb, lobePath);
                        }
                    }
                }
            }
        }

        private void RenderCosmicDust(Graphics g)
        {
            for (int i = 0; i < dustMotes.Length; i++)
            {
                float z = dustMotes[i].Z;
                if (z <= 2.0f) continue;

                float k = fov / z;
                float sx = dustMotes[i].X * k + centerX;
                float sy = dustMotes[i].Y * k + centerY;

                if (sx < -20 || sx > width + 20 || sy < -20 || sy > height + 20)
                    continue;

                float depthNorm = 1.0f - (z / maxZ);
                int alpha = (int)(depthNorm * 180.0f);
                if (alpha < 6) continue;

                Color col = dustMotes[i].Tint;
                Color dustColor = Color.FromArgb(alpha, col.R, col.G, col.B);

                float r = dustMotes[i].Size * (0.4f + depthNorm * 0.9f);
                using (SolidBrush dBrush = new SolidBrush(dustColor))
                {
                    g.FillEllipse(dBrush, sx - r, sy - r, r * 2.0f, r * 2.0f);
                }
            }
        }

        private void RenderStars(Graphics g)
        {
            // Calculate warp speed strike scale
            // At warp speeds, forward velocity produces dramatic relativistic light strikes radiating outward
            float warpFactor = Math.Max(0.5f, Speed / 4.5f);
            float strikeBaseLength = StreakMultiplier * (warpFactor * 0.85f);

            for (int i = 0; i < stars.Length; i++)
            {
                float z = stars[i].Z;
                if (z <= 1.0f) continue;

                float k = fov / z;
                float sx = stars[i].X * k + centerX;
                float sy = stars[i].Y * k + centerY;

                // Screen bounds cull
                if (sx < -150 || sx > width + 150 || sy < -150 || sy > height + 150)
                {
                    if (z < maxZ * 0.3f) ResetStar(ref stars[i], false);
                    continue;
                }

                // Relativistic depth luminosity
                float depthNorm = 1.0f - (z / maxZ);
                depthNorm = Math.Max(0f, Math.Min(1f, depthNorm));

                int alpha = (int)(depthNorm * 255.0f);
                if (alpha < 8) continue;

                Color col = stars[i].BaseColor;
                Color renderColor = Color.FromArgb(alpha, col.R, col.G, col.B);

                // Relativistic Warp Star Strikes:
                // Calculate radial vector from screen center (starship forward apex)
                float radX = sx - centerX;
                float radY = sy - centerY;
                float radDist = (float)Math.Sqrt(radX * radX + radY * radY + 0.001f);

                // Strike length scales with radial distance, forward speed, and depth proximity
                // Clamped so streaks never cross the forward vanishing apex
                float maxAllowedLength = radDist * 0.80f;
                float calculatedLength = (radDist / 500.0f) * strikeBaseLength * (depthNorm * 52.0f + 8.0f);
                float strikeLength = Math.Min(maxAllowedLength, calculatedLength);

                if (strikeLength > 2.0f && radDist > 14.0f)
                {
                    // Tail coordinate radiating back toward the forward apex
                    float dirX = radX / radDist;
                    float dirY = radY / radDist;
                    float tailX = sx - (dirX * strikeLength);
                    float tailY = sy - (dirY * strikeLength);

                    // Dynamic stroke thickness: thin at distant tail, blazing at approaching head
                    float strokeWidth = Math.Max(1.0f, (stars[i].Size * 0.9f) + (depthNorm * 2.6f));

                    // Luminous ionization streak
                    using (Pen pen = new Pen(renderColor, strokeWidth))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        g.DrawLine(pen, tailX, tailY, sx, sy);
                    }

                    // Bright core head highlight
                    if (depthNorm > 0.45f)
                    {
                        int coreAlpha = Math.Min(255, (int)(alpha * 1.15f));
                        using (SolidBrush coreBrush = new SolidBrush(Color.FromArgb(coreAlpha, 255, 255, 255)))
                        {
                            float headR = strokeWidth * 0.9f;
                            g.FillEllipse(coreBrush, sx - headR, sy - headR, headR * 2.0f, headR * 2.0f);
                        }
                    }
                }
                else
                {
                    // Distant pinpoint star
                    float starRadius = Math.Max(0.8f, depthNorm * stars[i].Size * 1.8f);
                    using (SolidBrush brush = new SolidBrush(renderColor))
                    {
                        g.FillEllipse(brush, sx - starRadius, sy - starRadius, starRadius * 2.0f, starRadius * 2.0f);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (cosmicDustBackdrop != null)
            {
                cosmicDustBackdrop.Dispose();
                cosmicDustBackdrop = null;
            }
        }
    }
}

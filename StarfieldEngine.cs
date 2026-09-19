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

    public struct NebulaNode
    {
        public float X;
        public float Y;
        public float Z;
        public float Radius;
        public Color BaseColor;
        public float MaxAlpha;
    }

    public struct CosmicDustMote
    {
        public float X;
        public float Y;
        public float Z;
        public float PrevZ;
        public float DriftX;
        public float DriftY;
        public Color Tint;
        public float Size;
    }

    public class StarfieldEngine
    {
        private Star[] stars;
        private NebulaNode[] nebulae;
        private CosmicDustMote[] dustMotes;
        private Random rand = new Random(1701); // Enterprise NCC-1701 seed

        private int width;
        private int height;
        private float centerX;
        private float centerY;
        private float fov = 450.0f;
        private float maxZ = 2000.0f;
        private float maxNebulaZ = 3400.0f;

        // Configuration
        public float Speed { get; set; }
        public float StreakMultiplier { get; set; }
        public bool SpectralColors { get; set; }
        public bool EnableNebula { get; set; }
        public bool EnableCosmicDust { get; set; }

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

        // Astronomical cosmic nebula palette
        private static readonly Color[] NebulaPalette = new Color[]
        {
            Color.FromArgb(24, 145, 205),  // Celestial Cyan
            Color.FromArgb(140, 45, 185),  // Deep Violet
            Color.FromArgb(195, 50, 110),  // Hydrogen-Alpha Rose
            Color.FromArgb(185, 120, 35),  // Stellar Amber
            Color.FromArgb(35, 170, 130),  // Ionized Emerald
            Color.FromArgb(85, 70, 215)   // Deep Space Indigo
        };

        private static readonly Color[] DustPalette = new Color[]
        {
            Color.FromArgb(190, 210, 235), // Ice-silvery
            Color.FromArgb(235, 215, 185), // Starlight-gold
            Color.FromArgb(175, 230, 225)  // Faint teal
        };

        public StarfieldEngine(int count, float speed, float streakMult, bool spectralColors, bool enableNebula = true, bool enableDust = true)
        {
            this.Speed = speed;
            this.StreakMultiplier = streakMult;
            this.SpectralColors = spectralColors;
            this.EnableNebula = enableNebula;
            this.EnableCosmicDust = enableDust;

            InitStars(count);
            InitNebulae(18);
            InitCosmicDust(320);
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

        private void InitNebulae(int count)
        {
            nebulae = new NebulaNode[count];
            for (int i = 0; i < count; i++)
            {
                ResetNebula(ref nebulae[i], true);
            }
        }

        private void ResetNebula(ref NebulaNode node, bool randomizeZ)
        {
            float spreadX = width > 0 ? width * 2.4f : 3200f;
            float spreadY = height > 0 ? height * 2.4f : 2400f;

            node.X = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadX;
            node.Y = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadY;

            if (randomizeZ)
            {
                node.Z = (float)rand.NextDouble() * (maxNebulaZ - 400.0f) + 400.0f;
            }
            else
            {
                node.Z = maxNebulaZ;
            }

            node.Radius = (float)(rand.NextDouble() * 450.0 + 350.0); // 350 to 800 world units
            node.BaseColor = NebulaPalette[rand.Next(NebulaPalette.Length)];
            node.MaxAlpha = (float)(rand.NextDouble() * 42.0 + 26.0); // Soft, subtle ethereal glow (26 to 68 alpha)
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
            float spreadX = width > 0 ? width * 1.2f : 1600f;
            float spreadY = height > 0 ? height * 1.2f : 1200f;

            dust.X = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadX;
            dust.Y = ((float)rand.NextDouble() * 2.0f - 1.0f) * spreadY;

            if (randomizeZ)
            {
                dust.Z = (float)rand.NextDouble() * (maxZ - 10.0f) + 10.0f;
            }
            else
            {
                dust.Z = maxZ;
            }

            dust.PrevZ = dust.Z;
            dust.DriftX = (float)((rand.NextDouble() - 0.5) * 6.0);
            dust.DriftY = (float)((rand.NextDouble() - 0.5) * 6.0);
            dust.Tint = DustPalette[rand.Next(DustPalette.Length)];
            dust.Size = (float)(rand.NextDouble() * 0.9 + 0.4);
        }

        public void Update(float deltaTime)
        {
            float zStep = Speed * deltaTime * 60.0f;

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
                // Nebulae drift forward at a stately cosmic pace (~35% of ship warp velocity)
                float nebulaZStep = zStep * 0.35f;
                for (int i = 0; i < nebulae.Length; i++)
                {
                    nebulae[i].Z -= nebulaZStep;
                    if (nebulae[i].Z <= 160.0f)
                    {
                        ResetNebula(ref nebulae[i], false);
                    }
                }
            }

            // 3. Advance Interstellar Cosmic Dust
            if (EnableCosmicDust && dustMotes != null)
            {
                for (int i = 0; i < dustMotes.Length; i++)
                {
                    dustMotes[i].PrevZ = dustMotes[i].Z;
                    dustMotes[i].Z -= zStep;
                    dustMotes[i].X += dustMotes[i].DriftX * deltaTime * 12.0f;
                    dustMotes[i].Y += dustMotes[i].DriftY * deltaTime * 12.0f;

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

            // 1. Render Procedural 3D Nebula Gas Clouds (in deep background)
            if (EnableNebula && nebulae != null)
            {
                RenderNebulae(g);
            }

            // 2. Render Interstellar Dust Particles
            if (EnableCosmicDust && dustMotes != null)
            {
                RenderCosmicDust(g);
            }

            // 3. Render Stars & Relativistic Motion Streaks
            if (stars != null)
            {
                RenderStars(g);
            }
        }

        private void RenderNebulae(Graphics g)
        {
            for (int i = 0; i < nebulae.Length; i++)
            {
                float z = nebulae[i].Z;
                if (z <= 160.0f) continue;

                float k = fov / z;
                float sx = nebulae[i].X * k + centerX;
                float sy = nebulae[i].Y * k + centerY;
                float sRadius = nebulae[i].Radius * k;

                // Screen bounds cull with generous margin for giant plumes
                if (sx < -sRadius || sx > width + sRadius || sy < -sRadius || sy > height + sRadius)
                {
                    continue;
                }

                // Smooth volumetric atmospheric alpha fade curve:
                // Fades in as it approaches from deep space (3400 -> 1800), peak at 1200, smoothly dissolves as it passes camera (<500)
                float alphaFactor = 1.0f;
                if (z > 2200.0f)
                {
                    alphaFactor = (maxNebulaZ - z) / (maxNebulaZ - 2200.0f);
                }
                else if (z < 550.0f)
                {
                    alphaFactor = Math.Max(0f, (z - 160.0f) / 390.0f);
                }

                int currentAlpha = (int)(nebulae[i].MaxAlpha * alphaFactor);
                if (currentAlpha < 2) continue;

                Color c = nebulae[i].BaseColor;
                RectangleF cloudBounds = new RectangleF(sx - sRadius, sy - sRadius, sRadius * 2.0f, sRadius * 2.0f);

                using (GraphicsPath cloudPath = new GraphicsPath())
                {
                    cloudPath.AddEllipse(cloudBounds);
                    using (PathGradientBrush pgb = new PathGradientBrush(cloudPath))
                    {
                        pgb.CenterColor = Color.FromArgb(currentAlpha, c.R, c.G, c.B);
                        pgb.SurroundColors = new Color[] { Color.FromArgb(0, c.R, c.G, c.B) };
                        g.FillPath(pgb, cloudPath);
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
                {
                    if (z < maxZ * 0.3f) ResetDust(ref dustMotes[i], false);
                    continue;
                }

                // Micro-dust visibility curve: most visible when passing within close range (< 900)
                float depthNorm = 1.0f - (z / maxZ);
                int alpha = (int)(depthNorm * 160.0f);
                if (alpha < 6) continue;

                Color col = dustMotes[i].Tint;
                Color dustColor = Color.FromArgb(alpha, col.R, col.G, col.B);

                // Micro-streak for dust passing at warp speed
                float pz = dustMotes[i].PrevZ;
                if (StreakMultiplier > 0.05f && pz > z && z < 700.0f)
                {
                    float pk = fov / pz;
                    float psx = dustMotes[i].X * pk + centerX;
                    float psy = dustMotes[i].Y * pk + centerY;

                    float distSq = (sx - psx) * (sx - psx) + (sy - psy) * (sy - psy);
                    if (distSq > 1.0f)
                    {
                        using (Pen dPen = new Pen(dustColor, 0.75f))
                        {
                            g.DrawLine(dPen, psx, psy, sx, sy);
                        }
                    }
                }

                float r = dustMotes[i].Size * (0.4f + depthNorm * 0.9f);
                using (SolidBrush dBrush = new SolidBrush(dustColor))
                {
                    g.FillEllipse(dBrush, sx - r, sy - r, r * 2.0f, r * 2.0f);
                }
            }
        }

        private void RenderStars(Graphics g)
        {
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
                    if (z < maxZ * 0.3f)
                    {
                        ResetStar(ref stars[i], false);
                    }
                    continue;
                }

                // Alpha based on depth
                float depthNorm = 1.0f - (z / maxZ);
                depthNorm = Math.Max(0f, Math.Min(1f, depthNorm));

                int alpha = (int)(depthNorm * 255.0f);
                if (alpha < 8) continue;

                Color col = stars[i].BaseColor;
                Color renderColor = Color.FromArgb(alpha, col.R, col.G, col.B);

                // Project previous position for relativistic streak
                float pz = stars[i].PrevZ;
                if (StreakMultiplier > 0.05f && pz > z)
                {
                    float pzEffective = z + (pz - z) * StreakMultiplier;
                    float pk = fov / pzEffective;
                    float psx = stars[i].X * pk + centerX;
                    float psy = stars[i].Y * pk + centerY;

                    float distSq = (sx - psx) * (sx - psx) + (sy - psy) * (sy - psy);

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

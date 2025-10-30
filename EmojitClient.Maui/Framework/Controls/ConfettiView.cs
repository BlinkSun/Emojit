using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace EmojitClient.Maui.Framework.Controls;

/// <summary>
/// A custom view that renders a confetti explosion animation using SkiaSharp.
/// </summary>
public partial class ConfettiView : SKCanvasView
{
    private readonly List<ConfettiParticle> particles = [];
    private readonly Random random = new();

    /// <summary>
    /// Gets or sets the number of confetti particles created during an explosion.
    /// </summary>
    public int ParticlesCount { get; set; } = 200;

    /// <summary>
    /// Gets or sets the explosion force determining the initial particle velocity.
    /// </summary>
    public float ExplosionForce { get; set; } = 12f;

    /// <summary>
    /// Gets or sets the gravity factor affecting particle fall speed.
    /// </summary>
    public float Gravity { get; set; } = 0.4f;

    /// <summary>
    /// Gets or sets the maximum particle lifetime in frames (approx 60 fps).
    /// </summary>
    public float Lifetime { get; set; } = 100f;

    /// <summary>
    /// Gets or sets a value indicating whether the confetti rotates.
    /// </summary>
    public bool EnableRotation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether color randomization is enabled.
    /// </summary>
    public bool EnableRandomColors { get; set; } = true;

    /// <summary>
    /// Gets or sets a list of predefined confetti colors (used if EnableRandomColors is false).
    /// </summary>
    public List<SKColor> ConfettiColors { get; set; } =
        [
            SKColors.Red, SKColors.Blue, SKColors.Yellow, SKColors.Green, SKColors.Purple, SKColors.Orange
        ];

    public ConfettiView()
    {
        // Timer: refresh ~60 FPS
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            InvalidateSurface();
            return true;
        });
    }

    /// <summary>
    /// Triggers a confetti explosion centered in the view.
    /// </summary>
    public void TriggerExplosion()
    {
        particles.Clear();

        float centerX = (float)Width / 2;
        float centerY = (float)Height / 2;

        for (int i = 0; i < ParticlesCount; i++)
        {
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float speed = (float)(random.NextDouble() * ExplosionForce);
            float size = (float)random.NextDouble() * 8f + 4f;

            SKColor color = EnableRandomColors
                ? new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256))
                : ConfettiColors[random.Next(ConfettiColors.Count)];

            particles.Add(new ConfettiParticle
            {
                Position = new SKPoint(centerX, centerY),
                Velocity = new SKPoint((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed),
                Size = size,
                Color = color,
                Lifetime = Lifetime,
                Rotation = (float)(random.NextDouble() * 360),
                RotationSpeed = (float)(random.NextDouble() * 10 - 5)
            });
        }
    }

    /// <summary>
    /// Handles the rendering of particles.
    /// </summary>
    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        for (int i = particles.Count - 1; i >= 0; i--)
        {
            ConfettiParticle p = particles[i];

            // Update particle physics
            p.Position = new SKPoint(p.Position.X + p.Velocity.X, p.Position.Y + p.Velocity.Y);
            p.Velocity = new SKPoint(p.Velocity.X, p.Velocity.Y + Gravity);
            p.Lifetime -= 1f;

            if (EnableRotation)
                p.Rotation += p.RotationSpeed;

            // Fade out
            byte alpha = (byte)(255 * (p.Lifetime / Lifetime));
            if (alpha < 10)
            {
                particles.RemoveAt(i);
                continue;
            }

            using SKPaint paint = new()
            {
                Color = p.Color.WithAlpha(alpha),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            canvas.Save();
            canvas.Translate(p.Position.X, p.Position.Y);
            canvas.RotateDegrees(p.Rotation);

            // Draw a rectangle (you can switch to DrawCircle or custom shapes)
            canvas.DrawRect(-p.Size / 2, -p.Size / 2, p.Size, p.Size, paint);

            canvas.Restore();
        }
    }

    /// <summary>
    /// Represents a single confetti particle.
    /// </summary>
    private class ConfettiParticle
    {
        public SKPoint Position { get; set; }
        public SKPoint Velocity { get; set; }
        public float Size { get; set; }
        public SKColor Color { get; set; }
        public float Lifetime { get; set; }
        public float Rotation { get; set; }
        public float RotationSpeed { get; set; }
    }
}
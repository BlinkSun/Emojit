using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace EmojitClient.Maui.Framework.Controls;

/// <summary>
/// A custom SkiaSharp view rendering rotating light rays from the bottom center of the screen.
/// </summary>
public partial class RadiantRaysView : SKCanvasView
{
    private float rotationAngle;
    private readonly Random random = new();
    private DateTime lastFrameTime = DateTime.Now;

    /// <summary>
    /// Gets or sets the number of rays.
    /// </summary>
    public int RayCount { get; set; } = 12;

    /// <summary>
    /// Gets or sets the ray thickness.
    /// </summary>
    public float RayThickness { get; set; } = 50f;

    /// <summary>
    /// Gets or sets the rotation speed (degrees per second).
    /// </summary>
    public float RotationSpeed { get; set; } = 25f;

    /// <summary>
    /// Gets or sets the ray length multiplier (relative to screen height).
    /// </summary>
    public float RayLengthFactor { get; set; } = 2.0f;

    /// <summary>
    /// Gets or sets the color of the rays.
    /// </summary>
    public SKColor RayColor { get; set; } = new SKColor(255, 255, 200); // Warm yellow-white

    public RadiantRaysView()
    {
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            UpdateRotation();
            InvalidateSurface();
            return true;
        });
    }

    private void UpdateRotation()
    {
        DateTime now = DateTime.Now;
        double delta = (now - lastFrameTime).TotalSeconds;
        lastFrameTime = now;

        rotationAngle += (float)(RotationSpeed * delta);
        if (rotationAngle > 360f)
            rotationAngle -= 360f;
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        float width = e.Info.Width;
        float height = e.Info.Height;
        SKPoint center = new(width / 2, height);

        float radius = height * RayLengthFactor;
        float angleStep = 360f / RayCount;

        using SKPaint paint = new()
        {
            Color = RayColor.WithAlpha((byte)(255 * Opacity)),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        for (int i = 0; i < RayCount; i++)
        {
            float angle = rotationAngle + i * angleStep;
            float rad = angle * (float)Math.PI / 180f;

            float x1 = center.X;
            float y1 = center.Y;

            float x2 = x1 + (float)Math.Cos(rad) * radius;
            float y2 = y1 + (float)Math.Sin(rad) * radius;

            // Draw each ray as a thick triangle sector
            using SKPath path = new();
            path.MoveTo(x1, y1);
            path.LineTo(x2 + RayThickness, y2);
            path.LineTo(x2 - RayThickness, y2);
            path.Close();

            canvas.DrawPath(path, paint);
        }
    }
}
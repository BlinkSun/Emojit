using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace EmojitClient.Maui.Framework.Controls;

/// <summary>
/// Animated emoji background that moves emojis diagonally across the screen.
/// </summary>
public partial class Emoji2BackgroundView : SKCanvasView
{
    private readonly List<EmojiParticle> emojis = [];
    private readonly Random random = new();
    private DateTime lastFrameTime = DateTime.Now;

    public List<string> EmojiSet { get; set; } =
    [
        // 😀 Expressions & visages
        "😂", "🤣", "😊", "😍", "😘", "🥰", "😎", "🤩", "😭", "😉",

        // 🎉 Fête & ambiance
        "🎉", "🎊", "🎈", "✨", "🌟", "🎆", "🎇", "🎵", "🎶", "💫",

        // 💖 Amour & positifs
        "❤️", "💖", "💗", "💓", "💕", "💞", "💘", "💝", "💟", "💌",

        // 🔥 Objets & symboles
        "🔥", "⚡", "💎", "💰", "🏆", "🎯", "⭐", "🌈", "☀️", "🌙",

        // 🍀 Nature & chance
        "🍀", "🌸", "🌻", "🌼", "🍂", "🍁", "🌊", "🌴", "🌹", "🌺",

        // 🐾 Animaux
        "🐶", "🐱", "🐻", "🐼", "🐵", "🦊", "🐸", "🐰", "🦄", "🐝",

        // 🍔 Nourriture & fun
        "🍕", "🍔", "🍟", "🌭", "🍩", "🍦", "🍉", "🍓", "🍿", "☕",

        // ⚙️ Activités & objets modernes
        "🎮", "🕹️", "💻", "📱", "🎧", "🎥", "🚀", "✈️", "⏰", "📸"
    ];

    public int EmojiCount { get; set; } = 50;
    public float MinEmojiSize { get; set; } = 32f;
    public float MaxEmojiSize { get; set; } = 96f;
    public float EmojiOpacity { get; set; } = 0.35f;
    public float EmojiSpeed { get; set; } = 40f;

    // Fixe à 315° (bas-gauche → haut-droit)
    private const float MovementAngle = 315f;

    public Emoji2BackgroundView()
    {
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            UpdateFrame();
            InvalidateSurface();
            return true;
        });
    }

    private void UpdateFrame()
    {
        DateTime now = DateTime.Now;
        double delta = (now - lastFrameTime).TotalSeconds;
        lastFrameTime = now;

        if (emojis.Count == 0 && Width > 0)
            InitializeEmojis();

        //float move = (float)(EmojiSpeed * delta);

        // Mouvement fixe à 315°
        foreach (EmojiParticle emoji in emojis)
        {
            float move = (float)(emoji.Speed * delta);
            float rad = MovementAngle * (float)Math.PI / 180f;
            float dx = (float)Math.Cos(rad) * move;
            float dy = (float)Math.Sin(rad) * move;

            emoji.Position = new SKPoint(emoji.Position.X + dx, emoji.Position.Y + dy);

            // Disparaît hors de l'écran
            if (emoji.Position.X > Width + (emoji.Size * 2) || emoji.Position.Y < -emoji.Size * 2)
            {
                RespawnEmoji(emoji);
            }
        }
    }

    private void InitializeEmojis()
    {
        emojis.Clear();

        for (int i = 0; i < EmojiCount; i++)
        {
            emojis.Add(CreateNewEmoji());
        }
    }

    private EmojiParticle CreateNewEmoji()
    {
        string emoji = EmojiSet[random.Next(EmojiSet.Count)];
        float size = (float)(MinEmojiSize + (random.NextDouble() * (MaxEmojiSize - MinEmojiSize)));
        float spawnX, spawnY;

        // On fait apparaître les emojis uniquement à GAUCHE ou en BAS
        if (random.NextDouble() < 0.5)
        {
            // Spawn à GAUCHE (juste en dehors de l’écran)
            spawnX = -size - (float)(random.NextDouble() * size);
            spawnY = (float)(random.NextDouble() * (Height + (size * 2)));
        }
        else
        {
            // Spawn en BAS (juste en dehors de l’écran)
            spawnX = (float)(random.NextDouble() * (Width + (size * 2)));
            spawnY = (float)(Height + size + (float)(random.NextDouble() * size));
        }

        EmojiParticle particle = new()
        {
            Emoji = emoji,
            Size = size,
            Position = new SKPoint(spawnX, spawnY)
        };

        // Petits = rapides, gros = lents
        float speedScale = MaxEmojiSize / particle.Size;
        particle.Speed = EmojiSpeed * speedScale;

        //float speedScale = (float)Math.Pow(MaxEmojiSize / particle.Size, 0.6);
        //particle.Speed = EmojiSpeed * speedScale;

        return particle;
    }

    private void RespawnEmoji(EmojiParticle emoji)
    {
        // Lorsqu'un emoji quitte l’écran, on le respawn comme un nouveau
        EmojiParticle newEmoji = CreateNewEmoji();
        emoji.Emoji = newEmoji.Emoji;
        emoji.Size = newEmoji.Size;
        emoji.Position = newEmoji.Position;
    }

    private static SKColor ToSKColor(Color mauiColor)
    {
        return new SKColor(
            (byte)(mauiColor.Red * 255),
            (byte)(mauiColor.Green * 255),
            (byte)(mauiColor.Blue * 255),
            (byte)(mauiColor.Alpha * 255)
        );
    }

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        SKCanvas canvas = e.Surface.Canvas;
        canvas.Clear(ToSKColor(BackgroundColor ?? Colors.Transparent));

        using SKPaint textPaint = new()
        {
            IsAntialias = true,
            Color = SKColors.White.WithAlpha((byte)(255 * EmojiOpacity))
        };

        foreach (EmojiParticle emoji in emojis)
        {
            SKFont font = new(SKTypeface.FromFamilyName(
                DeviceInfo.Platform == DevicePlatform.Android ? "Noto Color Emoji" :
                DeviceInfo.Platform == DevicePlatform.iOS ? "Apple Color Emoji" :
                "Segoe UI Emoji"
            ), emoji.Size);

            canvas.DrawText(emoji.Emoji, emoji.Position.X, emoji.Position.Y, SKTextAlign.Center, font, textPaint);
        }
    }

    private class EmojiParticle
    {
        public string Emoji { get; set; } = "⭐";
        public SKPoint Position { get; set; }
        public float Size { get; set; }
        public float Speed { get; set; }
    }
}

/// <summary>
/// Animated emoji background using MAUI Graphics.
/// Works on Android, iOS, Windows — supports colored emoji natively.
/// </summary>
public partial class EmojiBackgroundView : AbsoluteLayout
{
    private readonly List<EmojiParticle> emojis = [];
    private readonly Random random = new();
    private DateTime lastFrameTime = DateTime.Now;
    private bool initialized;

    public List<string> EmojiSet { get; set; } =
    [
        "😂","🤣","😊","😍","😘","🥰","😎","🤩","😭","😉",
        "🎉","🎊","🎈","✨","🌟","🎆","🎇","🎵","🎶","💫",
        "❤️","💖","💗","💓","💕","💞","💘","💝","💟","💌",
        "🔥","⚡","💎","💰","🏆","🎯","🦴","🌈","☀️","🌙",
        "🍀","🌸","🌻","🌼","🍂","🍁","🌊","🌴","🌹","🌺",
        "🐶","🐱","🐻","🐼","🐵","🦊","🐸","🐰","🦄","🐝",
        "🍕","🍔","🍟","🌭","🍩","🍦","🍉","🍓","🍿","☕",
        "🎮","🕹️","💻","📱","🎧","🎥","🚀","✈️","⏰","📸"
    ];

    public int EmojiCount { get; set; } = 30;
    public float MinScale { get; set; } = 0.5f;
    public float MaxScale { get; set; } = 1.5f;
    public float EmojiOpacity { get; set; } = 0.5f;
    public float EmojiSpeed { get; set; } = 50f;

    private const double MovementAngleDeg = 315.0;
    private readonly double rad = MovementAngleDeg * Math.PI / 180.0;

    // base uniforme pour tous les labels
    private const double BaseSize = 96;
    private const double BaseFontSize = 80;

    public EmojiBackgroundView()
    {
        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.Fill;
        InputTransparent = true;
        IsClippedToBounds = false;

        SizeChanged += OnSizeChanged;

        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(16), () =>
        {
            Tick();
            return true;
        });
    }

    private void OnSizeChanged(object? sender, EventArgs e)
    {
        if (Width <= 0 || Height <= 0)
            return;

        Clip = new RectangleGeometry(new Rect(0, 0, Width, Height));

        if (!initialized)
        {
            InitializeEmojis();
            initialized = true;
        }
    }

    private void InitializeEmojis()
    {
        emojis.Clear();
        Children.Clear();

        for (int i = 0; i < EmojiCount; i++)
        {
            EmojiParticle p = CreateEmoji();
            emojis.Add(p);

            // taille de base fixe pour tous
            double oversize = 1.25; // 25% plus grand autour
            SetLayoutBounds((IView)p.Label, new Rect(0, 0, BaseSize * oversize, BaseSize * oversize));
            //SetLayoutBounds((IView)p.Label, new Rect(0, 0, BaseSize, BaseSize));
            SetLayoutFlags((IView)p.Label, AbsoluteLayoutFlags.None);
            Children.Add(p.Label);

            // position initiale
            p.Label.TranslationX = p.Position.X;
            p.Label.TranslationY = p.Position.Y;
        }

        lastFrameTime = DateTime.Now;
    }

    private EmojiParticle CreateEmoji()
    {
        string emoji = EmojiSet[random.Next(EmojiSet.Count)];

        // échelle visuelle
        double scale = MinScale + (random.NextDouble() * (MaxScale - MinScale));

        // spawn
        double spawnX, spawnY;
        double box = BaseSize * scale;
        if (random.NextDouble() < 0.5)
        {
            spawnX = -box - (random.NextDouble() * box);
            spawnY = random.NextDouble() * (Height + (box * 2));
        }
        else
        {
            spawnX = random.NextDouble() * (Width + (box * 2));
            spawnY = Height + box + (random.NextDouble() * box);
        }

        // petits = rapides
        double speed = EmojiSpeed * (1 / scale);
        double opacity = EmojiOpacity;// * (0.5 + (0.5 * scale));

        Label label = new()
        {
            Text = emoji,
            //FontSize = BaseFontSize,
            FontSize = BaseFontSize * 0.85,
            LineHeight = 1.0,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center,
            InputTransparent = true,
            Padding = 0,
            Margin = 0,
            Opacity = opacity,
            Scale = scale
        };
        label.SetValue(Label.FontAutoScalingEnabledProperty, false);

        return new EmojiParticle
        {
            Emoji = emoji,
            Label = label,
            Size = BaseSize,
            Speed = speed,
            Scale = scale,
            Position = new Point(spawnX, spawnY)
        };
    }

    private void Respawn(EmojiParticle p)
    {
        EmojiParticle n = CreateEmoji();

        p.Emoji = n.Emoji;
        p.Label.Text = p.Emoji;
        p.Scale = n.Scale;
        p.Label.Scale = n.Scale;
        p.Speed = n.Speed;

        p.Position = n.Position;
        p.Label.TranslationX = p.Position.X;
        p.Label.TranslationY = p.Position.Y;
    }

    private void Tick()
    {
        if (!initialized)
            return;

        DateTime now = DateTime.Now;
        double dt = (now - lastFrameTime).TotalSeconds;
        lastFrameTime = now;

        foreach (EmojiParticle p in emojis)
        {
            p.Position = new Point(
                p.Position.X + (Math.Cos(rad) * p.Speed * dt),
                p.Position.Y + (Math.Sin(rad) * p.Speed * dt)
            );

            p.Label.TranslationX = p.Position.X;
            p.Label.TranslationY = p.Position.Y;

            if (p.Position.X > Width + (BaseSize * p.Scale * 2) ||
                p.Position.Y < -BaseSize * p.Scale * 2)
            {
                Respawn(p);
            }
        }
    }

    private sealed class EmojiParticle
    {
        public string Emoji { get; set; } = "⭐";
        public Point Position { get; set; }
        public double Size { get; set; }
        public double Speed { get; set; }
        public double Scale { get; set; }
        public Label Label { get; set; } = default!;
    }
}

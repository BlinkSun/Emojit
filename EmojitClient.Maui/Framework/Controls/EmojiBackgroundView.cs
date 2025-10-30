using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace EmojitClient.Maui.Framework.Controls;

/// <summary>
/// Animated emoji background that moves emojis diagonally across the screen.
/// </summary>
public partial class EmojiBackgroundView : SKCanvasView
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

    public EmojiBackgroundView()
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
            if (emoji.Position.X > Width + emoji.Size * 2 || emoji.Position.Y < -emoji.Size * 2)
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
        float size = (float)(MinEmojiSize + random.NextDouble() * (MaxEmojiSize - MinEmojiSize));
        float spawnX, spawnY;

        // On fait apparaître les emojis uniquement à GAUCHE ou en BAS
        if (random.NextDouble() < 0.5)
        {
            // Spawn à GAUCHE (juste en dehors de l’écran)
            spawnX = -size - (float)(random.NextDouble() * size);
            spawnY = (float)(random.NextDouble() * (Height + size * 2));
        }
        else
        {
            // Spawn en BAS (juste en dehors de l’écran)
            spawnX = (float)(random.NextDouble() * (Width + size * 2));
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
        canvas.Clear(ToSKColor(BackgroundColor));

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
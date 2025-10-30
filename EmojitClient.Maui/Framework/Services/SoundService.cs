using System.Collections.Concurrent;
using System.Diagnostics;

#if ANDROID
using Android.Media;
using Android.Content;
using Android.Content.Res;
#endif

#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;
#endif

namespace EmojitClient.Maui.Framework.Services;

/// <summary>
/// Cross-platform unified sound service for background music (BGM) and sound effects (SFX).
/// - Android: BGM via MediaPlayer (looped), SFX via SoundPool.
/// - Windows: BGM and SFX via MediaPlayer (in-memory streams).
/// - Uses MauiAsset (no Resource.Raw prefix required).
/// - Thread-safe, persistent toggles, dependency-injection friendly.
/// </summary>
public sealed partial class SoundService : ISoundService, IDisposable
{
    private readonly Lock playerLock = new();

    private const string PrefKeyMusicEnabled = "SoundService.MusicEnabled";
    private const string PrefKeySfxEnabled = "SoundService.SfxEnabled";

    private readonly Dictionary<string, string> bgmMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "menu",    "menu.mp3" },
        { "battle",  "battle.mp3" },
        { "victory", "victory.mp3" }
    };

    private readonly Dictionary<string, string> sfxMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "click",   "click.wav" },
        { "success", "success.wav" },
        { "error",   "error.wav" }
    };

    private double sfxVolume = 1.0;
    private double bgmVolume = 0.7;
    private double lastSfxVolume = 1.0;
    private double lastBgmVolume = 0.7;

    public bool IsMusicEnabled { get; private set; } = true;
    public bool IsSfxEnabled { get; private set; } = true;

#if ANDROID
    private readonly ConcurrentDictionary<string, int> sfxSoundIds = new();
    private SoundPool? soundPool;
    private MediaPlayer? bgmPlayer;
    private Context? androidContext;
#endif

#if WINDOWS
    private readonly ConcurrentDictionary<string, byte[]> bgmBytes = new();
    private readonly ConcurrentDictionary<string, byte[]> sfxBytes = new();
    private readonly ConcurrentDictionary<string, MediaPlayer> sfxPlayers = new();
    private MediaPlayer? bgmPlayer;
#endif

    private bool isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="SoundService"/> class.
    /// </summary>
    public SoundService() { }

    /// <summary>
    /// Gets or sets the current SFX volume (0.0 to 1.0).
    /// </summary>
    public double SfxVolume
    {
        get => sfxVolume;
        set
        {
            double clamped = Math.Clamp(value, 0.0, 1.0);
            sfxVolume = clamped;
#if WINDOWS
            foreach (KeyValuePair<string, MediaPlayer> pair in sfxPlayers)
            {
                try { pair.Value.Volume = clamped; } catch { }
            }
#endif
        }
    }

    /// <summary>
    /// Gets or sets the current BGM volume (0.0 to 1.0).
    /// </summary>
    public double BgmVolume
    {
        get => bgmVolume;
        set
        {
            double clamped = Math.Clamp(value, 0.0, 1.0);
            bgmVolume = clamped;
#if ANDROID
            try { bgmPlayer?.SetVolume((float)clamped, (float)clamped); } catch { }
#elif WINDOWS
            if (bgmPlayer != null) bgmPlayer.Volume = clamped;
#endif
        }
    }

    /// <summary>
    /// Initializes the sound service (loads assets and restores preferences).
    /// </summary>
    public async Task InitializeAsync()
    {
        if (isInitialized) return;

        try
        {
            IsMusicEnabled = Preferences.Get(PrefKeyMusicEnabled, true);
            IsSfxEnabled = Preferences.Get(PrefKeySfxEnabled, true);

#if ANDROID
            androidContext = Android.App.Application.Context;

            AudioAttributes? audioAttrs = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Game)?
                .SetContentType(AudioContentType.Sonification)?
                .Build();

            SoundPool.Builder builder = new();
            builder.SetMaxStreams(8);
            builder.SetAudioAttributes(audioAttrs);
            soundPool = builder.Build();

            foreach (KeyValuePair<string, string> kv in sfxMap)
            {
                try
                {
                    using AssetFileDescriptor? afd = androidContext?.Assets?.OpenFd(kv.Value);
                    int soundId = soundPool?.Load(afd, 1) ?? 0;
                    sfxSoundIds[kv.Key] = soundId;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SoundService] Failed to load SFX '{kv.Value}': {ex.Message}");
                }
            }

            await Task.CompletedTask;
#endif

#if WINDOWS
            foreach (KeyValuePair<string, string> kv in bgmMap)
            {
                bgmBytes[kv.Key] = await ReadPackagedRawAsync(kv.Value);
            }

            foreach (KeyValuePair<string, string> kv in sfxMap)
            {
                byte[] data = await ReadPackagedRawAsync(kv.Value);
                sfxBytes[kv.Key] = data;

                MediaPlayer player = new()
                {
                    Source = MediaSource.CreateFromStream(BytesToRandomAccessStream(data), GetContentType(kv.Value)),
                    Volume = sfxVolume
                };
                sfxPlayers[kv.Key] = player;
            }
#endif

            isInitialized = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SoundService] Initialization failed: {ex.Message}");
        }
    }


    /// <summary>
    /// Plays a background music track by name.
    /// </summary>
    public void PlayBgm(string name)
    {
        if (!isInitialized || !IsMusicEnabled) return;
        if (!bgmMap.TryGetValue(name, out string? fileName)) return;

        lock (playerLock)
        {
#if ANDROID
            StopBgmInternal();
            try
            {
                bgmPlayer = new MediaPlayer();

                AudioAttributes? bgmAttrs = new AudioAttributes.Builder()?
                    .SetUsage(AudioUsageKind.Media)?
                    .SetContentType(AudioContentType.Music)?
                    .Build();
                bgmPlayer.SetAudioAttributes(bgmAttrs);

                using AssetFileDescriptor? afd = androidContext?.Assets?.OpenFd(fileName);
                bgmPlayer.SetDataSource(afd?.FileDescriptor, afd?.StartOffset ?? 0, afd?.Length ?? 0);
                bgmPlayer.Looping = true;
                bgmPlayer.SetVolume((float)bgmVolume, (float)bgmVolume);
                bgmPlayer.Prepare();
                bgmPlayer.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SoundService] PlayBgm error: {ex.Message}");
                StopBgmInternal();
            }
#elif WINDOWS
            StopBgmInternal();
            if (!bgmBytes.TryGetValue(name, out byte[]? data)) return;

            bgmPlayer = new MediaPlayer
            {
                IsLoopingEnabled = true,
                Volume = bgmVolume,
                Source = MediaSource.CreateFromStream(BytesToRandomAccessStream(data), GetContentType(fileName))
            };
            bgmPlayer.Play();
#endif
        }
    }

    /// <summary>
    /// Stops currently playing background music.
    /// </summary>
    public void StopBgm()
    {
        if (!isInitialized) return;
        lock (playerLock) StopBgmInternal();
    }

    /// <summary>
    /// Plays a sound effect by name.
    /// </summary>
    public void PlaySfx(string name)
    {
        if (!(isInitialized && IsSfxEnabled && sfxMap.ContainsKey(name))) return;
#if ANDROID
        if (soundPool != null && sfxSoundIds.TryGetValue(name, out int id))
        {
            float vol = (float)Math.Clamp(sfxVolume, 0.0, 1.0);
            soundPool.Play(id, vol, vol, 1, 0, 1.0f);
        }
#elif WINDOWS
        if (!sfxPlayers.TryGetValue(name, out MediaPlayer? player))
        {
            if (!sfxBytes.TryGetValue(name, out byte[]? data)) return;
            player = new MediaPlayer
            {
                Source = MediaSource.CreateFromStream(BytesToRandomAccessStream(data), GetContentType(sfxMap[name])),
                Volume = sfxVolume
            };
            sfxPlayers[name] = player;
        }

        player.Pause();
        player.PlaybackSession.Position = TimeSpan.Zero;
        player.Volume = sfxVolume;
        player.Play();
#endif
    }

    /// <summary>Sets both BGM and SFX volumes.</summary>
    public void SetVolumes(double sfx, double bgm)
    {
        SfxVolume = sfx;
        BgmVolume = bgm;
    }

    /// <summary>Mutes both BGM and SFX.</summary>
    public void MuteAll()
    {
        lastSfxVolume = sfxVolume;
        lastBgmVolume = bgmVolume;
        SfxVolume = 0;
        BgmVolume = 0;
    }

    /// <summary>Restores last BGM and SFX volumes before mute.</summary>
    public void RestoreVolumes()
    {
        SfxVolume = lastSfxVolume;
        BgmVolume = lastBgmVolume;
    }

    /// <summary>Enables or disables background music playback.</summary>
    public void ToggleMusic(bool enabled)
    {
        IsMusicEnabled = enabled;
        Preferences.Set(PrefKeyMusicEnabled, enabled);
        if (!enabled) StopBgm();
    }

    /// <summary>Enables or disables sound effects playback.</summary>
    public void ToggleSfx(bool enabled)
    {
        IsSfxEnabled = enabled;
        Preferences.Set(PrefKeySfxEnabled, enabled);
    }

    /// <summary>Releases resources and stops all playback.</summary>
    public void Dispose()
    {
#if ANDROID
        StopBgmInternal();
        soundPool?.Release();
        soundPool?.Dispose();
#elif WINDOWS
        StopBgmInternal();
        foreach (KeyValuePair<string, MediaPlayer> kv in sfxPlayers)
        {
            kv.Value.Dispose();
        }
        sfxPlayers.Clear();
#endif
    }

#if WINDOWS
    private static async Task<byte[]> ReadPackagedRawAsync(string fileName)
    {
        using Stream stream = await FileSystem.OpenAppPackageFileAsync(fileName);
        using MemoryStream ms = new();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static InMemoryRandomAccessStream BytesToRandomAccessStream(byte[] bytes)
    {
        InMemoryRandomAccessStream ras = new();
        DataWriter writer = new(ras);
        writer.WriteBytes(bytes);
        writer.StoreAsync().AsTask().Wait();
        ras.Seek(0);
        return ras;
    }

    private static string GetContentType(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/mp4",
            _ => "audio/mpeg"
        };
    }
#endif

    private void StopBgmInternal()
    {
#if ANDROID
        if (bgmPlayer != null)
        {
            try
            {
                bgmPlayer.Stop();
                bgmPlayer.Release();
                bgmPlayer.Dispose();
            }
            catch { }
            finally { bgmPlayer = null; }
        }
#elif WINDOWS
        if (bgmPlayer != null)
        {
            try
            {
                bgmPlayer.Pause();
                bgmPlayer.Dispose();
            }
            catch { }
            finally { bgmPlayer = null; }
        }
#endif
    }
}

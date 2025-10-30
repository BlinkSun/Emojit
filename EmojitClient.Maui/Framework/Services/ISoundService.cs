namespace EmojitClient.Maui.Framework.Services;

/// <summary>
/// Contract for a unified audio service supporting both background music (BGM)
/// and sound effects (SFX), with persistent toggles and preload support.
/// </summary>
public interface ISoundService : IDisposable
{
    /// <summary>Initializes audio backends and preloads configured BGM and SFX.</summary>
    Task InitializeAsync();

    /// <summary>Plays a background music track (looped until stopped).</summary>
    void PlayBgm(string name);

    /// <summary>Stops any currently playing background music.</summary>
    void StopBgm();

    /// <summary>Plays a sound effect by logical name.</summary>
    void PlaySfx(string name);

    /// <summary>Sets both SFX and BGM volumes (each clamped to [0..1]).</summary>
    void SetVolumes(double sfx, double bgm);

    /// <summary>Temporarily mutes both BGM and SFX (non-persistent).</summary>
    void MuteAll();

    /// <summary>Restores volumes after a previous <see cref="MuteAll"/>.</summary>
    void RestoreVolumes();

    /// <summary>Enables or disables background music, persisted immediately.</summary>
    void ToggleMusic(bool enabled);

    /// <summary>Enables or disables sound effects, persisted immediately.</summary>
    void ToggleSfx(bool enabled);

    /// <summary>Indicates whether music is currently enabled (persisted).</summary>
    bool IsMusicEnabled { get; }

    /// <summary>Indicates whether sound effects are currently enabled (persisted).</summary>
    bool IsSfxEnabled { get; }

    /// <summary>Gets or sets the global BGM volume (0..1).</summary>
    double BgmVolume { get; set; }

    /// <summary>Gets or sets the global SFX volume (0..1).</summary>
    double SfxVolume { get; set; }
}

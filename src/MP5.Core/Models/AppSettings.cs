namespace MP5.Core.Models;

/// <summary>
/// All application settings with sensible defaults.
/// </summary>
public class AppSettings
{
    /// <summary>Default accent color (Purple in ARGB)</summary>
    public string AccentColorHex { get; set; } = "#9B59B6";
    
    /// <summary>Player position - top (psychopath mode) or bottom (normal)</summary>
    public PlayerPosition PlayerPosition { get; set; } = PlayerPosition.Top;
    
    /// <summary>Prevent audio ducking when other apps make sounds</summary>
    public bool PreventAudioDucking { get; set; } = true;
    
    /// <summary>Enable Discord Rich Presence</summary>
    public bool EnableDiscordRpc { get; set; } = true;
    
    /// <summary>Enable Last.fm scrobbling</summary>
    public bool EnableLastFmScrobbling { get; set; }
    
    /// <summary>Enable ListenBrainz scrobbling</summary>
    public bool EnableListenBrainz { get; set; }
    
    /// <summary>Volume boost multiplier (1.0 = normal, up to 2.0 = boosted)</summary>
    public double VolumeBoostMultiplier { get; set; } = 1.0;
    
    /// <summary>Enable Google account sync</summary>
    public bool GoogleSyncEnabled { get; set; }

    public string? LastFmUsername { get; set; }
    
    /// <summary>Last.fm session key</summary>
    public string? LastFmSessionKey { get; set; }
    
    /// <summary>ListenBrainz user token</summary>
    public string? ListenBrainzToken { get; set; }
    
    /// <summary>Current volume level (0.0 to 1.0)</summary>
    public double Volume { get; set; } = 0.7;
    
    /// <summary>Enable shuffle mode</summary>
    public bool ShuffleEnabled { get; set; }
    
    /// <summary>Repeat mode</summary>
    public RepeatMode RepeatMode { get; set; } = RepeatMode.None;

    /// <summary>Enable AdBlock and content filtering</summary>
    public bool AdBlockEnabled { get; set; } = true;

    /// <summary>Launch app on Windows startup</summary>
    public bool IsStartupEnabled { get; set; }

    /// <summary>Launch in fullscreen mode</summary>
    public bool IsFullscreenEnabled { get; set; }
}

/// <summary>
/// Player bar position preference.
/// </summary>
public enum PlayerPosition
{
    /// <summary>Player at top - "psychopath mode" (default)</summary>
    Top,
    /// <summary>Player at bottom - traditional layout</summary>
    Bottom
}

/// <summary>
/// Repeat mode for playback.
/// </summary>
public enum RepeatMode
{
    None,
    All,
    One
}

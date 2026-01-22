namespace MP5.Core.Models;

/// <summary>
/// Represents a playlist containing tracks.
/// </summary>
public class Playlist
{
    public required string Id { get; init; }
    public required string Name { get; set; }
    public PlaylistType Type { get; init; } = PlaylistType.Default;
    public List<Track> Tracks { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsOfflineAvailable { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Type of playlist - determines behavior and sync options.
/// </summary>
public enum PlaylistType
{
    /// <summary>Default local playlist</summary>
    Default,
    /// <summary>YouTube Music playlist (synced with YT)</summary>
    YouTube
}

namespace MP5.Core.Models;

/// <summary>
/// Represents a music track from any source.
/// </summary>
public record Track
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Artist { get; init; }
    public string? Album { get; init; }
    public TimeSpan Duration { get; init; }
    public string? ThumbnailUrl { get; init; }
    public MusicSourceType Source { get; init; }
    public required string SourceId { get; init; }
    public bool IsOfflineAvailable { get; init; }
    public string? LocalFilePath { get; init; }
}

/// <summary>
/// Identifies the source of a music track.
/// </summary>
public enum MusicSourceType
{
    Local,
    YouTube,
    YouTubeMusic,
    SoundCloud,
    Fallback
}

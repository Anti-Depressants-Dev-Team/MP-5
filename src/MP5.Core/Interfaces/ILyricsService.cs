using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Lyrics aggregation service supporting multiple providers.
/// </summary>
public interface ILyricsService
{
    /// <summary>Get lyrics for a track</summary>
    Task<LyricsResult?> GetLyricsAsync(Track track);
    
    /// <summary>Search for lyrics by title and artist</summary>
    Task<LyricsResult?> SearchLyricsAsync(string title, string artist);
}

/// <summary>
/// Lyrics search result.
/// </summary>
public class LyricsResult
{
    public required string Lyrics { get; init; }
    public string? Source { get; init; }
    public bool IsSynced { get; init; }
    public List<SyncedLyricLine>? SyncedLines { get; init; }
}

/// <summary>
/// A synced lyric line with timestamp.
/// </summary>
public class SyncedLyricLine
{
    public TimeSpan Time { get; init; }
    public required string Text { get; init; }
}

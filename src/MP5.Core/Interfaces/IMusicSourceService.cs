using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Unified service to manage music sources and resolve streams with fallback.
/// </summary>
public interface IMusicSourceService
{
    /// <summary>
    /// aggregated search across all sources (or primary source).
    /// </summary>
    Task<IEnumerable<Track>> SearchAsync(string query);

    /// <summary>
    /// Resolves the audio stream URL for a given track, using fallback sources if necessary.
    /// </summary>
    Task<string> GetStreamUrlAsync(Track track);

    /// <summary>
    /// Gets recommendations based on a seed track.
    /// </summary>
    Task<IEnumerable<Track>> GetRecommendationsAsync(Track seedTrack);
}

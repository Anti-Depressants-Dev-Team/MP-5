using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Interface for a music source (e.g. YouTube, SoundCloud, Local).
/// </summary>
public interface IMusicSource
{
    /// <summary>
    /// Gets the unique name of this source.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Search for tracks.
    /// </summary>
    Task<IEnumerable<Track>> SearchAsync(string query);

    /// <summary>
    /// Resolves the audio stream URL for a given track ID.
    /// </summary>
    /// <param name="trackId">The source-specific track ID (e.g. YouTube Video ID)</param>
    /// <returns>A playable URL or file path.</returns>
    Task<string> GetAudioStreamUrlAsync(string trackId);
}

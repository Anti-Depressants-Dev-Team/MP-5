using MP5.Core.Models;

namespace MP5.Core.Interfaces;

public interface IOfflineService
{
    Task InitializeAsync();
    
    /// <summary>
    /// Checks if a track is available offline.
    /// </summary>
    bool IsTrackDownloaded(Track track);
    
    /// <summary>
    /// Gets the local file path for a track if downloaded.
    /// </summary>
    string? GetOfflinePath(Track track);
    
    /// <summary>
    /// Downloads a track from the provided stream URL.
    /// </summary>
    Task DownloadTrackAsync(Track track, string streamUrl, IProgress<double>? progress = null);
    
    /// <summary>
    /// Removes a downloaded track.
    /// </summary>
    Task RemoveTrackAsync(Track track);
}

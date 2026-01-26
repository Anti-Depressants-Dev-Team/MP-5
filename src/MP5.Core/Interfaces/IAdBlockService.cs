using MP5.Core.Models;

namespace MP5.Core.Interfaces;

public interface IAdBlockService
{
    /// <summary>
    /// Checks if the track appears to be an ad or promotional content.
    /// </summary>
    bool IsAd(Track track);
    
    /// <summary>
    /// Filters a list of tracks to remove ads.
    /// </summary>
    IEnumerable<Track> RemoveAds(IEnumerable<Track> tracks);
    
    /// <summary>
    /// Enable or disable ad blocking.
    /// </summary>
    bool IsEnabled { get; set; }
}

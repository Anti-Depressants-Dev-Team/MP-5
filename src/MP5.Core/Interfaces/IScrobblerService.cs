using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Scrobbling service for Last.fm and ListenBrainz.
/// </summary>
public interface IScrobblerService
{
    /// <summary>Whether the service is authenticated</summary>
    bool IsAuthenticated { get; }
    
    /// <summary>Authenticate with credentials</summary>
    Task<bool> AuthenticateAsync(string username, string password);
    
    /// <summary>Authenticate with existing session key</summary>
    Task<bool> AuthenticateWithSessionAsync(string sessionKey);
    
    /// <summary>Log out</summary>
    Task LogoutAsync();
    
    /// <summary>Update "now playing" status</summary>
    Task UpdateNowPlayingAsync(Track track);
    
    /// <summary>Scrobble a track (after listening threshold)</summary>
    Task ScrobbleAsync(Track track, DateTime timestamp);
    
    /// <summary>Love a track</summary>
    Task LoveTrackAsync(Track track);
    
    /// <summary>Unlove a track</summary>
    Task UnloveTrackAsync(Track track);
}

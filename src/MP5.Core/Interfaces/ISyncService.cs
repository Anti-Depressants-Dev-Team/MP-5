using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Google account sync service for cross-device synchronization.
/// </summary>
public interface ISyncService
{
    /// <summary>Whether user is signed in</summary>
    bool IsSignedIn { get; }
    
    /// <summary>User's email address</summary>
    string? UserEmail { get; }
    
    /// <summary>Sign in with Google OAuth</summary>
    Task<bool> SignInAsync();
    
    /// <summary>Sign out</summary>
    Task SignOutAsync();
    
    /// <summary>Sync playlists to cloud</summary>
    Task SyncPlaylistsAsync(IEnumerable<Playlist> playlists);
    
    /// <summary>Get playlists from cloud</summary>
    Task<IEnumerable<Playlist>> GetCloudPlaylistsAsync();
    
    /// <summary>Sync settings to cloud</summary>
    Task SyncSettingsAsync(AppSettings settings);
    
    /// <summary>Get settings from cloud</summary>
    Task<AppSettings?> GetCloudSettingsAsync();
    
    /// <summary>Last sync timestamp</summary>
    DateTime? LastSyncTime { get; }
}

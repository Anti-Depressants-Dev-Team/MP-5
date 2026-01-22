using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Discord Rich Presence service.
/// </summary>
public interface IDiscordRpcService
{
    /// <summary>Whether Discord RPC is connected</summary>
    bool IsConnected { get; }
    
    /// <summary>Initialize and connect to Discord</summary>
    Task InitializeAsync(string clientId);
    
    /// <summary>Update presence with currently playing track</summary>
    Task UpdatePresenceAsync(Track track, bool isPlaying, TimeSpan position);
    
    /// <summary>Clear presence (when stopped)</summary>
    Task ClearPresenceAsync();
    
    /// <summary>Disconnect from Discord</summary>
    Task DisconnectAsync();
}

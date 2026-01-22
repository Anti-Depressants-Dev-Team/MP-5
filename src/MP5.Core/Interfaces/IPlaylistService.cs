using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Playlist management service with offline and sync support.
/// </summary>
public interface IPlaylistService
{
    /// <summary>Get all playlists</summary>
    Task<IEnumerable<Playlist>> GetAllPlaylistsAsync();
    
    /// <summary>Get playlist by ID</summary>
    Task<Playlist?> GetPlaylistAsync(string id);
    
    /// <summary>Create a new playlist</summary>
    Task<Playlist> CreatePlaylistAsync(string name, PlaylistType type = PlaylistType.Default);
    
    /// <summary>Update playlist details</summary>
    Task UpdatePlaylistAsync(Playlist playlist);
    
    /// <summary>Delete a playlist</summary>
    Task DeletePlaylistAsync(string id);
    
    /// <summary>Add track to playlist</summary>
    Task AddTrackAsync(string playlistId, Track track);
    
    /// <summary>Remove track from playlist</summary>
    Task RemoveTrackAsync(string playlistId, string trackId);
    
    /// <summary>Reorder tracks in playlist</summary>
    Task ReorderTracksAsync(string playlistId, int fromIndex, int toIndex);
    
    /// <summary>Export playlist to file</summary>
    Task<string> ExportPlaylistAsync(string playlistId, string filePath);
    
    /// <summary>Import playlist from file</summary>
    Task<Playlist> ImportPlaylistAsync(string filePath);
    
    /// <summary>Make playlist available offline</summary>
    Task SetOfflineAvailableAsync(string playlistId, bool available);
}

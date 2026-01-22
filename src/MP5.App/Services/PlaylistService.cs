using MP5.Core.Interfaces;
using MP5.Core.Models;
using System.Text.Json;

namespace MP5.App.Services;

/// <summary>
/// Playlist service using local file storage for persistence.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private readonly string _playlistsFolder;
    private List<Playlist>? _playlists;
    
    public PlaylistService()
    {
        _playlistsFolder = Path.Combine(FileSystem.AppDataDirectory, "playlists");
        Directory.CreateDirectory(_playlistsFolder);
    }
    
    public async Task<IEnumerable<Playlist>> GetAllPlaylistsAsync()
    {
        if (_playlists != null)
            return _playlists;
        
        _playlists = [];
        
        var files = Directory.GetFiles(_playlistsFolder, "*.json");
        foreach (var file in files)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var playlist = JsonSerializer.Deserialize<Playlist>(json);
                if (playlist != null)
                {
                    _playlists.Add(playlist);
                }
            }
            catch
            {
                // Skip corrupted files
            }
        }
        
        return _playlists;
    }
    
    public async Task<Playlist?> GetPlaylistAsync(string id)
    {
        var playlists = await GetAllPlaylistsAsync();
        return playlists.FirstOrDefault(p => p.Id == id);
    }
    
    public async Task<Playlist> CreatePlaylistAsync(string name, PlaylistType type = PlaylistType.Default)
    {
        var playlist = new Playlist
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Type = type,
            Tracks = [],
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        
        await SavePlaylistAsync(playlist);
        _playlists?.Add(playlist);
        
        return playlist;
    }
    
    public async Task UpdatePlaylistAsync(Playlist playlist)
    {
        playlist.ModifiedAt = DateTime.UtcNow;
        await SavePlaylistAsync(playlist);
        
        if (_playlists != null)
        {
            var index = _playlists.FindIndex(p => p.Id == playlist.Id);
            if (index >= 0)
            {
                _playlists[index] = playlist;
            }
        }
    }
    
    public Task DeletePlaylistAsync(string id)
    {
        var filePath = GetPlaylistPath(id);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        _playlists?.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
    
    public async Task AddTrackAsync(string playlistId, Track track)
    {
        var playlist = await GetPlaylistAsync(playlistId);
        if (playlist != null)
        {
            playlist.Tracks.Add(track);
            await UpdatePlaylistAsync(playlist);
        }
    }
    
    public async Task RemoveTrackAsync(string playlistId, string trackId)
    {
        var playlist = await GetPlaylistAsync(playlistId);
        if (playlist != null)
        {
            playlist.Tracks.RemoveAll(t => t.Id == trackId);
            await UpdatePlaylistAsync(playlist);
        }
    }
    
    public async Task ReorderTracksAsync(string playlistId, int fromIndex, int toIndex)
    {
        var playlist = await GetPlaylistAsync(playlistId);
        if (playlist != null && fromIndex >= 0 && fromIndex < playlist.Tracks.Count 
            && toIndex >= 0 && toIndex < playlist.Tracks.Count)
        {
            var track = playlist.Tracks[fromIndex];
            playlist.Tracks.RemoveAt(fromIndex);
            playlist.Tracks.Insert(toIndex, track);
            await UpdatePlaylistAsync(playlist);
        }
    }
    
    public async Task<string> ExportPlaylistAsync(string playlistId, string filePath)
    {
        var playlist = await GetPlaylistAsync(playlistId);
        if (playlist == null)
            throw new InvalidOperationException($"Playlist {playlistId} not found");
        
        var json = JsonSerializer.Serialize(playlist, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        await File.WriteAllTextAsync(filePath, json);
        return filePath;
    }
    
    public async Task<Playlist> ImportPlaylistAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var playlist = JsonSerializer.Deserialize<Playlist>(json) 
            ?? throw new InvalidOperationException("Invalid playlist file");
        
        // Generate new ID to avoid conflicts
        var importedPlaylist = new Playlist
        {
            Id = Guid.NewGuid().ToString(),
            Name = playlist.Name + " (Imported)",
            Type = playlist.Type,
            Tracks = playlist.Tracks,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        
        await SavePlaylistAsync(importedPlaylist);
        _playlists?.Add(importedPlaylist);
        
        return importedPlaylist;
    }
    
    public async Task SetOfflineAvailableAsync(string playlistId, bool available)
    {
        var playlist = await GetPlaylistAsync(playlistId);
        if (playlist != null)
        {
            playlist.IsOfflineAvailable = available;
            await UpdatePlaylistAsync(playlist);
            
            // TODO: Download/remove tracks for offline use
        }
    }
    
    private async Task SavePlaylistAsync(Playlist playlist)
    {
        var json = JsonSerializer.Serialize(playlist);
        var filePath = GetPlaylistPath(playlist.Id);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    private string GetPlaylistPath(string id) => Path.Combine(_playlistsFolder, $"{id}.json");
}

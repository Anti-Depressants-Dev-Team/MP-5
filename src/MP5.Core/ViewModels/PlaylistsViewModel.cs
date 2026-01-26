using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.ViewModels;

/// <summary>
/// ViewModel for playlist management.
/// </summary>
public partial class PlaylistsViewModel : ObservableObject
{
    private readonly IPlaylistService _playlistService;
    private readonly IMusicPlayerService _playerService;
    
    [ObservableProperty]
    private List<Playlist> _playlists = [];
    
    [ObservableProperty]
    private Playlist? _selectedPlaylist;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isEditing;
    
    [ObservableProperty]
    private string _newPlaylistName = string.Empty;
    
    [ObservableProperty]
    private PlaylistType _newPlaylistType = PlaylistType.Default;
    
    private readonly IFilePickerService _filePickerService;
    private readonly IPromptService _promptService;
    
    public PlaylistsViewModel(IPlaylistService playlistService, IMusicPlayerService playerService, IFilePickerService filePickerService, IPromptService promptService)
    {
        _playlistService = playlistService;
        _playerService = playerService;
        _filePickerService = filePickerService;
        _promptService = promptService;
    }
    
    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var playlists = await _playlistService.GetAllPlaylistsAsync();
            Playlists = playlists.ToList();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task CreatePlaylistAsync()
    {
        var name = await _promptService.DisplayPromptAsync("New Playlist", "Enter playlist name:");
        if (string.IsNullOrWhiteSpace(name))
            return;
            
        var playlist = await _playlistService.CreatePlaylistAsync(name, NewPlaylistType);
        
        // Refresh list
        var playlists = await _playlistService.GetAllPlaylistsAsync();
        Playlists = playlists.ToList();
    }
    
    [RelayCommand]
    private async Task DeletePlaylistAsync(Playlist playlist)
    {
        await _playlistService.DeletePlaylistAsync(playlist.Id);
        Playlists = Playlists.Where(p => p.Id != playlist.Id).ToList();
    }
    
    [RelayCommand]
    private void SelectPlaylist(Playlist playlist)
    {
        SelectedPlaylist = playlist;
    }
    
    [RelayCommand]
    private void ClosePlaylist()
    {
        SelectedPlaylist = null;
    }
    
    [RelayCommand]
    private async Task PlayTrackAsync(Track track)
    {
        await _playerService.PlayAsync(track);
    }
    
    [RelayCommand]
    private async Task ExportPlaylistAsync(Playlist playlist)
    {
        try 
        {
            var json = System.Text.Json.JsonSerializer.Serialize(playlist);
            var fileName = $"{playlist.Name}.json";
            await _filePickerService.SaveJsonFileAsync(fileName, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Export Failed: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task ImportPlaylistAsync()
    {
        try
        {
            var json = await _filePickerService.PickJsonFileAsync();
            if (!string.IsNullOrEmpty(json))
            {
                var playlist = System.Text.Json.JsonSerializer.Deserialize<Playlist>(json);
                if (playlist != null)
                {
                    // Assign new ID to avoid conflicts
                    var newPlaylist = await _playlistService.CreatePlaylistAsync(playlist.Name, playlist.Type);
                    
                    // Add tracks
                    foreach (var track in playlist.Tracks)
                    {
                         await _playlistService.AddTrackAsync(newPlaylist.Id, track);
                    }
                    
                    // Refresh
                    await LoadAsync();
                }
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Import Failed: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task ToggleOfflineAsync(Playlist playlist)
    {
        playlist.IsOfflineAvailable = !playlist.IsOfflineAvailable;
        await _playlistService.SetOfflineAvailableAsync(playlist.Id, playlist.IsOfflineAvailable);
    }
}

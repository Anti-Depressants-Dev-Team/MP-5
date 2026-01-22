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
    
    public PlaylistsViewModel(IPlaylistService playlistService, IMusicPlayerService playerService)
    {
        _playlistService = playlistService;
        _playerService = playerService;
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
        if (string.IsNullOrWhiteSpace(NewPlaylistName))
            return;
            
        var playlist = await _playlistService.CreatePlaylistAsync(NewPlaylistName, NewPlaylistType);
        Playlists = [.. Playlists, playlist];
        NewPlaylistName = string.Empty;
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
    private async Task PlayTrackAsync(Track track)
    {
        await _playerService.PlayAsync(track);
    }
    
    [RelayCommand]
    private async Task ExportPlaylistAsync(Playlist playlist)
    {
        // File picker would be used here
        // var path = await FilePicker.SaveAsync(...);
        // await _playlistService.ExportPlaylistAsync(playlist.Id, path);
    }
    
    [RelayCommand]
    private async Task ImportPlaylistAsync()
    {
        // File picker would be used here
        // var result = await FilePicker.PickAsync(...);
        // var playlist = await _playlistService.ImportPlaylistAsync(result.FullPath);
        // Playlists = [.. Playlists, playlist];
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task ToggleOfflineAsync(Playlist playlist)
    {
        playlist.IsOfflineAvailable = !playlist.IsOfflineAvailable;
        await _playlistService.SetOfflineAvailableAsync(playlist.Id, playlist.IsOfflineAvailable);
    }
}

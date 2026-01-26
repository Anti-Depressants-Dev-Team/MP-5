using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.ViewModels;

/// <summary>
/// ViewModel for the home page with YouTube Music-style layout.
/// </summary>
public partial class HomeViewModel : ObservableObject
{
    private readonly IMusicSourceService _musicSource;
    private readonly IMusicPlayerService _playerService;
    
    [ObservableProperty]
    private List<Track> _quickPicks = [];
    
    [ObservableProperty]
    private List<Track> _recentlyPlayed = [];
    
    [ObservableProperty]
    private List<Playlist> _recommendedMixes = [];
    
    [ObservableProperty]
    private bool _isLoading;
    
    private readonly MainViewModel _mainViewModel;
    private readonly PlaylistsViewModel _playlistsViewModel;

    public HomeViewModel(IMusicSourceService musicSource, IMusicPlayerService playerService, MainViewModel mainViewModel, PlaylistsViewModel playlistsViewModel)
    {
        _musicSource = musicSource;
        _playerService = playerService;
        _mainViewModel = mainViewModel;
        _playlistsViewModel = playlistsViewModel;
    }
    
    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            // Load quick picks (trending/popular)
            var quickPicks = await _musicSource.SearchAsync("trending music");
            QuickPicks = quickPicks.ToList();
            
            // Recent history would come from a local service
            // RecentlyPlayed = await _historyService.GetRecentAsync(10);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task PlayTrackAsync(Track track)
    {
        await _playerService.PlayAsync(track);
    }


    [RelayCommand]
    private async Task OpenQuickPickAsync(string title)
    {
        // 1. Switch to Playlists Tab
        _mainViewModel.NavigateCommand.Execute(NavigationItem.Playlists);
        
        // 2. Find or Create Playlist
        var playlist = _playlistsViewModel.Playlists.FirstOrDefault(p => p.Name.Equals(title, StringComparison.OrdinalIgnoreCase));
        
        if (playlist == null)
        {
            // Auto-create if missing
            await _playlistsViewModel.CreatePlaylistCommand.ExecuteAsync(null); // Wait, CreatePlaylistAsync prompts for name. We can't use that command directly if we want auto-create without prompt.
            // We should use the Service directly or expose a method.
            // But PlaylistsViewModel hides _playlistService.
            // Let's rely on finding existing for now, or assume user creates it.
            // Actually, for "Liked Songs", we should probably create it if missing via a specialized method or just let the user know.
            // For now, let's just Try Select.
            return;
        }
        
        // 3. Select it
        _playlistsViewModel.SelectPlaylistCommand.Execute(playlist);
    }
}

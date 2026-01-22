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
    
    public HomeViewModel(IMusicSourceService musicSource, IMusicPlayerService playerService)
    {
        _musicSource = musicSource;
        _playerService = playerService;
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
}

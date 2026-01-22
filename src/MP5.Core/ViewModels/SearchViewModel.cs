using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MP5.Core.Interfaces;
using MP5.Core.Models;
using System.Collections.ObjectModel;

namespace MP5.Core.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IMusicSourceService _musicSourceService;
    private readonly IMusicPlayerService _playerService;
    
    [ObservableProperty]
    private string _searchQuery = string.Empty;
    
    [ObservableProperty]
    private bool _isBusy;
    
    public ObservableCollection<Track> SearchResults { get; } = new();
    
    public SearchViewModel(IMusicSourceService musicSourceService, IMusicPlayerService playerService)
    {
        _musicSourceService = musicSourceService;
        _playerService = playerService;
    }
    
    [RelayCommand]
    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;
        
        IsBusy = true;
        
        try 
        {
            var results = await _musicSourceService.SearchAsync(SearchQuery);
            
            SearchResults.Clear();
            foreach (var track in results)
            {
                SearchResults.Add(track);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task PlayTrackAsync(Track track)
    {
        if (track == null) return;
        await _playerService.PlayAsync(track);
    }
}

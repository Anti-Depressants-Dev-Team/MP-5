using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.ViewModels;

/// <summary>
/// Main ViewModel for navigation and app-level state.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    
    [ObservableProperty]
    private NavigationItem _selectedNavItem = NavigationItem.Home;
    
    [ObservableProperty]
    private AppSettings _settings = new();
    
    [ObservableProperty]
    private PlayerPosition _playerPosition = PlayerPosition.Top;
    
    [ObservableProperty]
    private string _accentColorHex = "#9B59B6";
    
    public bool IsPlayerAtTop => PlayerPosition == PlayerPosition.Top;
    public bool IsPlayerAtBottom => PlayerPosition == PlayerPosition.Bottom;
    
    public MainViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _ = LoadSettingsAsync();
    }
    
    private async Task LoadSettingsAsync()
    {
        Settings = await _settingsService.GetSettingsAsync();
        PlayerPosition = Settings.PlayerPosition;
        AccentColorHex = Settings.AccentColorHex;
        OnPropertyChanged(nameof(IsPlayerAtTop));
        OnPropertyChanged(nameof(IsPlayerAtBottom));
    }
    
    [RelayCommand]
    private void Navigate(NavigationItem item) => SelectedNavItem = item;
    
    [RelayCommand]
    private async Task TogglePlayerPositionAsync()
    {
        PlayerPosition = IsPlayerAtTop ? PlayerPosition.Bottom : PlayerPosition.Top;
        Settings.PlayerPosition = PlayerPosition;
        await _settingsService.SaveSettingsAsync(Settings);
        OnPropertyChanged(nameof(IsPlayerAtTop));
        OnPropertyChanged(nameof(IsPlayerAtBottom));
    }
}

/// <summary>
/// Navigation items for sidebar.
/// </summary>
public enum NavigationItem
{
    Home,
    Search,
    Playlists,
    Settings
}

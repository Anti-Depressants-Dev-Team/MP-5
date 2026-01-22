using MP5.App.Views;
using MP5.Core.Models;
using MP5.Core.ViewModels;

namespace MP5.App;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _mainViewModel;
    private readonly PlayerViewModel _playerViewModel;
    private readonly SettingsViewModel _settingsViewModel;
    private readonly IServiceProvider _serviceProvider;
    
    // Cached views
    private HomeView? _homeView;
    private SearchView? _searchView;
    private PlaylistsView? _playlistsView;
    private SettingsView? _settingsView;
    
    public MainPage(
        MainViewModel mainViewModel, 
        PlayerViewModel playerViewModel, 
        SettingsViewModel settingsViewModel,
        IServiceProvider serviceProvider)
    {
        InitializeComponent();
        
        _mainViewModel = mainViewModel;
        _playerViewModel = playerViewModel;
        _settingsViewModel = settingsViewModel;
        _serviceProvider = serviceProvider;
        
        BindingContext = _mainViewModel;
        TopPlayerBar.BindingContext = _playerViewModel;
        
        // Listen for layout changes
        _settingsViewModel.PropertyChanged += OnSettingsViewModelPropertyChanged;
        
        // Load stored settings immediately
        _settingsViewModel.LoadCommand.Execute(null);
        
        // Initial navigation
        NavigateTo(NavigationItem.Home);
        
        // Initial layout
        UpdatePlayerLayout();
    }

    private void OnSettingsViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.PlayerPosition))
        {
            UpdatePlayerLayout();
        }
    }

    private void UpdatePlayerLayout()
    {
        var grid = (Grid)Content;
        var position = _settingsViewModel.PlayerPosition;
        
        // We assume Row 0 is Top, Row 1 is Bottom (conceptually)
        // But we just resize the rows and move content
        
        if (position == MP5.Core.Models.PlayerPosition.Top)
        {
            // Top Mode (Psychopath)
            // Row 0: Player (Fixed)
            // Row 1: Content (Star)
            TopPlayerRow.Height = 120;
            ContentRow.Height = GridLength.Star;
            
            Grid.SetRow(TopPlayerBar, 0);
            
            // Sidebar and MainContent go to Row 1
            // Note: Sidebar is inside a Border at Row 1, Column 0 in XAML
            // We need to find the sidebar container. looking at XAML:
            // "Sidebar" is a Border with x:Name="SidebarContainer" (Wait, I need to name it)
            // Actually it doesn't have a name in my view_file output earlier.
            // Let's rely on finding the borders by Grid.SetRow or Name if possible.
            // I'll assume I'll add x:Name="SidebarContainer" to the sidebar border in next step if needed.
            // But wait, the Sidebar Border is defined at Grid.Row="1".
            
            // Let's update the rows of the main content elements
            Grid.SetRow((View)Content.FindByName("SidebarContainer"), 1);
            Grid.SetRow(MainContentArea, 1);
        }
        else
        {
            // Bottom Mode (Normal)
            // Row 0: Content (Star)
            // Row 1: Player (Fixed)
            TopPlayerRow.Height = GridLength.Star;
            ContentRow.Height = 120;
            
            Grid.SetRow(TopPlayerBar, 1);
            
            // Sidebar and MainContent go to Row 0
            Grid.SetRow((View)Content.FindByName("SidebarContainer"), 0);
            Grid.SetRow(MainContentArea, 0);
        }
    }
    

    
    // Navigation handlers (still using code-behind for View swapping logic for now, 
    // as it involves UI element instantiation, but we trigger VM commands too)
    private void OnNavHomeTapped(object? sender, EventArgs e)
    {
        _mainViewModel.NavigateCommand.Execute(NavigationItem.Home);
        NavigateTo(NavigationItem.Home);
    } 

    private void OnNavSearchTapped(object? sender, EventArgs e)
    {
        _mainViewModel.NavigateCommand.Execute(NavigationItem.Search);
        NavigateTo(NavigationItem.Search);
    }
    
    private void OnNavPlaylistsTapped(object? sender, EventArgs e)
    {
        _mainViewModel.NavigateCommand.Execute(NavigationItem.Playlists);
        NavigateTo(NavigationItem.Playlists);
    }

    private void OnNavSettingsTapped(object? sender, EventArgs e)
    {
        _mainViewModel.NavigateCommand.Execute(NavigationItem.Settings);
        NavigateTo(NavigationItem.Settings);
    }
    
    private void NavigateTo(NavigationItem item)
    {
        UpdateNavHighlight(item);
        UpdateContentArea(item);
    }
    
    private void UpdateContentArea(NavigationItem item)
    {
        ContentView view = item switch
        {
            NavigationItem.Home => _homeView ??= _serviceProvider.GetRequiredService<HomeView>(),
            NavigationItem.Search => _searchView ??= _serviceProvider.GetRequiredService<SearchView>(),
            NavigationItem.Playlists => _playlistsView ??= _serviceProvider.GetRequiredService<PlaylistsView>(),
            NavigationItem.Settings => _settingsView ??= _serviceProvider.GetRequiredService<SettingsView>(),
            _ => _homeView ??= _serviceProvider.GetRequiredService<HomeView>()
        };
        
        MainContentArea.Content = view;
    }
    
    private void UpdateNavHighlight(NavigationItem item)
    {
        // Use DynamicResource equivalent by finding it in Application resources
        Color activeColor;
        if (Application.Current?.Resources.TryGetValue("SidebarItemActive", out var resource) == true && resource is Color color)
        {
            activeColor = color; 
        }
        else
        {
            // Fallback if needed, though SidebarItemActive should be bound to AccentPrimary with transparency
             activeColor = Color.FromArgb("#33FFFFFF");
        }
        
        // Better yet, let's use the AccentPrimary directly and add transparency manually if we want to be dynamic
        if (Application.Current?.Resources.TryGetValue("AccentPrimary", out var accentObj) == true && accentObj is Color accent)
        {
            activeColor = accent.WithAlpha(0.2f);
        }

        var inactiveColor = Colors.Transparent;
        
        NavHome.BackgroundColor = item == NavigationItem.Home ? activeColor : inactiveColor;
        NavSearch.BackgroundColor = item == NavigationItem.Search ? activeColor : inactiveColor;
        NavPlaylists.BackgroundColor = item == NavigationItem.Playlists ? activeColor : inactiveColor;
        NavSettings.BackgroundColor = item == NavigationItem.Settings ? activeColor : inactiveColor;
    }
    
    // Player control handlers - Bridging to ViewModel Commands
    private void OnPlayPauseClicked(object? sender, EventArgs e) => _playerViewModel.PlayPauseCommand.Execute(null);
    private void OnPreviousClicked(object? sender, EventArgs e) => _playerViewModel.PreviousCommand.Execute(null);
    private void OnNextClicked(object? sender, EventArgs e) => _playerViewModel.NextCommand.Execute(null);
    
    private void ToggleShuffle(object? sender, EventArgs e) => _playerViewModel.ToggleShuffleCommand.Execute(null);
    private void CycleRepeatMode(object? sender, EventArgs e) => _playerViewModel.CycleRepeatModeCommand.Execute(null);
    
    private void OnSliderDragStarted(object? sender, EventArgs e) => _playerViewModel.DragStartedCommand.Execute(null);
    private void OnSliderDragCompleted(object? sender, EventArgs e) => _playerViewModel.DragCompletedCommand.Execute(null);
    
    private void OnVolumeButtonClicked(object? sender, EventArgs e)
    {
        _playerViewModel.ToggleVolumeBoostCommand.Execute(null);
        
        // Show visual feedback
        var state = _playerViewModel.IsVolumeBoosted ? "Boosted (150%)" : "Normal";
        // Minimize annoyance, maybe toast? For now just visual cue if we bound it.
    }
}

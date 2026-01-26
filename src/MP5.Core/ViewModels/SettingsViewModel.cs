using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.ViewModels;

/// <summary>
/// ViewModel for application settings.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IScrobblerService? _lastFmService;
    private readonly IScrobblerService? _listenBrainzService;
    private readonly IDiscordRpcService _discordService;
    private readonly ISyncService _syncService;
    private readonly IUpdateService _updateService;
    
    [ObservableProperty]
    private AppSettings _settings = new();
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private string _accentColorHex = "#9B59B6";
    
    [ObservableProperty]
    private PlayerPosition _playerPosition = PlayerPosition.Top;
    
    [ObservableProperty]
    private bool _preventAudioDucking = true;
    
    [ObservableProperty]
    private bool _enableDiscordRpc = true;

    partial void OnEnableDiscordRpcChanged(bool value)
    {
        _discordService.SetEnabled(value);
        _ = SaveAsync();
    }
    
    [ObservableProperty]
    private bool _enableLastFm;

    partial void OnEnableLastFmChanged(bool value) => _ = SaveAsync();
    
    [ObservableProperty]
    private bool _enableListenBrainz;

    partial void OnEnableListenBrainzChanged(bool value) => _ = SaveAsync();
    
    [ObservableProperty]
    private bool _googleSyncEnabled;
    
    [ObservableProperty]
    private string? _googleEmail;
    
    [ObservableProperty]
    private double _volumeBoostMultiplier = 1.0;

    partial void OnVolumeBoostMultiplierChanged(double value) => _ = SaveAsync();
    
    [ObservableProperty]
    private UpdateInfo? _availableUpdate;

    [ObservableProperty]
    private bool _glassmorphismEnabled = true;

    [ObservableProperty]
    private bool _crossfadeEnabled;

    [ObservableProperty]
    private bool _isStartupEnabled;

    [ObservableProperty]
    private bool _isFullscreenEnabled;

    partial void OnIsStartupEnabledChanged(bool value)
    {
         _platformService.SetStartup(value);
         _ = SaveAsync();
    }

    partial void OnIsFullscreenEnabledChanged(bool value)
    {
         _platformService.SetFullscreen(value);
         _ = SaveAsync();
    }

    [ObservableProperty]
    private bool _adBlockEnabled;

    partial void OnAdBlockEnabledChanged(bool value)
    {
         _adBlockService.IsEnabled = value;
         _ = SaveAsync();
    }

    public bool IsPsychopathMode
    {
        get => PlayerPosition == PlayerPosition.Top;
        set
        {
            PlayerPosition = value ? PlayerPosition.Top : PlayerPosition.Bottom;
            OnPropertyChanged();
            _ = SaveAsync();
        }
    }

    partial void OnGlassmorphismEnabledChanged(bool value) => _ = SaveAsync();
    partial void OnCrossfadeEnabledChanged(bool value) => _ = SaveAsync();
    partial void OnPreventAudioDuckingChanged(bool value) => _ = SaveAsync();
    
    // Accent color presets
    public List<string> AccentColorPresets { get; } =
    [
        "#9B59B6", // Purple (default)
        "#3498DB", // Blue
        "#E74C3C", // Red
        "#2ECC71", // Green
        "#F39C12", // Orange
        "#1ABC9C", // Teal
        "#E91E63", // Pink
        "#00BCD4", // Cyan
    ];
    
    private readonly IThemeService _themeService;
    private readonly IPlatformService _platformService;
    private readonly IAdBlockService _adBlockService;
    
    public SettingsViewModel(
        ISettingsService settingsService,
        IDiscordRpcService discordService,
        IEnumerable<IScrobblerService> scrobblers,
        ISyncService syncService,
        IUpdateService updateService,
        IThemeService themeService,
        IPlatformService platformService,
        IAdBlockService adBlockService)
    {
        _settingsService = settingsService;
        _discordService = discordService;
        
        // Resolve specific services for settings management
        _lastFmService = scrobblers.FirstOrDefault(s => s is MP5.Core.Services.LastFmScrobblerService);
        _listenBrainzService = scrobblers.FirstOrDefault(s => s is MP5.Core.Services.ListenBrainzScrobblerService);
        
        _syncService = syncService;
        _updateService = updateService;
        _themeService = themeService;
        _platformService = platformService;
        _adBlockService = adBlockService;
    }
    
    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            Settings = await _settingsService.GetSettingsAsync();
            ApplySettingsToProperties();
            
            // Check Google sync status
            GoogleSyncEnabled = _syncService.IsSignedIn;
            GoogleEmail = _syncService.UserEmail;
            
            // Check for updates
            AvailableUpdate = await _updateService.CheckForUpdateAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private void ApplySettingsToProperties()
    {
        AccentColorHex = Settings.AccentColorHex;
        PlayerPosition = Settings.PlayerPosition;
        PreventAudioDucking = Settings.PreventAudioDucking;
        EnableDiscordRpc = Settings.EnableDiscordRpc;
        EnableLastFm = Settings.EnableLastFmScrobbling;
        EnableListenBrainz = Settings.EnableListenBrainz;
        VolumeBoostMultiplier = Settings.VolumeBoostMultiplier;
        AdBlockEnabled = Settings.AdBlockEnabled;
        
        IsLastFmAuthenticated = !string.IsNullOrEmpty(Settings.LastFmSessionKey);
        AuthenticatedLastFmUser = Settings.LastFmUsername;
        
        IsListenBrainzAuthenticated = !string.IsNullOrEmpty(Settings.ListenBrainzToken);
        
        IsStartupEnabled = Settings.IsStartupEnabled;
        IsFullscreenEnabled = Settings.IsFullscreenEnabled;
        
        // Apply platform side effects immediately on load
        _platformService.SetStartup(IsStartupEnabled);
        _platformService.SetFullscreen(IsFullscreenEnabled);
    }
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        Settings.AccentColorHex = AccentColorHex;
        Settings.PlayerPosition = PlayerPosition;
        Settings.PreventAudioDucking = PreventAudioDucking;
        Settings.EnableDiscordRpc = EnableDiscordRpc;
        Settings.EnableLastFmScrobbling = EnableLastFm;
        Settings.EnableListenBrainz = EnableListenBrainz;
        Settings.VolumeBoostMultiplier = VolumeBoostMultiplier;
        Settings.AdBlockEnabled = AdBlockEnabled;
        Settings.IsStartupEnabled = IsStartupEnabled;
        Settings.IsFullscreenEnabled = IsFullscreenEnabled;
        
        await _settingsService.SaveSettingsAsync(Settings);
    }
    
    [RelayCommand]
    private async Task SetAccentColorAsync(string colorHex)
    {
        AccentColorHex = colorHex;
        _themeService.SetAccentColor(colorHex);
        await SaveAsync();
    }
    
    [RelayCommand]
    private async Task TogglePlayerPositionAsync()
    {
        PlayerPosition = PlayerPosition == PlayerPosition.Top 
            ? PlayerPosition.Bottom 
            : PlayerPosition.Top;
        await SaveAsync();
    }
    
    [RelayCommand]
    private async Task SignInGoogleAsync()
    {
        var success = await _syncService.SignInAsync();
        if (success)
        {
            GoogleSyncEnabled = true;
            GoogleEmail = _syncService.UserEmail;
        }
    }
    
    [RelayCommand]
    private async Task SignOutGoogleAsync()
    {
        await _syncService.SignOutAsync();
        GoogleSyncEnabled = false;
        GoogleEmail = null;
    }
    
    [RelayCommand]
    private async Task InstallUpdateAsync()
    {
        if (AvailableUpdate != null)
        {
            await _updateService.DownloadAndInstallAsync(AvailableUpdate);
        }
    }
    
    [RelayCommand]
    private async Task ResetToDefaultsAsync()
    {
        await _settingsService.ResetToDefaultsAsync();
        await LoadAsync();
    }
    
    // --- Last.fm ---
    
    [ObservableProperty]
    private string _lastFmUsernameInput = string.Empty;
    
    [ObservableProperty]
    private string _lastFmPasswordInput = string.Empty;
    
    [ObservableProperty]
    private bool _isLastFmAuthenticated;
    
    [ObservableProperty]
    private string? _authenticatedLastFmUser;
    
    [RelayCommand]
    private async Task ConnectLastFmAsync()
    {
        if (string.IsNullOrWhiteSpace(LastFmUsernameInput) || string.IsNullOrWhiteSpace(LastFmPasswordInput))
            return;
            
        IsLoading = true;
        try 
        {
            if (_lastFmService != null)
            {
                var success = await _lastFmService.AuthenticateAsync(LastFmUsernameInput, LastFmPasswordInput);
                if (success)
                {
                    IsLastFmAuthenticated = true;
                    AuthenticatedLastFmUser = LastFmUsernameInput; // Or get from service
                    LastFmPasswordInput = string.Empty; // Clear secure data
                    EnableLastFm = true;
                    await SaveAsync();
                }
                else
                {
                    // Would show alert here
                    System.Diagnostics.Debug.WriteLine("Last.fm Login Failed");
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task DisconnectLastFmAsync()
    {
        if (_lastFmService != null)
        {
            await _lastFmService.LogoutAsync();
            IsLastFmAuthenticated = false;
            AuthenticatedLastFmUser = null;
            EnableLastFm = false;
            await SaveAsync();
        }
    }
    
    // --- ListenBrainz ---
    
    [ObservableProperty]
    private string _listenBrainzTokenInput = string.Empty;
    
    [ObservableProperty]
    private bool _isListenBrainzAuthenticated;
    
    [RelayCommand]
    private async Task ConnectListenBrainzAsync()
    {
        if (string.IsNullOrWhiteSpace(ListenBrainzTokenInput)) return;
        
        IsLoading = true;
        try
        {
            if (_listenBrainzService != null)
            {
                var success = await _listenBrainzService.AuthenticateAsync("token", ListenBrainzTokenInput);
                if (success)
                {
                    IsListenBrainzAuthenticated = true;
                    EnableListenBrainz = true;
                    ListenBrainzTokenInput = string.Empty;
                    await SaveAsync();
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task DisconnectListenBrainzAsync()
    {
        if (_listenBrainzService != null)
        {
            await _listenBrainzService.LogoutAsync();
            IsListenBrainzAuthenticated = false;
            EnableListenBrainz = false;
            await SaveAsync();
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.ViewModels;

/// <summary>
/// ViewModel for the music player bar and playback controls.
/// </summary>
public partial class PlayerViewModel : ObservableObject
{
    private readonly IMusicPlayerService _playerService;
    
    [ObservableProperty]
    private Track? _currentTrack;
    
    [ObservableProperty]
    private PlaybackState _playbackState = PlaybackState.Stopped;
    
    [ObservableProperty]
    private TimeSpan _position;
    
    [ObservableProperty]
    private TimeSpan _duration;
    
    [ObservableProperty]
    private double _volume = 0.7;
    
    [ObservableProperty]
    private bool _isVolumeBoosted;
    
    [ObservableProperty] 
    private double _volumeBoostMultiplier = 1.0;
    
    [ObservableProperty]
    private bool _shuffleEnabled;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRepeatOne))]
    [NotifyPropertyChangedFor(nameof(IsRepeatAll))]
    private RepeatMode _repeatMode = RepeatMode.None;
    
    public bool IsRepeatOne => RepeatMode == RepeatMode.One;
    public bool IsRepeatAll => RepeatMode == RepeatMode.All;
    
    [ObservableProperty]
    private bool _autoplayEnabled = true;
    
    partial void OnAutoplayEnabledChanged(bool value)
    {
        _playerService.AutoplayEnabled = value;
    }
    
    [ObservableProperty]
    private bool _showVolumePopup;
    
    public bool IsPlaying => PlaybackState == PlaybackState.Playing;
    
    [ObservableProperty]
    private bool _isDragging;

    private CancellationTokenSource? _seekDebounceCts;
    private bool _isSeeking;

    public double PositionSeconds
    {
        get => Position.TotalSeconds;
        set
        {
            if (Math.Abs(value - Position.TotalSeconds) > 0.5) // Increased threshold
            {
                Position = TimeSpan.FromSeconds(value);
                
                // If dragging, we just update local Position (visual).
                // Seek happens on DragCompleted.
                if (IsDragging) return;

                // If not dragging (e.g. Click/Tap or "Slide without Drag event"), we Debounce the seek.
                _seekDebounceCts?.Cancel();
                _seekDebounceCts = new CancellationTokenSource();
                var token = _seekDebounceCts.Token;

                _isSeeking = true; // Block incoming position updates
                
                Task.Delay(100, token).ContinueWith(async t =>
                {
                    if (t.IsCanceled) return;
                    try
                    {
                        await _playerService.SeekAsync(Position);
                        // Brief grace period before allowing incoming updates again
                        await Task.Delay(500); 
                        _isSeeking = false;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Seek Failed: {ex.Message}");
                        _isSeeking = false;
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }

    [RelayCommand]
    private void DragStarted() => IsDragging = true;

    [RelayCommand]
    private async Task DragCompleted()
    {
        IsDragging = false;
        await _playerService.SeekAsync(Position);
    }
    
    public double DurationSeconds => Duration.TotalSeconds;
    
    private readonly IOfflineService _offlineService;
    private readonly IMusicSourceService _musicSourceService;
    private readonly ISettingsService _settingsService;
    private readonly IPlatformService _platformService;
    private readonly ILyricsService _lyricsService;
    
    public PlayerViewModel(
        IMusicPlayerService playerService, 
        IOfflineService offlineService, 
        IMusicSourceService musicSourceService,
        ISettingsService settingsService,
        IPlatformService platformService,
        ILyricsService lyricsService)
    {
        _playerService = playerService;
        _offlineService = offlineService;
        _musicSourceService = musicSourceService;
        _settingsService = settingsService;
        _platformService = platformService;
        _lyricsService = lyricsService;
        
        _playerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _playerService.PositionChanged += OnPositionChanged;
        
        // Load settings asynchronously
        Task.Run(LoadSettingsAsync);
    }

    private async Task LoadSettingsAsync()
    {
        try 
        {
            var settings = await _settingsService.GetSettingsAsync();
            
            // Apply on UI thread
            _platformService.InvokeOnMainThread(() =>
            {
                Volume = settings.Volume;
                ShuffleEnabled = settings.ShuffleEnabled;
                RepeatMode = settings.RepeatMode;
                VolumeBoostMultiplier = settings.VolumeBoostMultiplier;
                IsVolumeBoosted = VolumeBoostMultiplier > 1.0;
            });
            
            // Apply to Player Service
            await _playerService.SetVolumeAsync(settings.Volume);
            await _playerService.SetVolumeBoostAsync(settings.VolumeBoostMultiplier);
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Failed to load player settings: {ex.Message}");
        }
    }

    private async Task SaveSettingsAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        settings.Volume = Volume;
        settings.ShuffleEnabled = ShuffleEnabled;
        settings.RepeatMode = RepeatMode;
        settings.VolumeBoostMultiplier = VolumeBoostMultiplier;
        await _settingsService.SaveSettingsAsync(settings);
    }
    
    private void OnPlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs e)
    {
        var trackChanged = CurrentTrack?.Id != e.CurrentTrack?.Id;
        PlaybackState = e.State;
        CurrentTrack = e.CurrentTrack;
        CheckOfflineStatus();
        
        if (trackChanged && CurrentTrack != null)
        {
            _ = LoadLyricsAsync(CurrentTrack);
        }
        else if (CurrentTrack == null)
        {
            Lyrics = null;
            ActiveLyricsLine = null;
        }

        OnPropertyChanged(nameof(IsPlaying));
    }
    
    private void OnPositionChanged(object? sender, PositionChangedEventArgs e)
    {
        // Don't update position if user is dragging OR if we are handling a pending seek
        if (!IsDragging && !_isSeeking)
        {
            Position = e.Position;
            Duration = e.Duration;
            OnPropertyChanged(nameof(PositionSeconds));
            OnPropertyChanged(nameof(DurationSeconds));
            
            UpdateSyncedLyrics(e.Position);
        }
    }
    
    private CancellationTokenSource? _volumeDebounceCts;

    partial void OnVolumeChanged(double value)
    {
        _volumeDebounceCts?.Cancel();
        _volumeDebounceCts = new CancellationTokenSource();
        var token = _volumeDebounceCts.Token;

        Task.Delay(100, token).ContinueWith(async t =>
        {
            if (t.IsCanceled) return;
            try
            {
                await _playerService.SetVolumeAsync(value);
                // Save volume settings
                await SaveSettingsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Volume Set Failed: {ex.Message}");
            }
        });
    }

    partial void OnShuffleEnabledChanged(bool value) => _ = SaveSettingsAsync();
    partial void OnRepeatModeChanged(RepeatMode value) => _ = SaveSettingsAsync();
    partial void OnVolumeBoostMultiplierChanged(double value) => _ = SaveSettingsAsync();

    [RelayCommand]
    private async Task AddToQueueAsync(Track track)
    {
        if (track == null) return;
        await _playerService.AddToQueueAsync(track);
        // Visual feedback? maybe toast
    }

    [RelayCommand]
    private async Task PlayPauseAsync()
    {
        if (IsPlaying)
            await _playerService.PauseAsync();
        else
            await _playerService.ResumeAsync();
    }
    
    [RelayCommand]
    private async Task NextAsync() => await _playerService.NextAsync();
    
    [RelayCommand]
    private async Task PreviousAsync() => await _playerService.PreviousAsync();
    
    [RelayCommand]
    private async Task SeekAsync(TimeSpan position) => await _playerService.SeekAsync(position);
    
    [RelayCommand]
    private async Task SetVolumeAsync(double volume)
    {
        Volume = volume;
        await _playerService.SetVolumeAsync(volume);
    }
    
    [RelayCommand]
    private async Task ToggleVolumeBoostAsync()
    {
        IsVolumeBoosted = !IsVolumeBoosted;
        VolumeBoostMultiplier = IsVolumeBoosted ? 1.5 : 1.0;
        await _playerService.SetVolumeBoostAsync(VolumeBoostMultiplier);
    }
    
    [RelayCommand]
    private void ToggleVolumePopup() => ShowVolumePopup = !ShowVolumePopup;
    
    [RelayCommand]
    private void ToggleShuffle() => ShuffleEnabled = !ShuffleEnabled;
    
    [RelayCommand]
    private void CycleRepeatMode()
    {
        RepeatMode = RepeatMode switch
        {
            RepeatMode.None => RepeatMode.All,
            RepeatMode.All => RepeatMode.One,
            RepeatMode.One => RepeatMode.None,
            _ => RepeatMode.None
        };
    }
    
    // --- Offline Logic ---
    
    [ObservableProperty]
    private bool _isOfflineAvailable;
    
    [ObservableProperty]
    private bool _isDownloading;
    
    [ObservableProperty]
    private double _downloadProgress;

    [RelayCommand]
    private async Task ToggleDownloadAsync()
    {
        if (CurrentTrack == null || IsDownloading) return;

        if (IsOfflineAvailable)
        {
            // Remove
            await _offlineService.RemoveTrackAsync(CurrentTrack);
            IsOfflineAvailable = false;
        }
        else
        {
            // Download
            IsDownloading = true;
            DownloadProgress = 0;
            try
            {
                var url = await _musicSourceService.GetStreamUrlAsync(CurrentTrack);
                if (!string.IsNullOrEmpty(url))
                {
                    var progress = new Progress<double>(p => DownloadProgress = p);
                    await _offlineService.DownloadTrackAsync(CurrentTrack, url, progress);
                    IsOfflineAvailable = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Download Failed: {ex.Message}");
            }
            finally
            {
                IsDownloading = false;
            }
        }
    }
    
    private void CheckOfflineStatus()
    {
        if (CurrentTrack != null)
        {
            IsOfflineAvailable = _offlineService.IsTrackDownloaded(CurrentTrack);
        }
        else
        {
            IsOfflineAvailable = false;
        }
    }

    // --- Lyrics Logic ---
    
    [ObservableProperty]
    private bool _showLyrics;
    
    [ObservableProperty]
    private Lyrics? _lyrics;
    
    [ObservableProperty]
    private LyricsLine? _activeLyricsLine;

    [ObservableProperty]
    private string _lyricStatus = "Loading...";

    [RelayCommand]
    private void ToggleLyrics() => ShowLyrics = !ShowLyrics;

    private async Task LoadLyricsAsync(Track track)
    {
        Lyrics = null;
        ActiveLyricsLine = null;
        LyricStatus = "Searching lyrics...";
        
        try
        {
            var lyrics = await _lyricsService.GetLyricsAsync(track);
            
            // Avoid race condition if track changed while fetching
            if (CurrentTrack?.Id == track.Id)
            {
                _platformService.InvokeOnMainThread(() =>
                {
                    Lyrics = lyrics;
                    if (Lyrics == null)
                        LyricStatus = "No lyrics found";
                    else
                        LyricStatus = string.Empty;
                });
            }
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"Lyrics Fetch Error: {ex.Message}");
             LyricStatus = "Error loading lyrics";
        }
    }

    private void UpdateSyncedLyrics(TimeSpan position)
    {
        if (Lyrics == null || !Lyrics.IsSynced) return;
        
        // Find the current line:
        // The last line whose Time <= position
        // Optimization: Could cache index, but list is small.
        var line = Lyrics.SyncedLines.FindLast(l => l.Time <= position + TimeSpan.FromSeconds(0.2)); // Slight offset
        
        if (line != ActiveLyricsLine)
        {
            ActiveLyricsLine = line;
        }
    }
}

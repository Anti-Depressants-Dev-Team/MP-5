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
    
    public PlayerViewModel(IMusicPlayerService playerService)
    {
        _playerService = playerService;
        
        _playerService.PlaybackStateChanged += OnPlaybackStateChanged;
        _playerService.PositionChanged += OnPositionChanged;
    }
    
    private void OnPlaybackStateChanged(object? sender, PlaybackStateChangedEventArgs e)
    {
        PlaybackState = e.State;
        CurrentTrack = e.CurrentTrack;
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
        }
    }
    
    private CancellationTokenSource? _volumeDebounceCts;

    partial void OnVolumeChanged(double value)
    {
        _volumeDebounceCts?.Cancel();
        _volumeDebounceCts = new CancellationTokenSource();
        var token = _volumeDebounceCts.Token;

        Task.Delay(50, token).ContinueWith(async t =>
        {
            if (t.IsCanceled) return;
            try
            {
                await _playerService.SetVolumeAsync(value);
            }
            catch (Exception ex)
            {
                // Swiftly swallow errors during rapid sliding to prevent crash
                System.Diagnostics.Debug.WriteLine($"Volume Set Failed: {ex.Message}");
            }
        });
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
}

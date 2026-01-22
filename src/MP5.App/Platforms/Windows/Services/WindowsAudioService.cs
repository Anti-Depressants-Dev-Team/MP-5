using MP5.Core.Interfaces;
#if WINDOWS
using Windows.Media.Core;
using Windows.Media.Playback;
#endif

namespace MP5.App.Platforms.Windows.Services;

#if WINDOWS
public class WindowsAudioService : IAudioService
{
    private MediaPlayer? _mediaPlayer;
    private bool _isInitialized;

    public event EventHandler? PlaybackEnded;
    public event EventHandler<Exception>? PlaybackFailed;

    public TimeSpan Duration => _mediaPlayer?.PlaybackSession?.NaturalDuration ?? TimeSpan.Zero;
    
    public TimeSpan Position => _mediaPlayer?.PlaybackSession?.Position ?? TimeSpan.Zero;
    
    public bool IsPlaying => _mediaPlayer?.PlaybackSession?.PlaybackState == MediaPlaybackState.Playing;

    public WindowsAudioService()
    {
        _mediaPlayer = new MediaPlayer();
        _mediaPlayer.MediaEnded += OnMediaEnded;
        _mediaPlayer.MediaFailed += OnMediaFailed;
    }

    public async Task InitializeAsync(string audioUri)
    {
        if (_mediaPlayer == null) return;

        try
        {
            // For now, assume URI. Local file handling requires StorageFile.
            // If it's a web URL:
            if (Uri.TryCreate(audioUri, UriKind.Absolute, out var uri))
            {
                _mediaPlayer.Source = MediaSource.CreateFromUri(uri);
            }
            // TODO: Handle local file paths properly if needed later
            
            _isInitialized = true;
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            PlaybackFailed?.Invoke(this, ex);
        }
    }

    public async Task PlayAsync()
    {
        if (!_isInitialized || _mediaPlayer == null) return;
        _mediaPlayer.Play();
        await Task.CompletedTask;
    }

    public async Task PauseAsync()
    {
        if (!_isInitialized || _mediaPlayer == null) return;
        _mediaPlayer.Pause();
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (!_isInitialized || _mediaPlayer == null) return;
        _mediaPlayer.Pause();
        _mediaPlayer.PlaybackSession.Position = TimeSpan.Zero;
        await Task.CompletedTask;
    }

    public async Task SetVolumeAsync(double volume)
    {
        if (_mediaPlayer == null) return;
        _mediaPlayer.Volume = Math.Clamp(volume, 0, 1.0);
        await Task.CompletedTask;
    }

    public async Task SeekToAsync(TimeSpan position)
    {
        if (!_isInitialized || _mediaPlayer == null) return;
        _mediaPlayer.PlaybackSession.Position = position;
        await Task.CompletedTask;
    }

    private void OnMediaEnded(MediaPlayer sender, object args)
    {
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        // Simple exception wrapper
        PlaybackFailed?.Invoke(this, new Exception($"Media Failed: {args.Error} - {args.ErrorMessage}"));
    }

    public async ValueTask DisposeAsync()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.MediaEnded -= OnMediaEnded;
            _mediaPlayer.MediaFailed -= OnMediaFailed;
            _mediaPlayer.Dispose();
            _mediaPlayer = null;
        }
        _isInitialized = false;
        await Task.CompletedTask;
    }
}
#endif

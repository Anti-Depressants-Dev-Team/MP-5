using MP5.Core.Interfaces;
#if ANDROID
using Android.Media;
using Android.Content;
#endif

namespace MP5.App.Platforms.Android.Services;

#if ANDROID
public class AndroidAudioService : IAudioService
{
    private MediaPlayer? _mediaPlayer;
    private bool _isPrepared;

    public event EventHandler? PlaybackEnded;
    public event EventHandler<Exception>? PlaybackFailed;

    public TimeSpan Duration => _mediaPlayer != null && _isPrepared ? TimeSpan.FromMilliseconds(_mediaPlayer.Duration) : TimeSpan.Zero;
    
    public TimeSpan Position => _mediaPlayer != null && _isPrepared ? TimeSpan.FromMilliseconds(_mediaPlayer.CurrentPosition) : TimeSpan.Zero;
    
    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;

    public AndroidAudioService()
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        _mediaPlayer = new MediaPlayer();
        _mediaPlayer.Completion += OnCompletion;
        _mediaPlayer.Error += OnError;
        _mediaPlayer.Prepared += OnPrepared;
    }

    public async Task InitializeAsync(string audioUri)
    {
        if (_mediaPlayer == null) InitializePlayer();
        
        try
        {
            _mediaPlayer?.Reset();
            _isPrepared = false;
            
            // Handle URI
             if (global::Android.Net.Uri.Parse(audioUri) is var uri)
             {
                 _mediaPlayer?.SetDataSource(Platform.AppContext, uri);
                 _mediaPlayer?.PrepareAsync(); // Async prep
             }
             
             await Task.CompletedTask;
        }
        catch (Exception ex)
        {
             PlaybackFailed?.Invoke(this, ex);
        }
    }

    private void OnPrepared(object? sender, EventArgs e)
    {
        _isPrepared = true;
    }

    public async Task PlayAsync()
    {
        // Wait for preparation if needed? 
        // For simplicity, assume user won't click play instantly after init, 
        // or check _isPrepared. 
        // Better: logic to queue play if not prepared.
        if (_mediaPlayer != null && _isPrepared)
        {
            _mediaPlayer.Start();
        }
        await Task.CompletedTask;
    }

    public async Task PauseAsync()
    {
        if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
        {
            _mediaPlayer.Pause();
        }
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_mediaPlayer != null)
        {
            if (_mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Stop();
            }
            // Android MediaPlayer needs reset after stop to play again usually, 
            // or just Prepare again.
            // Simpler to just pause and seek to zero for "Stop" behavior in UI logic
            _mediaPlayer.Pause();
            _mediaPlayer.SeekTo(0);
        }
        await Task.CompletedTask;
    }

    public async Task SetVolumeAsync(double volume)
    {
        if (_mediaPlayer != null)
        {
            float v = (float)Math.Clamp(volume, 0, 1.0);
            _mediaPlayer.SetVolume(v, v);
        }
        await Task.CompletedTask;
    }

    public async Task SeekToAsync(TimeSpan position)
    {
        if (_mediaPlayer != null && _isPrepared)
        {
            _mediaPlayer.SeekTo((int)position.TotalMilliseconds);
        }
        await Task.CompletedTask;
    }

    private void OnCompletion(object? sender, EventArgs e)
    {
        PlaybackEnded?.Invoke(this, EventArgs.Empty);
    }

    private void OnError(object? sender, MediaPlayer.ErrorEventArgs e)
    {
        PlaybackFailed?.Invoke(this, new Exception($"Android Player Error: {e.What}"));
    }

    public async ValueTask DisposeAsync()
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.Release();
            _mediaPlayer.Dispose();
            _mediaPlayer = null;
        }
        _isPrepared = false;
        await Task.CompletedTask;
    }
}
#endif

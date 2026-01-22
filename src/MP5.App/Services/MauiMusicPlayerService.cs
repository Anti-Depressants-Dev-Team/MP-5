using MP5.Core.Interfaces;
using MP5.Core.Services;
using MP5.Core.Models;

namespace MP5.App.Services;

/// <summary>
/// MAUI implementation of playback service that delegates to native handlers via IAudioService.
/// </summary>
public class MauiMusicPlayerService : MusicPlayerServiceBase
{
    private readonly IAudioService _audioService;
    private readonly IMusicSourceService _musicSourceService;
    private readonly IScrobblerService _scrobblerService;
    
    public MauiMusicPlayerService(IAudioService audioService, IMusicSourceService musicSourceService, IScrobblerService scrobblerService)
    {
        _audioService = audioService;
        _musicSourceService = musicSourceService;
        _scrobblerService = scrobblerService;
        _audioService.PlaybackEnded += OnPlaybackEnded;
        _audioService.PlaybackFailed += OnPlaybackFailed;
    }
    
    // We update position via a timer loop in the base class or ViewModel
    protected override void UpdatePosition()
    {
        if (_audioService.IsPlaying)
        {
            Position = _audioService.Position;
            Duration = _audioService.Duration;
        }
    }
    
    protected override async Task PlayInternalAsync(string source)
    {
        string playbackUrl = source;
        
        // If source is not a valid absolute URI (likely a Source ID), try to resolve it
        if (!Uri.IsWellFormedUriString(source, UriKind.Absolute) && !File.Exists(source) && CurrentTrack != null)
        {
            System.Diagnostics.Debug.WriteLine($"Resolving stream for Track: {CurrentTrack.Title}");
            var resolvedUrl = await _musicSourceService.GetStreamUrlAsync(CurrentTrack);
            
            if (!string.IsNullOrEmpty(resolvedUrl))
            {
                playbackUrl = resolvedUrl;
            }
        }
        
        await _audioService.InitializeAsync(playbackUrl);
        // Initial Play
        await _audioService.PlayAsync();
        
        // Scrobbling: Now Playing
        if (CurrentTrack != null)
        {
            _ = _scrobblerService.UpdateNowPlayingAsync(CurrentTrack);
        }
    }
    
    protected override async Task PauseInternalAsync()
    {
        await _audioService.PauseAsync();
    }
    
    protected override async Task ResumeInternalAsync()
    {
        await _audioService.PlayAsync();
    }
    
    protected override async Task StopInternalAsync()
    {
        await _audioService.StopAsync();
    }
    
    protected override async Task SeekInternalAsync(TimeSpan position)
    {
        await _audioService.SeekToAsync(position);
    }
    
    protected override async Task SetVolumeInternalAsync(double volume)
    {
        // Apply volume boost multiplier
        double finalVolume = volume * VolumeBoostMultiplier;
        
        // Cap sent to player at 1.0, but boost logic is conceptual
        // Native players usually cap at 1.0 (100%)
        // If we want actual amplification, we need audio effects, but for now 
        // "Boost" just means allows going up to max system volume faster or logic scaling.
        // Simple logic: If boost is 1.5x, and user slider is 0.7, request is 1.05 -> clamped to 1.0
        
        await _audioService.SetVolumeAsync(finalVolume);
    }
    
    protected override async Task SetVolumeBoostInternalAsync(double boostMultiplier)
    {
        // Re-apply volume with new multiplier
        // We read the current conceptual volume from the base/ViewModel (not stored here directly easily, 
        // assume SetVolume is called by VM when boost changes, or we cache it).
        // For now, just acknowledged.
        await Task.CompletedTask; 
    }
    
    private async void OnPlaybackEnded(object? sender, EventArgs e)
    {
        // Scrobble the track that just finished (if it was valid)
        if (CurrentTrack != null)
        {
            await _scrobblerService.ScrobbleAsync(CurrentTrack, DateTime.Now);
        }

        // When track ends, try to play next
        await NextAsync();
    }
    
    private void OnPlaybackFailed(object? sender, Exception e)
    {
        State = PlaybackState.Error;
        System.Diagnostics.Debug.WriteLine($"Playback failed: {e.Message}");
    }
    
    public override void Dispose()
    {
        _audioService.PlaybackEnded -= OnPlaybackEnded;
        _audioService.PlaybackFailed -= OnPlaybackFailed;
        base.Dispose();
    }
}

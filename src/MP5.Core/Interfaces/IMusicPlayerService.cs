using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Core music player service for playback control.
/// </summary>
public interface IMusicPlayerService
{
    /// <summary>Play a specific track</summary>
    Task PlayAsync(Track track);
    
    /// <summary>Pause current playback</summary>
    Task PauseAsync();
    
    /// <summary>Resume paused playback</summary>
    Task ResumeAsync();
    
    /// <summary>Stop playback completely</summary>
    Task StopAsync();
    
    /// <summary>Seek to a specific position</summary>
    Task SeekAsync(TimeSpan position);
    
    /// <summary>Set volume (0.0 to 1.0)</summary>
    Task SetVolumeAsync(double volume);
    
    /// <summary>Set volume boost multiplier (1.0 = normal, up to 2.0 = boosted)</summary>
    Task SetVolumeBoostAsync(double boostMultiplier);
    
    /// <summary>Add a track to the end of the queue</summary>
    Task AddToQueueAsync(Track track);
    
    /// <summary>Whether generated autoplay is enabled</summary>
    bool AutoplayEnabled { get; set; }
    
    /// <summary>Skip to next track in queue</summary>
    Task NextAsync();
    
    /// <summary>Go to previous track in queue</summary>
    Task PreviousAsync();
    
    /// <summary>Current playback state</summary>
    PlaybackState State { get; }
    
    /// <summary>Currently playing track</summary>
    Track? CurrentTrack { get; }
    
    /// <summary>Current playback position</summary>
    TimeSpan Position { get; }
    
    /// <summary>Total duration of current track</summary>
    TimeSpan Duration { get; }
    
    /// <summary>Current volume (0.0 to 1.0)</summary>
    double Volume { get; }
    
    /// <summary>Whether volume boost is active</summary>
    bool IsVolumeBoosted { get; }
    
    /// <summary>Current volume boost multiplier</summary>
    double VolumeBoostMultiplier { get; }
    
    /// <summary>Fired when playback state changes</summary>
    event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    
    /// <summary>Fired when playback position changes</summary>
    event EventHandler<PositionChangedEventArgs>? PositionChanged;
}

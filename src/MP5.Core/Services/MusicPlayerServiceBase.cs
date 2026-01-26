using MP5.Core.Interfaces;
using MP5.Core.Models;
using System.Timers;

namespace MP5.Core.Services;

/// <summary>
/// Base music player service with queue management.
/// Platform-specific implementations should inherit from this.
/// </summary>
public abstract class MusicPlayerServiceBase : IMusicPlayerService, IDisposable
{
    protected readonly PlaybackQueue Queue = new();
    private readonly System.Timers.Timer _positionTimer;
    
    private PlaybackState _state = PlaybackState.Stopped;
    private Track? _currentTrack;
    private TimeSpan _position;
    private TimeSpan _duration;
    private double _volume = 0.7;
    private double _volumeBoostMultiplier = 1.0;
    
    public PlaybackState State
    {
        get => _state;
        protected set
        {
            if (_state != value)
            {
                _state = value;
                PlaybackStateChanged?.Invoke(this, new PlaybackStateChangedEventArgs
                {
                    State = value,
                    CurrentTrack = _currentTrack
                });
            }
        }
    }
    
    public Track? CurrentTrack
    {
        get => _currentTrack;
        protected set => _currentTrack = value;
    }
    
    public TimeSpan Position
    {
        get => _position;
        protected set
        {
            _position = value;
            PositionChanged?.Invoke(this, new PositionChangedEventArgs
            {
                Position = value,
                Duration = _duration
            });
        }
    }
    
    public TimeSpan Duration
    {
        get => _duration;
        protected set => _duration = value;
    }
    
    public double Volume
    {
        get => _volume;
        protected set => _volume = Math.Clamp(value, 0, 1);
    }
    
    public bool IsVolumeBoosted => _volumeBoostMultiplier > 1.0;
    
    public double VolumeBoostMultiplier
    {
        get => _volumeBoostMultiplier;
        protected set => _volumeBoostMultiplier = Math.Clamp(value, 1.0, 2.0);
    }
    
    public event EventHandler<PlaybackStateChangedEventArgs>? PlaybackStateChanged;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;
    
    protected MusicPlayerServiceBase()
    {
        _positionTimer = new System.Timers.Timer(250); // Update position every 250ms
        _positionTimer.Elapsed += OnPositionTimerElapsed;
    }
    
    private void OnPositionTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (State == PlaybackState.Playing)
        {
            UpdatePosition();
        }
    }
    
    /// <summary>
    /// Platform-specific position update implementation.
    /// </summary>
    protected abstract void UpdatePosition();
    
    /// <summary>
    /// Platform-specific play implementation.
    /// </summary>
    protected abstract Task PlayInternalAsync(string source);
    
    /// <summary>
    /// Platform-specific pause implementation.
    /// </summary>
    protected abstract Task PauseInternalAsync();
    
    /// <summary>
    /// Platform-specific resume implementation.
    /// </summary>
    protected abstract Task ResumeInternalAsync();
    
    /// <summary>
    /// Platform-specific stop implementation.
    /// </summary>
    protected abstract Task StopInternalAsync();
    
    /// <summary>
    /// Platform-specific seek implementation.
    /// </summary>
    protected abstract Task SeekInternalAsync(TimeSpan position);
    
    /// <summary>
    /// Platform-specific volume implementation.
    /// </summary>
    protected abstract Task SetVolumeInternalAsync(double volume);
    
    /// <summary>
    /// Platform-specific volume boost implementation.
    /// </summary>
    protected abstract Task SetVolumeBoostInternalAsync(double boostMultiplier);
    
    public virtual async Task PlayAsync(Track track)
    {
        // If this track is part of a queue, update position
        // Otherwise, create a single-track queue
        if (Queue.CurrentTrack?.Id != track.Id)
        {
            Queue.SetQueue([track], track);
        }
        
        CurrentTrack = track;
        Duration = track.Duration;
        Position = TimeSpan.Zero;
        State = PlaybackState.Buffering;
        
        try
        {
            // Get the stream URL or local path
            var source = track.LocalFilePath ?? track.SourceId;
            await PlayInternalAsync(source);
            
            State = PlaybackState.Playing;
            _positionTimer.Start();
        }
        catch (Exception)
        {
            State = PlaybackState.Error;
            throw;
        }
    }
    
    public virtual async Task PauseAsync()
    {
        if (State == PlaybackState.Playing)
        {
            await PauseInternalAsync();
            State = PlaybackState.Paused;
            _positionTimer.Stop();
        }
    }
    
    public virtual async Task ResumeAsync()
    {
        if (State == PlaybackState.Paused)
        {
            await ResumeInternalAsync();
            State = PlaybackState.Playing;
            _positionTimer.Start();
        }
        else if (State == PlaybackState.Stopped && CurrentTrack != null)
        {
            await PlayAsync(CurrentTrack);
        }
    }
    
    public virtual async Task StopAsync()
    {
        await StopInternalAsync();
        State = PlaybackState.Stopped;
        Position = TimeSpan.Zero;
        _positionTimer.Stop();
    }
    
    public virtual async Task SeekAsync(TimeSpan position)
    {
        await SeekInternalAsync(position);
        Position = position;
    }
    
    public virtual async Task SetVolumeAsync(double volume)
    {
        Volume = volume;
        await SetVolumeInternalAsync(volume * VolumeBoostMultiplier);
    }
    
    public virtual async Task SetVolumeBoostAsync(double boostMultiplier)
    {
        VolumeBoostMultiplier = boostMultiplier;
        await SetVolumeBoostInternalAsync(boostMultiplier);
        // Reapply volume with new boost
        await SetVolumeInternalAsync(Volume * VolumeBoostMultiplier);
    }
    
    public bool AutoplayEnabled { get; set; } = true;
    protected IMusicSourceService? MusicSourceService { get; set; } // Protected setter for injection in derived classes

    public virtual async Task AddToQueueAsync(Track track)
    {
        Queue.AddToQueue(track);
        // If stopped/empty, play immediately
        if (State == PlaybackState.Stopped && CurrentTrack == null)
        {
             await PlayAsync(track);
        }
    }

    public virtual async Task NextAsync()
    {
        var nextTrack = Queue.GetNext();
        if (nextTrack != null)
        {
            await PlayAsync(nextTrack);
        }
        else
        {
            // End of queue reached. Check Autoplay.
            if (AutoplayEnabled && CurrentTrack != null && MusicSourceService != null)
            {
                 System.Diagnostics.Debug.WriteLine($"Autoplay triggering for: {CurrentTrack.Title}");
                 try 
                 {
                     var recommendations = await MusicSourceService.GetRecommendationsAsync(CurrentTrack);
                     var nextAuto = recommendations.FirstOrDefault();
                     
                     if (nextAuto != null)
                     {
                         System.Diagnostics.Debug.WriteLine($"Autoplay found: {nextAuto.Title}");
                         Queue.AddToQueue(nextAuto);
                         await NextAsync(); // Recurse to play it
                         return;
                     }
                 }
                 catch (Exception ex)
                 {
                     System.Diagnostics.Debug.WriteLine($"Autoplay processing failed: {ex.Message}");
                 }
            }
            
            await StopAsync();
        }
    }
    
    public virtual async Task PreviousAsync()
    {
        // If more than 3 seconds into track, restart instead of going to previous
        if (Position.TotalSeconds > 3)
        {
            await SeekAsync(TimeSpan.Zero);
            return;
        }
        
        var prevTrack = Queue.GetPrevious();
        if (prevTrack != null)
        {
            await PlayAsync(prevTrack);
        }
    }
    
    public virtual void Dispose()
    {
        _positionTimer.Stop();
        _positionTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}

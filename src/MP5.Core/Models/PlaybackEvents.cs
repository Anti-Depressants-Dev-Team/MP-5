namespace MP5.Core.Models;

/// <summary>
/// Event args for playback state changes.
/// </summary>
public class PlaybackStateChangedEventArgs : EventArgs
{
    public required PlaybackState State { get; init; }
    public Track? CurrentTrack { get; init; }
}

/// <summary>
/// Event args for position changes during playback.
/// </summary>
public class PositionChangedEventArgs : EventArgs
{
    public TimeSpan Position { get; init; }
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Current playback state.
/// </summary>
public enum PlaybackState
{
    Stopped,
    Playing,
    Paused,
    Buffering,
    Error
}

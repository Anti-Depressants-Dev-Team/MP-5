using System;
using System.Threading.Tasks;

namespace MP5.Core.Interfaces;

public interface IAudioService : IAsyncDisposable
{
    event EventHandler PlaybackEnded;
    event EventHandler<Exception> PlaybackFailed;

    Task InitializeAsync(string audioUri);
    Task PlayAsync();
    Task PauseAsync();
    Task StopAsync();
    Task SetVolumeAsync(double volume);
    Task SeekToAsync(TimeSpan position);
    
    TimeSpan Duration { get; }
    TimeSpan Position { get; }
    bool IsPlaying { get; }
}

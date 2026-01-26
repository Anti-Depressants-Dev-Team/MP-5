using MP5.Core.Models;

namespace MP5.Core.Interfaces;

public interface ILyricsProvider
{
    string Name { get; }
    Task<Lyrics?> GetLyricsAsync(Track track);
}

public interface ILyricsService
{
    Task<Lyrics?> GetLyricsAsync(Track track);
}

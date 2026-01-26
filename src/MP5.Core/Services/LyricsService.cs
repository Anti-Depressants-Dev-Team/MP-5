using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class LyricsService : ILyricsService
{
    private readonly IEnumerable<ILyricsProvider> _providers;

    public LyricsService(IEnumerable<ILyricsProvider> providers)
    {
        _providers = providers;
    }

    public async Task<Lyrics?> GetLyricsAsync(Track track)
    {
        foreach (var provider in _providers)
        {
            var lyrics = await provider.GetLyricsAsync(track);
            if (lyrics != null)
            {
                return lyrics;
            }
        }
        return null;
    }
}

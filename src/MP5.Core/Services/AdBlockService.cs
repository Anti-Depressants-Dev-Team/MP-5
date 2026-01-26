using MP5.Core.Models;
using MP5.Core.Interfaces;

namespace MP5.Core.Services;

public class AdBlockService : IAdBlockService
{
    public bool IsEnabled { get; set; } = true;

    // Keywords that strongly suggest spam, ads, or irrelevant content
    private readonly string[] _adKeywords = 
    {
        "marketin", "trailer", "teaser", "preview", "advertisement", "commercial", 
        "buy now", "click link", "promotional", "interview", "commentary", "reaction"
    };

    public bool IsAd(Track track)
    {
        if (!IsEnabled) return false;

        // 1. Duration Check
        // Songs correspond to Music Video usually > 1:30. 
        // Anything < 30s is likely a short, intro, or ad.
        if (track.Duration.TotalSeconds < 30 && track.Duration.TotalSeconds > 0)
        {
             return true;
        }

        // 2. Keyword Check
        if (string.IsNullOrEmpty(track.Title)) return false;
        var titleLower = track.Title.ToLowerInvariant();
        if (_adKeywords.Any(k => titleLower.Contains(k)))
        {
            // Exception: "Official" + "Trailer" sometimes matches songs, but usually "Official Video" is fine.
            // If it literally says "Trailer", skip it.
             return true;
        }

        return false;
    }

    public IEnumerable<Track> RemoveAds(IEnumerable<Track> tracks)
    {
        if (tracks == null) return Enumerable.Empty<Track>();
        if (!IsEnabled) return tracks;
        return tracks.Where(t => t != null && !IsAd(t));
    }
}

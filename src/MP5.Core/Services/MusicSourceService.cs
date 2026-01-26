using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class MusicSourceService : IMusicSourceService
{
    private readonly IEnumerable<IMusicSource> _sources;
    private readonly IOfflineService _offlineService;
    private readonly IAdBlockService _adBlockService;

    public MusicSourceService(IEnumerable<IMusicSource> sources, IOfflineService offlineService, IAdBlockService adBlockService)
    {
        _sources = sources;
        _offlineService = offlineService;
        _adBlockService = adBlockService;
    }

    public async Task<IEnumerable<Track>> SearchAsync(string query)
    {
        // For now, primarly search using the first available source (YouTube)
        // In the future, we can aggregate results from multiple sources
        var primarySource = _sources.FirstOrDefault();
        if (primarySource == null)
        {
            return Enumerable.Empty<Track>();
        }

        var results = await primarySource.SearchAsync(query);
        return _adBlockService.RemoveAds(results);
    }

    public async Task<string> GetStreamUrlAsync(Track track)
    {
        if (string.IsNullOrEmpty(track.SourceId))
            return string.Empty;

        // 0. Check offline availability
        var offlinePath = _offlineService.GetOfflinePath(track);
        if (!string.IsNullOrEmpty(offlinePath))
        {
            System.Diagnostics.Debug.WriteLine($"Playing from offline cache: {track.Title}");
            return offlinePath;
        }

        // 1. Try to find the specific source if specified in the track
        // (For now, we just use the first matching source capability or default to YouTube)
        
        // Simple strategy: Go through sources and try to resolve
        foreach (var source in _sources)
        {
             if (source.Name == "YouTube" && track.Source == MusicSourceType.YouTube)
            {
                var url = await source.GetAudioStreamUrlAsync(track.SourceId);
                if (!string.IsNullOrEmpty(url)) return url;
            }
             else if (source.Name == "SoundCloud" && track.Source == MusicSourceType.SoundCloud)
             {
                 var url = await source.GetAudioStreamUrlAsync(track.SourceId);
                 if (!string.IsNullOrEmpty(url)) return url;
             }
        }
        
        // --- FALLBACK ARCHITECTURE ---
        // If we reached here, the primary source failed.
        // Let's try to find this track on OTHER sources.
        
        System.Diagnostics.Debug.WriteLine($"Primary source resolution failed for '{track.Title}'. Attempting fallback...");
        
        string searchQuery = $"{track.Artist} - {track.Title}";
        
        foreach (var source in _sources)
        {
            // Skip the source that already failed (optimization)
            if (track.Source == MusicSourceType.YouTube && source.Name == "YouTube") continue;
            if (track.Source == MusicSourceType.SoundCloud && source.Name == "SoundCloud") continue;
            
            try
            {
                // Search this fallback source
                var searchResults = await source.SearchAsync(searchQuery);
                var filteredResults = _adBlockService.RemoveAds(searchResults);
                var fallbackMatch = filteredResults.FirstOrDefault();
                
                if (fallbackMatch != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback match found on {source.Name}: {fallbackMatch.Title}");
                    var url = await source.GetAudioStreamUrlAsync(fallbackMatch.SourceId);
                    if (!string.IsNullOrEmpty(url))
                    {
                         System.Diagnostics.Debug.WriteLine($"Fallback stream resolved!");
                         return url;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback attempt on {source.Name} failed: {ex.Message}");
            }
        }
        
        return string.Empty;
    }


    public async Task<IEnumerable<Track>> GetRecommendationsAsync(Track seedTrack)
    {
        // Smart recommendation strategy:
        // 1. "Mix" based on artist
        // 2. "Radio" based on track title
        
        string query = $"Msg mix {seedTrack.Artist}";
        if (string.IsNullOrWhiteSpace(seedTrack.Artist) || seedTrack.Artist == "Unknown Artist")
        {
            query = $"{seedTrack.Title} similar songs";
        }
        
        return await SearchAsync(query);
    }
}

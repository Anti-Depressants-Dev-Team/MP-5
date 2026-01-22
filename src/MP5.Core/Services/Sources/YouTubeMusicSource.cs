using MP5.Core.Interfaces;
using MP5.Core.Models;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;

namespace MP5.Core.Services.Sources;

public class YouTubeMusicSource : IMusicSource
{
    private readonly YoutubeClient _youtube = new();
    
    public string Name => "YouTube";

    public async Task<IEnumerable<Track>> SearchAsync(string query)
    {
        var results = new List<Track>();
        try
        {
            // Search for videos
            // We use 'await foreach' because Search.GetResultAsync returns IAsyncEnumerable for pagination
            // We'll take the first 20 results for now
            var videoResults = _youtube.Search.GetVideosAsync(query);
            
            int count = 0;
            await foreach (var video in videoResults)
            {
                if (count >= 20) break;
                
                var track = new Track
                {
                    Id = Guid.NewGuid().ToString(), // Internal ID
                    SourceId = video.Id.Value, // YouTube Video ID
                    Title = video.Title,
                    Artist = video.Author.ChannelTitle,
                    Duration = video.Duration ?? TimeSpan.Zero,
                    ThumbnailUrl = video.Thumbnails.TryGetWithHighestResolution()?.Url ?? "",
                    Album = "YouTube", // Default album name
                    Source = MusicSourceType.YouTube
                };
                
                results.Add(track);
                count++;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"YouTube search failed: {ex.Message}");
            // Return empty list on failure for now
        }
        
        return results;
    }

    public async Task<string> GetAudioStreamUrlAsync(string trackId)
    {
        try
        {
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(trackId);
            
            // Get audio only stream with highest bitrate
            var streamInfo = streamManifest
                .GetAudioOnlyStreams()
                .GetWithHighestBitrate();
                
            return streamInfo?.Url ?? string.Empty;
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"YouTube stream resolution failed: {ex.Message}");
             return string.Empty;
        }
    }
}

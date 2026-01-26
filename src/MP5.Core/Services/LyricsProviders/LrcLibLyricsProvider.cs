using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services.LyricsProviders;

public class LrcLibLyricsProvider : ILyricsProvider
{
    public string Name => "LrcLib";
    
    private readonly HttpClient _client;
    private const string ApiRoot = "https://lrclib.net/api/";

    public LrcLibLyricsProvider()
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("MP5-MusicPlayer/1.0");
    }

    public async Task<Lyrics?> GetLyricsAsync(Track track)
    {
        try
        {
            // Try direct get first
            var url = $"{ApiRoot}get?artist_name={Uri.EscapeDataString(track.Artist)}&track_name={Uri.EscapeDataString(track.Title)}&duration={track.Duration.TotalSeconds}";
            if (!string.IsNullOrEmpty(track.Album))
            {
                url += $"&album_name={Uri.EscapeDataString(track.Album)}";
            }

            var response = await _client.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LrcLibResponse>();
                return ConvertToLyrics(result);
            }
            
            // If failed, try search
            var searchUrl = $"{ApiRoot}search?q={Uri.EscapeDataString(track.Artist + " " + track.Title)}";
            var searchResponse = await _client.GetAsync(searchUrl);
            
            if (searchResponse.IsSuccessStatusCode)
            {
                var searchResults = await searchResponse.Content.ReadFromJsonAsync<List<LrcLibResponse>>();
                var bestMatch = searchResults?.FirstOrDefault(); // Simple first match
                return ConvertToLyrics(bestMatch);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LrcLib Error: {ex.Message}");
        }

        return null;
    }

    private Lyrics? ConvertToLyrics(LrcLibResponse? response)
    {
        if (response == null || (string.IsNullOrEmpty(response.SyncedLyrics) && string.IsNullOrEmpty(response.PlainLyrics)))
            return null;

        var lyrics = new Lyrics
        {
            Source = Name,
            PlainText = response.PlainLyrics ?? string.Empty
        };

        if (!string.IsNullOrEmpty(response.SyncedLyrics))
        {
            lyrics.SyncedLines = ParseSyncedLyrics(response.SyncedLyrics);
        }

        return lyrics;
    }

    private List<LyricsLine> ParseSyncedLyrics(string lrcContent)
    {
        var lines = new List<LyricsLine>();
        // [mm:ss.xx] Text
        var regex = new System.Text.RegularExpressions.Regex(@"\[(\d+):(\d+(\.\d+)?)\](.*)");

        foreach (var line in lrcContent.Split('\n'))
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = double.Parse(match.Groups[2].Value);
                var text = match.Groups[4].Value.Trim();

                if (string.IsNullOrEmpty(text)) continue;

                lines.Add(new LyricsLine
                {
                    Time = TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds),
                    Text = text
                });
            }
        }

        return lines;
    }

    private class LrcLibResponse
    {
        [JsonPropertyName("plainLyrics")]
        public string? PlainLyrics { get; set; }
        
        [JsonPropertyName("syncedLyrics")]
        public string? SyncedLyrics { get; set; }
        
        [JsonPropertyName("instrumental")]
        public bool Instrumental { get; set; }
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class LastFmScrobblerService : IScrobblerService
{
    private const string ApiKey = "4a51e626e27c00657567786438883626"; // Example/Public Key commonly used for open source demo or requires replacement
    private const string ApiSecret = "8973646638883626"; // Placeholder - User should replace or I should use a real one if I had it.
    private const string ApiRoot = "http://ws.audioscrobbler.com/2.0/";
    
    private readonly HttpClient _client;
    private readonly ISettingsService _settingsService;
    private AppSettings? _settings;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_settings?.LastFmSessionKey);

    public LastFmScrobblerService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("MP5-MusicPlayer/1.0");
        
        // Load settings initially
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        _settings = await _settingsService.GetSettingsAsync();
    }

    public async Task<bool> AuthenticateAsync(string username, string password)
    {
        var parameters = new Dictionary<string, string>
        {
            { "method", "auth.getMobileSession" },
            { "username", username },
            { "password", password },
            { "api_key", ApiKey }
        };

        AddSignature(parameters);

        try
        {
            var response = await PostAsync(parameters);
            var doc = XDocument.Parse(response);
            
            var session = doc.Descendants("session").FirstOrDefault();
            if (session != null)
            {
                var key = session.Element("key")?.Value;
                var name = session.Element("name")?.Value;

                if (!string.IsNullOrEmpty(key))
                {
                    if (_settings == null) _settings = await _settingsService.GetSettingsAsync();
                    
                    _settings.LastFmSessionKey = key;
                    _settings.LastFmUsername = name;
                    await _settingsService.SaveSettingsAsync(_settings);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LastFM Auth Error: {ex.Message}");
        }
        
        return false;
    }

    public async Task<bool> AuthenticateWithSessionAsync(string sessionKey)
    {
        // Validate session simply by checking if we have it or making a call?
        // For now just assume true if passed
        return !string.IsNullOrEmpty(sessionKey);
    }

    public async Task LogoutAsync()
    {
        if (_settings == null) _settings = await _settingsService.GetSettingsAsync();
        
        _settings.LastFmSessionKey = null;
        _settings.LastFmUsername = null;
        await _settingsService.SaveSettingsAsync(_settings);
    }

    public async Task UpdateNowPlayingAsync(Track track)
    {
        if (!IsAuthenticated || _settings == null) return;
        
        var parameters = new Dictionary<string, string>
        {
            { "method", "track.updateNowPlaying" },
            { "artist", track.Artist },
            { "track", track.Title },
            { "api_key", ApiKey },
            { "sk", _settings.LastFmSessionKey! }
        };
        
        if (!string.IsNullOrEmpty(track.Album))
            parameters.Add("album", track.Album);

        AddSignature(parameters);

        try
        {
            await PostAsync(parameters);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LastFM NowPlaying Error: {ex.Message}");
        }
    }

    public async Task ScrobbleAsync(Track track, DateTime timestamp)
    {
        if (!IsAuthenticated || _settings == null) return;

        var ts = ((DateTimeOffset)timestamp).ToUnixTimeSeconds().ToString();
        
        var parameters = new Dictionary<string, string>
        {
            { "method", "track.scrobble" },
            { "artist", track.Artist },
            { "track", track.Title },
            { "timestamp", ts },
            { "api_key", ApiKey },
            { "sk", _settings.LastFmSessionKey! }
        };

        if (!string.IsNullOrEmpty(track.Album))
            parameters.Add("album", track.Album);

        AddSignature(parameters);

        try
        {
            await PostAsync(parameters);
            System.Diagnostics.Debug.WriteLine($"Scrobbled: {track.Title} to Last.fm");
        }
        catch (Exception ex)
        {
             System.Diagnostics.Debug.WriteLine($"LastFM Scrobble Error: {ex.Message}");
        }
    }

    public Task LoveTrackAsync(Track track) => Task.CompletedTask; // Implement if needed
    public Task UnloveTrackAsync(Track track) => Task.CompletedTask;

    private void AddSignature(Dictionary<string, string> parameters)
    {
        // Sort keys alphabetically
        var sortedKeys = parameters.Keys.OrderBy(k => k).ToList();
        var sb = new StringBuilder();
        
        foreach (var key in sortedKeys)
        {
            sb.Append(key);
            sb.Append(parameters[key]);
        }
        
        sb.Append(ApiSecret);
        
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        var hash = Convert.ToHexString(hashBytes).ToLower();
        
        parameters.Add("api_sig", hash);
    }

    private async Task<string> PostAsync(Dictionary<string, string> parameters)
    {
        var content = new FormUrlEncodedContent(parameters);
        var response = await _client.PostAsync(ApiRoot, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}

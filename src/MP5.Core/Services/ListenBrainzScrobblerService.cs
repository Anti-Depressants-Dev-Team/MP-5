using System.Net.Http.Json;
using System.Text.Json.Serialization;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class ListenBrainzScrobblerService : IScrobblerService
{
    private const string ApiRoot = "https://api.listenbrainz.org/1/";
    private readonly HttpClient _client;
    private readonly ISettingsService _settingsService;
    private AppSettings? _settings;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_settings?.ListenBrainzToken);

    public ListenBrainzScrobblerService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("MP5-MusicPlayer/1.0");
        
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        _settings = await _settingsService.GetSettingsAsync();
    }

    // Authenticate here means "Validate Token"
    // Using username param as token placeholder if needed, or just specialized method.
    // Interface has (username, password). ListenBrainz uses Token only.
    // We will assume 'password' argument contains the Token if username is "token" or empty.
    public async Task<bool> AuthenticateAsync(string username, string token)
    {
        if (string.IsNullOrEmpty(token)) return false;
        
        try
        {
            var isValid = await ValidateTokenAsync(token);
            if (isValid)
            {
                if (_settings == null) _settings = await _settingsService.GetSettingsAsync();
                _settings.ListenBrainzToken = token;
                await _settingsService.SaveSettingsAsync(_settings);
                return true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ListenBrainz Auth Error: {ex.Message}");
        }
        return false;
    }

    public Task<bool> AuthenticateWithSessionAsync(string sessionKey)
    {
        // Not applicable essentially, but we can validate it.
        return Task.FromResult(!string.IsNullOrEmpty(sessionKey));
    }

    public async Task LogoutAsync()
    {
        if (_settings == null) _settings = await _settingsService.GetSettingsAsync();
        _settings.ListenBrainzToken = null;
        await _settingsService.SaveSettingsAsync(_settings);
    }

    public async Task UpdateNowPlayingAsync(Track track)
    {
        if (!IsAuthenticated || _settings == null) return;
        await SubmitListenAsync(track, "playing_now");
    }

    public async Task ScrobbleAsync(Track track, DateTime timestamp)
    {
        if (!IsAuthenticated || _settings == null) return;
        await SubmitListenAsync(track, "single", timestamp);
    }

    public Task LoveTrackAsync(Track track) => Task.CompletedTask; // Not implemented yet (feedback API)
    public Task UnloveTrackAsync(Track track) => Task.CompletedTask;

    private async Task<bool> ValidateTokenAsync(string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{ApiRoot}validate-token?token={token}");
        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return false;
        
        var json = await response.Content.ReadFromJsonAsync<ListenBrainzResponse>();
        return json?.Valid ?? false; // API returns { "valid": true, ... }
    }

    private async Task SubmitListenAsync(Track track, string listenType, DateTime? timestamp = null)
    {
         var payload = new
         {
             listen_type = listenType,
             payload = new[]
             {
                 new
                 {
                     track_metadata = new
                     {
                         artist_name = track.Artist,
                         track_name = track.Title,
                         release_name = track.Album,
                         additional_info = new
                         {
                             media_player = "MP5",
                             submission_client = "MP5",
                             duration_ms = track.Duration.TotalMilliseconds > 0 ? (int?)track.Duration.TotalMilliseconds : null
                         }
                     },
                     listened_at = timestamp.HasValue 
                        ? ((DateTimeOffset)timestamp.Value).ToUnixTimeSeconds() 
                        : (long?)null
                 }
             }
         };

         var request = new HttpRequestMessage(HttpMethod.Post, $"{ApiRoot}submit-listens");
         request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Token", _settings!.ListenBrainzToken);
         request.Content = JsonContent.Create(payload);
         
         try
         {
             var response = await _client.SendAsync(request);
             response.EnsureSuccessStatusCode();
         }
         catch(Exception ex)
         {
             System.Diagnostics.Debug.WriteLine($"ListenBrainz Submit Error: {ex.Message}");
         }
    }
    
    private class ListenBrainzResponse
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }
    }
}

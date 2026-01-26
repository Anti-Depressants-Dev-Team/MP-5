using DiscordRPC;
using DiscordRPC.Logging;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class DiscordRpcService : IDiscordRpcService, IDisposable
{
    private const string ClientId = "1461757069769314425"; // Updated App ID from Discord Dev Portal
    private DiscordRpcClient? _client;
    private readonly ISettingsService _settingsService;
    private bool _isEnabled;

    public bool IsConnected => _client != null && _client.IsInitialized;

    public DiscordRpcService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _ = InitializeAsync(ClientId);
    }

    public async Task InitializeAsync(string clientId)
    {
        var settings = await _settingsService.GetSettingsAsync();
        _isEnabled = settings.EnableDiscordRpc;

        if (_isEnabled)
        {
            InitializeClient(clientId);
        }
    }

    public void SetEnabled(bool isEnabled)
    {
        _isEnabled = isEnabled;
        if (_isEnabled)
        {
            InitializeClient(ClientId);
        }
        else
        {
            Dispose();
        }
    }

    private void InitializeClient(string clientId)
    {
        if (_client != null && !_client.IsDisposed) return;

        try
        {
            _client = new DiscordRpcClient(clientId);
            _client.Logger = new ConsoleLogger(LogLevel.Trace); // Trace for debugging
            
            _client.OnReady += (sender, e) => 
            {
                 System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Ready: {e.User.Username}");
            };
            
            _client.OnConnectionFailed += (sender, e) =>
            {
                 System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Connection Failed: {e}");
            };

            _client.OnError += (sender, e) =>
            {
                 System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Error: {e.Message} ({e.Code})");
            };
            
            _client.Initialize();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Init Exception: {ex.Message}");
        }
    }

    public Task UpdatePresenceAsync(Track track, bool isPlaying, TimeSpan position)
    {
        try
        {
            if (!_isEnabled) return Task.CompletedTask;
            if (_client == null || !IsConnected) 
            {
                System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Update skipped: Client not ready");
                return Task.CompletedTask;
            }

            System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Updating Presence: {track.Title} - {track.Artist}");
            
            var state = isPlaying ? PlaybackState.Playing : PlaybackState.Paused;
            
            var presence = new RichPresence()
            {
                Details = track.Title,
                State = $"by {track.Artist}"
            };

            if (state == PlaybackState.Playing)
            {
                if (track.Duration != TimeSpan.Zero)
                {
                   var timeLeft = track.Duration - position;
                   var endTime = DateTime.UtcNow + timeLeft;
                   presence.Timestamps = new Timestamps()
                   {
                       Start = DateTime.UtcNow,
                       End = endTime
                   };
                }
                else
                {
                   presence.Timestamps = Timestamps.Now;
                }
            }
            else
            {
                // presence.Assets.SmallImageKey = "pause"; // Disabled for text-only mode
                presence.State += " (Paused)";
            }

            _client.SetPresence(presence);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DiscordRPC] Critical Update Error: {ex.Message}");
        }
        return Task.CompletedTask;
    }

    public Task ClearPresenceAsync()
    {
        if (_client != null && IsConnected)
        {
            _client.ClearPresence();
        }
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _client?.Dispose();
        _client = null;
    }
}

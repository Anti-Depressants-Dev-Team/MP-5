using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.App.Services;

public class DiscordRpcServiceStub : IDiscordRpcService
{
    public bool IsConnected => false;
    public Task InitializeAsync(string clientId) => Task.CompletedTask;
    public Task UpdatePresenceAsync(Track track, bool isPlaying, TimeSpan position) => Task.CompletedTask;
    public Task ClearPresenceAsync() => Task.CompletedTask;
    public Task DisconnectAsync() => Task.CompletedTask;
}

public class SyncServiceStub : ISyncService
{
    public bool IsSignedIn => false;
    public string? UserEmail => null;
    public DateTime? LastSyncTime => null;

    public Task<bool> SignInAsync() => Task.FromResult(false);
    public Task SignOutAsync() => Task.CompletedTask;
    public Task SyncPlaylistsAsync(IEnumerable<Playlist> playlists) => Task.CompletedTask;
    public Task<IEnumerable<Playlist>> GetCloudPlaylistsAsync() => Task.FromResult(Enumerable.Empty<Playlist>());
    public Task SyncSettingsAsync(AppSettings settings) => Task.CompletedTask;
    public Task<AppSettings?> GetCloudSettingsAsync() => Task.FromResult<AppSettings?>(null);
}

public class UpdateServiceStub : IUpdateService
{
    public string CurrentVersion => "1.0.0";
    public Task<UpdateInfo?> CheckForUpdateAsync() => Task.FromResult<UpdateInfo?>(null);
    public Task<bool> DownloadAndInstallAsync(UpdateInfo update) => Task.FromResult(false);
}

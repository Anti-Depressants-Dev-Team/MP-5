using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class GoogleSyncService : ISyncService
{
    // Placeholder Client ID/Secret - In a real app these should be secured or obtained via User Input/Config
    // These are dummy values for the structure. The user would need to replace them or we provide a way to input them.
    private const string ClientId = "YOUR_CLIENT_ID.apps.googleusercontent.com";
    private const string ClientSecret = "YOUR_CLIENT_SECRET";
    
    // Scopes: Drive AppData (hidden folder) and UserInfo Email
    private static readonly string[] Scopes = { DriveService.Scope.DriveAppdata, "email" };
    private const string ApplicationName = "MP5 Music Player";
    
    private UserCredential? _credential;
    private DriveService? _driveService;
    private string? _userEmail;
    
    public bool IsSignedIn => _credential != null;
    public string? UserEmail => _userEmail;
    public DateTime? LastSyncTime { get; private set; }

    public async Task<bool> SignInAsync()
    {
        try
        {
            // Using FileDataStore for simplicity on Desktop. 
            // folder: "token.json" in local app data.
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var tokenPath = Path.Combine(appData, "MP5", "token.json");
            
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                },
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(tokenPath, true));

            // Initialize Drive Service
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = ApplicationName,
            });

            // Get User Email (from About or UserInfo)
            var aboutRequest = _driveService.About.Get();
            aboutRequest.Fields = "user";
            var about = await aboutRequest.ExecuteAsync();
            _userEmail = about.User.EmailAddress;
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Google Sign In Failed: {ex.Message}");
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        if (_credential != null)
        {
            await _credential.RevokeTokenAsync(CancellationToken.None);
            _credential = null;
            _driveService = null;
            _userEmail = null;
            
            // Should also clear FileDataStore if possible, or delete the folder
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var tokenPath = Path.Combine(appData, "MP5", "token.json");
                if (Directory.Exists(tokenPath)) Directory.Delete(tokenPath, true);
            }
            catch { }
        }
    }

    public async Task SyncPlaylistsAsync(IEnumerable<Playlist> playlists)
    {
        if (!IsSignedIn || _driveService == null) return;
        
        try 
        {
            var json = JsonSerializer.Serialize(playlists);
            await UploadFileAsync("playlists.json", json);
            LastSyncTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sync Playlists Failed: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Playlist>> GetCloudPlaylistsAsync()
    {
        if (!IsSignedIn || _driveService == null) return Enumerable.Empty<Playlist>();
        
        try
        {
            var json = await DownloadFileAsync("playlists.json");
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<IEnumerable<Playlist>>(json) ?? Enumerable.Empty<Playlist>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Get Cloud Playlists Failed: {ex.Message}");
        }
        return Enumerable.Empty<Playlist>();
    }

    public async Task SyncSettingsAsync(AppSettings settings)
    {
        if (!IsSignedIn || _driveService == null) return;
        
        try
        {
            var json = JsonSerializer.Serialize(settings);
            await UploadFileAsync("settings.json", json);
            LastSyncTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Sync Settings Failed: {ex.Message}");
        }
    }

    public async Task<AppSettings?> GetCloudSettingsAsync()
    {
        if (!IsSignedIn || _driveService == null) return null;
        
        try
        {
            var json = await DownloadFileAsync("settings.json");
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<AppSettings>(json);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Get Cloud Settings Failed: {ex.Message}");
        }
        return null;
    }

    private async Task UploadFileAsync(string fileName, string content)
    {
        // 1. Find file
        var fileId = await FindFileIdAsync(fileName);
        
        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName,
            Parents = new List<string> { "appDataFolder" }
        };

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

        if (fileId == null)
        {
            // Create
            var request = _driveService!.Files.Create(fileMetadata, stream, "application/json");
            request.Fields = "id";
            await request.UploadAsync();
        }
        else
        {
            // Update
            var request = _driveService!.Files.Update(fileMetadata, fileId, stream, "application/json");
            await request.UploadAsync();
        }
    }

    private async Task<string?> DownloadFileAsync(string fileName)
    {
        var fileId = await FindFileIdAsync(fileName);
        if (fileId == null) return null;

        var request = _driveService!.Files.Get(fileId);
        using var stream = new MemoryStream();
        await request.DownloadAsync(stream);
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private async Task<string?> FindFileIdAsync(string fileName)
    {
        var request = _driveService!.Files.List();
        request.Q = $"name = '{fileName}' and 'appDataFolder' in parents and trashed = false";
        request.Spaces = "appDataFolder";
        request.Fields = "files(id, name)";
        
        var result = await request.ExecuteAsync();
        return result.Files.FirstOrDefault()?.Id;
    }
}

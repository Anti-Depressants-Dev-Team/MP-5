using MP5.Core.Interfaces;
using MP5.Core.Models;

namespace MP5.Core.Services;

public class OfflineService : IOfflineService
{
    private string _offlineFolder = string.Empty;
    private readonly HttpClient _client;

    public OfflineService()
    {
        _client = new HttpClient();
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _offlineFolder = Path.Combine(appData, "MP5", "Offline");
        
        if (!Directory.Exists(_offlineFolder))
        {
            Directory.CreateDirectory(_offlineFolder);
        }
    }

    public bool IsTrackDownloaded(Track track)
    {
        var path = GetFilePath(track);
        return File.Exists(path);
    }

    public string? GetOfflinePath(Track track)
    {
        var path = GetFilePath(track);
        return File.Exists(path) ? path : null;
    }

    public async Task DownloadTrackAsync(Track track, string streamUrl, IProgress<double>? progress = null)
    {
        var filePath = GetFilePath(track);
        if (File.Exists(filePath)) return;

        try 
        {
            using var response = await _client.GetAsync(streamUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytes != -1 && progress != null;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var totalRead = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            while (isMoreToRead)
            {
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await fileStream.WriteAsync(buffer, 0, read);

                    totalRead += read;
                    if (canReportProgress)
                    {
                        progress!.Report((double)totalRead / totalBytes);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Clean up partial file
            if (File.Exists(filePath)) File.Delete(filePath);
            System.Diagnostics.Debug.WriteLine($"Download failed: {ex.Message}");
            throw;
        }
    }

    public Task RemoveTrackAsync(Track track)
    {
        var filePath = GetFilePath(track);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        return Task.CompletedTask;
    }

    private string GetFilePath(Track track)
    {
        // Sanitize filename
        var safeId = string.Join("_", track.Id.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_offlineFolder, $"{safeId}.mp3");
    }
}

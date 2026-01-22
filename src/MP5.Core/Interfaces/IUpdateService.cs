namespace MP5.Core.Interfaces;

/// <summary>
/// Update service for checking and notifying about app updates.
/// </summary>
public interface IUpdateService
{
    /// <summary>Current app version</summary>
    string CurrentVersion { get; }
    
    /// <summary>Check for updates</summary>
    Task<UpdateInfo?> CheckForUpdateAsync();
    
    /// <summary>Download and install update</summary>
    Task<bool> DownloadAndInstallAsync(UpdateInfo update);
}

/// <summary>
/// Update information.
/// </summary>
public class UpdateInfo
{
    public required string Version { get; init; }
    public required string DownloadUrl { get; init; }
    public string? ReleaseNotes { get; init; }
    public DateTime ReleaseDate { get; init; }
    public bool IsMandatory { get; init; }
}

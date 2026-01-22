using MP5.Core.Models;

namespace MP5.Core.Interfaces;

/// <summary>
/// Settings persistence service.
/// </summary>
public interface ISettingsService
{
    /// <summary>Get current settings</summary>
    Task<AppSettings> GetSettingsAsync();
    
    /// <summary>Save settings</summary>
    Task SaveSettingsAsync(AppSettings settings);
    
    /// <summary>Reset to defaults</summary>
    Task ResetToDefaultsAsync();
}

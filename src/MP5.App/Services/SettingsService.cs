using MP5.Core.Interfaces;
using MP5.Core.Models;
using System.Text.Json;

namespace MP5.App.Services;

/// <summary>
/// Settings service using MAUI Preferences for persistence.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string SettingsKey = "app_settings";
    private AppSettings? _cachedSettings;
    
    public Task<AppSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return Task.FromResult(_cachedSettings);
        
        var json = Preferences.Get(SettingsKey, string.Empty);
        
        if (string.IsNullOrEmpty(json))
        {
            _cachedSettings = new AppSettings();
        }
        else
        {
            try
            {
                _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                _cachedSettings = new AppSettings();
            }
        }
        
        return Task.FromResult(_cachedSettings);
    }
    
    public Task SaveSettingsAsync(AppSettings settings)
    {
        _cachedSettings = settings;
        var json = JsonSerializer.Serialize(settings);
        Preferences.Set(SettingsKey, json);
        return Task.CompletedTask;
    }
    
    public Task ResetToDefaultsAsync()
    {
        _cachedSettings = new AppSettings();
        Preferences.Remove(SettingsKey);
        return Task.CompletedTask;
    }
}

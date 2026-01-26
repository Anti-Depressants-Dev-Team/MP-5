using Microsoft.Win32;
using MP5.Core.Interfaces;

namespace MP5.App.Platforms.Windows.Services;

public class WindowsPlatformService : IPlatformService
{
    private const string AppName = "MP5MusicPlayer";

    public void SetFullscreen(bool isFullscreen)
    {
        // Not implemented for Windows
    }

    public void SetStartup(bool isEnabled)
    {
        try
        {
            var rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk == null) return;

            if (isEnabled)
            {
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    rk.SetValue(AppName, exePath);
                }
            }
            else
            {
                rk.DeleteValue(AppName, false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SetStartup Failed: {ex.Message}");
        }
    }

    public void InvokeOnMainThread(Action action)
    {
        MainThread.BeginInvokeOnMainThread(action);
    }
}

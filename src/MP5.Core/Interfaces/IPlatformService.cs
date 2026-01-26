namespace MP5.Core.Interfaces;

public interface IPlatformService
{
    /// <summary>
    /// Toggle fullscreen/immersive mode (Android only).
    /// </summary>
    void SetFullscreen(bool isFullscreen);

    /// <summary>Toggle run on startup (Windows only).</summary>
    void SetStartup(bool isEnabled);

    /// <summary>Execute action on UI thread</summary>
    void InvokeOnMainThread(Action action);
}

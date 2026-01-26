using MP5.Core.Interfaces;

namespace MP5.App.Services;

public class StubPlatformService : IPlatformService
{
    public void SetFullscreen(bool isFullscreen) { }
    public void SetStartup(bool isEnabled) { }
    public void InvokeOnMainThread(Action action) => action?.Invoke();
}

using Android.Views;
using AndroidX.Core.View;
using MP5.Core.Interfaces;

namespace MP5.App.Platforms.Android.Services;

public class AndroidPlatformService : IPlatformService
{
    public void SetFullscreen(bool isFullscreen)
    {
        var activity = Platform.CurrentActivity;
        if (activity?.Window == null) return;

        if (isFullscreen)
        {
            WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
            var controller = WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
            controller.Hide(WindowInsetsCompat.Type.SystemBars());
            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        }
        else
        {
            WindowCompat.SetDecorFitsSystemWindows(activity.Window, true);
            var controller = WindowCompat.GetInsetsController(activity.Window, activity.Window.DecorView);
            controller.Show(WindowInsetsCompat.Type.SystemBars());
        }
    }

    public void SetStartup(bool isEnabled)
    {
        // Not implemented for Android
    }

    public void InvokeOnMainThread(Action action)
    {
        MainThread.BeginInvokeOnMainThread(action);
    }
}

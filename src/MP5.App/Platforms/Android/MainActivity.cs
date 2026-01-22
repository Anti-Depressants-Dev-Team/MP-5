using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

namespace MP5.App;

[Activity(
    Theme = "@style/Maui.SplashTheme", 
    MainLauncher = true, 
    LaunchMode = LaunchMode.SingleTop,
    // Prevent fullscreen - keep status bar and navigation bar visible
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Ensure the app does NOT run in fullscreen mode
        // Force the status bar and navigation bar to always be visible
        if (Window != null)
        {
            Window.AddFlags(WindowManagerFlags.ForceNotFullscreen);
            Window.ClearFlags(WindowManagerFlags.Fullscreen);
            
            // Ensure system UI is visible
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Window.SetDecorFitsSystemWindows(true);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Window.DecorView.SystemUiVisibility = (StatusBarVisibility)
                    (SystemUiFlags.Visible | SystemUiFlags.LayoutStable);
#pragma warning restore CS0618
            }
        }
    }
}

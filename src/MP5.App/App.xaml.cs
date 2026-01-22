namespace MP5.App;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider)
    {
        try
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
            
            // Load saved accent color
            var settingsService = _serviceProvider.GetRequiredService<MP5.Core.Interfaces.ISettingsService>();
            // Use Task.Run to sync wait safely in constructor (or move to OnStart)
            var settings = Task.Run(async () => await settingsService.GetSettingsAsync()).Result;
            
            if (Color.TryParse(settings.AccentColorHex, out var color))
            {
                Resources["AccentPrimary"] = color;
                Resources["TextAccent"] = color;
                Resources["BorderAccent"] = color;
                Resources["ProgressFill"] = color;
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                LogCrash(e.ExceptionObject as Exception);
            };
        }
        catch (Exception ex)
        {
            LogCrash(ex);
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var mainPage = _serviceProvider.GetRequiredService<MainPage>();
            return new Window(mainPage);
        }
        catch (Exception ex)
        {
            LogCrash(ex);
            throw;
        }
    }

    private void LogCrash(Exception? ex)
    {
        if (ex == null) return;
        
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "mp5_crash_log.txt");
        var message = $"[{DateTime.Now}] CRASH: {ex.Message}\nSTACK: {ex.StackTrace}\nINNER: {ex.InnerException?.Message}\n\n";
        File.AppendAllText(path, message);
    }
}
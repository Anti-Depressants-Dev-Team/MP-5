using Microsoft.Extensions.Logging;
using MP5.Core.Interfaces;
using MP5.Core.Services;
using MP5.Core.ViewModels;
using MP5.App.Services;

namespace MP5.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        RegisterServices(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
    
    private static void RegisterServices(IServiceCollection services)
    {
        // Platform specific Services
#if WINDOWS
        services.AddSingleton<MP5.Core.Interfaces.IAudioService, MP5.App.Platforms.Windows.Services.WindowsAudioService>();
        services.AddSingleton<IPlatformService, MP5.App.Platforms.Windows.Services.WindowsPlatformService>();
#elif ANDROID
        services.AddSingleton<MP5.Core.Interfaces.IAudioService, MP5.App.Platforms.Android.Services.AndroidAudioService>();
        services.AddSingleton<IPlatformService, MP5.App.Platforms.Android.Services.AndroidPlatformService>();
#else
        services.AddSingleton<IPlatformService, MP5.App.Services.StubPlatformService>();
#endif
        
        // Core services
        services.AddSingleton<MauiMusicPlayerService>();
        // Forward IMusicPlayerService to MauiMusicPlayerService
        services.AddSingleton<IMusicPlayerService>(sp => sp.GetRequiredService<MauiMusicPlayerService>());
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IPlaylistService, PlaylistService>();
        services.AddSingleton<IThemeService, ThemeService>();
        
        // Music Sources
        services.AddSingleton<IDiscordRpcService, MP5.Core.Services.DiscordRpcService>();
        services.AddSingleton<IScrobblerService, MP5.Core.Services.LastFmScrobblerService>();
        services.AddSingleton<IScrobblerService, MP5.Core.Services.ListenBrainzScrobblerService>();
        services.AddSingleton<ISyncService, MP5.Core.Services.GoogleSyncService>();
        
        // Offline
        services.AddSingleton<IOfflineService, MP5.Core.Services.OfflineService>();
        
        // File Picker
        services.AddSingleton<IFilePickerService, MP5.App.Services.FilePickerService>();
        services.AddSingleton<IPromptService, MP5.App.Services.MauiPromptService>();
        
        // Ad Block
        services.AddSingleton<IAdBlockService, MP5.Core.Services.AdBlockService>();
        
        // Lyrics
        services.AddSingleton<ILyricsProvider, MP5.Core.Services.LyricsProviders.LrcLibLyricsProvider>();
        services.AddSingleton<ILyricsService, MP5.Core.Services.LyricsService>();
        
        services.AddSingleton<IUpdateService, UpdateServiceStub>();
        
        // Unified Source Service
        services.AddSingleton<IMusicSourceService, MusicSourceService>();
        
        // Individual Sources (Registered as IMusicSource)
        services.AddSingleton<IMusicSource, MP5.Core.Services.Sources.YouTubeMusicSource>();
        services.AddSingleton<IMusicSource, MP5.Core.Services.Sources.SoundCloudMusicSource>();
        
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<HomeViewModel>();
        services.AddSingleton<PlayerViewModel>();
        services.AddSingleton<PlaylistsViewModel>();
        services.AddSingleton<SearchViewModel>();
        services.AddSingleton<SettingsViewModel>();
        
        // Pages
        services.AddTransient<MainPage>();
        
        // Views
        services.AddTransient<MP5.App.Views.HomeView>();
        services.AddTransient<MP5.App.Views.SearchView>();
        services.AddTransient<MP5.App.Views.PlaylistsView>();
        services.AddTransient<MP5.App.Views.SettingsView>();
    }
}

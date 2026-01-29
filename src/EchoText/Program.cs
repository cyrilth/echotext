using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;
using EchoText.Platform;
using EchoText.Services;
using EchoText.Services.Interfaces;

namespace EchoText;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var serviceProvider = ConfigureServices();
        BuildAvaloniaApp(serviceProvider)
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider? serviceProvider = null)
    {
        var app = AppBuilder.Configure(() => new App(serviceProvider))
            .UsePlatformDetect()
            .LogToTrace();

        return app;
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register platform-specific services
        PlatformServices.Register(services);

        // Register core services
        services.AddSingleton<IAppStateManager, AppStateManager>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<IHotkeyService, HotkeyService>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<IModelManager, ModelManager>();
        services.AddSingleton<ITranscriptionService, TranscriptionService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IOutputService, OutputService>();

        // TODO: Register remaining core services here as they are implemented in later tasks

        return services.BuildServiceProvider();
    }
}

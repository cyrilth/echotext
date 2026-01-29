using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;

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

        // TODO: Register services here when they are implemented
        // Platform-specific services will be registered via PlatformServices.Register(services)
        // Core services will be registered as they are implemented in later tasks

        return services.BuildServiceProvider();
    }
}

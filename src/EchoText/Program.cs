using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
        // Ensure log directory exists
        EnsureLogDirectoryExists();

        var serviceProvider = ConfigureServices();

        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("EchoText application starting");

        try
        {
            BuildAvaloniaApp(serviceProvider)
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application crashed with unhandled exception");
            throw;
        }
        finally
        {
            logger.LogInformation("EchoText application shutting down");
        }
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

        // Configure logging with Serilog file logging
        services.AddLogging(builder =>
        {
            builder.AddFile(Path.Combine(PlatformInfo.LogDirectory, "echotext-{Date}.log"), minimumLevel: LogLevel.Information);

            // Set log levels for different namespaces
            builder.SetMinimumLevel(LogLevel.Information);

            // Reduce logging noise from system components
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
            builder.AddFilter("Avalonia", LogLevel.Warning);
        });

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
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<IUpdateService, UpdateService>();

        // Register ViewModels
        services.AddSingleton<ViewModels.MainViewModel>();
        services.AddTransient<ViewModels.SettingsViewModel>();
        services.AddTransient<ViewModels.RecordingOverlayViewModel>();
        services.AddTransient<ViewModels.FirstRunViewModel>();

        // TODO: Register remaining core services here as they are implemented in later tasks

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Ensures the log directory exists before logging starts.
    /// </summary>
    private static void EnsureLogDirectoryExists()
    {
        try
        {
            var logDir = PlatformInfo.LogDirectory;
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // Clean up old log files (keep last 7 days)
            CleanupOldLogFiles(logDir);
        }
        catch
        {
            // Silently fail - logging is not critical for app startup
        }
    }

    /// <summary>
    /// Removes log files older than 7 days.
    /// </summary>
    private static void CleanupOldLogFiles(string logDirectory)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-7);
            var logFiles = Directory.GetFiles(logDirectory, "echotext-*.log");

            foreach (var logFile in logFiles)
            {
                var fileInfo = new FileInfo(logFile);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    File.Delete(logFile);
                }
            }
        }
        catch
        {
            // Silently fail - cleanup failure shouldn't stop the app
        }
    }
}

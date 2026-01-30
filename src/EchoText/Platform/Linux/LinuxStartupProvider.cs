using System;
using System.IO;
using EchoText.Platform.Interfaces;
using Microsoft.Extensions.Logging;

namespace EchoText.Platform.Linux;

/// <summary>
/// Linux implementation of startup management using XDG Autostart.
/// Creates/removes .desktop file in ~/.config/autostart/
/// </summary>
public class LinuxStartupProvider : IPlatformStartup
{
    private const string AppName = "echotext";
    private const string DesktopFileName = "echotext.desktop";

    private readonly ILogger<LinuxStartupProvider> _logger;
    private readonly string _autostartDirectory;
    private readonly string _desktopFilePath;
    private bool _disposed;

    public LinuxStartupProvider(ILogger<LinuxStartupProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // XDG autostart directory
        var configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");

        _autostartDirectory = Path.Combine(configHome, "autostart");
        _desktopFilePath = Path.Combine(_autostartDirectory, DesktopFileName);

        _logger.LogInformation("LinuxStartupProvider initialized, desktop file path: {Path}", _desktopFilePath);
    }

    /// <inheritdoc />
    public bool IsEnabled => File.Exists(_desktopFilePath);

    /// <inheritdoc />
    public bool Enable(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            _logger.LogError("Cannot enable startup: executable path is empty");
            return false;
        }

        try
        {
            // Ensure autostart directory exists
            if (!Directory.Exists(_autostartDirectory))
            {
                Directory.CreateDirectory(_autostartDirectory);
                _logger.LogInformation("Created autostart directory: {Path}", _autostartDirectory);
            }

            // Create .desktop file content
            var desktopContent = $"""
                [Desktop Entry]
                Type=Application
                Name=EchoText
                Comment=Voice to text application
                Exec={executablePath}
                Icon=echotext
                Terminal=false
                Categories=Utility;Audio;
                StartupNotify=false
                X-GNOME-Autostart-enabled=true
                """;

            File.WriteAllText(_desktopFilePath, desktopContent);
            _logger.LogInformation("Created autostart desktop file: {Path}", _desktopFilePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create autostart desktop file");
            return false;
        }
    }

    /// <inheritdoc />
    public bool Disable()
    {
        try
        {
            if (File.Exists(_desktopFilePath))
            {
                File.Delete(_desktopFilePath);
                _logger.LogInformation("Removed autostart desktop file: {Path}", _desktopFilePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove autostart desktop file");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

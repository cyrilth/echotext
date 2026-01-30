using System;
using System.IO;
using EchoText.Platform.Interfaces;
using Microsoft.Extensions.Logging;

namespace EchoText.Platform.MacOS;

/// <summary>
/// macOS implementation of startup management using LaunchAgents.
/// Creates/removes plist file in ~/Library/LaunchAgents/
/// </summary>
public class MacOSStartupProvider : IPlatformStartup
{
    private const string AppIdentifier = "com.echotext.app";
    private const string PlistFileName = "com.echotext.app.plist";

    private readonly ILogger<MacOSStartupProvider> _logger;
    private readonly string _launchAgentsDirectory;
    private readonly string _plistFilePath;
    private bool _disposed;

    public MacOSStartupProvider(ILogger<MacOSStartupProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // LaunchAgents directory
        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _launchAgentsDirectory = Path.Combine(homeDirectory, "Library", "LaunchAgents");
        _plistFilePath = Path.Combine(_launchAgentsDirectory, PlistFileName);

        _logger.LogInformation("MacOSStartupProvider initialized, plist path: {Path}", _plistFilePath);
    }

    /// <inheritdoc />
    public bool IsEnabled => File.Exists(_plistFilePath);

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
            // Ensure LaunchAgents directory exists
            if (!Directory.Exists(_launchAgentsDirectory))
            {
                Directory.CreateDirectory(_launchAgentsDirectory);
                _logger.LogInformation("Created LaunchAgents directory: {Path}", _launchAgentsDirectory);
            }

            // Create plist file content
            var plistContent = $"""
                <?xml version="1.0" encoding="UTF-8"?>
                <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
                <plist version="1.0">
                <dict>
                    <key>Label</key>
                    <string>{AppIdentifier}</string>
                    <key>ProgramArguments</key>
                    <array>
                        <string>{executablePath}</string>
                    </array>
                    <key>RunAtLoad</key>
                    <true/>
                    <key>KeepAlive</key>
                    <false/>
                    <key>ProcessType</key>
                    <string>Interactive</string>
                </dict>
                </plist>
                """;

            File.WriteAllText(_plistFilePath, plistContent);
            _logger.LogInformation("Created LaunchAgent plist: {Path}", _plistFilePath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create LaunchAgent plist");
            return false;
        }
    }

    /// <inheritdoc />
    public bool Disable()
    {
        try
        {
            if (File.Exists(_plistFilePath))
            {
                File.Delete(_plistFilePath);
                _logger.LogInformation("Removed LaunchAgent plist: {Path}", _plistFilePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove LaunchAgent plist");
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

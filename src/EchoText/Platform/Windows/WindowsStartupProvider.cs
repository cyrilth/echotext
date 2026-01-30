using System;
using System.Runtime.Versioning;
using EchoText.Platform.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace EchoText.Platform.Windows;

/// <summary>
/// Windows implementation of startup management using the Registry.
/// Adds/removes entry from HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsStartupProvider : IPlatformStartup
{
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "EchoText";

    private readonly ILogger<WindowsStartupProvider> _logger;
    private bool _disposed;

    public WindowsStartupProvider(ILogger<WindowsStartupProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("WindowsStartupProvider initialized");
    }

    /// <inheritdoc />
    public bool IsEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                var value = key?.GetValue(AppName);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check startup registry key");
                return false;
            }
        }
    }

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
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null)
            {
                _logger.LogError("Failed to open registry key for writing: {KeyPath}", RegistryKeyPath);
                return false;
            }

            // Quote the path in case it contains spaces
            var quotedPath = $"\"{executablePath}\"";
            key.SetValue(AppName, quotedPath, RegistryValueKind.String);

            _logger.LogInformation("Enabled startup with system: {Path}", quotedPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enable startup in registry");
            return false;
        }
    }

    /// <inheritdoc />
    public bool Disable()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
            if (key == null)
            {
                _logger.LogWarning("Registry key not found: {KeyPath}", RegistryKeyPath);
                return true; // Already disabled
            }

            if (key.GetValue(AppName) != null)
            {
                key.DeleteValue(AppName, false);
                _logger.LogInformation("Disabled startup with system");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disable startup in registry");
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

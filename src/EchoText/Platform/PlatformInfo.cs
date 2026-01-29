using System;
using System.IO;
using System.Runtime.InteropServices;

namespace EchoText.Platform;

/// <summary>
/// Provides platform detection and platform-specific directory paths.
/// </summary>
public static class PlatformInfo
{
    /// <summary>
    /// Gets whether the current platform is Windows.
    /// </summary>
    public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    /// <summary>
    /// Gets whether the current platform is Linux.
    /// </summary>
    public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    /// <summary>
    /// Gets whether the current platform is macOS.
    /// </summary>
    public static bool IsMacOS => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    /// <summary>
    /// Gets the configuration directory path for the current platform.
    /// Windows: %APPDATA%\EchoText
    /// macOS: ~/Library/Application Support/EchoText
    /// Linux: ~/.config/echotext
    /// </summary>
    public static string ConfigDirectory { get; } = GetConfigDirectory();

    /// <summary>
    /// Gets the models directory path for the current platform.
    /// Windows: %APPDATA%\EchoText\models
    /// macOS: ~/Library/Application Support/EchoText/models
    /// Linux: ~/.local/share/echotext/models
    /// </summary>
    public static string ModelsDirectory { get; } = GetModelsDirectory();

    /// <summary>
    /// Gets the logs directory path for the current platform.
    /// Windows: %APPDATA%\EchoText\logs
    /// macOS: ~/Library/Logs/EchoText
    /// Linux: ~/.local/share/echotext/logs
    /// </summary>
    public static string LogDirectory { get; } = GetLogDirectory();

    private static string GetConfigDirectory()
    {
        if (IsWindows)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "EchoText");
        }
        else if (IsMacOS)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Application Support",
                "EchoText");
        }
        else // Linux
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "echotext");
        }
    }

    private static string GetModelsDirectory()
    {
        if (IsWindows)
        {
            return Path.Combine(ConfigDirectory, "models");
        }
        else if (IsMacOS)
        {
            return Path.Combine(ConfigDirectory, "models");
        }
        else // Linux
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local",
                "share",
                "echotext",
                "models");
        }
    }

    private static string GetLogDirectory()
    {
        if (IsWindows)
        {
            return Path.Combine(ConfigDirectory, "logs");
        }
        else if (IsMacOS)
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Logs",
                "EchoText");
        }
        else // Linux
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local",
                "share",
                "echotext",
                "logs");
        }
    }
}

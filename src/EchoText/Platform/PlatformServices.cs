using System;
using EchoText.Platform.Interfaces;
using EchoText.Platform.Windows;
using EchoText.Platform.Linux;
using EchoText.Platform.MacOS;
using Microsoft.Extensions.DependencyInjection;

namespace EchoText.Platform;

/// <summary>
/// Provides platform-specific service registration for dependency injection.
/// </summary>
public static class PlatformServices
{
    /// <summary>
    /// Register platform-specific implementations based on the current operating system.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current OS is not supported.</exception>
    public static void Register(IServiceCollection services)
    {
        if (PlatformInfo.IsWindows)
        {
#pragma warning disable CA1416 // Platform compatibility - guarded by IsWindows check
            services.AddSingleton<IPlatformHotkey, WindowsHotkeyProvider>();
            services.AddSingleton<IPlatformAudio, WindowsAudioProvider>();
            services.AddSingleton<IPlatformClipboard, WindowsClipboardProvider>();
            services.AddSingleton<IPlatformOutput, WindowsOutputProvider>();
            services.AddSingleton<IPlatformStartup, WindowsStartupProvider>();
#pragma warning restore CA1416
        }
        else if (PlatformInfo.IsLinux)
        {
            services.AddSingleton<IPlatformHotkey, LinuxHotkeyProvider>();
            services.AddSingleton<IPlatformAudio, LinuxAudioProvider>();
            services.AddSingleton<IPlatformClipboard, LinuxClipboardProvider>();
            services.AddSingleton<IPlatformOutput, LinuxOutputProvider>();
            services.AddSingleton<IPlatformStartup, LinuxStartupProvider>();
        }
        else if (PlatformInfo.IsMacOS)
        {
            services.AddSingleton<IPlatformHotkey, MacOSHotkeyProvider>();
            services.AddSingleton<IPlatformAudio, MacOSAudioProvider>();
            services.AddSingleton<IPlatformClipboard, MacOSClipboardProvider>();
            services.AddSingleton<IPlatformOutput, MacOSOutputProvider>();
            services.AddSingleton<IPlatformStartup, MacOSStartupProvider>();
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system. EchoText supports Windows, Linux, and macOS.");
        }
    }
}

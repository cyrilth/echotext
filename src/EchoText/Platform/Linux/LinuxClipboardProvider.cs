using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EchoText.Platform.Interfaces;

namespace EchoText.Platform.Linux;

/// <summary>
/// Linux implementation of clipboard operations using Avalonia's clipboard API
/// Works on both X11 and Wayland (via Avalonia's platform abstraction)
/// </summary>
public class LinuxClipboardProvider : IPlatformClipboard
{
    public async Task SetTextAsync(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var clipboard = GetClipboard();
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    public async Task<string?> GetTextAsync()
    {
        var clipboard = GetClipboard();
        if (clipboard != null)
        {
            return await clipboard.GetTextAsync();
        }

        return null;
    }

    private static Avalonia.Input.Platform.IClipboard? GetClipboard()
    {
        // Try to get clipboard from the main window
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow?.Clipboard;
        }

        // Fallback to top-level clipboard if available
        var topLevel = Avalonia.Controls.TopLevel.GetTopLevel(
            Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime dl
                ? dl.MainWindow
                : null
        );

        return topLevel?.Clipboard;
    }
}

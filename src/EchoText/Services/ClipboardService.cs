using System;
using System.Threading.Tasks;
using EchoText.Platform.Interfaces;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Service for clipboard operations
/// Wraps the platform-specific clipboard provider
/// </summary>
public class ClipboardService : IClipboardService
{
    private readonly IPlatformClipboard _platformClipboard;

    public ClipboardService(IPlatformClipboard platformClipboard)
    {
        _platformClipboard = platformClipboard ?? throw new ArgumentNullException(nameof(platformClipboard));
    }

    /// <summary>
    /// Copy text to the system clipboard
    /// </summary>
    /// <param name="text">Text to copy to clipboard</param>
    public async Task SetTextAsync(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        await _platformClipboard.SetTextAsync(text);
    }

    /// <summary>
    /// Get text from the system clipboard
    /// </summary>
    /// <returns>Text from clipboard, or null if clipboard is empty or doesn't contain text</returns>
    public async Task<string?> GetTextAsync()
    {
        return await _platformClipboard.GetTextAsync();
    }
}

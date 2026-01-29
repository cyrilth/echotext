using System.Threading.Tasks;

namespace EchoText.Platform.Interfaces;

/// <summary>
/// Platform-specific interface for clipboard operations
/// </summary>
public interface IPlatformClipboard
{
    /// <summary>
    /// Copy text to the system clipboard
    /// </summary>
    /// <param name="text">Text to copy to clipboard</param>
    Task SetTextAsync(string text);

    /// <summary>
    /// Get text from the system clipboard
    /// </summary>
    /// <returns>Text from clipboard, or null if clipboard is empty or doesn't contain text</returns>
    Task<string?> GetTextAsync();
}

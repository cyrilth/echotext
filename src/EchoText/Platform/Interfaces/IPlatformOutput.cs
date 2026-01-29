using System.Threading.Tasks;

namespace EchoText.Platform.Interfaces;

/// <summary>
/// Platform-specific interface for auto-typing text into the active application
/// </summary>
public interface IPlatformOutput
{
    /// <summary>
    /// Type text into the currently focused application window
    /// </summary>
    /// <param name="text">Text to type</param>
    /// <param name="delayMs">Delay in milliseconds between each keystroke (default: 10ms)</param>
    Task TypeTextAsync(string text, int delayMs = 10);
}

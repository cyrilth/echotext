using System.Threading.Tasks;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for outputting transcribed text to clipboard and/or active window
/// </summary>
public interface IOutputService
{
    /// <summary>
    /// Output text based on current settings (clipboard and/or auto-type)
    /// </summary>
    /// <param name="text">Text to output</param>
    /// <returns>Task representing the async operation</returns>
    Task OutputTextAsync(string text);
}

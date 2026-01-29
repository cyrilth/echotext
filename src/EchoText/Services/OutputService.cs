using System;
using System.Threading.Tasks;
using EchoText.Platform.Interfaces;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Service for outputting transcribed text to clipboard and/or active window
/// Handles both clipboard copy and auto-typing based on user settings
/// </summary>
public class OutputService : IOutputService
{
    private readonly IConfigService _configService;
    private readonly IClipboardService _clipboardService;
    private readonly IPlatformOutput _platformOutput;

    public OutputService(
        IConfigService configService,
        IClipboardService clipboardService,
        IPlatformOutput platformOutput)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _platformOutput = platformOutput ?? throw new ArgumentNullException(nameof(platformOutput));
    }

    /// <summary>
    /// Output text based on current settings (clipboard and/or auto-type)
    /// </summary>
    /// <param name="text">Text to output</param>
    /// <returns>Task representing the async operation</returns>
    public async Task OutputTextAsync(string text)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));

        var outputSettings = _configService.Settings.Output;

        // Copy to clipboard if enabled
        if (outputSettings.CopyToClipboard)
        {
            await _clipboardService.SetTextAsync(text);
        }

        // Auto-type if enabled
        if (outputSettings.AutoType)
        {
            var delayMs = outputSettings.KeystrokeDelayMs;
            await _platformOutput.TypeTextAsync(text, delayMs);
        }
    }
}

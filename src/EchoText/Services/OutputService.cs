using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<OutputService> _logger;

    public OutputService(
        IConfigService configService,
        IClipboardService clipboardService,
        IPlatformOutput platformOutput,
        ILogger<OutputService> logger)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _clipboardService = clipboardService ?? throw new ArgumentNullException(nameof(clipboardService));
        _platformOutput = platformOutput ?? throw new ArgumentNullException(nameof(platformOutput));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("OutputService initialized");
    }

    /// <summary>
    /// Output text based on current settings (clipboard and/or auto-type)
    /// </summary>
    /// <param name="text">Text to output</param>
    /// <returns>Task representing the async operation</returns>
    public async Task OutputTextAsync(string text)
    {
        if (text == null)
        {
            _logger.LogWarning("Attempted to output null text");
            throw new ArgumentNullException(nameof(text));
        }

        var outputSettings = _configService.Settings.Output;

        _logger.LogInformation("Outputting text (length: {TextLength}, clipboard: {Clipboard}, auto-type: {AutoType})",
            text.Length, outputSettings.CopyToClipboard, outputSettings.AutoType);

        try
        {
            // Copy to clipboard if enabled
            if (outputSettings.CopyToClipboard)
            {
                _logger.LogDebug("Copying text to clipboard");
                await _clipboardService.SetTextAsync(text);
                _logger.LogInformation("Text copied to clipboard successfully");
            }

            // Auto-type if enabled
            if (outputSettings.AutoType)
            {
                var delayMs = outputSettings.KeystrokeDelayMs;
                _logger.LogDebug("Auto-typing text with {DelayMs}ms delay", delayMs);
                await _platformOutput.TypeTextAsync(text, delayMs);
                _logger.LogInformation("Text auto-typed successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to output text");
            throw;
        }
    }
}

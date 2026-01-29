using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Service for managing global hotkey registration and events.
/// Wraps the platform-specific IPlatformHotkey provider and adds hotkey mode logic.
/// </summary>
public sealed class HotkeyService : IHotkeyService
{
    private readonly IPlatformHotkey _platformHotkey;
    private readonly IConfigService _configService;
    private readonly ILogger<HotkeyService> _logger;
    private bool _isToggled; // Tracks toggle state for Toggle mode
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler? HotkeyPressed;

    /// <inheritdoc />
    public event EventHandler? HotkeyReleased;

    /// <inheritdoc />
    public bool IsRegistered => _platformHotkey.IsRegistered;

    /// <summary>
    /// Initializes a new instance of the HotkeyService.
    /// </summary>
    /// <param name="platformHotkey">Platform-specific hotkey provider</param>
    /// <param name="configService">Configuration service for hotkey settings</param>
    /// <param name="logger">Logger for diagnostic output</param>
    public HotkeyService(IPlatformHotkey platformHotkey, IConfigService configService, ILogger<HotkeyService> logger)
    {
        _platformHotkey = platformHotkey ?? throw new ArgumentNullException(nameof(platformHotkey));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("HotkeyService initialized");

        // Subscribe to platform hotkey events
        _platformHotkey.HotkeyPressed += OnPlatformHotkeyPressed;
        _platformHotkey.HotkeyReleased += OnPlatformHotkeyReleased;

        // Subscribe to config changes to re-register hotkey when settings change
        _configService.SettingsChanged += OnSettingsChanged;
    }

    /// <inheritdoc />
    public Task<bool> RegisterAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var settings = _configService.Settings.Hotkey;
        _logger.LogInformation("Registering global hotkey: {Modifiers}+{Key}, mode: {Mode}",
            settings.Modifiers, settings.Key, settings.Mode);

        var success = _platformHotkey.Register(settings.Modifiers, settings.Key);

        if (success)
        {
            // Reset toggle state when registering
            _isToggled = false;
            _logger.LogInformation("Hotkey registered successfully");
        }
        else
        {
            _logger.LogWarning("Failed to register hotkey");
        }

        return Task.FromResult(success);
    }

    /// <inheritdoc />
    public Task UnregisterAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _logger.LogInformation("Unregistering global hotkey");
        _platformHotkey.Unregister();
        _isToggled = false;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles platform hotkey pressed events and applies mode-specific logic.
    /// </summary>
    private void OnPlatformHotkeyPressed(object? sender, EventArgs e)
    {
        var mode = _configService.Settings.Hotkey.Mode;

        _logger.LogDebug("Hotkey pressed, mode: {Mode}", mode);

        if (mode == HotkeyMode.PushToTalk)
        {
            // In PushToTalk mode, always fire pressed event
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
        else if (mode == HotkeyMode.Toggle)
        {
            // In Toggle mode, alternate between start and stop
            // Fire HotkeyPressed for both start and stop (consumers will use AppState to determine action)
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
            _isToggled = !_isToggled;
        }
    }

    /// <summary>
    /// Handles platform hotkey released events and applies mode-specific logic.
    /// </summary>
    private void OnPlatformHotkeyReleased(object? sender, EventArgs e)
    {
        var mode = _configService.Settings.Hotkey.Mode;

        _logger.LogDebug("Hotkey released, mode: {Mode}", mode);

        // Only fire released event in PushToTalk mode
        if (mode == HotkeyMode.PushToTalk)
        {
            HotkeyReleased?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Handles configuration changes to re-register the hotkey with new settings.
    /// </summary>
    private async void OnSettingsChanged(object? sender, EventArgs e)
    {
        _logger.LogInformation("Settings changed, re-registering hotkey");

        // Re-register hotkey if already registered
        if (IsRegistered)
        {
            await UnregisterAsync();
            await RegisterAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        // Unsubscribe from events
        _platformHotkey.HotkeyPressed -= OnPlatformHotkeyPressed;
        _platformHotkey.HotkeyReleased -= OnPlatformHotkeyReleased;
        _configService.SettingsChanged -= OnSettingsChanged;

        // Unregister hotkey
        _platformHotkey.Unregister();

        // Dispose platform provider
        _platformHotkey.Dispose();

        _disposed = true;
    }
}

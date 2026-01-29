using System;
using System.Threading.Tasks;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for managing global hotkey registration and events.
/// Wraps platform-specific hotkey providers and handles hotkey mode logic.
/// </summary>
public interface IHotkeyService : IDisposable
{
    /// <summary>
    /// Fired when the configured hotkey is pressed down.
    /// In Toggle mode, this fires on every press (alternating start/stop).
    /// In PushToTalk mode, this fires when the key is pressed down.
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// Fired when the configured hotkey is released.
    /// Only fires in PushToTalk mode.
    /// </summary>
    event EventHandler? HotkeyReleased;

    /// <summary>
    /// Gets whether a hotkey is currently registered.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Registers a global hotkey using settings from configuration.
    /// </summary>
    /// <returns>A task that represents the asynchronous register operation. Returns true if successful.</returns>
    Task<bool> RegisterAsync();

    /// <summary>
    /// Unregisters the current global hotkey.
    /// </summary>
    /// <returns>A task that represents the asynchronous unregister operation.</returns>
    Task UnregisterAsync();
}

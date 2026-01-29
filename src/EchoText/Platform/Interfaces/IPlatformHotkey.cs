using System;
using EchoText.Models;

namespace EchoText.Platform.Interfaces;

/// <summary>
/// Platform-specific interface for global hotkey registration and monitoring
/// </summary>
public interface IPlatformHotkey : IDisposable
{
    /// <summary>
    /// Fired when the registered hotkey is pressed down
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// Fired when the registered hotkey is released
    /// </summary>
    event EventHandler? HotkeyReleased;

    /// <summary>
    /// Register a global hotkey combination
    /// </summary>
    /// <param name="modifiers">Modifier keys (Ctrl, Shift, Alt, etc.)</param>
    /// <param name="key">The main key code (e.g., "Space", "A", "F1")</param>
    /// <returns>True if registration succeeded, false otherwise</returns>
    bool Register(KeyModifiers modifiers, string key);

    /// <summary>
    /// Unregister the currently registered hotkey
    /// </summary>
    void Unregister();

    /// <summary>
    /// Gets whether a hotkey is currently registered
    /// </summary>
    bool IsRegistered { get; }
}

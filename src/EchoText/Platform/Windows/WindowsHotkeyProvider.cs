using System;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using Microsoft.Extensions.Logging;
using SharpHook;
using SharpHook.Native;

namespace EchoText.Platform.Windows;

/// <summary>
/// Windows implementation of global hotkey registration using SharpHook
/// </summary>
public class WindowsHotkeyProvider : IPlatformHotkey
{
    private readonly SimpleGlobalHook _hook;
    private readonly ILogger<WindowsHotkeyProvider> _logger;
    private KeyModifiers _registeredModifiers;
    private string _registeredKey;
    private bool _isRegistered;
    private bool _isPressed;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;
    public event EventHandler? HotkeyReleased;

    public bool IsRegistered => _isRegistered;

    public WindowsHotkeyProvider(ILogger<WindowsHotkeyProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hook = new SimpleGlobalHook();
        _hook.KeyPressed += OnKeyPressed;
        _hook.KeyReleased += OnKeyReleased;
        _registeredModifiers = KeyModifiers.None;
        _registeredKey = string.Empty;
        _logger.LogInformation("WindowsHotkeyProvider initialized");
    }

    public bool Register(KeyModifiers modifiers, string key)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsHotkeyProvider));

        try
        {
            // Unregister existing hotkey if any
            if (_isRegistered)
            {
                Unregister();
            }

            _registeredModifiers = modifiers;
            _registeredKey = key;
            _isRegistered = true;

            _logger.LogInformation("Registering hotkey: {Modifiers}+{Key}", modifiers, key);

            // Start the hook if not already running
            if (!_hook.IsRunning)
            {
                _logger.LogInformation("Starting global keyboard hook");
                _hook.RunAsync();
            }

            _logger.LogInformation("Hotkey registered successfully, hook running: {IsRunning}", _hook.IsRunning);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register hotkey");
            _isRegistered = false;
            return false;
        }
    }

    public void Unregister()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsHotkeyProvider));

        _isRegistered = false;
        _isPressed = false;
        _registeredModifiers = KeyModifiers.None;
        _registeredKey = string.Empty;
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (!_isRegistered || _isPressed)
            return;

        var keyString = MapSharpHookKeyToString(e.Data.KeyCode);
        var currentModifiers = GetCurrentModifiers(e);

        _logger.LogDebug("Key pressed: {Key}, modifiers: {Modifiers}, registered: {RegKey}+{RegMod}",
            keyString, currentModifiers, _registeredKey, _registeredModifiers);

        if (IsHotkeyMatch(e))
        {
            _logger.LogInformation("Hotkey match detected - firing HotkeyPressed event");
            _isPressed = true;
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnKeyReleased(object? sender, KeyboardHookEventArgs e)
    {
        if (!_isRegistered || !_isPressed)
            return;

        if (IsHotkeyMatch(e))
        {
            _logger.LogInformation("Hotkey released - firing HotkeyReleased event");
            _isPressed = false;
            HotkeyReleased?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool IsHotkeyMatch(KeyboardHookEventArgs e)
    {
        // Check if the key matches
        var keyMatches = MapSharpHookKeyToString(e.Data.KeyCode).Equals(_registeredKey, StringComparison.OrdinalIgnoreCase);
        if (!keyMatches)
            return false;

        // Check modifiers
        var currentModifiers = GetCurrentModifiers(e);
        return currentModifiers == _registeredModifiers;
    }

    private KeyModifiers GetCurrentModifiers(KeyboardHookEventArgs e)
    {
        var modifiers = KeyModifiers.None;

        // Use the RawEvent.Mask which contains the modifier state at the time of key press
        var mask = e.RawEvent.Mask;

        // Check for Ctrl (left or right)
        if ((mask & ModifierMask.LeftCtrl) != 0 || (mask & ModifierMask.RightCtrl) != 0)
        {
            modifiers |= KeyModifiers.Ctrl;
        }

        // Check for Shift (left or right)
        if ((mask & ModifierMask.LeftShift) != 0 || (mask & ModifierMask.RightShift) != 0)
        {
            modifiers |= KeyModifiers.Shift;
        }

        // Check for Alt (left or right)
        if ((mask & ModifierMask.LeftAlt) != 0 || (mask & ModifierMask.RightAlt) != 0)
        {
            modifiers |= KeyModifiers.Alt;
        }

        // Check for Meta/Windows key (left or right)
        if ((mask & ModifierMask.LeftMeta) != 0 || (mask & ModifierMask.RightMeta) != 0)
        {
            modifiers |= KeyModifiers.Meta;
        }

        return modifiers;
    }

    private static string MapSharpHookKeyToString(KeyCode keyCode)
    {
        return keyCode switch
        {
            KeyCode.VcSpace => "Space",
            KeyCode.VcA => "A",
            KeyCode.VcB => "B",
            KeyCode.VcC => "C",
            KeyCode.VcD => "D",
            KeyCode.VcE => "E",
            KeyCode.VcF => "F",
            KeyCode.VcG => "G",
            KeyCode.VcH => "H",
            KeyCode.VcI => "I",
            KeyCode.VcJ => "J",
            KeyCode.VcK => "K",
            KeyCode.VcL => "L",
            KeyCode.VcM => "M",
            KeyCode.VcN => "N",
            KeyCode.VcO => "O",
            KeyCode.VcP => "P",
            KeyCode.VcQ => "Q",
            KeyCode.VcR => "R",
            KeyCode.VcS => "S",
            KeyCode.VcT => "T",
            KeyCode.VcU => "U",
            KeyCode.VcV => "V",
            KeyCode.VcW => "W",
            KeyCode.VcX => "X",
            KeyCode.VcY => "Y",
            KeyCode.VcZ => "Z",
            KeyCode.Vc0 => "0",
            KeyCode.Vc1 => "1",
            KeyCode.Vc2 => "2",
            KeyCode.Vc3 => "3",
            KeyCode.Vc4 => "4",
            KeyCode.Vc5 => "5",
            KeyCode.Vc6 => "6",
            KeyCode.Vc7 => "7",
            KeyCode.Vc8 => "8",
            KeyCode.Vc9 => "9",
            KeyCode.VcF1 => "F1",
            KeyCode.VcF2 => "F2",
            KeyCode.VcF3 => "F3",
            KeyCode.VcF4 => "F4",
            KeyCode.VcF5 => "F5",
            KeyCode.VcF6 => "F6",
            KeyCode.VcF7 => "F7",
            KeyCode.VcF8 => "F8",
            KeyCode.VcF9 => "F9",
            KeyCode.VcF10 => "F10",
            KeyCode.VcF11 => "F11",
            KeyCode.VcF12 => "F12",
            _ => keyCode.ToString().Replace("Vc", "")
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _hook.KeyPressed -= OnKeyPressed;
            _hook.KeyReleased -= OnKeyReleased;
            _hook.Dispose();
        }
        catch
        {
            // Suppress exceptions during disposal
        }

        GC.SuppressFinalize(this);
    }
}

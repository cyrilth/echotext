using System;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using SharpHook;
using SharpHook.Native;

namespace EchoText.Platform.MacOS;

/// <summary>
/// macOS implementation of global hotkey registration using SharpHook
/// Requires Accessibility permissions to capture global keyboard events
/// User will be prompted to grant permissions on first use
/// </summary>
public class MacOSHotkeyProvider : IPlatformHotkey
{
    private readonly SimpleGlobalHook _hook;
    private KeyModifiers _registeredModifiers;
    private string _registeredKey;
    private bool _isRegistered;
    private bool _isPressed;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;
    public event EventHandler? HotkeyReleased;

    public bool IsRegistered => _isRegistered;

    public MacOSHotkeyProvider()
    {
        _hook = new SimpleGlobalHook();
        _hook.KeyPressed += OnKeyPressed;
        _hook.KeyReleased += OnKeyReleased;
        _registeredModifiers = KeyModifiers.None;
        _registeredKey = string.Empty;
    }

    public bool Register(KeyModifiers modifiers, string key)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSHotkeyProvider));

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

            // Start the hook if not already running
            // On macOS, this will trigger Accessibility permission prompt if needed
            if (!_hook.IsRunning)
            {
                _hook.RunAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            // If hook fails to start, it's likely due to missing Accessibility permissions
            // Log or notify the user to grant permissions in System Preferences
            _isRegistered = false;

            // The exception message from SharpHook will indicate permission issues
            Console.Error.WriteLine($"Failed to register hotkey on macOS: {ex.Message}");
            Console.Error.WriteLine("This app requires Accessibility permissions.");
            Console.Error.WriteLine("Go to: System Preferences > Security & Privacy > Privacy > Accessibility");

            return false;
        }
    }

    public void Unregister()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSHotkeyProvider));

        _isRegistered = false;
        _isPressed = false;
        _registeredModifiers = KeyModifiers.None;
        _registeredKey = string.Empty;
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        if (!_isRegistered || _isPressed)
            return;

        if (IsHotkeyMatch(e))
        {
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

        var rawModifiers = e.Data.RawCode;

        // Check for Ctrl
        if ((rawModifiers & (ushort)ModifierMask.LeftCtrl) != 0 ||
            (rawModifiers & (ushort)ModifierMask.RightCtrl) != 0 ||
            e.Data.KeyCode == KeyCode.VcLeftControl ||
            e.Data.KeyCode == KeyCode.VcRightControl)
        {
            if ((_registeredModifiers & KeyModifiers.Ctrl) != 0)
                modifiers |= KeyModifiers.Ctrl;
        }

        // Check for Shift
        if ((rawModifiers & (ushort)ModifierMask.LeftShift) != 0 ||
            (rawModifiers & (ushort)ModifierMask.RightShift) != 0 ||
            e.Data.KeyCode == KeyCode.VcLeftShift ||
            e.Data.KeyCode == KeyCode.VcRightShift)
        {
            if ((_registeredModifiers & KeyModifiers.Shift) != 0)
                modifiers |= KeyModifiers.Shift;
        }

        // Check for Alt (Option on macOS)
        if ((rawModifiers & (ushort)ModifierMask.LeftAlt) != 0 ||
            (rawModifiers & (ushort)ModifierMask.RightAlt) != 0 ||
            e.Data.KeyCode == KeyCode.VcLeftAlt ||
            e.Data.KeyCode == KeyCode.VcRightAlt)
        {
            if ((_registeredModifiers & KeyModifiers.Alt) != 0)
                modifiers |= KeyModifiers.Alt;
        }

        // Check for Meta (Command key on macOS)
        if ((rawModifiers & (ushort)ModifierMask.LeftMeta) != 0 ||
            (rawModifiers & (ushort)ModifierMask.RightMeta) != 0 ||
            e.Data.KeyCode == KeyCode.VcLeftMeta ||
            e.Data.KeyCode == KeyCode.VcRightMeta)
        {
            if ((_registeredModifiers & KeyModifiers.Meta) != 0)
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

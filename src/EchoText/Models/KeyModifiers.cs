using System;

namespace EchoText.Models;

/// <summary>
/// Keyboard modifier keys for hotkey combinations
/// </summary>
[Flags]
public enum KeyModifiers
{
    None = 0,
    Ctrl = 1,
    Shift = 2,
    Alt = 4,
    Meta = 8  // Windows key / Cmd key
}

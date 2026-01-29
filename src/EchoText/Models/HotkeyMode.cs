namespace EchoText.Models;

/// <summary>
/// Defines how the hotkey behaves for recording
/// </summary>
public enum HotkeyMode
{
    /// <summary>
    /// Hold to record, release to stop
    /// </summary>
    PushToTalk,

    /// <summary>
    /// Press to start, press again to stop
    /// </summary>
    Toggle
}

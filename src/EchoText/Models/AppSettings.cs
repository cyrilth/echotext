namespace EchoText.Models;

/// <summary>
/// Main application settings
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Selected audio input device ID, or null for default
    /// </summary>
    public string? SelectedAudioDevice { get; set; }

    /// <summary>
    /// Hotkey configuration
    /// </summary>
    public HotkeySettings Hotkey { get; set; } = new();

    /// <summary>
    /// Output behavior configuration
    /// </summary>
    public OutputSettings Output { get; set; } = new();

    /// <summary>
    /// Speech recognition configuration
    /// </summary>
    public RecognitionSettings Recognition { get; set; } = new();

    /// <summary>
    /// General application settings
    /// </summary>
    public GeneralSettings General { get; set; } = new();
}

/// <summary>
/// Hotkey configuration
/// </summary>
public class HotkeySettings
{
    /// <summary>
    /// Modifier keys (Ctrl, Shift, Alt, Meta)
    /// </summary>
    public KeyModifiers Modifiers { get; set; } = KeyModifiers.Ctrl | KeyModifiers.Shift;

    /// <summary>
    /// Main key (e.g., "Space")
    /// </summary>
    public string Key { get; set; } = "Space";

    /// <summary>
    /// Hotkey mode (PushToTalk or Toggle)
    /// </summary>
    public HotkeyMode Mode { get; set; } = HotkeyMode.PushToTalk;
}

/// <summary>
/// Output behavior settings
/// </summary>
public class OutputSettings
{
    /// <summary>
    /// Whether to copy transcribed text to clipboard
    /// </summary>
    public bool CopyToClipboard { get; set; } = true;

    /// <summary>
    /// Whether to automatically type transcribed text into active window
    /// </summary>
    public bool AutoType { get; set; } = false;

    /// <summary>
    /// Whether to play sound effects on completion
    /// </summary>
    public bool PlaySoundOnComplete { get; set; } = true;
}

/// <summary>
/// Speech recognition settings
/// </summary>
public class RecognitionSettings
{
    /// <summary>
    /// Whisper model name to use (tiny, base, small, medium, large)
    /// </summary>
    public string ModelName { get; set; } = "base";

    /// <summary>
    /// Language code for transcription, or "auto" for automatic detection
    /// </summary>
    public string Language { get; set; } = "auto";
}

/// <summary>
/// General application settings
/// </summary>
public class GeneralSettings
{
    /// <summary>
    /// Whether to start the application with the system
    /// </summary>
    public bool StartWithSystem { get; set; } = false;

    /// <summary>
    /// Whether to show toast notifications
    /// </summary>
    public bool ShowNotifications { get; set; } = true;

    /// <summary>
    /// Whether to check for updates on startup
    /// </summary>
    public bool CheckForUpdates { get; set; } = true;
}

namespace EchoText.Models;

/// <summary>
/// Represents the current state of the application
/// </summary>
public enum AppState
{
    /// <summary>
    /// Application is starting or loading the Whisper model
    /// </summary>
    Loading,

    /// <summary>
    /// Application is ready for input
    /// </summary>
    Idle,

    /// <summary>
    /// Currently capturing audio from the microphone
    /// </summary>
    Recording,

    /// <summary>
    /// Transcribing captured audio
    /// </summary>
    Processing,

    /// <summary>
    /// An error has occurred
    /// </summary>
    Error
}

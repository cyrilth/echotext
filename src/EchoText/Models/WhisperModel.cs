namespace EchoText.Models;

/// <summary>
/// Represents a Whisper model
/// </summary>
/// <param name="Name">Model identifier (tiny, base, small, medium, large)</param>
/// <param name="DisplayName">Human-readable model name</param>
/// <param name="SizeBytes">Model file size in bytes</param>
/// <param name="IsDownloaded">Whether the model is currently downloaded</param>
/// <param name="LocalPath">Path to the downloaded model file, or null if not downloaded</param>
public record WhisperModel(
    string Name,
    string DisplayName,
    long SizeBytes,
    bool IsDownloaded,
    string? LocalPath
);

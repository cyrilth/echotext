using EchoText.Models;
using System;
using System.Threading.Tasks;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for transcribing audio using Whisper model
/// </summary>
public interface ITranscriptionService : IDisposable
{
    /// <summary>
    /// Load a Whisper model from file
    /// </summary>
    /// <param name="modelPath">Path to the model file</param>
    Task<Result<bool>> LoadModelAsync(string modelPath);

    /// <summary>
    /// Unload the currently loaded model
    /// </summary>
    void UnloadModel();

    /// <summary>
    /// Transcribe audio data to text
    /// </summary>
    /// <param name="audioData">WAV audio bytes (16kHz, mono, 16-bit)</param>
    /// <param name="language">Language code (e.g., "en", "es") or null/empty for auto-detect</param>
    /// <returns>Result containing transcribed text or error message</returns>
    Task<Result<string>> TranscribeAsync(byte[] audioData, string? language = null);

    /// <summary>
    /// Whether a model is currently loaded
    /// </summary>
    bool IsModelLoaded { get; }

    /// <summary>
    /// Name of the currently loaded model
    /// </summary>
    string? LoadedModelName { get; }
}

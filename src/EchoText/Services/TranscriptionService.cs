using EchoText.Models;
using EchoText.Services.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Whisper.net;

namespace EchoText.Services;

/// <summary>
/// Service for transcribing audio using Whisper.net
/// </summary>
public class TranscriptionService : ITranscriptionService
{
    private readonly IModelManager _modelManager;
    private WhisperProcessor? _processor;
    private string? _loadedModelName;
    private bool _disposed;

    public bool IsModelLoaded => _processor != null;
    public string? LoadedModelName => _loadedModelName;

    public TranscriptionService(IModelManager modelManager)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
    }

    /// <summary>
    /// Load a Whisper model from file
    /// </summary>
    public async Task<Result<bool>> LoadModelAsync(string modelPath)
    {
        if (_disposed)
            return Result<bool>.Failure("Service has been disposed");

        if (string.IsNullOrWhiteSpace(modelPath))
            return Result<bool>.Failure("Model path cannot be empty");

        if (!File.Exists(modelPath))
            return Result<bool>.Failure($"Model file not found: {modelPath}");

        try
        {
            // Unload any existing model first
            UnloadModel();

            // Load the model using Whisper.net factory
            var factory = WhisperFactory.FromPath(modelPath);
            _processor = factory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            // Extract model name from path
            _loadedModelName = Path.GetFileNameWithoutExtension(modelPath);

            return await Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception ex)
        {
            _processor?.Dispose();
            _processor = null;
            _loadedModelName = null;
            return Result<bool>.Failure($"Failed to load model: {ex.Message}");
        }
    }

    /// <summary>
    /// Unload the currently loaded model
    /// </summary>
    public void UnloadModel()
    {
        if (_processor != null)
        {
            _processor.Dispose();
            _processor = null;
            _loadedModelName = null;
        }
    }

    /// <summary>
    /// Transcribe audio data to text
    /// </summary>
    public async Task<Result<string>> TranscribeAsync(byte[] audioData, string? language = null)
    {
        if (_disposed)
            return Result<string>.Failure("Service has been disposed");

        if (audioData == null || audioData.Length == 0)
            return Result<string>.Failure("Audio data is empty");

        if (!IsModelLoaded)
            return Result<string>.Failure("No model loaded. Call LoadModelAsync first.");

        try
        {
            // If language is specified and not auto, rebuild processor with language
            if (!string.IsNullOrWhiteSpace(language) && language != "auto")
            {
                // We need to rebuild the processor with the new language
                var currentModelPath = _modelManager.GetModelPath(_loadedModelName!);
                if (currentModelPath == null)
                    return Result<string>.Failure("Current model path not found");

                var factory = WhisperFactory.FromPath(currentModelPath);
                var oldProcessor = _processor;

                _processor = factory.CreateBuilder()
                    .WithLanguage(language)
                    .Build();

                oldProcessor?.Dispose();
            }

            // Create a memory stream from the audio data
            using var audioStream = new MemoryStream(audioData);

            // Process the audio and collect all segments
            var transcriptionBuilder = new StringBuilder();

            await foreach (var segment in _processor!.ProcessAsync(audioStream))
            {
                transcriptionBuilder.Append(segment.Text);
            }

            var transcription = transcriptionBuilder.ToString().Trim();

            // Return empty string for silence (not an error)
            return Result<string>.Success(transcription);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Transcription failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        UnloadModel();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

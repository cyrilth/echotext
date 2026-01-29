using EchoText.Models;
using EchoText.Services.Interfaces;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<TranscriptionService> _logger;
    private WhisperProcessor? _processor;
    private string? _loadedModelName;
    private bool _disposed;

    public bool IsModelLoaded => _processor != null;
    public string? LoadedModelName => _loadedModelName;

    public TranscriptionService(IModelManager modelManager, ILogger<TranscriptionService> logger)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("TranscriptionService initialized");
    }

    /// <summary>
    /// Load a Whisper model from file
    /// </summary>
    public async Task<Result<bool>> LoadModelAsync(string modelPath)
    {
        if (_disposed)
        {
            _logger.LogWarning("Attempted to load model on disposed service");
            return Result<bool>.Failure("Service has been disposed");
        }

        if (string.IsNullOrWhiteSpace(modelPath))
        {
            _logger.LogWarning("Attempted to load model with empty path");
            return Result<bool>.Failure("Model path cannot be empty");
        }

        if (!File.Exists(modelPath))
        {
            _logger.LogError("Model file not found: {ModelPath}", modelPath);
            return Result<bool>.Failure($"Model file not found: {modelPath}");
        }

        _logger.LogInformation("Loading Whisper model from: {ModelPath}", modelPath);

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

            _logger.LogInformation("Whisper model loaded successfully: {ModelName}", _loadedModelName);
            return await Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Whisper model from {ModelPath}", modelPath);
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
            _logger.LogInformation("Unloading Whisper model: {ModelName}", _loadedModelName);
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
        {
            _logger.LogWarning("Attempted to transcribe on disposed service");
            return Result<string>.Failure("Service has been disposed");
        }

        if (audioData == null || audioData.Length == 0)
        {
            _logger.LogWarning("Attempted to transcribe empty audio data");
            return Result<string>.Failure("Audio data is empty");
        }

        if (!IsModelLoaded)
        {
            _logger.LogWarning("Attempted to transcribe without model loaded");
            return Result<string>.Failure("No model loaded. Call LoadModelAsync first.");
        }

        _logger.LogInformation("Starting transcription of {AudioSize} bytes, language: {Language}",
            audioData.Length, language ?? "auto");

        try
        {
            // If language is specified and not auto, rebuild processor with language
            if (!string.IsNullOrWhiteSpace(language) && language != "auto")
            {
                _logger.LogDebug("Switching to language: {Language}", language);

                // We need to rebuild the processor with the new language
                var currentModelPath = _modelManager.GetModelPath(_loadedModelName!);
                if (currentModelPath == null)
                {
                    _logger.LogError("Current model path not found for {ModelName}", _loadedModelName);
                    return Result<string>.Failure("Current model path not found");
                }

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

            if (string.IsNullOrEmpty(transcription))
            {
                _logger.LogInformation("Transcription completed but no speech detected");
            }
            else
            {
                _logger.LogInformation("Transcription completed successfully, text length: {TextLength} characters",
                    transcription.Length);
            }

            // Return empty string for silence (not an error)
            return Result<string>.Success(transcription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcription failed");
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

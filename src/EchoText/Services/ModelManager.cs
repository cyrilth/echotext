using EchoText.Models;
using EchoText.Platform;
using EchoText.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EchoText.Services;

/// <summary>
/// Manages Whisper model downloads and lifecycle
/// </summary>
public class ModelManager : IModelManager
{
    private static readonly HttpClient _httpClient = new();
    private static readonly SemaphoreSlim _downloadLock = new(1, 1);
    private static string? _currentlyDownloading;
    private readonly ILogger<ModelManager> _logger;

    private static readonly Dictionary<string, ModelInfo> _modelDefinitions = new()
    {
        ["tiny"] = new ModelInfo(
            "tiny",
            "Tiny (~75 MB)",
            75_000_000,
            "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin"),
        ["base"] = new ModelInfo(
            "base",
            "Base (~142 MB)",
            142_000_000,
            "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin"),
        ["small"] = new ModelInfo(
            "small",
            "Small (~466 MB)",
            466_000_000,
            "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-small.bin"),
        ["medium"] = new ModelInfo(
            "medium",
            "Medium (~1.5 GB)",
            1_500_000_000,
            "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin"),
        ["large"] = new ModelInfo(
            "large",
            "Large (~2.9 GB)",
            2_900_000_000,
            "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-large-v3.bin")
    };

    public ModelManager(ILogger<ModelManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("ModelManager initialized");
    }

    /// <summary>
    /// Get list of available models (downloaded + available for download)
    /// </summary>
    public Task<IReadOnlyList<WhisperModel>> GetAvailableModelsAsync()
    {
        EnsureModelsDirectoryExists();

        var models = _modelDefinitions.Select(kvp =>
        {
            var modelInfo = kvp.Value;
            var modelPath = GetModelFilePath(kvp.Key);
            var isDownloaded = File.Exists(modelPath);

            return new WhisperModel(
                kvp.Key,
                modelInfo.DisplayName,
                modelInfo.SizeBytes,
                isDownloaded,
                isDownloaded ? modelPath : null
            );
        }).ToList();

        return Task.FromResult<IReadOnlyList<WhisperModel>>(models);
    }

    /// <summary>
    /// Download a model
    /// </summary>
    /// <param name="modelName">Model name (tiny, base, small, medium, large)</param>
    /// <param name="progress">Progress callback (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the download</param>
    public async Task DownloadModelAsync(string modelName, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!_modelDefinitions.TryGetValue(modelName, out var modelInfo))
        {
            _logger.LogError("Attempted to download unknown model: {ModelName}", modelName);
            throw new ArgumentException($"Unknown model: {modelName}", nameof(modelName));
        }

        // Prevent concurrent downloads of the same model
        await _downloadLock.WaitAsync(cancellationToken);
        try
        {
            // Check if this model is already being downloaded
            if (_currentlyDownloading == modelName)
            {
                _logger.LogWarning("Model '{ModelName}' is already being downloaded", modelName);
                throw new InvalidOperationException($"Model '{modelName}' is already being downloaded.");
            }
            _currentlyDownloading = modelName;

            _logger.LogInformation("Starting download of model '{ModelName}' from {Url}", modelName, modelInfo.Url);

            EnsureModelsDirectoryExists();

            var modelPath = GetModelFilePath(modelName);
            var tempPath = modelPath + ".tmp";

            try
            {
                // Delete temp file if it exists from a previous failed download
                if (File.Exists(tempPath))
                {
                    _logger.LogDebug("Cleaning up temp file from previous download: {TempPath}", tempPath);
                    File.Delete(tempPath);
                }

                using var response = await _httpClient.GetAsync(modelInfo.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? modelInfo.SizeBytes;
                var downloadedBytes = 0L;

                _logger.LogInformation("Downloading {TotalMB:F1} MB for model '{ModelName}'", totalBytes / 1_000_000.0, modelName);

                // Use explicit using blocks so streams are closed before File.Move
                using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
                using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        downloadedBytes += bytesRead;

                        // Report progress
                        if (progress != null && totalBytes > 0)
                        {
                            var progressPercentage = (double)downloadedBytes / totalBytes;
                            progress.Report(progressPercentage);
                        }
                    }
                }

                // Download complete, move temp file to final location (streams are now closed)
                if (File.Exists(modelPath))
                {
                    File.Delete(modelPath);
                }
                File.Move(tempPath, modelPath);

                _logger.LogInformation("Model '{ModelName}' downloaded successfully to {ModelPath}", modelName, modelPath);

                // Report 100% completion
                progress?.Report(1.0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download model '{ModelName}'", modelName);

                // Clean up temp file on error
                if (File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
                throw;
            }
        }
        finally
        {
            _currentlyDownloading = null;
            _downloadLock.Release();
        }
    }

    /// <summary>
    /// Get path to a downloaded model
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <returns>Full path or null if not downloaded</returns>
    public string? GetModelPath(string modelName)
    {
        if (!_modelDefinitions.ContainsKey(modelName))
        {
            return null;
        }

        var modelPath = GetModelFilePath(modelName);
        return File.Exists(modelPath) ? modelPath : null;
    }

    /// <summary>
    /// Check if a model is downloaded
    /// </summary>
    public bool IsModelDownloaded(string modelName)
    {
        if (!_modelDefinitions.ContainsKey(modelName))
        {
            return false;
        }

        var modelPath = GetModelFilePath(modelName);
        return File.Exists(modelPath);
    }

    /// <summary>
    /// Delete a downloaded model
    /// </summary>
    public Task DeleteModelAsync(string modelName)
    {
        if (!_modelDefinitions.ContainsKey(modelName))
        {
            _logger.LogError("Attempted to delete unknown model: {ModelName}", modelName);
            throw new ArgumentException($"Unknown model: {modelName}", nameof(modelName));
        }

        var modelPath = GetModelFilePath(modelName);
        if (File.Exists(modelPath))
        {
            _logger.LogInformation("Deleting model '{ModelName}' from {ModelPath}", modelName, modelPath);
            File.Delete(modelPath);
        }
        else
        {
            _logger.LogWarning("Attempted to delete model '{ModelName}' but file not found", modelName);
        }

        return Task.CompletedTask;
    }

    private static string GetModelFilePath(string modelName)
    {
        return Path.Combine(PlatformInfo.ModelsDirectory, $"ggml-{modelName}.bin");
    }

    private static void EnsureModelsDirectoryExists()
    {
        if (!Directory.Exists(PlatformInfo.ModelsDirectory))
        {
            Directory.CreateDirectory(PlatformInfo.ModelsDirectory);
        }
    }

    private record ModelInfo(string Name, string DisplayName, long SizeBytes, string Url);
}

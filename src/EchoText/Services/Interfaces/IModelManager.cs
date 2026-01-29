using EchoText.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Manages Whisper model downloads and lifecycle
/// </summary>
public interface IModelManager
{
    /// <summary>
    /// Get list of available models (downloaded + available for download)
    /// </summary>
    Task<IReadOnlyList<WhisperModel>> GetAvailableModelsAsync();

    /// <summary>
    /// Download a model
    /// </summary>
    /// <param name="modelName">Model name (tiny, base, small, medium, large)</param>
    /// <param name="progress">Progress callback (0.0 to 1.0)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the download</param>
    Task DownloadModelAsync(string modelName, IProgress<double>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get path to a downloaded model
    /// </summary>
    /// <param name="modelName">Model name</param>
    /// <returns>Full path or null if not downloaded</returns>
    string? GetModelPath(string modelName);

    /// <summary>
    /// Check if a model is downloaded
    /// </summary>
    bool IsModelDownloaded(string modelName);

    /// <summary>
    /// Delete a downloaded model
    /// </summary>
    Task DeleteModelAsync(string modelName);
}

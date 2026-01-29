using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EchoText.Models;
using EchoText.Services.Interfaces;

namespace EchoText.ViewModels;

/// <summary>
/// ViewModel for the first-run experience dialog
/// </summary>
public partial class FirstRunViewModel : ViewModelBase
{
    private readonly IModelManager _modelManager;
    private readonly INotificationService _notificationService;
    private CancellationTokenSource? _downloadCancellation;

    [ObservableProperty]
    private ObservableCollection<WhisperModel> _availableModels = new();

    [ObservableProperty]
    private WhisperModel? _selectedModel;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private double _downloadProgress;

    [ObservableProperty]
    private bool _isLoadingModels;

    [ObservableProperty]
    private string _statusMessage = "Welcome to EchoText!";

    /// <summary>
    /// Indicates whether a model was successfully downloaded
    /// </summary>
    public bool ModelDownloaded { get; private set; }

    /// <summary>
    /// Initializes a new instance of the FirstRunViewModel
    /// </summary>
    public FirstRunViewModel(
        IModelManager modelManager,
        INotificationService notificationService)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

        // Load available models
        _ = LoadModelsAsync();
    }

    /// <summary>
    /// Load available Whisper models
    /// </summary>
    private async Task LoadModelsAsync()
    {
        IsLoadingModels = true;

        try
        {
            var models = await _modelManager.GetAvailableModelsAsync();
            AvailableModels.Clear();

            foreach (var model in models)
            {
                AvailableModels.Add(model);
            }

            // Default to "base" model
            SelectedModel = AvailableModels.FirstOrDefault(m => m.Name == "base")
                ?? AvailableModels.FirstOrDefault();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load models: {ex.Message}";
            await _notificationService.ShowNotificationAsync(
                "Model Loading Error",
                "Failed to load available models. Please try again later.",
                NotificationType.Error);
        }
        finally
        {
            IsLoadingModels = false;
        }
    }

    /// <summary>
    /// Command to download the selected model
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDownloadModel))]
    private async Task DownloadModelAsync()
    {
        if (SelectedModel == null || SelectedModel.IsDownloaded)
            return;

        IsDownloading = true;
        DownloadProgress = 0;
        StatusMessage = $"Downloading {SelectedModel.DisplayName}...";
        _downloadCancellation = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(p =>
            {
                DownloadProgress = p * 100;
                StatusMessage = $"Downloading {SelectedModel.DisplayName}... {DownloadProgress:F0}%";
            });

            await _modelManager.DownloadModelAsync(SelectedModel.Name, progress, _downloadCancellation.Token);

            // Mark as successful
            ModelDownloaded = true;
            StatusMessage = "Download complete! Ready to use.";

            // Reload models to update download status
            await LoadModelsAsync();
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Download cancelled.";
            ModelDownloaded = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Download failed: {ex.Message}";
            ModelDownloaded = false;

            await _notificationService.ShowNotificationAsync(
                "Download Failed",
                $"Failed to download model: {ex.Message}",
                NotificationType.Error);
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
            _downloadCancellation?.Dispose();
            _downloadCancellation = null;
        }
    }

    private bool CanDownloadModel()
    {
        return SelectedModel != null && !SelectedModel.IsDownloaded && !IsDownloading;
    }

    /// <summary>
    /// Command to cancel model download
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCancelDownload))]
    private void CancelDownload()
    {
        _downloadCancellation?.Cancel();
    }

    private bool CanCancelDownload()
    {
        return IsDownloading;
    }

    /// <summary>
    /// Command to skip the first-run setup
    /// </summary>
    [RelayCommand]
    private void Skip()
    {
        ModelDownloaded = false;
        // The dialog will close and return false
    }

    /// <summary>
    /// Dispose resources
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _downloadCancellation?.Cancel();
            _downloadCancellation?.Dispose();
        }
        base.Dispose(disposing);
    }
}

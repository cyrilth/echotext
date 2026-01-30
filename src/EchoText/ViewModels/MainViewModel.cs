using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using EchoText.Models;
using EchoText.Services.Interfaces;

namespace EchoText.ViewModels;

/// <summary>
/// ViewModel for the main application window and tray icon
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IAppStateManager _appStateManager;
    private readonly IWindowService _windowService;
    private readonly IHotkeyService _hotkeyService;
    private readonly IAudioService _audioService;
    private readonly ITranscriptionService _transcriptionService;
    private readonly IOutputService _outputService;
    private readonly INotificationService _notificationService;
    private readonly IConfigService _configService;
    private readonly IModelManager _modelManager;
    private readonly IUpdateService _updateService;
    private readonly ILogger<MainViewModel> _logger;
    private string? _loadedModelName;

    [ObservableProperty]
    private string _statusText = "Idle";

    [ObservableProperty]
    private string _trayIconPath = "avares://EchoText/Assets/Icons/tray-idle.ico";

    /// <summary>
    /// Initializes a new instance of the MainViewModel
    /// </summary>
    /// <param name="appStateManager">Application state manager</param>
    /// <param name="windowService">Service for managing windows</param>
    /// <param name="hotkeyService">Hotkey service for global hotkeys</param>
    /// <param name="audioService">Audio service for recording</param>
    /// <param name="transcriptionService">Transcription service for speech-to-text</param>
    /// <param name="outputService">Output service for clipboard/auto-type</param>
    /// <param name="notificationService">Notification service for toast and sounds</param>
    /// <param name="configService">Configuration service for settings</param>
    /// <param name="modelManager">Model manager for Whisper models</param>
    /// <param name="updateService">Update service for checking GitHub releases</param>
    /// <param name="logger">Logger for diagnostic output</param>
    public MainViewModel(
        IAppStateManager appStateManager,
        IWindowService windowService,
        IHotkeyService hotkeyService,
        IAudioService audioService,
        ITranscriptionService transcriptionService,
        IOutputService outputService,
        INotificationService notificationService,
        IConfigService configService,
        IModelManager modelManager,
        IUpdateService updateService,
        ILogger<MainViewModel> logger)
    {
        _appStateManager = appStateManager ?? throw new ArgumentNullException(nameof(appStateManager));
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _transcriptionService = transcriptionService ?? throw new ArgumentNullException(nameof(transcriptionService));
        _outputService = outputService ?? throw new ArgumentNullException(nameof(outputService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _updateService = updateService ?? throw new ArgumentNullException(nameof(updateService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("MainViewModel initialized");

        // Subscribe to state changes
        _appStateManager.StateChanged += OnAppStateChanged;

        // Subscribe to hotkey events
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        _hotkeyService.HotkeyReleased += OnHotkeyReleased;

        // Subscribe to settings changes to reload model if needed
        _configService.SettingsChanged += OnSettingsChanged;

        // Initialize status based on current state
        UpdateStatusForState(_appStateManager.CurrentState);

        // Initialize the application
        _ = InitializeAsync();
    }

    /// <summary>
    /// Command to open settings window
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        _windowService.ShowSettingsWindow();
    }

    /// <summary>
    /// Command to check for updates
    /// </summary>
    [RelayCommand]
    private async Task CheckForUpdates()
    {
        try
        {
            var result = await _updateService.CheckForUpdatesAsync();

            if (result.UpdateAvailable)
            {
                _windowService.ShowUpdateAvailableDialog(
                    result.CurrentVersion ?? "Unknown",
                    result.LatestVersion ?? "Unknown",
                    result.ReleaseUrl ?? "https://github.com/cyrilth/echotext/releases");
            }
            else
            {
                _windowService.ShowUpToDateDialog(result.CurrentVersion ?? "Unknown");
            }
        }
        catch (Exception ex)
        {
            _windowService.ShowUpdateErrorDialog($"Failed to check for updates: {ex.Message}");
        }
    }

    /// <summary>
    /// Command to show about dialog
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        _windowService.ShowAboutWindow();
    }

    /// <summary>
    /// Command to exit the application
    /// </summary>
    [RelayCommand]
    private void ExitApplication()
    {
        // Request application shutdown via WindowService
        _windowService.ExitApplication();
    }

    /// <summary>
    /// Checks for updates on startup (silently, only shows notification if update available)
    /// </summary>
    private async Task CheckForUpdatesOnStartupAsync()
    {
        try
        {
            // Wait a bit before checking to not interfere with startup
            await Task.Delay(TimeSpan.FromSeconds(3));

            var result = await _updateService.CheckForUpdatesAsync();

            // Only show notification if an update is available
            if (result.UpdateAvailable)
            {
                await _notificationService.ShowNotificationAsync(
                    "Update Available",
                    $"A new version ({result.LatestVersion}) is available! Check the tray menu to download.",
                    NotificationType.Info);
            }
        }
        catch
        {
            // Silently fail - don't bother user with update check errors on startup
        }
    }

    /// <summary>
    /// Initializes the application on startup
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting application initialization");
            _appStateManager.TransitionTo(AppState.Loading);

            // Load configuration
            _logger.LogInformation("Loading application configuration");
            await _configService.LoadAsync();

            // Check if this is the first run (no models downloaded)
            var availableModels = await _modelManager.GetAvailableModelsAsync();
            var hasDownloadedModel = availableModels.Any(m => m.IsDownloaded);

            if (!hasDownloadedModel)
            {
                _logger.LogWarning("No Whisper models downloaded, showing first-run dialog");
                // Show first-run dialog
                var modelDownloaded = await _windowService.ShowFirstRunDialogAsync();

                if (!modelDownloaded)
                {
                    _logger.LogWarning("User skipped model download in first-run dialog");
                    // User skipped - show warning notification
                    await _notificationService.ShowNotificationAsync(
                        "No Model Downloaded",
                        "You'll need to download a model in Settings before you can use speech recognition.",
                        NotificationType.Warning);
                }
                else
                {
                    _logger.LogInformation("Model downloaded via first-run dialog");
                    // Reload available models after download
                    availableModels = await _modelManager.GetAvailableModelsAsync();
                }
            }

            // Load the model if one is available
            await LoadModelIfNeededAsync();

            // Register hotkey
            _logger.LogInformation("Registering global hotkey");
            var registered = await _hotkeyService.RegisterAsync();
            if (!registered)
            {
                _logger.LogWarning("Failed to register global hotkey");
                await _notificationService.ShowNotificationAsync(
                    "Hotkey Registration Failed",
                    "Failed to register global hotkey. Check settings.",
                    NotificationType.Warning);
            }

            // Transition to idle
            _appStateManager.TransitionTo(AppState.Idle);
            _logger.LogInformation("Application initialization completed successfully");

            // Check for updates on startup if enabled in settings
            if (_configService.Settings.General.CheckForUpdates)
            {
                _ = CheckForUpdatesOnStartupAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Application initialization failed");
            _appStateManager.TransitionTo(AppState.Error);
            await _notificationService.ShowNotificationAsync(
                "Initialization Failed",
                $"Failed to initialize: {ex.Message}",
                NotificationType.Error);
        }
    }

    /// <summary>
    /// Handles hotkey pressed event
    /// </summary>
    private async void OnHotkeyPressed(object? sender, EventArgs e)
    {
        try
        {
            var mode = _configService.Settings.Hotkey.Mode;
            var currentState = _appStateManager.CurrentState;

            _logger.LogDebug("Hotkey pressed, mode: {Mode}, state: {State}", mode, currentState);

            if (mode == HotkeyMode.PushToTalk)
            {
                // Push-to-Talk: Start recording on press
                if (currentState == AppState.Idle)
                {
                    await StartRecordingAsync();
                }
            }
            else // Toggle mode
            {
                if (currentState == AppState.Idle)
                {
                    // Start recording
                    await StartRecordingAsync();
                }
                else if (currentState == AppState.Recording)
                {
                    // Stop recording and process
                    await StopRecordingAndProcessAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling hotkey press");
            _appStateManager.TransitionTo(AppState.Error);
            await _notificationService.ShowNotificationAsync(
                "Error",
                $"An error occurred: {ex.Message}",
                NotificationType.Error);
            await _notificationService.PlaySoundAsync(SoundEffect.Error);

            // Return to idle after error
            _appStateManager.TransitionTo(AppState.Idle);
        }
    }

    /// <summary>
    /// Handles hotkey released event
    /// </summary>
    private async void OnHotkeyReleased(object? sender, EventArgs e)
    {
        try
        {
            var mode = _configService.Settings.Hotkey.Mode;
            var currentState = _appStateManager.CurrentState;

            _logger.LogDebug("Hotkey released, mode: {Mode}, state: {State}", mode, currentState);

            // Only handle release in Push-to-Talk mode
            if (mode == HotkeyMode.PushToTalk && currentState == AppState.Recording)
            {
                await StopRecordingAndProcessAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling hotkey release");
            _appStateManager.TransitionTo(AppState.Error);
            await _notificationService.ShowNotificationAsync(
                "Error",
                $"An error occurred: {ex.Message}",
                NotificationType.Error);
            await _notificationService.PlaySoundAsync(SoundEffect.Error);

            // Return to idle after error
            _appStateManager.TransitionTo(AppState.Idle);
        }
    }

    /// <summary>
    /// Starts audio recording
    /// </summary>
    private async Task StartRecordingAsync()
    {
        _logger.LogInformation("Starting audio recording");

        // Transition to recording state
        _appStateManager.TransitionTo(AppState.Recording);

        // Play start sound
        if (_configService.Settings.Output.PlaySoundOnComplete)
        {
            await _notificationService.PlaySoundAsync(SoundEffect.RecordingStart);
        }

        // Show recording overlay
        _windowService.ShowRecordingOverlay();

        // Start audio capture
        await _audioService.StartRecordingAsync(_configService.Settings.SelectedAudioDevice);
    }

    /// <summary>
    /// Stops recording and processes the audio
    /// </summary>
    private async Task StopRecordingAndProcessAsync()
    {
        _logger.LogInformation("Stopping audio recording and processing");

        // Stop audio capture
        var audioData = await _audioService.StopRecordingAsync();

        // Hide recording overlay
        _windowService.HideRecordingOverlay();

        // Play stop sound
        if (_configService.Settings.Output.PlaySoundOnComplete)
        {
            await _notificationService.PlaySoundAsync(SoundEffect.RecordingStop);
        }

        // Transition to processing state
        _appStateManager.TransitionTo(AppState.Processing);

        // Check if we have audio data
        if (audioData == null || audioData.Length == 0)
        {
            _logger.LogWarning("No audio data captured");
            await _notificationService.ShowNotificationAsync(
                "No Audio",
                "No audio was recorded.",
                NotificationType.Warning);
            _appStateManager.TransitionTo(AppState.Idle);
            return;
        }

        // Transcribe the audio
        var language = _configService.Settings.Recognition.Language;
        if (language == "auto")
        {
            language = null; // Use auto-detect
        }

        _logger.LogInformation("Starting transcription of {AudioSize} bytes, language: {Language}",
            audioData.Length, language ?? "auto");

        var transcriptionResult = await _transcriptionService.TranscribeAsync(audioData, language);

        if (!transcriptionResult.IsSuccess || string.IsNullOrWhiteSpace(transcriptionResult.Value))
        {
            // Transcription failed or returned empty
            _logger.LogWarning("Transcription failed or returned empty: {Error}",
                transcriptionResult.Error ?? "No speech detected");
            await _notificationService.ShowNotificationAsync(
                "Transcription Failed",
                transcriptionResult.Error ?? "No speech detected or transcription returned empty.",
                NotificationType.Warning);
            await _notificationService.PlaySoundAsync(SoundEffect.Error);
            _appStateManager.TransitionTo(AppState.Idle);
            return;
        }

        // Output the transcribed text
        var transcribedText = transcriptionResult.Value;
        _logger.LogInformation("Transcription successful, outputting {TextLength} characters", transcribedText.Length);

        await _outputService.OutputTextAsync(transcribedText);

        // Show success notification
        await _notificationService.ShowNotificationAsync(
            "Transcription Complete",
            $"Text: {transcribedText}",
            NotificationType.Success);

        // Play success sound
        if (_configService.Settings.Output.PlaySoundOnComplete)
        {
            await _notificationService.PlaySoundAsync(SoundEffect.Success);
        }

        _logger.LogInformation("Recording workflow completed successfully");

        // Return to idle state
        _appStateManager.TransitionTo(AppState.Idle);
    }

    /// <summary>
    /// Handles application state changes and updates UI accordingly
    /// </summary>
    private void OnAppStateChanged(object? sender, AppState newState)
    {
        UpdateStatusForState(newState);
    }

    /// <summary>
    /// Handles settings changes to reload model if needed
    /// </summary>
    private async void OnSettingsChanged(object? sender, EventArgs e)
    {
        var configuredModel = _configService.Settings.Recognition.ModelName;

        // Check if the model changed
        if (_loadedModelName != configuredModel)
        {
            _logger.LogInformation("Model changed from {OldModel} to {NewModel}, reloading...",
                _loadedModelName ?? "(none)", configuredModel);
            await LoadModelIfNeededAsync();
        }
    }

    /// <summary>
    /// Loads the configured model if it's available and different from the currently loaded one
    /// </summary>
    private async Task LoadModelIfNeededAsync()
    {
        var configuredModel = _configService.Settings.Recognition.ModelName;
        var modelPath = _modelManager.GetModelPath(configuredModel);

        if (!string.IsNullOrEmpty(modelPath))
        {
            _logger.LogInformation("Loading Whisper model: {ModelName}", configuredModel);
            var loadResult = await _transcriptionService.LoadModelAsync(modelPath);
            if (loadResult.IsSuccess)
            {
                _loadedModelName = configuredModel;
                _logger.LogInformation("Model {ModelName} loaded successfully", configuredModel);
            }
            else
            {
                _logger.LogError("Failed to load Whisper model: {Error}", loadResult.Error);
                _loadedModelName = null;
                await _notificationService.ShowNotificationAsync(
                    "Model Load Failed",
                    $"Failed to load model: {loadResult.Error}",
                    NotificationType.Error);
            }
        }
        else
        {
            _logger.LogWarning("No model available to load for {ModelName}", configuredModel);
            _loadedModelName = null;
        }
    }

    /// <summary>
    /// Updates the status text and tray icon based on the current app state
    /// </summary>
    private void UpdateStatusForState(AppState state)
    {
        switch (state)
        {
            case AppState.Loading:
                StatusText = "Loading...";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-processing.ico";
                break;

            case AppState.Idle:
                StatusText = "Ready";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-idle.ico";
                break;

            case AppState.Recording:
                StatusText = "Recording...";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-recording.ico";
                break;

            case AppState.Processing:
                StatusText = "Processing...";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-processing.ico";
                break;

            case AppState.Error:
                StatusText = "Error";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-error.ico";
                break;

            default:
                StatusText = "Unknown";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-idle.ico";
                break;
        }
    }

    /// <summary>
    /// Disposes resources and unsubscribes from events
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from events to prevent memory leaks
            _appStateManager.StateChanged -= OnAppStateChanged;
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
            _hotkeyService.HotkeyReleased -= OnHotkeyReleased;
            _configService.SettingsChanged -= OnSettingsChanged;
        }
        base.Dispose(disposing);
    }
}

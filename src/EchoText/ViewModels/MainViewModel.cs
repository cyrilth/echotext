using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public MainViewModel(
        IAppStateManager appStateManager,
        IWindowService windowService,
        IHotkeyService hotkeyService,
        IAudioService audioService,
        ITranscriptionService transcriptionService,
        IOutputService outputService,
        INotificationService notificationService,
        IConfigService configService,
        IModelManager modelManager)
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

        // Subscribe to state changes
        _appStateManager.StateChanged += OnAppStateChanged;

        // Subscribe to hotkey events
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        _hotkeyService.HotkeyReleased += OnHotkeyReleased;

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
    private void CheckForUpdates()
    {
        // TODO: Implement in TASK-602
        // Check for application updates
    }

    /// <summary>
    /// Command to show about dialog
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        // TODO: Implement later
        // Show about dialog with version info
    }

    /// <summary>
    /// Command to exit the application
    /// </summary>
    [RelayCommand]
    private void ExitApplication()
    {
        // Request application shutdown
        System.Environment.Exit(0);
    }

    /// <summary>
    /// Initializes the application on startup
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            _appStateManager.TransitionTo(AppState.Loading);

            // Load configuration
            await _configService.LoadAsync();

            // Load the model
            var modelPath = _modelManager.GetModelPath(_configService.Settings.Recognition.ModelName);
            if (!string.IsNullOrEmpty(modelPath))
            {
                var loadResult = await _transcriptionService.LoadModelAsync(modelPath);
                if (!loadResult.IsSuccess)
                {
                    await _notificationService.ShowNotificationAsync(
                        "Model Load Failed",
                        $"Failed to load model: {loadResult.Error}",
                        NotificationType.Error);
                }
            }

            // Register hotkey
            var registered = await _hotkeyService.RegisterAsync();
            if (!registered)
            {
                await _notificationService.ShowNotificationAsync(
                    "Hotkey Registration Failed",
                    "Failed to register global hotkey. Check settings.",
                    NotificationType.Warning);
            }

            // Transition to idle
            _appStateManager.TransitionTo(AppState.Idle);
        }
        catch (Exception ex)
        {
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

            // Only handle release in Push-to-Talk mode
            if (mode == HotkeyMode.PushToTalk && currentState == AppState.Recording)
            {
                await StopRecordingAndProcessAsync();
            }
        }
        catch (Exception ex)
        {
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

        var transcriptionResult = await _transcriptionService.TranscribeAsync(audioData, language);

        if (!transcriptionResult.IsSuccess || string.IsNullOrWhiteSpace(transcriptionResult.Value))
        {
            // Transcription failed or returned empty
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
}

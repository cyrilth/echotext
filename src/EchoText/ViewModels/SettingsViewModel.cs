using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services.Interfaces;

namespace EchoText.ViewModels;

/// <summary>
/// ViewModel for the settings window
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IConfigService _configService;
    private readonly IAudioService _audioService;
    private readonly IModelManager _modelManager;
    private readonly IHotkeyService _hotkeyService;
    private readonly INotificationService _notificationService;
    private readonly IPlatformStartup _platformStartup;

    // Working copy of settings (not saved until user clicks Save)
    private AppSettings _workingSettings;

    // Audio Settings
    [ObservableProperty]
    private ObservableCollection<AudioDevice> _audioDevices = new();

    [ObservableProperty]
    private AudioDevice? _selectedAudioDevice;

    [ObservableProperty]
    private bool _isLoadingAudioDevices;

    // Hotkey Settings
    [ObservableProperty]
    private bool _isPushToTalkMode = true;

    [ObservableProperty]
    private bool _isToggleMode;

    [ObservableProperty]
    private string _hotkeyDisplay = "Ctrl+Shift+Space";

    [ObservableProperty]
    private bool _isCapturingHotkey;

    // Output Settings
    [ObservableProperty]
    private bool _copyToClipboard = true;

    [ObservableProperty]
    private bool _autoType;

    [ObservableProperty]
    private bool _playSoundOnComplete = true;

    // Recognition Settings
    [ObservableProperty]
    private ObservableCollection<WhisperModel> _availableModels = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadModelCommand))]
    private WhisperModel? _selectedModel;

    [ObservableProperty]
    private ObservableCollection<LanguageOption> _availableLanguages = new();

    [ObservableProperty]
    private LanguageOption? _selectedLanguage;

    [ObservableProperty]
    private bool _isLoadingModels;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DownloadModelCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelDownloadCommand))]
    private bool _isDownloadingModel;

    [ObservableProperty]
    private double _downloadProgress;

    // General Settings
    [ObservableProperty]
    private bool _startWithSystem;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private bool _checkForUpdates = true;

    // UI State
    [ObservableProperty]
    private bool _hasUnsavedChanges;

    private CancellationTokenSource? _downloadCancellation;

    /// <summary>
    /// Initializes a new instance of the SettingsViewModel
    /// </summary>
    public SettingsViewModel(
        IConfigService configService,
        IAudioService audioService,
        IModelManager modelManager,
        IHotkeyService hotkeyService,
        INotificationService notificationService,
        IPlatformStartup platformStartup)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _hotkeyService = hotkeyService ?? throw new ArgumentNullException(nameof(hotkeyService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _platformStartup = platformStartup ?? throw new ArgumentNullException(nameof(platformStartup));

        // Create a working copy of settings
        _workingSettings = CloneSettings(_configService.Settings);

        // Initialize language options
        InitializeLanguageOptions();

        // Load initial data
        _ = LoadDataAsync();
    }

    /// <summary>
    /// Load audio devices and models
    /// </summary>
    private async Task LoadDataAsync()
    {
        await LoadAudioDevicesAsync();
        await LoadModelsAsync();
        LoadSettingsToUI();
    }

    /// <summary>
    /// Load available audio devices
    /// </summary>
    private async Task LoadAudioDevicesAsync()
    {
        IsLoadingAudioDevices = true;

        try
        {
            var devices = await _audioService.GetInputDevicesAsync();
            AudioDevices.Clear();

            foreach (var device in devices)
            {
                AudioDevices.Add(device);
            }

            // Select the configured device or default
            if (_workingSettings.SelectedAudioDevice != null)
            {
                SelectedAudioDevice = AudioDevices.FirstOrDefault(d => d.Id == _workingSettings.SelectedAudioDevice)
                    ?? AudioDevices.FirstOrDefault(d => d.IsDefault);
            }
            else
            {
                SelectedAudioDevice = AudioDevices.FirstOrDefault(d => d.IsDefault);
            }
        }
        catch (Exception ex)
        {
            // Log the error and notify user
            System.Diagnostics.Debug.WriteLine($"Failed to load audio devices: {ex.Message}");
            await _notificationService.ShowNotificationAsync(
                "Audio Device Error",
                "Failed to load audio devices. Please check your audio settings.",
                NotificationType.Error);
        }
        finally
        {
            IsLoadingAudioDevices = false;
        }
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

            // Select the configured model
            SelectedModel = AvailableModels.FirstOrDefault(m => m.Name == _workingSettings.Recognition.ModelName)
                ?? AvailableModels.FirstOrDefault();
        }
        catch (Exception ex)
        {
            // Log the error and notify user
            System.Diagnostics.Debug.WriteLine($"Failed to load models: {ex.Message}");
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
    /// Initialize language options
    /// </summary>
    private void InitializeLanguageOptions()
    {
        AvailableLanguages.Clear();
        AvailableLanguages.Add(new LanguageOption("auto", "Auto-detect"));
        AvailableLanguages.Add(new LanguageOption("en", "English"));
        AvailableLanguages.Add(new LanguageOption("es", "Spanish"));
        AvailableLanguages.Add(new LanguageOption("fr", "French"));
        AvailableLanguages.Add(new LanguageOption("de", "German"));
        AvailableLanguages.Add(new LanguageOption("it", "Italian"));
        AvailableLanguages.Add(new LanguageOption("pt", "Portuguese"));
        AvailableLanguages.Add(new LanguageOption("nl", "Dutch"));
        AvailableLanguages.Add(new LanguageOption("ja", "Japanese"));
        AvailableLanguages.Add(new LanguageOption("zh", "Chinese"));
        AvailableLanguages.Add(new LanguageOption("ko", "Korean"));
    }

    /// <summary>
    /// Load settings from working copy to UI properties
    /// </summary>
    private void LoadSettingsToUI()
    {
        // Hotkey settings
        IsPushToTalkMode = _workingSettings.Hotkey.Mode == HotkeyMode.PushToTalk;
        IsToggleMode = _workingSettings.Hotkey.Mode == HotkeyMode.Toggle;
        UpdateHotkeyDisplay();

        // Output settings
        CopyToClipboard = _workingSettings.Output.CopyToClipboard;
        AutoType = _workingSettings.Output.AutoType;
        PlaySoundOnComplete = _workingSettings.Output.PlaySoundOnComplete;

        // Recognition settings
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == _workingSettings.Recognition.Language)
            ?? AvailableLanguages.FirstOrDefault();

        // General settings - sync StartWithSystem with actual platform state
        StartWithSystem = _platformStartup.IsEnabled;
        _workingSettings.General.StartWithSystem = StartWithSystem;
        ShowNotifications = _workingSettings.General.ShowNotifications;
        CheckForUpdates = _workingSettings.General.CheckForUpdates;

        HasUnsavedChanges = false;
    }

    /// <summary>
    /// Update hotkey display string
    /// </summary>
    private void UpdateHotkeyDisplay()
    {
        var parts = new List<string>();

        if (_workingSettings.Hotkey.Modifiers.HasFlag(KeyModifiers.Ctrl))
            parts.Add("Ctrl");
        if (_workingSettings.Hotkey.Modifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        if (_workingSettings.Hotkey.Modifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (_workingSettings.Hotkey.Modifiers.HasFlag(KeyModifiers.Meta))
            parts.Add("Win");

        parts.Add(_workingSettings.Hotkey.Key);

        HotkeyDisplay = string.Join("+", parts);
    }

    /// <summary>
    /// Handle hotkey mode change to Push-to-Talk
    /// </summary>
    partial void OnIsPushToTalkModeChanged(bool value)
    {
        if (value)
        {
            IsToggleMode = false;
            _workingSettings.Hotkey.Mode = HotkeyMode.PushToTalk;
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Handle hotkey mode change to Toggle
    /// </summary>
    partial void OnIsToggleModeChanged(bool value)
    {
        if (value)
        {
            IsPushToTalkMode = false;
            _workingSettings.Hotkey.Mode = HotkeyMode.Toggle;
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Handle audio device selection change
    /// </summary>
    partial void OnSelectedAudioDeviceChanged(AudioDevice? value)
    {
        if (value != null)
        {
            _workingSettings.SelectedAudioDevice = value.Id;
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Handle model selection change
    /// </summary>
    partial void OnSelectedModelChanged(WhisperModel? value)
    {
        if (value != null)
        {
            _workingSettings.Recognition.ModelName = value.Name;
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Handle language selection change
    /// </summary>
    partial void OnSelectedLanguageChanged(LanguageOption? value)
    {
        if (value != null)
        {
            _workingSettings.Recognition.Language = value.Code;
            HasUnsavedChanges = true;
        }
    }

    /// <summary>
    /// Handle checkbox changes
    /// </summary>
    partial void OnCopyToClipboardChanged(bool value)
    {
        _workingSettings.Output.CopyToClipboard = value;
        HasUnsavedChanges = true;
    }

    partial void OnAutoTypeChanged(bool value)
    {
        _workingSettings.Output.AutoType = value;
        HasUnsavedChanges = true;
    }

    partial void OnPlaySoundOnCompleteChanged(bool value)
    {
        _workingSettings.Output.PlaySoundOnComplete = value;
        HasUnsavedChanges = true;
    }

    partial void OnStartWithSystemChanged(bool value)
    {
        _workingSettings.General.StartWithSystem = value;
        HasUnsavedChanges = true;
    }

    partial void OnShowNotificationsChanged(bool value)
    {
        _workingSettings.General.ShowNotifications = value;
        HasUnsavedChanges = true;
    }

    partial void OnCheckForUpdatesChanged(bool value)
    {
        _workingSettings.General.CheckForUpdates = value;
        HasUnsavedChanges = true;
    }

    /// <summary>
    /// Command to save settings
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        // Copy working settings to config service
        _configService.Settings.SelectedAudioDevice = _workingSettings.SelectedAudioDevice;
        _configService.Settings.Hotkey.Mode = _workingSettings.Hotkey.Mode;
        _configService.Settings.Hotkey.Modifiers = _workingSettings.Hotkey.Modifiers;
        _configService.Settings.Hotkey.Key = _workingSettings.Hotkey.Key;
        _configService.Settings.Output.CopyToClipboard = _workingSettings.Output.CopyToClipboard;
        _configService.Settings.Output.AutoType = _workingSettings.Output.AutoType;
        _configService.Settings.Output.PlaySoundOnComplete = _workingSettings.Output.PlaySoundOnComplete;
        _configService.Settings.Recognition.ModelName = _workingSettings.Recognition.ModelName;
        _configService.Settings.Recognition.Language = _workingSettings.Recognition.Language;
        _configService.Settings.General.StartWithSystem = _workingSettings.General.StartWithSystem;
        _configService.Settings.General.ShowNotifications = _workingSettings.General.ShowNotifications;
        _configService.Settings.General.CheckForUpdates = _workingSettings.General.CheckForUpdates;

        // Save to disk
        await _configService.SaveAsync();

        HasUnsavedChanges = false;

        // Update startup with system setting
        try
        {
            var executablePath = Environment.ProcessPath;
            if (_workingSettings.General.StartWithSystem)
            {
                if (!string.IsNullOrEmpty(executablePath) && !_platformStartup.IsEnabled)
                {
                    var success = _platformStartup.Enable(executablePath);
                    if (!success)
                    {
                        await _notificationService.ShowNotificationAsync(
                            "Startup Setting Failed",
                            "Failed to enable start with system. You may need to configure this manually.",
                            NotificationType.Warning);
                    }
                }
            }
            else
            {
                if (_platformStartup.IsEnabled)
                {
                    _platformStartup.Disable();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to update startup setting: {ex.Message}");
            await _notificationService.ShowNotificationAsync(
                "Startup Setting Error",
                "An error occurred while updating the startup setting.",
                NotificationType.Error);
        }

        // Re-register hotkey if it changed
        if (_hotkeyService.IsRegistered)
        {
            try
            {
                await _hotkeyService.UnregisterAsync();
                var success = await _hotkeyService.RegisterAsync();

                if (!success)
                {
                    await _notificationService.ShowNotificationAsync(
                        "Hotkey Registration Failed",
                        "Failed to register the new hotkey. It may be in use by another application.",
                        NotificationType.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to re-register hotkey: {ex.Message}");
                await _notificationService.ShowNotificationAsync(
                    "Hotkey Registration Error",
                    "An error occurred while registering the hotkey. Please try again.",
                    NotificationType.Error);
            }
        }
    }

    /// <summary>
    /// Command to cancel changes
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // Reload settings from config service
        _workingSettings = CloneSettings(_configService.Settings);
        LoadSettingsToUI();
        HasUnsavedChanges = false;
    }

    /// <summary>
    /// Command to download selected model
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDownloadModel))]
    private async Task DownloadModelAsync()
    {
        if (SelectedModel == null || SelectedModel.IsDownloaded)
            return;

        IsDownloadingModel = true;
        DownloadProgress = 0;
        _downloadCancellation = new CancellationTokenSource();

        try
        {
            var progress = new Progress<double>(p => DownloadProgress = p * 100);
            await _modelManager.DownloadModelAsync(SelectedModel.Name, progress, _downloadCancellation.Token);

            // Reload models to update download status
            await LoadModelsAsync();
        }
        catch (OperationCanceledException)
        {
            // Download was cancelled
        }
        catch (Exception)
        {
            // Handle error - could show notification
        }
        finally
        {
            IsDownloadingModel = false;
            DownloadProgress = 0;
            _downloadCancellation?.Dispose();
            _downloadCancellation = null;
        }
    }

    private bool CanDownloadModel()
    {
        return SelectedModel != null && !SelectedModel.IsDownloaded && !IsDownloadingModel;
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
        return IsDownloadingModel;
    }

    /// <summary>
    /// Command to test audio device
    /// </summary>
    [RelayCommand]
    private async Task TestAudioAsync()
    {
        // TODO: Implement audio test
        // Could start recording for 2 seconds and show audio level
        await Task.CompletedTask;
    }

    /// <summary>
    /// Command to change hotkey - toggles capture mode
    /// </summary>
    [RelayCommand]
    private void ChangeHotkey()
    {
        IsCapturingHotkey = !IsCapturingHotkey;
        if (IsCapturingHotkey)
        {
            HotkeyDisplay = "Press a key combination...";
        }
        else
        {
            UpdateHotkeyDisplay();
        }
    }

    /// <summary>
    /// Called from the view when a key is pressed during hotkey capture
    /// </summary>
    public void CaptureHotkey(KeyModifiers modifiers, string key)
    {
        if (!IsCapturingHotkey)
            return;

        // Require at least one modifier
        if (modifiers == KeyModifiers.None)
        {
            HotkeyDisplay = "Please include a modifier (Ctrl, Shift, Alt)";
            return;
        }

        // Update working settings
        _workingSettings.Hotkey.Modifiers = modifiers;
        _workingSettings.Hotkey.Key = key;
        HasUnsavedChanges = true;

        // Exit capture mode and update display
        IsCapturingHotkey = false;
        UpdateHotkeyDisplay();
    }

    /// <summary>
    /// Cancel hotkey capture
    /// </summary>
    public void CancelHotkeyCapture()
    {
        if (IsCapturingHotkey)
        {
            IsCapturingHotkey = false;
            UpdateHotkeyDisplay();
        }
    }

    /// <summary>
    /// Clone settings to create working copy
    /// </summary>
    private static AppSettings CloneSettings(AppSettings original)
    {
        return new AppSettings
        {
            SelectedAudioDevice = original.SelectedAudioDevice,
            Hotkey = new HotkeySettings
            {
                Modifiers = original.Hotkey.Modifiers,
                Key = original.Hotkey.Key,
                Mode = original.Hotkey.Mode
            },
            Output = new OutputSettings
            {
                CopyToClipboard = original.Output.CopyToClipboard,
                AutoType = original.Output.AutoType,
                PlaySoundOnComplete = original.Output.PlaySoundOnComplete,
                KeystrokeDelayMs = original.Output.KeystrokeDelayMs
            },
            Recognition = new RecognitionSettings
            {
                ModelName = original.Recognition.ModelName,
                Language = original.Recognition.Language
            },
            General = new GeneralSettings
            {
                StartWithSystem = original.General.StartWithSystem,
                ShowNotifications = original.General.ShowNotifications,
                CheckForUpdates = original.General.CheckForUpdates
            }
        };
    }
}

/// <summary>
/// Represents a language option for the UI
/// </summary>
/// <param name="Code">Language code (e.g., "en", "es")</param>
/// <param name="DisplayName">Human-readable name</param>
public record LanguageOption(string Code, string DisplayName);

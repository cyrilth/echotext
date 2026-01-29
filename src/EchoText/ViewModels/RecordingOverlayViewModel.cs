using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EchoText.Models;
using EchoText.Services.Interfaces;

namespace EchoText.ViewModels;

/// <summary>
/// ViewModel for the recording overlay window.
/// Displays recording duration, audio level, and cancel button.
/// </summary>
public partial class RecordingOverlayViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private readonly IAppStateManager _appStateManager;

    /// <summary>
    /// Recording duration displayed as formatted string (mm:ss)
    /// </summary>
    [ObservableProperty]
    private string _recordingDuration = "00:00";

    /// <summary>
    /// Audio level from 0 to 100 for visualization
    /// </summary>
    [ObservableProperty]
    private double _audioLevel = 0;

    /// <summary>
    /// Initializes a new instance of the RecordingOverlayViewModel
    /// </summary>
    /// <param name="audioService">Audio service for recording duration and audio level</param>
    /// <param name="appStateManager">App state manager for state transitions</param>
    public RecordingOverlayViewModel(
        IAudioService audioService,
        IAppStateManager appStateManager)
    {
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _appStateManager = appStateManager ?? throw new ArgumentNullException(nameof(appStateManager));

        // Subscribe to audio level changes
        _audioService.AudioLevelChanged += OnAudioLevelChanged;

        // Subscribe to state changes
        _appStateManager.StateChanged += OnStateChanged;

        // Initialize with current values
        UpdateRecordingDuration();
    }

    /// <summary>
    /// Handle audio level changes from the audio service
    /// </summary>
    private void OnAudioLevelChanged(object? sender, float level)
    {
        // Convert from 0.0-1.0 to 0-100 for UI display
        AudioLevel = level * 100;
    }

    /// <summary>
    /// Handle state changes to update the overlay visibility
    /// </summary>
    private void OnStateChanged(object? sender, AppState newState)
    {
        // When state changes, update recording duration
        UpdateRecordingDuration();
    }

    /// <summary>
    /// Update the recording duration display
    /// </summary>
    private void UpdateRecordingDuration()
    {
        if (_audioService.IsRecording)
        {
            var duration = _audioService.RecordingDuration;
            RecordingDuration = $"{duration.Minutes:D2}:{duration.Seconds:D2}";
        }
        else
        {
            RecordingDuration = "00:00";
            AudioLevel = 0;
        }
    }

    /// <summary>
    /// Command to cancel the current recording
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        // Transition to idle state which will stop recording
        _appStateManager.TransitionTo(AppState.Idle);
    }

    /// <summary>
    /// Start periodic duration updates
    /// </summary>
    public void StartDurationUpdates()
    {
        // Update duration every 100ms
        var timer = new System.Timers.Timer(100);
        timer.Elapsed += (sender, args) =>
        {
            if (_audioService.IsRecording)
            {
                UpdateRecordingDuration();
            }
            else
            {
                timer.Stop();
                timer.Dispose();
            }
        };
        timer.Start();
    }

    /// <summary>
    /// Clean up event subscriptions
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _audioService.AudioLevelChanged -= OnAudioLevelChanged;
            _appStateManager.StateChanged -= OnStateChanged;
        }
        base.Dispose(disposing);
    }
}

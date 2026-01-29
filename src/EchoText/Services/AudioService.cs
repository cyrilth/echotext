using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Audio capture service that wraps platform-specific audio providers.
/// Adds max recording duration enforcement and device selection from config.
/// </summary>
public class AudioService : IAudioService
{
    private const int MaxRecordingSeconds = 120;

    private readonly IPlatformAudio _platformAudio;
    private readonly IConfigService _configService;
    private Timer? _maxDurationTimer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AudioService.
    /// </summary>
    /// <param name="platformAudio">Platform-specific audio provider.</param>
    /// <param name="configService">Configuration service for device selection.</param>
    public AudioService(IPlatformAudio platformAudio, IConfigService configService)
    {
        _platformAudio = platformAudio ?? throw new ArgumentNullException(nameof(platformAudio));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        // Forward audio level events from platform provider
        _platformAudio.AudioLevelChanged += OnPlatformAudioLevelChanged;
    }

    /// <inheritdoc/>
    public bool IsRecording => _platformAudio.IsRecording;

    /// <inheritdoc/>
    public TimeSpan RecordingDuration => _platformAudio.RecordingDuration;

    /// <inheritdoc/>
    public event EventHandler<float>? AudioLevelChanged;

    /// <inheritdoc/>
    public Task<IReadOnlyList<AudioDevice>> GetInputDevicesAsync()
    {
        return Task.Run(() => _platformAudio.GetInputDevices());
    }

    /// <inheritdoc/>
    public Task StartRecordingAsync(string? deviceId = null)
    {
        return Task.Run(() =>
        {
            if (IsRecording)
            {
                throw new InvalidOperationException("Recording is already in progress.");
            }

            // Use provided device ID, or fall back to configured device, or null for default
            var actualDeviceId = deviceId ?? _configService.Settings.SelectedAudioDevice;

            // Start recording
            _platformAudio.StartCapture(actualDeviceId);

            // Set up max duration timer
            _maxDurationTimer = new Timer(
                OnMaxDurationReached,
                null,
                TimeSpan.FromSeconds(MaxRecordingSeconds),
                Timeout.InfiniteTimeSpan
            );
        });
    }

    /// <inheritdoc/>
    public Task<byte[]> StopRecordingAsync()
    {
        return Task.Run(() =>
        {
            if (!IsRecording)
            {
                throw new InvalidOperationException("No recording is in progress.");
            }

            // Cancel max duration timer
            _maxDurationTimer?.Dispose();
            _maxDurationTimer = null;

            // Stop capture and get audio data
            return _platformAudio.StopCapture();
        });
    }

    /// <summary>
    /// Called when the platform audio provider reports a level change.
    /// </summary>
    private void OnPlatformAudioLevelChanged(object? sender, float level)
    {
        AudioLevelChanged?.Invoke(this, level);
    }

    /// <summary>
    /// Called when max recording duration is reached. Automatically stops recording.
    /// </summary>
    private void OnMaxDurationReached(object? state)
    {
        if (IsRecording)
        {
            // Stop recording (this will discard the timer already)
            _ = Task.Run(async () =>
            {
                try
                {
                    await StopRecordingAsync();
                }
                catch
                {
                    // Silently handle - this is a safety mechanism
                }
            });
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _maxDurationTimer?.Dispose();
        _maxDurationTimer = null;

        _platformAudio.AudioLevelChanged -= OnPlatformAudioLevelChanged;
        _platformAudio.Dispose();
        _disposed = true;
    }
}

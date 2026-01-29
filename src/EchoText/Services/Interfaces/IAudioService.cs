using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoText.Models;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for audio capture and device management.
/// Wraps the platform-specific audio provider and adds application-level features.
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Get a list of available audio input devices.
    /// </summary>
    /// <returns>A task that returns a read-only list of audio input devices.</returns>
    Task<IReadOnlyList<AudioDevice>> GetInputDevicesAsync();

    /// <summary>
    /// Start capturing audio from the specified device.
    /// If no device is specified, uses the device from config or system default.
    /// </summary>
    /// <param name="deviceId">Device ID, or null to use configured/default device.</param>
    /// <returns>A task representing the asynchronous start operation.</returns>
    Task StartRecordingAsync(string? deviceId = null);

    /// <summary>
    /// Stop capturing audio and return the recorded data.
    /// </summary>
    /// <returns>A task that returns audio data in 16kHz mono 16-bit WAV format.</returns>
    Task<byte[]> StopRecordingAsync();

    /// <summary>
    /// Gets whether audio is currently being recorded.
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Gets the current recording duration.
    /// </summary>
    TimeSpan RecordingDuration { get; }

    /// <summary>
    /// Fired periodically during recording to report audio level (0.0 to 1.0).
    /// Useful for visualization.
    /// </summary>
    event EventHandler<float>? AudioLevelChanged;
}

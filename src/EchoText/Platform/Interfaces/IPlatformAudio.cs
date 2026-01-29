using System;
using System.Collections.Generic;
using EchoText.Models;

namespace EchoText.Platform.Interfaces;

/// <summary>
/// Platform-specific interface for audio capture functionality
/// </summary>
public interface IPlatformAudio : IDisposable
{
    /// <summary>
    /// Get a list of available audio input devices
    /// </summary>
    /// <returns>List of audio input devices</returns>
    IReadOnlyList<AudioDevice> GetInputDevices();

    /// <summary>
    /// Start capturing audio from the specified device
    /// </summary>
    /// <param name="deviceId">Device ID, or null to use the default device</param>
    void StartCapture(string? deviceId = null);

    /// <summary>
    /// Stop capturing audio and return the recorded data
    /// </summary>
    /// <returns>Audio data in 16kHz mono 16-bit WAV format</returns>
    byte[] StopCapture();

    /// <summary>
    /// Gets whether audio is currently being recorded
    /// </summary>
    bool IsRecording { get; }

    /// <summary>
    /// Gets the current recording duration
    /// </summary>
    TimeSpan RecordingDuration { get; }

    /// <summary>
    /// Fired periodically during recording to report audio level (0.0 to 1.0)
    /// </summary>
    event EventHandler<float>? AudioLevelChanged;
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using NAudio.Wave;

namespace EchoText.Platform.Windows;

/// <summary>
/// Windows implementation of audio capture using NAudio
/// </summary>
public class WindowsAudioProvider : IPlatformAudio
{
    private const int SampleRate = 16000;
    private const int Channels = 1; // Mono
    private const int BitsPerSample = 16;

    private WaveInEvent? _waveIn;
    private MemoryStream? _recordingStream;
    private WaveFileWriter? _waveWriter;
    private string? _currentDeviceId;
    private readonly Stopwatch _recordingTimer;
    private bool _disposed;

    public event EventHandler<float>? AudioLevelChanged;

    public bool IsRecording { get; private set; }

    public TimeSpan RecordingDuration => _recordingTimer.Elapsed;

    public WindowsAudioProvider()
    {
        _recordingTimer = new Stopwatch();
    }

    public IReadOnlyList<AudioDevice> GetInputDevices()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsAudioProvider));

        var devices = new List<AudioDevice>();

        for (int i = 0; i < WaveInEvent.DeviceCount; i++)
        {
            var capabilities = WaveInEvent.GetCapabilities(i);
            devices.Add(new AudioDevice(
                Id: i.ToString(),
                Name: capabilities.ProductName,
                IsDefault: i == 0
            ));
        }

        return devices;
    }

    public void StartCapture(string? deviceId = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsAudioProvider));

        if (IsRecording)
            throw new InvalidOperationException("Already recording");

        try
        {
            _currentDeviceId = deviceId;

            // Parse device ID
            int deviceNumber = 0;
            if (!string.IsNullOrEmpty(deviceId) && int.TryParse(deviceId, out var parsedId))
            {
                deviceNumber = parsedId;
            }

            // Create wave input with 16kHz mono format (Whisper requirement)
            _waveIn = new WaveInEvent
            {
                DeviceNumber = deviceNumber,
                WaveFormat = new WaveFormat(SampleRate, BitsPerSample, Channels)
            };

            // Create memory stream to store recording
            _recordingStream = new MemoryStream();
            _waveWriter = new WaveFileWriter(_recordingStream, _waveIn.WaveFormat);

            // Subscribe to data available event
            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            // Start recording
            _waveIn.StartRecording();
            _recordingTimer.Restart();
            IsRecording = true;
        }
        catch
        {
            CleanupRecording();
            throw;
        }
    }

    public byte[] StopCapture()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WindowsAudioProvider));

        if (!IsRecording)
            return Array.Empty<byte>();

        try
        {
            // Stop recording
            _waveIn?.StopRecording();
            _recordingTimer.Stop();
            IsRecording = false;

            // Flush and close the wave writer
            _waveWriter?.Flush();
            _waveWriter?.Dispose();
            _waveWriter = null;

            // Get the recorded audio data
            var audioData = _recordingStream?.ToArray() ?? Array.Empty<byte>();

            // Clean up
            CleanupRecording();

            return audioData;
        }
        catch
        {
            CleanupRecording();
            throw;
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (_waveWriter == null || !IsRecording)
            return;

        // Write the recorded data to the wave file
        _waveWriter.Write(e.Buffer, 0, e.BytesRecorded);

        // Calculate audio level for visualization
        float level = CalculateAudioLevel(e.Buffer, e.BytesRecorded);
        AudioLevelChanged?.Invoke(this, level);
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            // Handle recording errors
            IsRecording = false;
            _recordingTimer.Stop();
            CleanupRecording();
        }
    }

    private static float CalculateAudioLevel(byte[] buffer, int bytesRecorded)
    {
        if (bytesRecorded == 0)
            return 0f;

        // Calculate RMS (Root Mean Square) for 16-bit samples
        long sum = 0;
        int sampleCount = bytesRecorded / 2; // 16-bit = 2 bytes per sample

        for (int i = 0; i < bytesRecorded; i += 2)
        {
            if (i + 1 < bytesRecorded)
            {
                short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
                sum += sample * sample;
            }
        }

        if (sampleCount == 0)
            return 0f;

        double rms = Math.Sqrt((double)sum / sampleCount);

        // Normalize to 0.0 - 1.0 range (32767 is max for 16-bit signed)
        float level = (float)(rms / 32767.0);

        return Math.Clamp(level, 0f, 1f);
    }

    private void CleanupRecording()
    {
        try
        {
            if (_waveIn != null)
            {
                _waveIn.DataAvailable -= OnDataAvailable;
                _waveIn.RecordingStopped -= OnRecordingStopped;
                _waveIn.Dispose();
                _waveIn = null;
            }

            _waveWriter?.Dispose();
            _waveWriter = null;

            _recordingStream?.Dispose();
            _recordingStream = null;
        }
        catch
        {
            // Suppress cleanup errors
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (IsRecording)
        {
            try
            {
                StopCapture();
            }
            catch
            {
                // Suppress errors during disposal
            }
        }

        CleanupRecording();
        GC.SuppressFinalize(this);
    }
}

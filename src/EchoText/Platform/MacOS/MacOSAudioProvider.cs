using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using EchoText.Models;
using EchoText.Platform.Interfaces;

namespace EchoText.Platform.MacOS;

/// <summary>
/// macOS implementation of audio capture functionality
/// Uses sox (Sound eXchange) for audio capture with CoreAudio support
/// Requires Microphone permission - macOS will prompt user on first use
/// Install sox via: brew install sox
/// </summary>
public class MacOSAudioProvider : IPlatformAudio
{
    private const int SampleRate = 16000;
    private const int Channels = 1;
    private const int BitsPerSample = 16;
    private const int MaxRecordingSeconds = 120;

    private Process? _recordingProcess;
    private string? _tempAudioFile;
    private DateTime _recordingStartTime;
    private bool _isRecording;
    private bool _disposed;
    private Timer? _levelTimer;

    public event EventHandler<float>? AudioLevelChanged;

    public bool IsRecording => _isRecording;

    public TimeSpan RecordingDuration =>
        _isRecording ? DateTime.Now - _recordingStartTime : TimeSpan.Zero;

    public IReadOnlyList<AudioDevice> GetInputDevices()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSAudioProvider));

        var devices = new List<AudioDevice>();

        try
        {
            // Try to get audio devices using system_profiler
            var soxDevices = GetSoxAudioDevices();
            if (soxDevices.Any())
            {
                return soxDevices;
            }

            // Fallback: return macOS default device
            devices.Add(new AudioDevice("default", "Default Microphone", true));
        }
        catch
        {
            // If all else fails, return a single default device
            devices.Add(new AudioDevice("default", "Default Microphone", true));
        }

        return devices;
    }

    public void StartCapture(string? deviceId = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSAudioProvider));

        if (_isRecording)
            throw new InvalidOperationException("Already recording");

        try
        {
            // Create a temporary file for recording
            _tempAudioFile = Path.Combine(Path.GetTempPath(), $"echotext_recording_{Guid.NewGuid()}.wav");

            // Use sox to capture audio
            // sox uses CoreAudio on macOS by default
            // This will trigger microphone permission prompt if needed
            var device = string.IsNullOrEmpty(deviceId) ? "default" : deviceId;

            // sox command: rec (alias for sox -d) outputs to file
            // -r = sample rate, -c = channels, -b = bits per sample
            // trim 0 MaxRecordingSeconds = record for max duration
            var startInfo = new ProcessStartInfo
            {
                FileName = "sox",
                Arguments = $"-d -r {SampleRate} -c {Channels} -b {BitsPerSample} \"{_tempAudioFile}\" trim 0 {MaxRecordingSeconds}",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            _recordingProcess = new Process { StartInfo = startInfo };
            _recordingProcess.Start();

            _recordingStartTime = DateTime.Now;
            _isRecording = true;

            // Start a timer to simulate audio level changes
            // Note: Getting real-time audio levels from sox would require parsing stderr
            // or using a different approach. For now, we simulate it.
            _levelTimer = new Timer(
                _ => AudioLevelChanged?.Invoke(this, GetRandomLevel()),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMilliseconds(100)
            );
        }
        catch (Exception ex)
        {
            _isRecording = false;
            _recordingProcess?.Dispose();
            _recordingProcess = null;

            if (File.Exists(_tempAudioFile))
            {
                File.Delete(_tempAudioFile);
                _tempAudioFile = null;
            }

            // Check if sox is not installed
            var message = "Failed to start audio capture. ";
            if (ex is System.ComponentModel.Win32Exception)
            {
                message += "Ensure 'sox' is installed (brew install sox). ";
            }
            message += "Also check that microphone permission is granted in System Preferences > Security & Privacy > Privacy > Microphone.";

            throw new InvalidOperationException(message, ex);
        }
    }

    public byte[] StopCapture()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(MacOSAudioProvider));

        if (!_isRecording)
            throw new InvalidOperationException("Not recording");

        try
        {
            // Stop the level timer
            _levelTimer?.Dispose();
            _levelTimer = null;

            // Stop the recording process
            if (_recordingProcess != null && !_recordingProcess.HasExited)
            {
                // Send SIGINT (Ctrl+C) to gracefully stop sox
                _recordingProcess.Kill();
                _recordingProcess.WaitForExit(1000);
            }

            _recordingProcess?.Dispose();
            _recordingProcess = null;
            _isRecording = false;

            // Wait a moment for the file to be fully written
            Thread.Sleep(100);

            // Read the recorded audio file
            if (_tempAudioFile != null && File.Exists(_tempAudioFile))
            {
                var audioData = File.ReadAllBytes(_tempAudioFile);

                // Clean up the temp file
                try
                {
                    File.Delete(_tempAudioFile);
                }
                catch
                {
                    // Ignore cleanup errors
                }

                _tempAudioFile = null;
                return audioData;
            }

            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            _isRecording = false;
            throw new InvalidOperationException("Failed to stop audio capture", ex);
        }
    }

    private static IReadOnlyList<AudioDevice> GetSoxAudioDevices()
    {
        var devices = new List<AudioDevice>();

        try
        {
            // On macOS, sox uses CoreAudio backend
            // We can try to list devices, but sox doesn't have a great device listing feature
            // For now, we'll just return the default device
            // A more advanced implementation could use system_profiler SPAudioDataType
            // or parse sox error output when trying different devices

            devices.Add(new AudioDevice("default", "Default Microphone", true));

            // You could enhance this by parsing system_profiler output:
            // system_profiler SPAudioDataType
        }
        catch
        {
            // If we can't enumerate devices, just return default
        }

        return devices;
    }

    private static float GetRandomLevel()
    {
        // Simulate audio level for visualization
        // In a real implementation, we could parse sox's stderr output
        // or use a different approach to get real-time levels
        return (float)Random.Shared.NextDouble() * 0.3f + 0.1f;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            _levelTimer?.Dispose();
            _levelTimer = null;

            if (_recordingProcess != null)
            {
                if (!_recordingProcess.HasExited)
                {
                    _recordingProcess.Kill();
                    _recordingProcess.WaitForExit(1000);
                }

                _recordingProcess.Dispose();
                _recordingProcess = null;
            }

            if (_tempAudioFile != null && File.Exists(_tempAudioFile))
            {
                try
                {
                    File.Delete(_tempAudioFile);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        catch
        {
            // Suppress exceptions during disposal
        }

        GC.SuppressFinalize(this);
    }
}

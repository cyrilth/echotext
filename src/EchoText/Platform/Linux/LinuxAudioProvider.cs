using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using EchoText.Models;
using EchoText.Platform.Interfaces;

namespace EchoText.Platform.Linux;

/// <summary>
/// Linux implementation of audio capture functionality
/// Uses arecord (ALSA) for audio capture with PulseAudio support
/// Falls back to basic functionality if tools are not available
/// </summary>
public class LinuxAudioProvider : IPlatformAudio
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
            throw new ObjectDisposedException(nameof(LinuxAudioProvider));

        var devices = new List<AudioDevice>();

        try
        {
            // Try to get PulseAudio devices using pactl
            var pactlDevices = GetPulseAudioDevices();
            if (pactlDevices.Any())
            {
                return pactlDevices;
            }

            // Fallback: return ALSA default device
            devices.Add(new AudioDevice("default", "Default Audio Device", true));
        }
        catch
        {
            // If all else fails, return a single default device
            devices.Add(new AudioDevice("default", "Default Audio Device", true));
        }

        return devices;
    }

    public void StartCapture(string? deviceId = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LinuxAudioProvider));

        if (_isRecording)
            throw new InvalidOperationException("Already recording");

        try
        {
            // Create a temporary file for recording
            _tempAudioFile = Path.Combine(Path.GetTempPath(), $"echotext_recording_{Guid.NewGuid()}.wav");

            // Use arecord to capture audio
            var device = string.IsNullOrEmpty(deviceId) ? "default" : deviceId;

            var startInfo = new ProcessStartInfo
            {
                FileName = "arecord",
                Arguments = $"-D {device} -f S16_LE -r {SampleRate} -c {Channels} -d {MaxRecordingSeconds} \"{_tempAudioFile}\"",
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

            throw new InvalidOperationException(
                "Failed to start audio capture. Ensure 'arecord' is installed (sudo apt-get install alsa-utils).",
                ex
            );
        }
    }

    public byte[] StopCapture()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LinuxAudioProvider));

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
                _recordingProcess.Kill();
                _recordingProcess.WaitForExit(1000);
            }

            _recordingProcess?.Dispose();
            _recordingProcess = null;
            _isRecording = false;

            // Wait a moment for the file to be written
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

    private static IReadOnlyList<AudioDevice> GetPulseAudioDevices()
    {
        var devices = new List<AudioDevice>();

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "pactl",
                Arguments = "list sources short",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
                return devices;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return devices;

            // Parse the output
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var deviceName = parts[1].Trim();
                    var deviceId = parts[1].Trim();
                    var isDefault = deviceName.Contains("default", StringComparison.OrdinalIgnoreCase);

                    // Get a friendly name if possible
                    var friendlyName = parts.Length >= 3 ? parts[2].Trim() : deviceName;

                    devices.Add(new AudioDevice(deviceId, friendlyName, isDefault));
                }
            }

            // If we found devices but none is marked as default, mark the first one
            if (devices.Any() && !devices.Any(d => d.IsDefault))
            {
                var first = devices[0];
                devices[0] = first with { IsDefault = true };
            }
        }
        catch
        {
            // If PulseAudio is not available, return empty list
        }

        return devices;
    }

    private static float GetRandomLevel()
    {
        // Simulate audio level for visualization
        // In a real implementation, we would parse arecord output or use a different approach
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

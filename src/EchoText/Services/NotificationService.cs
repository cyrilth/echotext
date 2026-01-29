using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Service for showing toast notifications and playing sound effects.
/// Respects user settings for notifications and sounds.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IConfigService _configService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="configService">Configuration service to check user preferences</param>
    public NotificationService(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <inheritdoc/>
    public Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info)
    {
        // Check if notifications are enabled
        if (!_configService.Settings.General.ShowNotifications)
        {
            return Task.CompletedTask;
        }

        // For now, we'll use console output as a placeholder
        // In a full implementation, this would use Avalonia's notification system
        // or a platform-specific notification API
        var icon = type switch
        {
            NotificationType.Info => "ℹ",
            NotificationType.Success => "✓",
            NotificationType.Warning => "⚠",
            NotificationType.Error => "✗",
            _ => "•"
        };

        Console.WriteLine($"[{icon}] {title}: {message}");

        // TODO: Implement proper toast notifications using Avalonia notification system
        // when UI integration is complete (TASK-501 and TASK-504)

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task PlaySoundAsync(SoundEffect sound)
    {
        // Check if sounds are enabled
        if (!_configService.Settings.Output.PlaySoundOnComplete)
        {
            return Task.CompletedTask;
        }

        // For now, we'll use system beep as a placeholder
        // In a full implementation, this would load and play .wav files from Assets/Sounds/
        try
        {
            // Different beep patterns for different sounds
            switch (sound)
            {
                case SoundEffect.RecordingStart:
                    // Short high beep
                    PlaySystemBeep(800, 100);
                    break;

                case SoundEffect.RecordingStop:
                    // Short low beep
                    PlaySystemBeep(400, 100);
                    break;

                case SoundEffect.Success:
                    // Two short high beeps
                    PlaySystemBeep(800, 100);
                    Task.Delay(50).Wait();
                    PlaySystemBeep(1000, 100);
                    break;

                case SoundEffect.Error:
                    // Long low beep
                    PlaySystemBeep(300, 300);
                    break;
            }

            // TODO: Implement proper WAV file playback when sound assets are added
            // Sound files should be stored in Assets/Sounds/:
            // - start.wav (RecordingStart)
            // - stop.wav (RecordingStop)
            // - success.wav (Success)
            // - error.wav (Error)
        }
        catch (Exception)
        {
            // Silently fail if sound playback fails
            // We don't want to crash the app over a sound effect
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Plays a system beep with specified frequency and duration.
    /// </summary>
    /// <param name="frequency">Frequency in Hz</param>
    /// <param name="duration">Duration in milliseconds</param>
    private static void PlaySystemBeep(int frequency, int duration)
    {
        try
        {
            // Console.Beep only works on Windows and requires specific permissions
            // On other platforms, this will silently fail
            if (OperatingSystem.IsWindows())
            {
                Console.Beep(frequency, duration);
            }
            else
            {
                // On Linux/macOS, we could use the system bell
                Console.Write("\a");
            }
        }
        catch
        {
            // Silently ignore beep failures
        }
    }

    /// <summary>
    /// Gets the path to a sound effect file.
    /// </summary>
    /// <param name="sound">The sound effect</param>
    /// <returns>Path to the WAV file</returns>
    private static string GetSoundFilePath(SoundEffect sound)
    {
        var fileName = sound switch
        {
            SoundEffect.RecordingStart => "start.wav",
            SoundEffect.RecordingStop => "stop.wav",
            SoundEffect.Success => "success.wav",
            SoundEffect.Error => "error.wav",
            _ => throw new ArgumentOutOfRangeException(nameof(sound))
        };

        // Sound files should be embedded resources or stored in Assets/Sounds/
        // For now, return the expected path
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "Assets", "Sounds", fileName);
    }
}

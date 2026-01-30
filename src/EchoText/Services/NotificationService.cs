using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using EchoText.Models;
using EchoText.Services.Interfaces;
using AppNotificationType = EchoText.Models.NotificationType;

namespace EchoText.Services;

/// <summary>
/// Service for showing toast notifications and playing sound effects.
/// Respects user settings for notifications and sounds.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IConfigService _configService;
    private WindowNotificationManager? _notificationManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationService"/> class.
    /// </summary>
    /// <param name="configService">Configuration service to check user preferences</param>
    public NotificationService(IConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
    }

    /// <inheritdoc/>
    public Task ShowNotificationAsync(string title, string message, AppNotificationType type = AppNotificationType.Info)
    {
        // Check if notifications are enabled
        if (!_configService.Settings.General.ShowNotifications)
        {
            return Task.CompletedTask;
        }

        // Must run on UI thread
        Dispatcher.UIThread.Post(() =>
        {
            EnsureNotificationManager();

            if (_notificationManager != null)
            {
                var notificationType = type switch
                {
                    AppNotificationType.Info => Avalonia.Controls.Notifications.NotificationType.Information,
                    AppNotificationType.Success => Avalonia.Controls.Notifications.NotificationType.Success,
                    AppNotificationType.Warning => Avalonia.Controls.Notifications.NotificationType.Warning,
                    AppNotificationType.Error => Avalonia.Controls.Notifications.NotificationType.Error,
                    _ => Avalonia.Controls.Notifications.NotificationType.Information
                };

                _notificationManager.Show(new Notification(title, message, notificationType));
            }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Ensures the notification manager is initialized.
    /// </summary>
    private void EnsureNotificationManager()
    {
        if (_notificationManager != null)
            return;

        // Get the main window to attach notifications to
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = desktop.MainWindow;
            if (mainWindow != null)
            {
                _notificationManager = new WindowNotificationManager(mainWindow)
                {
                    Position = NotificationPosition.BottomRight,
                    MaxItems = 3
                };
            }
        }
    }

    /// <inheritdoc/>
    public Task PlaySoundAsync(SoundEffect sound)
    {
        // Check if sounds are enabled
        if (!_configService.Settings.Output.PlaySoundOnComplete)
        {
            return Task.CompletedTask;
        }

        try
        {
            // Different beep patterns for different sounds
            switch (sound)
            {
                case SoundEffect.RecordingStart:
                    PlaySystemBeep(800, 100);
                    break;

                case SoundEffect.RecordingStop:
                    PlaySystemBeep(400, 100);
                    break;

                case SoundEffect.Success:
                    PlaySystemBeep(800, 100);
                    Task.Delay(50).Wait();
                    PlaySystemBeep(1000, 100);
                    break;

                case SoundEffect.Error:
                    PlaySystemBeep(300, 300);
                    break;
            }
        }
        catch (Exception)
        {
            // Silently fail if sound playback fails
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Plays a system beep with specified frequency and duration.
    /// </summary>
    private static void PlaySystemBeep(int frequency, int duration)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Console.Beep(frequency, duration);
            }
            else
            {
                Console.Write("\a");
            }
        }
        catch
        {
            // Silently ignore beep failures
        }
    }
}

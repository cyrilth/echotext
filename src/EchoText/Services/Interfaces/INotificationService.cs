using System.Threading.Tasks;
using EchoText.Models;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for showing toast notifications and playing sound effects.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows a toast notification to the user.
    /// </summary>
    /// <param name="title">Notification title</param>
    /// <param name="message">Notification message</param>
    /// <param name="type">Type of notification (Info, Success, Warning, Error)</param>
    Task ShowNotificationAsync(string title, string message, NotificationType type = NotificationType.Info);

    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="sound">The sound effect to play</param>
    Task PlaySoundAsync(SoundEffect sound);
}

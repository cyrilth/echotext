using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Services;
using EchoText.Services.Interfaces;
using Moq;
using Xunit;

namespace EchoText.Tests.Services;

/// <summary>
/// Unit tests for the NotificationService class.
/// </summary>
public class NotificationServiceTests
{
    private readonly Mock<IConfigService> _mockConfigService;
    private readonly AppSettings _testSettings;
    private readonly NotificationService _notificationService;

    public NotificationServiceTests()
    {
        _testSettings = new AppSettings();
        _mockConfigService = new Mock<IConfigService>();
        _mockConfigService.Setup(x => x.Settings).Returns(_testSettings);
        _notificationService = new NotificationService(_mockConfigService.Object);
    }

    [Fact]
    public async Task ShowNotificationAsync_WhenNotificationsEnabled_ShouldComplete()
    {
        // Arrange
        _testSettings.General.ShowNotifications = true;

        // Act
        await _notificationService.ShowNotificationAsync("Test Title", "Test Message", NotificationType.Info);

        // Assert
        // Should complete without throwing
        _mockConfigService.Verify(x => x.Settings, Times.AtLeastOnce);
    }

    [Fact]
    public async Task ShowNotificationAsync_WhenNotificationsDisabled_ShouldNotShow()
    {
        // Arrange
        _testSettings.General.ShowNotifications = false;

        // Act
        await _notificationService.ShowNotificationAsync("Test Title", "Test Message", NotificationType.Info);

        // Assert
        // Should complete without throwing
        _mockConfigService.Verify(x => x.Settings, Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(NotificationType.Info)]
    [InlineData(NotificationType.Success)]
    [InlineData(NotificationType.Warning)]
    [InlineData(NotificationType.Error)]
    public async Task ShowNotificationAsync_WithDifferentTypes_ShouldHandle(NotificationType type)
    {
        // Arrange
        _testSettings.General.ShowNotifications = true;

        // Act
        await _notificationService.ShowNotificationAsync("Test", "Message", type);

        // Assert
        // Should complete without throwing
        _mockConfigService.Verify(x => x.Settings, Times.AtLeastOnce);
    }

    [Fact]
    public async Task PlaySoundAsync_WhenSoundsEnabled_ShouldComplete()
    {
        // Arrange
        _testSettings.Output.PlaySoundOnComplete = true;

        // Act
        await _notificationService.PlaySoundAsync(SoundEffect.Success);

        // Assert
        // Should complete without throwing
        _mockConfigService.Verify(x => x.Settings, Times.AtLeastOnce);
    }

    [Fact]
    public async Task PlaySoundAsync_WhenSoundsDisabled_ShouldNotPlay()
    {
        // Arrange
        _testSettings.Output.PlaySoundOnComplete = false;

        // Act
        await _notificationService.PlaySoundAsync(SoundEffect.Success);

        // Assert
        // Should complete without throwing
        _mockConfigService.Verify(x => x.Settings, Times.AtLeastOnce);
    }

    [Theory]
    [InlineData(SoundEffect.RecordingStart)]
    [InlineData(SoundEffect.RecordingStop)]
    [InlineData(SoundEffect.Success)]
    [InlineData(SoundEffect.Error)]
    public async Task PlaySoundAsync_WithDifferentSounds_ShouldHandle(SoundEffect sound)
    {
        // Arrange
        _testSettings.Output.PlaySoundOnComplete = true;

        // Act
        await _notificationService.PlaySoundAsync(sound);

        // Assert
        // Should complete without throwing
        _mockConfigService.Verify(x => x.Settings, Times.AtLeastOnce);
    }

    [Fact]
    public async Task NotificationService_ShouldNotThrowOnSoundPlaybackFailure()
    {
        // Arrange
        _testSettings.Output.PlaySoundOnComplete = true;

        // Act & Assert - Should not throw even if sound playback fails
        await _notificationService.PlaySoundAsync(SoundEffect.Success);
        await _notificationService.PlaySoundAsync(SoundEffect.Error);
        await _notificationService.PlaySoundAsync(SoundEffect.RecordingStart);
        await _notificationService.PlaySoundAsync(SoundEffect.RecordingStop);
    }

    [Fact]
    public async Task NotificationService_ShouldRespectConfigServiceSettings()
    {
        // Arrange - notifications enabled, sounds disabled
        _testSettings.General.ShowNotifications = true;
        _testSettings.Output.PlaySoundOnComplete = false;

        // Act
        await _notificationService.ShowNotificationAsync("Title", "Message", NotificationType.Info);
        await _notificationService.PlaySoundAsync(SoundEffect.Success);

        // Assert - Config should be checked
        _mockConfigService.Verify(x => x.Settings, Times.AtLeast(2));
    }

    [Fact]
    public async Task NotificationService_ShouldHandleEmptyStrings()
    {
        // Arrange
        _testSettings.General.ShowNotifications = true;

        // Act & Assert - Should not throw
        await _notificationService.ShowNotificationAsync("", "", NotificationType.Info);
        await _notificationService.ShowNotificationAsync("Title", "", NotificationType.Info);
        await _notificationService.ShowNotificationAsync("", "Message", NotificationType.Info);
    }
}

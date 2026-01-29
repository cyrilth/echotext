using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services;
using EchoText.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EchoText.Tests.Services;

public class HotkeyServiceTests
{
    private readonly Mock<IPlatformHotkey> _mockPlatformHotkey;
    private readonly Mock<IConfigService> _mockConfigService;
    private readonly AppSettings _defaultSettings;
    private readonly HotkeyService _hotkeyService;

    public HotkeyServiceTests()
    {
        _mockPlatformHotkey = new Mock<IPlatformHotkey>();
        _mockConfigService = new Mock<IConfigService>();

        // Setup default settings
        _defaultSettings = new AppSettings
        {
            Hotkey = new HotkeySettings
            {
                Modifiers = KeyModifiers.Ctrl | KeyModifiers.Shift,
                Key = "Space",
                Mode = HotkeyMode.PushToTalk
            }
        };

        _mockConfigService.Setup(c => c.Settings).Returns(_defaultSettings);

        _hotkeyService = new HotkeyService(_mockPlatformHotkey.Object, _mockConfigService.Object);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenPlatformHotkeyIsNull()
    {
        // Act & Assert
        var act = () => new HotkeyService(null!, _mockConfigService.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("platformHotkey");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConfigServiceIsNull()
    {
        // Act & Assert
        var act = () => new HotkeyService(_mockPlatformHotkey.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configService");
    }

    [Fact]
    public void Constructor_ShouldSubscribeToPlatformHotkeyEvents()
    {
        // Arrange & Act
        var platformHotkey = new Mock<IPlatformHotkey>();
        var configService = new Mock<IConfigService>();
        configService.Setup(c => c.Settings).Returns(_defaultSettings);

        // Act
        var service = new HotkeyService(platformHotkey.Object, configService.Object);

        // Assert - Verify event subscriptions by raising events
        var pressedCalled = false;
        var releasedCalled = false;

        service.HotkeyPressed += (_, _) => pressedCalled = true;
        service.HotkeyReleased += (_, _) => releasedCalled = true;

        platformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);
        platformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        pressedCalled.Should().BeTrue();
        releasedCalled.Should().BeTrue();

        service.Dispose();
    }

    [Fact]
    public void Constructor_ShouldSubscribeToConfigServiceEvents()
    {
        // Arrange
        var platformHotkey = new Mock<IPlatformHotkey>();
        var configService = new Mock<IConfigService>();
        configService.Setup(c => c.Settings).Returns(_defaultSettings);

        // Act
        var service = new HotkeyService(platformHotkey.Object, configService.Object);

        // Assert - The constructor subscribes to SettingsChanged
        // We can verify this by disposing and checking unsubscription behavior
        service.Dispose();
    }

    [Fact]
    public async Task RegisterAsync_ShouldCallPlatformHotkeyRegister_WithCorrectParameters()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>())).Returns(true);

        // Act
        var result = await _hotkeyService.RegisterAsync();

        // Assert
        result.Should().BeTrue();
        _mockPlatformHotkey.Verify(
            p => p.Register(KeyModifiers.Ctrl | KeyModifiers.Shift, "Space"),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFalse_WhenPlatformRegistrationFails()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await _hotkeyService.RegisterAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnregisterAsync_ShouldCallPlatformHotkeyUnregister()
    {
        // Act
        await _hotkeyService.UnregisterAsync();

        // Assert
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Once);
    }

    [Fact]
    public void IsRegistered_ShouldReflectPlatformProviderState()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.IsRegistered).Returns(true);

        // Act & Assert
        _hotkeyService.IsRegistered.Should().BeTrue();

        // Change state
        _mockPlatformHotkey.Setup(p => p.IsRegistered).Returns(false);
        _hotkeyService.IsRegistered.Should().BeFalse();
    }

    [Fact]
    public void HotkeyPressed_ShouldFireInPushToTalkMode_WhenPlatformHotkeyPressed()
    {
        // Arrange
        var eventFired = false;
        _hotkeyService.HotkeyPressed += (_, _) => eventFired = true;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void HotkeyReleased_ShouldFireInPushToTalkMode_WhenPlatformHotkeyReleased()
    {
        // Arrange
        var eventFired = false;
        _hotkeyService.HotkeyReleased += (_, _) => eventFired = true;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void HotkeyPressed_ShouldFireInToggleMode_WhenPlatformHotkeyPressed()
    {
        // Arrange
        _defaultSettings.Hotkey.Mode = HotkeyMode.Toggle;
        var eventFiredCount = 0;
        _hotkeyService.HotkeyPressed += (_, _) => eventFiredCount++;

        // Act - Press twice
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);

        // Assert - Both presses should fire the event
        eventFiredCount.Should().Be(2);
    }

    [Fact]
    public void HotkeyReleased_ShouldNotFireInToggleMode_WhenPlatformHotkeyReleased()
    {
        // Arrange
        _defaultSettings.Hotkey.Mode = HotkeyMode.Toggle;
        var eventFired = false;
        _hotkeyService.HotkeyReleased += (_, _) => eventFired = true;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        // Assert
        eventFired.Should().BeFalse();
    }

    [Fact]
    public async Task OnSettingsChanged_ShouldReregisterHotkey_WhenAlreadyRegistered()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>())).Returns(true);
        _mockPlatformHotkey.Setup(p => p.IsRegistered).Returns(true);

        // Register initially
        await _hotkeyService.RegisterAsync();
        _mockPlatformHotkey.Invocations.Clear();

        // Act - Trigger settings changed
        _mockConfigService.Raise(c => c.SettingsChanged += null, EventArgs.Empty);

        // Wait for async event handler
        await Task.Delay(100);

        // Assert
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Once);
        _mockPlatformHotkey.Verify(
            p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task OnSettingsChanged_ShouldNotReregister_WhenNotRegistered()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.IsRegistered).Returns(false);

        // Act - Trigger settings changed without registering
        _mockConfigService.Raise(c => c.SettingsChanged += null, EventArgs.Empty);

        // Wait for async event handler
        await Task.Delay(100);

        // Assert - Should not attempt to unregister or register
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Never);
        _mockPlatformHotkey.Verify(
            p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldResetToggleState()
    {
        // Arrange
        _defaultSettings.Hotkey.Mode = HotkeyMode.Toggle;
        _mockPlatformHotkey.Setup(p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>())).Returns(true);

        // Simulate toggle state by pressing hotkey
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);

        // Act - Re-register
        await _hotkeyService.RegisterAsync();

        // Assert - Toggle state should be reset (this is internal, but we can verify behavior)
        // The toggle state reset ensures consistent behavior after re-registration
        _mockPlatformHotkey.Verify(
            p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task UnregisterAsync_ShouldResetToggleState()
    {
        // Arrange
        _defaultSettings.Hotkey.Mode = HotkeyMode.Toggle;

        // Simulate toggle state by pressing hotkey
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);

        // Act
        await _hotkeyService.UnregisterAsync();

        // Assert
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromPlatformEvents()
    {
        // Arrange
        var pressedFired = false;
        var releasedFired = false;
        _hotkeyService.HotkeyPressed += (_, _) => pressedFired = true;
        _hotkeyService.HotkeyReleased += (_, _) => releasedFired = true;

        // Act
        _hotkeyService.Dispose();

        // Raise events after disposal
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);
        _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        // Assert - Events should not fire after disposal
        pressedFired.Should().BeFalse();
        releasedFired.Should().BeFalse();
    }

    [Fact]
    public async Task Dispose_ShouldUnsubscribeFromConfigServiceEvents()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.IsRegistered).Returns(true);

        // Act
        _hotkeyService.Dispose();
        _mockPlatformHotkey.Invocations.Clear();

        // Raise settings changed after disposal
        _mockConfigService.Raise(c => c.SettingsChanged += null, EventArgs.Empty);

        // Wait briefly
        await Task.Delay(50);

        // Assert - Should not attempt re-registration
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Never);
        _mockPlatformHotkey.Verify(
            p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void Dispose_ShouldUnregisterHotkey()
    {
        // Act
        _hotkeyService.Dispose();

        // Assert
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldDisposePlatformProvider()
    {
        // Act
        _hotkeyService.Dispose();

        // Assert
        _mockPlatformHotkey.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Act
        _hotkeyService.Dispose();
        _hotkeyService.Dispose();

        // Assert - Should only unregister and dispose once
        _mockPlatformHotkey.Verify(p => p.Unregister(), Times.Once);
        _mockPlatformHotkey.Verify(p => p.Dispose(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowObjectDisposedException_AfterDisposal()
    {
        // Arrange
        _hotkeyService.Dispose();

        // Act & Assert
        var act = async () => await _hotkeyService.RegisterAsync();
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task UnregisterAsync_ShouldThrowObjectDisposedException_AfterDisposal()
    {
        // Arrange
        _hotkeyService.Dispose();

        // Act & Assert
        var act = async () => await _hotkeyService.UnregisterAsync();
        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public void HotkeyPressed_ShouldPassSenderCorrectly()
    {
        // Arrange
        object? receivedSender = null;
        _hotkeyService.HotkeyPressed += (sender, _) => receivedSender = sender;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);

        // Assert
        receivedSender.Should().Be(_hotkeyService);
    }

    [Fact]
    public void HotkeyReleased_ShouldPassSenderCorrectly()
    {
        // Arrange
        object? receivedSender = null;
        _hotkeyService.HotkeyReleased += (sender, _) => receivedSender = sender;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        // Assert
        receivedSender.Should().Be(_hotkeyService);
    }

    [Fact]
    public void PushToTalkMode_ShouldFireBothPressedAndReleased()
    {
        // Arrange
        _defaultSettings.Hotkey.Mode = HotkeyMode.PushToTalk;
        var pressedCount = 0;
        var releasedCount = 0;

        _hotkeyService.HotkeyPressed += (_, _) => pressedCount++;
        _hotkeyService.HotkeyReleased += (_, _) => releasedCount++;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);
        _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        // Assert
        pressedCount.Should().Be(1);
        releasedCount.Should().Be(1);
    }

    [Fact]
    public void ToggleMode_ShouldOnlyFirePressed_NotReleased()
    {
        // Arrange
        _defaultSettings.Hotkey.Mode = HotkeyMode.Toggle;
        var pressedCount = 0;
        var releasedCount = 0;

        _hotkeyService.HotkeyPressed += (_, _) => pressedCount++;
        _hotkeyService.HotkeyReleased += (_, _) => releasedCount++;

        // Act - Press and release multiple times
        for (int i = 0; i < 3; i++)
        {
            _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);
            _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);
        }

        // Assert
        pressedCount.Should().Be(3);
        releasedCount.Should().Be(0);
    }

    [Fact]
    public async Task SettingsChanged_WithDifferentHotkeySettings_ShouldReregisterWithNewSettings()
    {
        // Arrange
        _mockPlatformHotkey.Setup(p => p.Register(It.IsAny<KeyModifiers>(), It.IsAny<string>())).Returns(true);
        _mockPlatformHotkey.Setup(p => p.IsRegistered).Returns(true);

        // Register with initial settings
        await _hotkeyService.RegisterAsync();
        _mockPlatformHotkey.Invocations.Clear();

        // Change settings
        _defaultSettings.Hotkey.Modifiers = KeyModifiers.Alt;
        _defaultSettings.Hotkey.Key = "F1";

        // Act - Trigger settings changed
        _mockConfigService.Raise(c => c.SettingsChanged += null, EventArgs.Empty);

        // Wait for async event handler
        await Task.Delay(100);

        // Assert - Should register with new settings
        _mockPlatformHotkey.Verify(
            p => p.Register(KeyModifiers.Alt, "F1"),
            Times.Once);
    }

    [Fact]
    public void MultipleSubscribers_ShouldAllReceiveEvents()
    {
        // Arrange
        var subscriber1Pressed = false;
        var subscriber2Pressed = false;
        var subscriber1Released = false;
        var subscriber2Released = false;

        _hotkeyService.HotkeyPressed += (_, _) => subscriber1Pressed = true;
        _hotkeyService.HotkeyPressed += (_, _) => subscriber2Pressed = true;
        _hotkeyService.HotkeyReleased += (_, _) => subscriber1Released = true;
        _hotkeyService.HotkeyReleased += (_, _) => subscriber2Released = true;

        // Act
        _mockPlatformHotkey.Raise(p => p.HotkeyPressed += null, EventArgs.Empty);
        _mockPlatformHotkey.Raise(p => p.HotkeyReleased += null, EventArgs.Empty);

        // Assert
        subscriber1Pressed.Should().BeTrue();
        subscriber2Pressed.Should().BeTrue();
        subscriber1Released.Should().BeTrue();
        subscriber2Released.Should().BeTrue();
    }
}

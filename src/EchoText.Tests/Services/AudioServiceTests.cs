using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services;
using EchoText.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EchoText.Tests.Services;

public class AudioServiceTests
{
    private readonly Mock<IPlatformAudio> _mockPlatformAudio;
    private readonly Mock<IConfigService> _mockConfigService;
    private readonly AudioService _audioService;

    public AudioServiceTests()
    {
        _mockPlatformAudio = new Mock<IPlatformAudio>();
        _mockConfigService = new Mock<IConfigService>();

        // Setup default config
        _mockConfigService.Setup(x => x.Settings).Returns(new AppSettings
        {
            SelectedAudioDevice = null
        });

        _audioService = new AudioService(_mockPlatformAudio.Object, _mockConfigService.Object);
    }

    [Fact]
    public async Task GetInputDevicesAsync_ShouldReturnDevicesFromPlatformProvider()
    {
        // Arrange
        var expectedDevices = new List<AudioDevice>
        {
            new AudioDevice("device1", "Microphone 1", true),
            new AudioDevice("device2", "Microphone 2", false)
        };
        _mockPlatformAudio.Setup(x => x.GetInputDevices()).Returns(expectedDevices);

        // Act
        var result = await _audioService.GetInputDevicesAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedDevices);
        _mockPlatformAudio.Verify(x => x.GetInputDevices(), Times.Once);
    }

    [Fact]
    public async Task StartRecordingAsync_ShouldStartCaptureWithProvidedDeviceId()
    {
        // Arrange
        const string deviceId = "test-device";
        _mockPlatformAudio.Setup(x => x.IsRecording).Returns(false);

        // Act
        await _audioService.StartRecordingAsync(deviceId);

        // Assert
        _mockPlatformAudio.Verify(x => x.StartCapture(deviceId), Times.Once);
    }

    [Fact]
    public async Task StartRecordingAsync_WithNullDeviceId_ShouldUseConfiguredDevice()
    {
        // Arrange
        const string configuredDevice = "configured-device";
        _mockConfigService.Setup(x => x.Settings).Returns(new AppSettings
        {
            SelectedAudioDevice = configuredDevice
        });
        _mockPlatformAudio.Setup(x => x.IsRecording).Returns(false);

        // Act
        await _audioService.StartRecordingAsync();

        // Assert
        _mockPlatformAudio.Verify(x => x.StartCapture(configuredDevice), Times.Once);
    }

    [Fact]
    public async Task StartRecordingAsync_WhenAlreadyRecording_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockPlatformAudio.Setup(x => x.IsRecording).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _audioService.StartRecordingAsync());
    }

    [Fact]
    public async Task StopRecordingAsync_ShouldStopCaptureAndReturnAudioData()
    {
        // Arrange
        var expectedAudioData = new byte[] { 1, 2, 3, 4, 5 };
        _mockPlatformAudio.Setup(x => x.IsRecording).Returns(true);
        _mockPlatformAudio.Setup(x => x.StopCapture()).Returns(expectedAudioData);

        // Act
        var result = await _audioService.StopRecordingAsync();

        // Assert
        result.Should().BeEquivalentTo(expectedAudioData);
        _mockPlatformAudio.Verify(x => x.StopCapture(), Times.Once);
    }

    [Fact]
    public async Task StopRecordingAsync_WhenNotRecording_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockPlatformAudio.Setup(x => x.IsRecording).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _audioService.StopRecordingAsync());
    }

    [Fact]
    public void IsRecording_ShouldReturnPlatformAudioRecordingState()
    {
        // Arrange
        _mockPlatformAudio.Setup(x => x.IsRecording).Returns(true);

        // Act
        var result = _audioService.IsRecording;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void RecordingDuration_ShouldReturnPlatformAudioRecordingDuration()
    {
        // Arrange
        var expectedDuration = TimeSpan.FromSeconds(42);
        _mockPlatformAudio.Setup(x => x.RecordingDuration).Returns(expectedDuration);

        // Act
        var result = _audioService.RecordingDuration;

        // Assert
        result.Should().Be(expectedDuration);
    }

    [Fact]
    public void AudioLevelChanged_ShouldForwardEventFromPlatformProvider()
    {
        // Arrange
        float? capturedLevel = null;
        _audioService.AudioLevelChanged += (sender, level) => capturedLevel = level;

        // Act
        _mockPlatformAudio.Raise(x => x.AudioLevelChanged += null, _mockPlatformAudio.Object, 0.75f);

        // Assert
        capturedLevel.Should().Be(0.75f);
    }

    [Fact]
    public void Dispose_ShouldDisposeResources()
    {
        // Act
        _audioService.Dispose();

        // Assert
        _mockPlatformAudio.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Constructor_WithNullPlatformAudio_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AudioService(null!, _mockConfigService.Object));
    }

    [Fact]
    public void Constructor_WithNullConfigService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AudioService(_mockPlatformAudio.Object, null!));
    }
}

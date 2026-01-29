using EchoText.Models;
using EchoText.Services;
using EchoText.Services.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;
using System.IO;
using System.Threading.Tasks;

namespace EchoText.Tests.Services;

public class TranscriptionServiceTests
{
    private readonly Mock<IModelManager> _mockModelManager;
    private readonly TranscriptionService _service;

    public TranscriptionServiceTests()
    {
        _mockModelManager = new Mock<IModelManager>();
        _service = new TranscriptionService(_mockModelManager.Object);
    }

    [Fact]
    public void Constructor_WithNullModelManager_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TranscriptionService(null!));
    }

    [Fact]
    public void IsModelLoaded_WhenNoModelLoaded_ShouldReturnFalse()
    {
        // Assert
        _service.IsModelLoaded.Should().BeFalse();
    }

    [Fact]
    public void LoadedModelName_WhenNoModelLoaded_ShouldReturnNull()
    {
        // Assert
        _service.LoadedModelName.Should().BeNull();
    }

    [Fact]
    public async Task LoadModelAsync_WithEmptyPath_ShouldReturnFailure()
    {
        // Act
        var result = await _service.LoadModelAsync("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Model path cannot be empty");
    }

    [Fact]
    public async Task LoadModelAsync_WithNonExistentFile_ShouldReturnFailure()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent-model.bin");

        // Act
        var result = await _service.LoadModelAsync(nonExistentPath);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Model file not found");
    }

    [Fact]
    public async Task TranscribeAsync_WhenNoModelLoaded_ShouldReturnFailure()
    {
        // Arrange
        var audioData = new byte[] { 1, 2, 3, 4 };

        // Act
        var result = await _service.TranscribeAsync(audioData);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No model loaded");
    }

    [Fact]
    public async Task TranscribeAsync_WithNullAudioData_ShouldReturnFailure()
    {
        // Act
        var result = await _service.TranscribeAsync(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Audio data is empty");
    }

    [Fact]
    public async Task TranscribeAsync_WithEmptyAudioData_ShouldReturnFailure()
    {
        // Arrange
        var audioData = new byte[0];

        // Act
        var result = await _service.TranscribeAsync(audioData);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Audio data is empty");
    }

    [Fact]
    public void UnloadModel_WhenNoModelLoaded_ShouldNotThrow()
    {
        // Act
        var act = () => _service.UnloadModel();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldUnloadModel()
    {
        // Act
        _service.Dispose();

        // Assert
        _service.IsModelLoaded.Should().BeFalse();
        _service.LoadedModelName.Should().BeNull();
    }

    [Fact]
    public async Task LoadModelAsync_AfterDispose_ShouldReturnFailure()
    {
        // Arrange
        _service.Dispose();
        var modelPath = Path.Combine(Path.GetTempPath(), "test-model.bin");

        // Act
        var result = await _service.LoadModelAsync(modelPath);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task TranscribeAsync_AfterDispose_ShouldReturnFailure()
    {
        // Arrange
        _service.Dispose();
        var audioData = new byte[] { 1, 2, 3, 4 };

        // Act
        var result = await _service.TranscribeAsync(audioData);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Act
        var act = () =>
        {
            _service.Dispose();
            _service.Dispose();
            _service.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }
}

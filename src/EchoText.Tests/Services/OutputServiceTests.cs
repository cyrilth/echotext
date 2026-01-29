using System;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services;
using EchoText.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EchoText.Tests.Services;

public class OutputServiceTests
{
    private readonly Mock<IConfigService> _mockConfigService;
    private readonly Mock<IClipboardService> _mockClipboardService;
    private readonly Mock<IPlatformOutput> _mockPlatformOutput;
    private readonly IOutputService _outputService;
    private readonly AppSettings _appSettings;

    public OutputServiceTests()
    {
        _mockConfigService = new Mock<IConfigService>();
        _mockClipboardService = new Mock<IClipboardService>();
        _mockPlatformOutput = new Mock<IPlatformOutput>();

        // Setup default settings
        _appSettings = new AppSettings
        {
            Output = new OutputSettings
            {
                CopyToClipboard = true,
                AutoType = false,
                KeystrokeDelayMs = 10
            }
        };

        _mockConfigService
            .Setup(x => x.Settings)
            .Returns(_appSettings);

        _outputService = new OutputService(
            _mockConfigService.Object,
            _mockClipboardService.Object,
            _mockPlatformOutput.Object);
    }

    [Fact]
    public void Constructor_WithNullConfigService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => new OutputService(null!, _mockClipboardService.Object, _mockPlatformOutput.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullClipboardService_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => new OutputService(_mockConfigService.Object, null!, _mockPlatformOutput.Object);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("clipboardService");
    }

    [Fact]
    public void Constructor_WithNullPlatformOutput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => new OutputService(_mockConfigService.Object, _mockClipboardService.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("platformOutput");
    }

    [Fact]
    public async Task OutputTextAsync_WithNullText_ThrowsArgumentNullException()
    {
        // Act & Assert
        Func<Task> act = async () => await _outputService.OutputTextAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("text");
    }

    [Fact]
    public async Task OutputTextAsync_WhenCopyToClipboardEnabled_CopiesTextToClipboard()
    {
        // Arrange
        const string testText = "Hello, World!";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = false;

        _mockClipboardService
            .Setup(x => x.SetTextAsync(testText))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(testText);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(testText), Times.Once);
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task OutputTextAsync_WhenAutoTypeEnabled_TypesText()
    {
        // Arrange
        const string testText = "Test typing";
        _appSettings.Output.CopyToClipboard = false;
        _appSettings.Output.AutoType = true;
        _appSettings.Output.KeystrokeDelayMs = 15;

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(testText, 15))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(testText);

        // Assert
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(testText, 15), Times.Once);
        _mockClipboardService.Verify(x => x.SetTextAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OutputTextAsync_WhenBothEnabled_CopiesAndTypes()
    {
        // Arrange
        const string testText = "Both operations";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = true;
        _appSettings.Output.KeystrokeDelayMs = 20;

        _mockClipboardService
            .Setup(x => x.SetTextAsync(testText))
            .Returns(Task.CompletedTask);

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(testText, 20))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(testText);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(testText), Times.Once);
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(testText, 20), Times.Once);
    }

    [Fact]
    public async Task OutputTextAsync_WhenBothDisabled_DoesNothing()
    {
        // Arrange
        const string testText = "No output";
        _appSettings.Output.CopyToClipboard = false;
        _appSettings.Output.AutoType = false;

        // Act
        await _outputService.OutputTextAsync(testText);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(It.IsAny<string>()), Times.Never);
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task OutputTextAsync_WithEmptyString_ProcessesCorrectly()
    {
        // Arrange
        const string emptyText = "";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = true;

        _mockClipboardService
            .Setup(x => x.SetTextAsync(emptyText))
            .Returns(Task.CompletedTask);

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(emptyText, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(emptyText);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(emptyText), Times.Once);
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(emptyText, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task OutputTextAsync_WithMultilineText_ProcessesCorrectly()
    {
        // Arrange
        const string multilineText = "Line 1\nLine 2\nLine 3";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = true;

        _mockClipboardService
            .Setup(x => x.SetTextAsync(multilineText))
            .Returns(Task.CompletedTask);

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(multilineText, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(multilineText);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(multilineText), Times.Once);
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(multilineText, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task OutputTextAsync_WithSpecialCharacters_ProcessesCorrectly()
    {
        // Arrange
        const string specialText = "Special: ñ, é, ü, 中文, 🔥, @#$%";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = true;

        _mockClipboardService
            .Setup(x => x.SetTextAsync(specialText))
            .Returns(Task.CompletedTask);

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(specialText, It.IsAny<int>()))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(specialText);

        // Assert
        _mockClipboardService.Verify(x => x.SetTextAsync(specialText), Times.Once);
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(specialText, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task OutputTextAsync_RespectsKeystrokeDelayFromSettings()
    {
        // Arrange
        const string testText = "Delay test";
        const int customDelay = 50;
        _appSettings.Output.CopyToClipboard = false;
        _appSettings.Output.AutoType = true;
        _appSettings.Output.KeystrokeDelayMs = customDelay;

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(testText, customDelay))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(testText);

        // Assert
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(testText, customDelay), Times.Once);
    }

    [Fact]
    public async Task OutputTextAsync_WhenClipboardServiceThrows_PropagatesException()
    {
        // Arrange
        const string testText = "Test";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = false;

        var expectedException = new InvalidOperationException("Clipboard error");
        _mockClipboardService
            .Setup(x => x.SetTextAsync(testText))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Func<Task> act = async () => await _outputService.OutputTextAsync(testText);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Clipboard error");
    }

    [Fact]
    public async Task OutputTextAsync_WhenPlatformOutputThrows_PropagatesException()
    {
        // Arrange
        const string testText = "Test";
        _appSettings.Output.CopyToClipboard = false;
        _appSettings.Output.AutoType = true;

        var expectedException = new InvalidOperationException("Auto-type error");
        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(testText, It.IsAny<int>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Func<Task> act = async () => await _outputService.OutputTextAsync(testText);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Auto-type error");
    }

    [Fact]
    public async Task OutputTextAsync_WhenClipboardSucceedsButAutoTypeFails_PropagatesException()
    {
        // Arrange
        const string testText = "Test";
        _appSettings.Output.CopyToClipboard = true;
        _appSettings.Output.AutoType = true;

        _mockClipboardService
            .Setup(x => x.SetTextAsync(testText))
            .Returns(Task.CompletedTask);

        var expectedException = new InvalidOperationException("Auto-type error");
        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(testText, It.IsAny<int>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Func<Task> act = async () => await _outputService.OutputTextAsync(testText);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Auto-type error");

        // Clipboard should have been called before the error
        _mockClipboardService.Verify(x => x.SetTextAsync(testText), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task OutputTextAsync_WithVariousDelays_UsesCorrectDelay(int delayMs)
    {
        // Arrange
        const string testText = "Delay variation test";
        _appSettings.Output.CopyToClipboard = false;
        _appSettings.Output.AutoType = true;
        _appSettings.Output.KeystrokeDelayMs = delayMs;

        _mockPlatformOutput
            .Setup(x => x.TypeTextAsync(testText, delayMs))
            .Returns(Task.CompletedTask);

        // Act
        await _outputService.OutputTextAsync(testText);

        // Assert
        _mockPlatformOutput.Verify(x => x.TypeTextAsync(testText, delayMs), Times.Once);
    }
}

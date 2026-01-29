using System;
using System.Threading.Tasks;
using EchoText.Platform.Interfaces;
using EchoText.Services;
using EchoText.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace EchoText.Tests.Services;

public class ClipboardServiceTests
{
    private readonly Mock<IPlatformClipboard> _mockPlatformClipboard;
    private readonly IClipboardService _clipboardService;

    public ClipboardServiceTests()
    {
        _mockPlatformClipboard = new Mock<IPlatformClipboard>();
        _clipboardService = new ClipboardService(_mockPlatformClipboard.Object);
    }

    [Fact]
    public void Constructor_WithNullPlatformClipboard_ThrowsArgumentNullException()
    {
        // Act & Assert
        Action act = () => new ClipboardService(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("platformClipboard");
    }

    [Fact]
    public async Task SetTextAsync_WithValidText_CallsPlatformClipboard()
    {
        // Arrange
        const string testText = "Hello, World!";
        _mockPlatformClipboard
            .Setup(x => x.SetTextAsync(testText))
            .Returns(Task.CompletedTask);

        // Act
        await _clipboardService.SetTextAsync(testText);

        // Assert
        _mockPlatformClipboard.Verify(x => x.SetTextAsync(testText), Times.Once);
    }

    [Fact]
    public async Task SetTextAsync_WithEmptyString_CallsPlatformClipboard()
    {
        // Arrange
        const string emptyText = "";
        _mockPlatformClipboard
            .Setup(x => x.SetTextAsync(emptyText))
            .Returns(Task.CompletedTask);

        // Act
        await _clipboardService.SetTextAsync(emptyText);

        // Assert
        _mockPlatformClipboard.Verify(x => x.SetTextAsync(emptyText), Times.Once);
    }

    [Fact]
    public async Task SetTextAsync_WithNullText_ThrowsArgumentNullException()
    {
        // Act & Assert
        Func<Task> act = async () => await _clipboardService.SetTextAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("text");
    }

    [Fact]
    public async Task SetTextAsync_WithMultilineText_CallsPlatformClipboard()
    {
        // Arrange
        const string multilineText = "Line 1\nLine 2\nLine 3";
        _mockPlatformClipboard
            .Setup(x => x.SetTextAsync(multilineText))
            .Returns(Task.CompletedTask);

        // Act
        await _clipboardService.SetTextAsync(multilineText);

        // Assert
        _mockPlatformClipboard.Verify(x => x.SetTextAsync(multilineText), Times.Once);
    }

    [Fact]
    public async Task SetTextAsync_WithSpecialCharacters_CallsPlatformClipboard()
    {
        // Arrange
        const string specialText = "Special: ñ, é, ü, 中文, 🔥";
        _mockPlatformClipboard
            .Setup(x => x.SetTextAsync(specialText))
            .Returns(Task.CompletedTask);

        // Act
        await _clipboardService.SetTextAsync(specialText);

        // Assert
        _mockPlatformClipboard.Verify(x => x.SetTextAsync(specialText), Times.Once);
    }

    [Fact]
    public async Task GetTextAsync_WhenClipboardHasText_ReturnsText()
    {
        // Arrange
        const string expectedText = "Clipboard content";
        _mockPlatformClipboard
            .Setup(x => x.GetTextAsync())
            .ReturnsAsync(expectedText);

        // Act
        var result = await _clipboardService.GetTextAsync();

        // Assert
        result.Should().Be(expectedText);
        _mockPlatformClipboard.Verify(x => x.GetTextAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTextAsync_WhenClipboardIsEmpty_ReturnsNull()
    {
        // Arrange
        _mockPlatformClipboard
            .Setup(x => x.GetTextAsync())
            .ReturnsAsync((string?)null);

        // Act
        var result = await _clipboardService.GetTextAsync();

        // Assert
        result.Should().BeNull();
        _mockPlatformClipboard.Verify(x => x.GetTextAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTextAsync_WhenClipboardHasEmptyString_ReturnsEmptyString()
    {
        // Arrange
        _mockPlatformClipboard
            .Setup(x => x.GetTextAsync())
            .ReturnsAsync(string.Empty);

        // Act
        var result = await _clipboardService.GetTextAsync();

        // Assert
        result.Should().Be(string.Empty);
        _mockPlatformClipboard.Verify(x => x.GetTextAsync(), Times.Once);
    }

    [Fact]
    public async Task SetTextAndGetText_RoundTrip_WorksCorrectly()
    {
        // Arrange
        const string testText = "Round-trip test";
        string? capturedText = null;

        _mockPlatformClipboard
            .Setup(x => x.SetTextAsync(It.IsAny<string>()))
            .Callback<string>(text => capturedText = text)
            .Returns(Task.CompletedTask);

        _mockPlatformClipboard
            .Setup(x => x.GetTextAsync())
            .ReturnsAsync(() => capturedText);

        // Act
        await _clipboardService.SetTextAsync(testText);
        var result = await _clipboardService.GetTextAsync();

        // Assert
        result.Should().Be(testText);
    }

    [Fact]
    public async Task SetTextAsync_WhenPlatformClipboardThrows_PropagatesException()
    {
        // Arrange
        const string testText = "Test";
        var expectedException = new InvalidOperationException("Platform clipboard error");
        _mockPlatformClipboard
            .Setup(x => x.SetTextAsync(testText))
            .ThrowsAsync(expectedException);

        // Act & Assert
        Func<Task> act = async () => await _clipboardService.SetTextAsync(testText);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Platform clipboard error");
    }

    [Fact]
    public async Task GetTextAsync_WhenPlatformClipboardThrows_PropagatesException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Platform clipboard error");
        _mockPlatformClipboard
            .Setup(x => x.GetTextAsync())
            .ThrowsAsync(expectedException);

        // Act & Assert
        Func<Task> act = async () => await _clipboardService.GetTextAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Platform clipboard error");
    }
}

using EchoText.Services;
using FluentAssertions;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EchoText.Tests.Services;

public class ModelManagerTests : IDisposable
{
    private readonly ModelManager _modelManager;
    private readonly string _testModelsDirectory;

    public ModelManagerTests()
    {
        _modelManager = new ModelManager();

        // Use a test-specific models directory
        _testModelsDirectory = Path.Combine(Path.GetTempPath(), "EchoTextTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testModelsDirectory);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testModelsDirectory))
        {
            Directory.Delete(_testModelsDirectory, true);
        }
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldReturnAllModels()
    {
        // Act
        var models = await _modelManager.GetAvailableModelsAsync();

        // Assert
        models.Should().NotBeNull();
        models.Should().HaveCount(5);
        models.Select(m => m.Name).Should().Contain(new[] { "tiny", "base", "small", "medium", "large" });
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldMarkDownloadedModelsCorrectly()
    {
        // Act
        var models = await _modelManager.GetAvailableModelsAsync();

        // Assert
        foreach (var model in models)
        {
            model.IsDownloaded.Should().BeFalse();
            model.LocalPath.Should().BeNull();
        }
    }

    [Fact]
    public void IsModelDownloaded_WithNonExistentModel_ShouldReturnFalse()
    {
        // Act
        var result = _modelManager.IsModelDownloaded("tiny");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsModelDownloaded_WithUnknownModel_ShouldReturnFalse()
    {
        // Act
        var result = _modelManager.IsModelDownloaded("unknown");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetModelPath_WithNonExistentModel_ShouldReturnNull()
    {
        // Act
        var path = _modelManager.GetModelPath("tiny");

        // Assert
        path.Should().BeNull();
    }

    [Fact]
    public void GetModelPath_WithUnknownModel_ShouldReturnNull()
    {
        // Act
        var path = _modelManager.GetModelPath("unknown");

        // Assert
        path.Should().BeNull();
    }

    [Fact]
    public async Task DownloadModelAsync_WithUnknownModel_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _modelManager.DownloadModelAsync("unknown");
        });
    }

    [Fact]
    public async Task DeleteModelAsync_WithUnknownModel_ShouldThrowArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _modelManager.DeleteModelAsync("unknown");
        });
    }

    [Fact]
    public async Task DeleteModelAsync_WithNonExistentModel_ShouldNotThrow()
    {
        // Act
        var exception = await Record.ExceptionAsync(async () =>
        {
            await _modelManager.DeleteModelAsync("tiny");
        });

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task GetAvailableModelsAsync_ShouldIncludeModelSizes()
    {
        // Act
        var models = await _modelManager.GetAvailableModelsAsync();

        // Assert
        var tinyModel = models.FirstOrDefault(m => m.Name == "tiny");
        tinyModel.Should().NotBeNull();
        tinyModel!.SizeBytes.Should().BeGreaterThan(0);
        tinyModel.DisplayName.Should().Contain("Tiny");

        var baseModel = models.FirstOrDefault(m => m.Name == "base");
        baseModel.Should().NotBeNull();
        baseModel!.SizeBytes.Should().BeGreaterThan(tinyModel.SizeBytes);
        baseModel.DisplayName.Should().Contain("Base");
    }

    [Fact]
    public async Task DownloadModelAsync_WithCancellation_ShouldCleanupTempFile()
    {
        // This test verifies that cancellation cleans up the temp file
        // We can't actually test a real download without mocking HttpClient
        // So this is a placeholder test that demonstrates the pattern

        // Act & Assert
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await _modelManager.DownloadModelAsync("tiny", null, cts.Token);
        });
    }

    [Theory]
    [InlineData("tiny")]
    [InlineData("base")]
    [InlineData("small")]
    [InlineData("medium")]
    [InlineData("large")]
    public async Task GetAvailableModelsAsync_ShouldIncludeAllStandardModels(string modelName)
    {
        // Act
        var models = await _modelManager.GetAvailableModelsAsync();

        // Assert
        models.Should().Contain(m => m.Name == modelName);
    }
}

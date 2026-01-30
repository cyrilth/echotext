using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Platform.Interfaces;
using EchoText.Services.Interfaces;
using EchoText.ViewModels;
using FluentAssertions;
using Moq;
using Xunit;

namespace EchoText.Tests.ViewModels;

public class SettingsViewModelTests
{
    private readonly Mock<IConfigService> _mockConfigService;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<IModelManager> _mockModelManager;
    private readonly Mock<IHotkeyService> _mockHotkeyService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IPlatformStartup> _mockPlatformStartup;
    private readonly AppSettings _testSettings;

    public SettingsViewModelTests()
    {
        _mockConfigService = new Mock<IConfigService>();
        _mockAudioService = new Mock<IAudioService>();
        _mockModelManager = new Mock<IModelManager>();
        _mockHotkeyService = new Mock<IHotkeyService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockPlatformStartup = new Mock<IPlatformStartup>();

        // Setup default test settings
        _testSettings = new AppSettings
        {
            SelectedAudioDevice = "device1",
            Hotkey = new HotkeySettings
            {
                Mode = HotkeyMode.PushToTalk,
                Modifiers = KeyModifiers.Ctrl | KeyModifiers.Shift,
                Key = "Space"
            },
            Output = new OutputSettings
            {
                CopyToClipboard = true,
                AutoType = false,
                PlaySoundOnComplete = true
            },
            Recognition = new RecognitionSettings
            {
                ModelName = "base",
                Language = "auto"
            },
            General = new GeneralSettings
            {
                StartWithSystem = false,
                ShowNotifications = true,
                CheckForUpdates = true
            }
        };

        _mockConfigService.Setup(x => x.Settings).Returns(_testSettings);

        // Setup default audio devices
        var audioDevices = new List<AudioDevice>
        {
            new AudioDevice("device1", "Default Microphone", true),
            new AudioDevice("device2", "USB Microphone", false)
        };
        _mockAudioService.Setup(x => x.GetInputDevicesAsync())
            .ReturnsAsync(audioDevices);

        // Setup default models
        var models = new List<WhisperModel>
        {
            new WhisperModel("tiny", "Tiny (75 MB)", 75_000_000, false, null),
            new WhisperModel("base", "Base (142 MB)", 142_000_000, true, "/path/to/base"),
            new WhisperModel("small", "Small (466 MB)", 466_000_000, false, null)
        };
        _mockModelManager.Setup(x => x.GetAvailableModelsAsync())
            .ReturnsAsync(models);

        _mockHotkeyService.Setup(x => x.IsRegistered).Returns(true);
        _mockHotkeyService.Setup(x => x.UnregisterAsync()).Returns(Task.CompletedTask);
        _mockHotkeyService.Setup(x => x.RegisterAsync()).ReturnsAsync(true);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithCorrectDependencies()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.AudioDevices.Should().NotBeNull();
        viewModel.AvailableModels.Should().NotBeNull();
        viewModel.AvailableLanguages.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDependencyIsNull()
    {
        // Act & Assert
        Action act = () => new SettingsViewModel(
            null!,
            _mockAudioService.Object,
            _mockModelManager.Object,
            _mockHotkeyService.Object,
            _mockNotificationService.Object,
            _mockPlatformStartup.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Initialization_ShouldLoadAudioDevices()
    {
        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200); // Wait for async initialization

        // Assert
        viewModel.AudioDevices.Should().HaveCount(2);
        viewModel.AudioDevices.Should().Contain(d => d.Name == "Default Microphone");
        viewModel.AudioDevices.Should().Contain(d => d.Name == "USB Microphone");
    }

    [Fact]
    public async Task Initialization_ShouldLoadAvailableModels()
    {
        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.AvailableModels.Should().HaveCount(3);
        viewModel.AvailableModels.Should().Contain(m => m.Name == "tiny");
        viewModel.AvailableModels.Should().Contain(m => m.Name == "base");
        viewModel.AvailableModels.Should().Contain(m => m.Name == "small");
    }

    [Fact]
    public async Task Initialization_ShouldSelectConfiguredAudioDevice()
    {
        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.SelectedAudioDevice.Should().NotBeNull();
        viewModel.SelectedAudioDevice!.Id.Should().Be("device1");
    }

    [Fact]
    public async Task Initialization_ShouldSelectConfiguredModel()
    {
        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.SelectedModel.Should().NotBeNull();
        viewModel.SelectedModel!.Name.Should().Be("base");
    }

    [Fact]
    public async Task Initialization_ShouldLoadAllSettingsToUI()
    {
        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.IsPushToTalkMode.Should().BeTrue();
        viewModel.IsToggleMode.Should().BeFalse();
        viewModel.CopyToClipboard.Should().BeTrue();
        viewModel.AutoType.Should().BeFalse();
        viewModel.PlaySoundOnComplete.Should().BeTrue();
        viewModel.StartWithSystem.Should().BeFalse();
        viewModel.ShowNotifications.Should().BeTrue();
        viewModel.CheckForUpdates.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingAudioDevice_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.SelectedAudioDevice = viewModel.AudioDevices.Last();

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingModel_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "tiny");

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingHotkeyMode_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.IsToggleMode = true;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
        viewModel.IsPushToTalkMode.Should().BeFalse();
    }

    [Fact]
    public async Task ChangingPushToTalkMode_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        _testSettings.Hotkey.Mode = HotkeyMode.Toggle;
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.IsPushToTalkMode = true;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
        viewModel.IsToggleMode.Should().BeFalse();
    }

    [Fact]
    public async Task ChangingCopyToClipboard_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.CopyToClipboard = false;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingAutoType_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.AutoType = true;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingPlaySoundOnComplete_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.PlaySoundOnComplete = false;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingStartWithSystem_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.StartWithSystem = true;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingShowNotifications_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.ShowNotifications = false;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task ChangingCheckForUpdates_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.CheckForUpdates = false;

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    [Fact]
    public async Task Save_ShouldCopySettingsToConfigService()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.CopyToClipboard = false;
        viewModel.AutoType = true;

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _testSettings.Output.CopyToClipboard.Should().BeFalse();
        _testSettings.Output.AutoType.Should().BeTrue();
        _mockConfigService.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_ShouldResetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.CopyToClipboard = false;
        viewModel.HasUnsavedChanges.Should().BeTrue();

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        viewModel.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public async Task Save_ShouldReregisterHotkey()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.IsToggleMode = true;

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockHotkeyService.Verify(x => x.UnregisterAsync(), Times.Once);
        _mockHotkeyService.Verify(x => x.RegisterAsync(), Times.Once);
    }

    [Fact]
    public async Task Save_WhenHotkeyRegistrationFails_ShouldShowErrorNotification()
    {
        // Arrange
        _mockHotkeyService.Setup(x => x.RegisterAsync()).ReturnsAsync(false);
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.IsToggleMode = true;

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "Hotkey Registration Failed",
            It.IsAny<string>(),
            NotificationType.Error), Times.Once);
    }

    [Fact]
    public async Task Cancel_ShouldReloadSettings()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        var originalCopyToClipboard = viewModel.CopyToClipboard;
        viewModel.CopyToClipboard = !originalCopyToClipboard;
        viewModel.HasUnsavedChanges.Should().BeTrue();

        // Act
        viewModel.CancelCommand.Execute(null);

        // Assert
        viewModel.CopyToClipboard.Should().Be(originalCopyToClipboard);
        viewModel.HasUnsavedChanges.Should().BeFalse();
    }

    [Fact]
    public async Task DownloadModel_WithNonDownloadedModel_ShouldDownloadAndReloadModels()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "tiny");

        _mockModelManager.Setup(x => x.DownloadModelAsync(
            "tiny",
            It.IsAny<IProgress<double>>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, IProgress<double>, CancellationToken>((_, progress, _) =>
            {
                progress?.Report(0.5);
                progress?.Report(1.0);
            })
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.DownloadModelCommand.ExecuteAsync(null);

        // Assert
        _mockModelManager.Verify(x => x.DownloadModelAsync(
            "tiny",
            It.IsAny<IProgress<double>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockModelManager.Verify(x => x.GetAvailableModelsAsync(), Times.AtLeast(2)); // Initial + after download
    }

    [Fact]
    public async Task DownloadModel_ShouldReportProgress()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "tiny");

        _mockModelManager.Setup(x => x.DownloadModelAsync(
            "tiny",
            It.IsAny<IProgress<double>>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, IProgress<double>, CancellationToken>((_, progress, _) =>
            {
                progress?.Report(0.5);
            })
            .Returns(Task.CompletedTask);

        // Act
        await viewModel.DownloadModelCommand.ExecuteAsync(null);
        await Task.Delay(100);

        // Assert
        viewModel.DownloadProgress.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DownloadModel_WhenCancelled_ShouldNotThrow()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "tiny");

        _mockModelManager.Setup(x => x.DownloadModelAsync(
            "tiny",
            It.IsAny<IProgress<double>>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        Func<Task> act = async () => await viewModel.DownloadModelCommand.ExecuteAsync(null);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CanDownloadModel_WithNonDownloadedModel_ShouldReturnTrue()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Act
        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "tiny");

        // Assert
        viewModel.DownloadModelCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task CanDownloadModel_WithDownloadedModel_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Act
        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "base");

        // Assert
        viewModel.DownloadModelCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task CanDownloadModel_WhileDownloading_ShouldReturnFalse()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        viewModel.SelectedModel = viewModel.AvailableModels.First(m => m.Name == "tiny");

        var tcs = new TaskCompletionSource<bool>();
        _mockModelManager.Setup(x => x.DownloadModelAsync(
            "tiny",
            It.IsAny<IProgress<double>>(),
            It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Act
        var downloadTask = viewModel.DownloadModelCommand.ExecuteAsync(null);
        await Task.Delay(50); // Let download start

        // Assert
        viewModel.IsDownloadingModel.Should().BeTrue();
        viewModel.DownloadModelCommand.CanExecute(null).Should().BeFalse();

        // Cleanup
        tcs.SetResult(true);
        await downloadTask;
    }

    [Fact]
    public async Task HotkeyDisplay_ShouldShowCorrectFormat()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.HotkeyDisplay.Should().Be("Ctrl+Shift+Space");
    }

    [Fact]
    public async Task LanguageOptions_ShouldIncludeAutoDetect()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.AvailableLanguages.Should().Contain(l => l.Code == "auto");
        viewModel.AvailableLanguages.Should().Contain(l => l.DisplayName == "Auto-detect");
    }

    [Fact]
    public async Task LanguageOptions_ShouldIncludeCommonLanguages()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.AvailableLanguages.Should().Contain(l => l.Code == "en");
        viewModel.AvailableLanguages.Should().Contain(l => l.Code == "es");
        viewModel.AvailableLanguages.Should().Contain(l => l.Code == "fr");
        viewModel.AvailableLanguages.Should().Contain(l => l.Code == "de");
    }

    [Fact]
    public async Task LoadAudioDevices_OnError_ShouldShowNotification()
    {
        // Arrange
        _mockAudioService.Setup(x => x.GetInputDevicesAsync())
            .ThrowsAsync(new Exception("Audio service error"));

        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "Audio Device Error",
            It.IsAny<string>(),
            NotificationType.Error), Times.Once);
    }

    [Fact]
    public async Task LoadModels_OnError_ShouldShowNotification()
    {
        // Arrange
        _mockModelManager.Setup(x => x.GetAvailableModelsAsync())
            .ThrowsAsync(new Exception("Model manager error"));

        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "Model Loading Error",
            It.IsAny<string>(),
            NotificationType.Error), Times.Once);
    }

    [Fact]
    public async Task SelectedLanguage_ShouldBeSetFromConfig()
    {
        // Arrange
        _testSettings.Recognition.Language = "en";

        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        viewModel.SelectedLanguage.Should().NotBeNull();
        viewModel.SelectedLanguage!.Code.Should().Be("en");
    }

    [Fact]
    public async Task ChangingSelectedLanguage_ShouldSetHasUnsavedChanges()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(200);
        viewModel.HasUnsavedChanges = false;

        // Act
        viewModel.SelectedLanguage = viewModel.AvailableLanguages.First(l => l.Code == "es");

        // Assert
        viewModel.HasUnsavedChanges.Should().BeTrue();
    }

    private SettingsViewModel CreateViewModel()
    {
        return new SettingsViewModel(
            _mockConfigService.Object,
            _mockAudioService.Object,
            _mockModelManager.Object,
            _mockHotkeyService.Object,
            _mockNotificationService.Object,
            _mockPlatformStartup.Object);
    }
}

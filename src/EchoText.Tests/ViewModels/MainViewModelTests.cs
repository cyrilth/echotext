using System;
using System.Threading.Tasks;
using EchoText.Models;
using EchoText.Services.Interfaces;
using EchoText.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EchoText.Tests.ViewModels;

public class MainViewModelTests
{
    private readonly Mock<IAppStateManager> _mockAppStateManager;
    private readonly Mock<IWindowService> _mockWindowService;
    private readonly Mock<IHotkeyService> _mockHotkeyService;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<ITranscriptionService> _mockTranscriptionService;
    private readonly Mock<IOutputService> _mockOutputService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IConfigService> _mockConfigService;
    private readonly Mock<IModelManager> _mockModelManager;
    private readonly Mock<IUpdateService> _mockUpdateService;
    private readonly Mock<ILogger<MainViewModel>> _mockLogger;
    private readonly AppSettings _testSettings;

    public MainViewModelTests()
    {
        _mockAppStateManager = new Mock<IAppStateManager>();
        _mockWindowService = new Mock<IWindowService>();
        _mockHotkeyService = new Mock<IHotkeyService>();
        _mockAudioService = new Mock<IAudioService>();
        _mockTranscriptionService = new Mock<ITranscriptionService>();
        _mockOutputService = new Mock<IOutputService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockConfigService = new Mock<IConfigService>();
        _mockModelManager = new Mock<IModelManager>();
        _mockUpdateService = new Mock<IUpdateService>();
        _mockLogger = new Mock<ILogger<MainViewModel>>();

        // Setup default test settings
        _testSettings = new AppSettings
        {
            SelectedAudioDevice = "default",
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
        _mockConfigService.Setup(x => x.LoadAsync()).Returns(Task.CompletedTask);
        _mockConfigService.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Loading);
        _mockHotkeyService.Setup(x => x.RegisterAsync()).ReturnsAsync(true);
        _mockModelManager.Setup(x => x.GetAvailableModelsAsync())
            .ReturnsAsync(new[]
            {
                new WhisperModel("base", "Base", 74_000_000, true, "/path/to/base")
            });
        _mockModelManager.Setup(x => x.GetModelPath("base")).Returns("/path/to/base");
        _mockTranscriptionService.Setup(x => x.LoadModelAsync(It.IsAny<string>()))
            .ReturnsAsync(Result<bool>.Success(true));
    }

    [Fact]
    public void Constructor_ShouldInitializeWithCorrectDependencies()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        viewModel.Should().NotBeNull();
        viewModel.StatusText.Should().NotBeNull();
        viewModel.TrayIconPath.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenDependencyIsNull()
    {
        // Act & Assert
        Action act = () => new MainViewModel(
            null!,
            _mockWindowService.Object,
            _mockHotkeyService.Object,
            _mockAudioService.Object,
            _mockTranscriptionService.Object,
            _mockOutputService.Object,
            _mockNotificationService.Object,
            _mockConfigService.Object,
            _mockModelManager.Object,
            _mockUpdateService.Object,
            _mockLogger.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task StateChange_ToIdle_ShouldUpdateStatusTextAndTrayIcon()
    {
        // Arrange
        EventHandler<AppState>? stateChangedHandler = null;
        _mockAppStateManager.SetupAdd(x => x.StateChanged += It.IsAny<EventHandler<AppState>>())
            .Callback<EventHandler<AppState>>(handler => stateChangedHandler = handler);

        var viewModel = CreateViewModel();
        await Task.Delay(100); // Wait for initialization
        stateChangedHandler.Should().NotBeNull();

        // Act
        stateChangedHandler?.Invoke(_mockAppStateManager.Object, AppState.Idle);
        await Task.Delay(50); // Allow property change notifications

        // Assert
        viewModel.StatusText.Should().Be("Ready");
        viewModel.TrayIconPath.Should().Contain("tray-idle.ico");
    }

    [Fact]
    public async Task StateChange_ToRecording_ShouldUpdateStatusTextAndTrayIcon()
    {
        // Arrange
        EventHandler<AppState>? stateChangedHandler = null;
        _mockAppStateManager.SetupAdd(x => x.StateChanged += It.IsAny<EventHandler<AppState>>())
            .Callback<EventHandler<AppState>>(handler => stateChangedHandler = handler);

        var viewModel = CreateViewModel();
        await Task.Delay(100); // Wait for initialization

        // Act
        stateChangedHandler?.Invoke(_mockAppStateManager.Object, AppState.Recording);
        await Task.Delay(50);

        // Assert
        viewModel.StatusText.Should().Be("Recording...");
        viewModel.TrayIconPath.Should().Contain("tray-recording.ico");
    }

    [Fact]
    public async Task StateChange_ToProcessing_ShouldUpdateStatusTextAndTrayIcon()
    {
        // Arrange
        EventHandler<AppState>? stateChangedHandler = null;
        _mockAppStateManager.SetupAdd(x => x.StateChanged += It.IsAny<EventHandler<AppState>>())
            .Callback<EventHandler<AppState>>(handler => stateChangedHandler = handler);

        var viewModel = CreateViewModel();
        await Task.Delay(100);

        // Act
        stateChangedHandler?.Invoke(_mockAppStateManager.Object, AppState.Processing);
        await Task.Delay(50);

        // Assert
        viewModel.StatusText.Should().Be("Processing...");
        viewModel.TrayIconPath.Should().Contain("tray-processing.ico");
    }

    [Fact]
    public async Task StateChange_ToError_ShouldUpdateStatusTextAndTrayIcon()
    {
        // Arrange
        EventHandler<AppState>? stateChangedHandler = null;
        _mockAppStateManager.SetupAdd(x => x.StateChanged += It.IsAny<EventHandler<AppState>>())
            .Callback<EventHandler<AppState>>(handler => stateChangedHandler = handler);

        var viewModel = CreateViewModel();
        await Task.Delay(100);

        // Act
        stateChangedHandler?.Invoke(_mockAppStateManager.Object, AppState.Error);
        await Task.Delay(50);

        // Assert
        viewModel.StatusText.Should().Be("Error");
        viewModel.TrayIconPath.Should().Contain("tray-error.ico");
    }

    [Fact]
    public void OpenSettings_ShouldCallWindowService()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.OpenSettingsCommand.Execute(null);

        // Assert
        _mockWindowService.Verify(x => x.ShowSettingsWindow(), Times.Once);
    }

    [Fact]
    public async Task CheckForUpdates_WithUpdateAvailable_ShouldShowNotificationAndOpenReleases()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var updateResult = new UpdateCheckResult(true, "1.0.0", "1.1.0", "https://github.com/releases");
        _mockUpdateService.Setup(x => x.CheckForUpdatesAsync(default))
            .ReturnsAsync(updateResult);

        // Act
        await viewModel.CheckForUpdatesCommand.ExecuteAsync(null);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "Update Available",
            It.IsAny<string>(),
            NotificationType.Success), Times.Once);
        _mockUpdateService.Verify(x => x.OpenReleasesPage(), Times.Once);
    }

    [Fact]
    public async Task CheckForUpdates_WithNoUpdate_ShouldShowUpToDateNotification()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var updateResult = new UpdateCheckResult(false, "1.0.0", "1.0.0", null);
        _mockUpdateService.Setup(x => x.CheckForUpdatesAsync(default))
            .ReturnsAsync(updateResult);

        // Act
        await viewModel.CheckForUpdatesCommand.ExecuteAsync(null);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "No Updates",
            It.IsAny<string>(),
            NotificationType.Info), Times.Once);
        _mockUpdateService.Verify(x => x.OpenReleasesPage(), Times.Never);
    }

    [Fact]
    public async Task CheckForUpdates_WithException_ShouldShowErrorNotification()
    {
        // Arrange
        var viewModel = CreateViewModel();
        _mockUpdateService.Setup(x => x.CheckForUpdatesAsync(default))
            .ThrowsAsync(new Exception("Network error"));

        // Act
        await viewModel.CheckForUpdatesCommand.ExecuteAsync(null);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "Update Check Failed",
            It.IsAny<string>(),
            NotificationType.Error), Times.Once);
    }

    [Fact]
    public void ExitApplication_ShouldCallWindowService()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.ExitApplicationCommand.Execute(null);

        // Assert
        _mockWindowService.Verify(x => x.ExitApplication(), Times.Once);
    }

    [Fact]
    public async Task HotkeyPressed_InPushToTalkMode_WhenIdle_ShouldStartRecording()
    {
        // Arrange
        EventHandler? hotkeyPressedHandler = null;
        _mockHotkeyService.SetupAdd(x => x.HotkeyPressed += It.IsAny<EventHandler>())
            .Callback<EventHandler>(handler => hotkeyPressedHandler = handler);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Idle);
        _mockAudioService.Setup(x => x.StartRecordingAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var viewModel = CreateViewModel();
        await Task.Delay(150); // Wait for initialization

        // Act
        hotkeyPressedHandler?.Invoke(_mockHotkeyService.Object, EventArgs.Empty);
        await Task.Delay(100);

        // Assert
        _mockAppStateManager.Verify(x => x.TransitionTo(AppState.Recording), Times.AtLeastOnce);
        _mockAudioService.Verify(x => x.StartRecordingAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HotkeyReleased_InPushToTalkMode_WhenRecording_ShouldStopAndProcess()
    {
        // Arrange
        EventHandler? hotkeyReleasedHandler = null;
        _mockHotkeyService.SetupAdd(x => x.HotkeyReleased += It.IsAny<EventHandler>())
            .Callback<EventHandler>(handler => hotkeyReleasedHandler = handler);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Recording);
        _mockAudioService.Setup(x => x.StopRecordingAsync())
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });
        _mockTranscriptionService.Setup(x => x.TranscribeAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success("Hello world"));
        _mockOutputService.Setup(x => x.OutputTextAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var viewModel = CreateViewModel();
        await Task.Delay(150);

        // Act
        hotkeyReleasedHandler?.Invoke(_mockHotkeyService.Object, EventArgs.Empty);
        await Task.Delay(200);

        // Assert
        _mockAudioService.Verify(x => x.StopRecordingAsync(), Times.Once);
        _mockTranscriptionService.Verify(x => x.TranscribeAsync(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Once);
        _mockOutputService.Verify(x => x.OutputTextAsync("Hello world"), Times.Once);
    }

    [Fact]
    public async Task HotkeyPressed_InToggleMode_WhenIdle_ShouldStartRecording()
    {
        // Arrange
        _testSettings.Hotkey.Mode = HotkeyMode.Toggle;
        EventHandler? hotkeyPressedHandler = null;
        _mockHotkeyService.SetupAdd(x => x.HotkeyPressed += It.IsAny<EventHandler>())
            .Callback<EventHandler>(handler => hotkeyPressedHandler = handler);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Idle);
        _mockAudioService.Setup(x => x.StartRecordingAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var viewModel = CreateViewModel();
        await Task.Delay(150);

        // Act
        hotkeyPressedHandler?.Invoke(_mockHotkeyService.Object, EventArgs.Empty);
        await Task.Delay(100);

        // Assert
        _mockAppStateManager.Verify(x => x.TransitionTo(AppState.Recording), Times.AtLeastOnce);
        _mockAudioService.Verify(x => x.StartRecordingAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task HotkeyPressed_InToggleMode_WhenRecording_ShouldStopAndProcess()
    {
        // Arrange
        _testSettings.Hotkey.Mode = HotkeyMode.Toggle;
        EventHandler? hotkeyPressedHandler = null;
        _mockHotkeyService.SetupAdd(x => x.HotkeyPressed += It.IsAny<EventHandler>())
            .Callback<EventHandler>(handler => hotkeyPressedHandler = handler);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Recording);
        _mockAudioService.Setup(x => x.StopRecordingAsync())
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });
        _mockTranscriptionService.Setup(x => x.TranscribeAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Success("Test text"));
        _mockOutputService.Setup(x => x.OutputTextAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var viewModel = CreateViewModel();
        await Task.Delay(150);

        // Act
        hotkeyPressedHandler?.Invoke(_mockHotkeyService.Object, EventArgs.Empty);
        await Task.Delay(200);

        // Assert
        _mockAudioService.Verify(x => x.StopRecordingAsync(), Times.Once);
        _mockTranscriptionService.Verify(x => x.TranscribeAsync(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task TranscriptionFailure_ShouldShowErrorNotificationAndReturnToIdle()
    {
        // Arrange
        EventHandler? hotkeyReleasedHandler = null;
        _mockHotkeyService.SetupAdd(x => x.HotkeyReleased += It.IsAny<EventHandler>())
            .Callback<EventHandler>(handler => hotkeyReleasedHandler = handler);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Recording);
        _mockAudioService.Setup(x => x.StopRecordingAsync())
            .ReturnsAsync(new byte[] { 1, 2, 3, 4 });
        _mockTranscriptionService.Setup(x => x.TranscribeAsync(It.IsAny<byte[]>(), It.IsAny<string>()))
            .ReturnsAsync(Result<string>.Failure("Transcription failed"));

        var viewModel = CreateViewModel();
        await Task.Delay(150);

        // Act
        hotkeyReleasedHandler?.Invoke(_mockHotkeyService.Object, EventArgs.Empty);
        await Task.Delay(200);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "Transcription Failed",
            It.IsAny<string>(),
            NotificationType.Warning), Times.Once);
        _mockAppStateManager.Verify(x => x.TransitionTo(AppState.Idle), Times.AtLeastOnce);
    }

    [Fact]
    public async Task NoAudioRecorded_ShouldShowWarningAndReturnToIdle()
    {
        // Arrange
        EventHandler? hotkeyReleasedHandler = null;
        _mockHotkeyService.SetupAdd(x => x.HotkeyReleased += It.IsAny<EventHandler>())
            .Callback<EventHandler>(handler => hotkeyReleasedHandler = handler);

        _mockAppStateManager.Setup(x => x.CurrentState).Returns(AppState.Recording);
        _mockAudioService.Setup(x => x.StopRecordingAsync())
            .ReturnsAsync(Array.Empty<byte>());

        var viewModel = CreateViewModel();
        await Task.Delay(150);

        // Act
        hotkeyReleasedHandler?.Invoke(_mockHotkeyService.Object, EventArgs.Empty);
        await Task.Delay(200);

        // Assert
        _mockNotificationService.Verify(x => x.ShowNotificationAsync(
            "No Audio",
            It.IsAny<string>(),
            NotificationType.Warning), Times.Once);
        _mockAppStateManager.Verify(x => x.TransitionTo(AppState.Idle), Times.AtLeastOnce);
        _mockTranscriptionService.Verify(x => x.TranscribeAsync(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Initialization_WithNoModelsDownloaded_ShouldShowFirstRunDialog()
    {
        // Arrange
        _mockModelManager.Setup(x => x.GetAvailableModelsAsync())
            .ReturnsAsync(new[]
            {
                new WhisperModel("base", "Base", 74_000_000, false, null)
            });
        _mockWindowService.Setup(x => x.ShowFirstRunDialogAsync())
            .ReturnsAsync(true);

        // Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        _mockWindowService.Verify(x => x.ShowFirstRunDialogAsync(), Times.Once);
    }

    [Fact]
    public async Task Initialization_ShouldLoadConfigAndRegisterHotkey()
    {
        // Arrange & Act
        var viewModel = CreateViewModel();
        await Task.Delay(200);

        // Assert
        _mockConfigService.Verify(x => x.LoadAsync(), Times.Once);
        _mockHotkeyService.Verify(x => x.RegisterAsync(), Times.Once);
    }

    [Fact]
    public async Task Dispose_ShouldUnsubscribeFromEvents()
    {
        // Arrange
        var viewModel = CreateViewModel();
        await Task.Delay(100);

        // Act
        viewModel.Dispose();

        // Assert
        _mockAppStateManager.VerifyRemove(x => x.StateChanged -= It.IsAny<EventHandler<AppState>>(), Times.Once);
        _mockHotkeyService.VerifyRemove(x => x.HotkeyPressed -= It.IsAny<EventHandler>(), Times.Once);
        _mockHotkeyService.VerifyRemove(x => x.HotkeyReleased -= It.IsAny<EventHandler>(), Times.Once);
    }

    private MainViewModel CreateViewModel()
    {
        return new MainViewModel(
            _mockAppStateManager.Object,
            _mockWindowService.Object,
            _mockHotkeyService.Object,
            _mockAudioService.Object,
            _mockTranscriptionService.Object,
            _mockOutputService.Object,
            _mockNotificationService.Object,
            _mockConfigService.Object,
            _mockModelManager.Object,
            _mockUpdateService.Object,
            _mockLogger.Object);
    }
}

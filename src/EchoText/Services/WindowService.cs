using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using EchoText.Services.Interfaces;
using EchoText.ViewModels;
using EchoText.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EchoText.Services;

/// <summary>
/// Service for managing application windows.
/// Handles window creation and lifecycle to maintain MVVM separation.
/// </summary>
public sealed class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private SettingsWindow? _settingsWindow;
    private RecordingOverlay? _recordingOverlay;

    /// <summary>
    /// Initializes a new instance of the WindowService.
    /// </summary>
    /// <param name="serviceProvider">Service provider for creating ViewModels</param>
    public WindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public void ShowSettingsWindow()
    {
        // If settings window is already open, bring it to front
        if (_settingsWindow != null)
        {
            _settingsWindow.Activate();
            return;
        }

        // Create new settings window with ViewModel from DI
        var viewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        _settingsWindow = new SettingsWindow(viewModel);

        // Handle window closed to clear reference
        _settingsWindow.Closed += (sender, args) =>
        {
            _settingsWindow = null;
        };

        _settingsWindow.Show();
    }

    /// <inheritdoc />
    public void ShowRecordingOverlay()
    {
        // Close any existing overlay first
        HideRecordingOverlay();

        // Create new recording overlay with ViewModel from DI
        var viewModel = _serviceProvider.GetRequiredService<RecordingOverlayViewModel>();
        _recordingOverlay = new RecordingOverlay(viewModel);

        // Handle window closed to clear reference
        _recordingOverlay.Closed += (sender, args) =>
        {
            _recordingOverlay = null;
        };

        _recordingOverlay.Show();
    }

    /// <inheritdoc />
    public void HideRecordingOverlay()
    {
        if (_recordingOverlay != null)
        {
            _recordingOverlay.Close();
            _recordingOverlay = null;
        }
    }

    /// <inheritdoc />
    public void ExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}

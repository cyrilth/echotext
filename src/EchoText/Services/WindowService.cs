using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
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
        // Must run on UI thread since we're creating/showing a window
        Dispatcher.UIThread.Post(() =>
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
        });
    }

    /// <inheritdoc />
    public void ShowRecordingOverlay()
    {
        // Must run on UI thread since we're creating/showing a window
        Dispatcher.UIThread.Post(() =>
        {
            // Close any existing overlay first
            HideRecordingOverlayInternal();

            // Create new recording overlay with ViewModel from DI
            var viewModel = _serviceProvider.GetRequiredService<RecordingOverlayViewModel>();
            _recordingOverlay = new RecordingOverlay(viewModel);

            // Handle window closed to clear reference
            _recordingOverlay.Closed += (sender, args) =>
            {
                _recordingOverlay = null;
            };

            _recordingOverlay.Show();
        });
    }

    /// <inheritdoc />
    public void HideRecordingOverlay()
    {
        // Must run on UI thread since we're closing a window
        Dispatcher.UIThread.Post(HideRecordingOverlayInternal);
    }

    private void HideRecordingOverlayInternal()
    {
        if (_recordingOverlay != null)
        {
            _recordingOverlay.Close();
            _recordingOverlay = null;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ShowFirstRunDialogAsync()
    {
        // Create ViewModel from DI
        var viewModel = _serviceProvider.GetRequiredService<FirstRunViewModel>();
        var dialog = new FirstRunDialog
        {
            DataContext = viewModel
        };

        // Show dialog and wait for result
        var mainWindow = GetMainWindow();
        bool result;

        if (mainWindow == null)
        {
            // If no main window, show as regular window and wait for it to close
            var tcs = new TaskCompletionSource<bool>();
            dialog.Closed += (_, _) => tcs.TrySetResult(viewModel.ModelDownloaded);
            dialog.Show();
            result = await tcs.Task;
        }
        else
        {
            result = await dialog.ShowDialog<bool>(mainWindow);
        }

        // Dispose ViewModel
        viewModel.Dispose();

        return result;
    }

    /// <inheritdoc />
    public void ExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    /// <summary>
    /// Gets the main window of the application
    /// </summary>
    private Avalonia.Controls.Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }
}

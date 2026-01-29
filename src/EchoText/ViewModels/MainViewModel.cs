using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EchoText.Models;
using EchoText.Services.Interfaces;
using EchoText.Views;
using Microsoft.Extensions.DependencyInjection;

namespace EchoText.ViewModels;

/// <summary>
/// ViewModel for the main application window and tray icon
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IAppStateManager _appStateManager;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _statusText = "Idle";

    [ObservableProperty]
    private string _trayIconPath = "avares://EchoText/Assets/Icons/tray-idle.ico";

    /// <summary>
    /// Initializes a new instance of the MainViewModel
    /// </summary>
    /// <param name="appStateManager">Application state manager</param>
    /// <param name="serviceProvider">Service provider for creating windows</param>
    public MainViewModel(IAppStateManager appStateManager, IServiceProvider serviceProvider)
    {
        _appStateManager = appStateManager ?? throw new ArgumentNullException(nameof(appStateManager));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        // Subscribe to state changes
        _appStateManager.StateChanged += OnAppStateChanged;

        // Initialize status based on current state
        UpdateStatusForState(_appStateManager.CurrentState);
    }

    /// <summary>
    /// Command to open settings window
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        // Create settings window with ViewModel from DI
        var viewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();
        var settingsWindow = new SettingsWindow(viewModel);
        settingsWindow.Show();
    }

    /// <summary>
    /// Command to check for updates
    /// </summary>
    [RelayCommand]
    private void CheckForUpdates()
    {
        // TODO: Implement in TASK-602
        // Check for application updates
    }

    /// <summary>
    /// Command to show about dialog
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        // TODO: Implement later
        // Show about dialog with version info
    }

    /// <summary>
    /// Command to exit the application
    /// </summary>
    [RelayCommand]
    private void ExitApplication()
    {
        // Request application shutdown
        System.Environment.Exit(0);
    }

    /// <summary>
    /// Handles application state changes and updates UI accordingly
    /// </summary>
    private void OnAppStateChanged(object? sender, AppState newState)
    {
        UpdateStatusForState(newState);
    }

    /// <summary>
    /// Updates the status text and tray icon based on the current app state
    /// </summary>
    private void UpdateStatusForState(AppState state)
    {
        switch (state)
        {
            case AppState.Loading:
                StatusText = "Loading...";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-processing.ico";
                break;

            case AppState.Idle:
                StatusText = "Ready";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-idle.ico";
                break;

            case AppState.Recording:
                StatusText = "Recording...";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-recording.ico";
                break;

            case AppState.Processing:
                StatusText = "Processing...";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-processing.ico";
                break;

            case AppState.Error:
                StatusText = "Error";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-error.ico";
                break;

            default:
                StatusText = "Unknown";
                TrayIconPath = "avares://EchoText/Assets/Icons/tray-idle.ico";
                break;
        }
    }
}

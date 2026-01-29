using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using EchoText.ViewModels;
using System;

namespace EchoText.Views;

public partial class MainWindow : Window
{
    private TrayIcon? _trayIcon;
    private MainViewModel? _viewModel;
    private NativeMenuItem? _statusMenuItem;

    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor that accepts a ViewModel for dependency injection
    /// </summary>
    public MainWindow(MainViewModel viewModel) : this()
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        // Initialize tray icon
        InitializeTrayIcon();

        // Subscribe to property changes for icon updates
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void InitializeTrayIcon()
    {
        if (_viewModel == null) return;

        _trayIcon = new TrayIcon
        {
            ToolTipText = "EchoText - Voice to Text",
            IsVisible = true
        };

        // Set initial icon
        UpdateTrayIcon(_viewModel.TrayIconPath);

        // Create context menu
        var menu = new NativeMenu();

        // Status indicator (non-clickable)
        _statusMenuItem = new NativeMenuItem
        {
            Header = _viewModel.StatusText,
            IsEnabled = false
        };
        menu.Add(_statusMenuItem);

        menu.Add(new NativeMenuItemSeparator());

        // Settings menu item
        var settingsItem = new NativeMenuItem { Header = "Settings..." };
        settingsItem.Click += (s, e) => _viewModel.OpenSettingsCommand.Execute(null);
        menu.Add(settingsItem);

        // Check for Updates menu item
        var updatesItem = new NativeMenuItem { Header = "Check for Updates" };
        updatesItem.Click += (s, e) => _viewModel.CheckForUpdatesCommand.Execute(null);
        menu.Add(updatesItem);

        menu.Add(new NativeMenuItemSeparator());

        // About menu item
        var aboutItem = new NativeMenuItem { Header = "About" };
        aboutItem.Click += (s, e) => _viewModel.ShowAboutCommand.Execute(null);
        menu.Add(aboutItem);

        // Exit menu item
        var exitItem = new NativeMenuItem { Header = "Exit" };
        exitItem.Click += (s, e) => _viewModel.ExitApplicationCommand.Execute(null);
        menu.Add(exitItem);

        _trayIcon.Menu = menu;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_viewModel == null || _trayIcon == null) return;

        if (e.PropertyName == nameof(MainViewModel.TrayIconPath))
        {
            UpdateTrayIcon(_viewModel.TrayIconPath);
        }
        else if (e.PropertyName == nameof(MainViewModel.StatusText))
        {
            // Update status menu item text
            if (_statusMenuItem != null)
            {
                _statusMenuItem.Header = _viewModel.StatusText;
            }
        }
    }

    private void UpdateTrayIcon(string iconPath)
    {
        if (_trayIcon == null) return;

        try
        {
            // Load icon from assets using AssetLoader
            var uri = new Uri(iconPath);
            var assetLoader = AssetLoader.Open(uri);
            if (assetLoader != null)
            {
                _trayIcon.Icon = new WindowIcon(assetLoader);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Failed to load tray icon: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Clean up tray icon
        if (_trayIcon != null)
        {
            _trayIcon.IsVisible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
        }

        base.OnClosed(e);
    }
}

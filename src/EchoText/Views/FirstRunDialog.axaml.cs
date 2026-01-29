using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using EchoText.ViewModels;

namespace EchoText.Views;

/// <summary>
/// Code-behind for FirstRunDialog
/// </summary>
public partial class FirstRunDialog : Window
{
    /// <summary>
    /// Initializes a new instance of the FirstRunDialog
    /// </summary>
    public FirstRunDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handle the Skip button click
    /// </summary>
    private void OnSkipClick(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    /// <summary>
    /// Handle the Close button click
    /// </summary>
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as FirstRunViewModel;
        Close(viewModel?.ModelDownloaded ?? false);
    }
}

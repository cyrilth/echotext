using Avalonia.Controls;
using EchoText.ViewModels;

namespace EchoText.Views;

/// <summary>
/// Settings window code-behind
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="viewModel">The settings view model</param>
    public SettingsWindow(SettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}

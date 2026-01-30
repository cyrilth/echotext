using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EchoText.Views;

/// <summary>
/// About window showing application version and information.
/// </summary>
public partial class AboutWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the AboutWindow.
    /// </summary>
    public AboutWindow()
    {
        InitializeComponent();
        SetVersionText();
    }

    /// <summary>
    /// Sets the version text from the assembly version.
    /// </summary>
    private void SetVersionText()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var versionString = version != null
            ? $"Version {version.Major}.{version.Minor}.{version.Build}"
            : "Version unknown";

        VersionText.Text = versionString;
    }

    /// <summary>
    /// Handle the Close button click.
    /// </summary>
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

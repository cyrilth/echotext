using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EchoText.Views;

/// <summary>
/// Dialog for displaying update check results.
/// </summary>
public partial class UpdateCheckDialog : Window
{
    private string? _releaseUrl;

    /// <summary>
    /// Initializes a new instance of the UpdateCheckDialog.
    /// </summary>
    public UpdateCheckDialog()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Configures the dialog to show "up to date" state.
    /// </summary>
    /// <param name="currentVersion">The current application version</param>
    public void ShowUpToDate(string currentVersion)
    {
        StatusIcon.Text = "\u2714"; // Checkmark
        StatusIcon.Foreground = Avalonia.Media.Brushes.LimeGreen;
        TitleText.Text = "You're Up to Date";
        MessageText.Text = $"EchoText {currentVersion} is the latest version available.";
        VersionInfoPanel.IsVisible = false;
        DownloadButton.IsVisible = false;
    }

    /// <summary>
    /// Configures the dialog to show "update available" state.
    /// </summary>
    /// <param name="currentVersion">The current application version</param>
    /// <param name="latestVersion">The latest available version</param>
    /// <param name="releaseUrl">URL to the release page</param>
    public void ShowUpdateAvailable(string currentVersion, string latestVersion, string releaseUrl)
    {
        _releaseUrl = releaseUrl;

        StatusIcon.Text = "\u2B06"; // Up arrow
        StatusIcon.Foreground = Avalonia.Media.Brushes.DodgerBlue;
        TitleText.Text = "Update Available";
        MessageText.Text = "A new version of EchoText is available for download.";

        CurrentVersionText.Text = $"Current version: {currentVersion}";
        LatestVersionText.Text = $"Latest version: {latestVersion}";

        VersionInfoPanel.IsVisible = true;
        DownloadButton.IsVisible = true;
    }

    /// <summary>
    /// Configures the dialog to show error state.
    /// </summary>
    /// <param name="errorMessage">The error message to display</param>
    public void ShowError(string errorMessage)
    {
        StatusIcon.Text = "\u26A0"; // Warning
        StatusIcon.Foreground = Avalonia.Media.Brushes.Orange;
        TitleText.Text = "Update Check Failed";
        MessageText.Text = errorMessage;
        VersionInfoPanel.IsVisible = false;
        DownloadButton.IsVisible = false;
    }

    /// <summary>
    /// Handle the Download button click.
    /// </summary>
    private void OnDownloadClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_releaseUrl))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _releaseUrl,
                    UseShellExecute = true
                });
            }
            catch
            {
                // Ignore if browser fails to open
            }
        }
        Close();
    }

    /// <summary>
    /// Handle the Close button click.
    /// </summary>
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

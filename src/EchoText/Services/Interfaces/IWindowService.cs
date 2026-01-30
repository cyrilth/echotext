using System.Threading.Tasks;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for managing application windows.
/// Handles window creation and lifecycle to maintain MVVM separation.
/// </summary>
public interface IWindowService
{
    /// <summary>
    /// Shows the settings window.
    /// If the window is already open, brings it to front instead of creating a new instance.
    /// </summary>
    void ShowSettingsWindow();

    /// <summary>
    /// Shows the recording overlay window.
    /// Creates a new instance each time recording starts.
    /// </summary>
    void ShowRecordingOverlay();

    /// <summary>
    /// Hides and closes the recording overlay window.
    /// </summary>
    void HideRecordingOverlay();

    /// <summary>
    /// Shows the first-run dialog for downloading a Whisper model.
    /// </summary>
    /// <returns>True if a model was downloaded, false if the user skipped.</returns>
    Task<bool> ShowFirstRunDialogAsync();

    /// <summary>
    /// Shows the about window with version information.
    /// </summary>
    void ShowAboutWindow();

    /// <summary>
    /// Shows the update check dialog with "up to date" message.
    /// </summary>
    /// <param name="currentVersion">The current application version</param>
    void ShowUpToDateDialog(string currentVersion);

    /// <summary>
    /// Shows the update check dialog with "update available" message.
    /// </summary>
    /// <param name="currentVersion">The current application version</param>
    /// <param name="latestVersion">The latest available version</param>
    /// <param name="releaseUrl">URL to the release page</param>
    void ShowUpdateAvailableDialog(string currentVersion, string latestVersion, string releaseUrl);

    /// <summary>
    /// Shows the update check dialog with an error message.
    /// </summary>
    /// <param name="errorMessage">The error message to display</param>
    void ShowUpdateErrorDialog(string errorMessage);

    /// <summary>
    /// Exits the application gracefully.
    /// </summary>
    void ExitApplication();
}

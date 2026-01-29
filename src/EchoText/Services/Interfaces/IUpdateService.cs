using System.Threading;
using System.Threading.Tasks;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for checking for application updates from GitHub releases.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks GitHub for the latest release and compares with current version.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Update check result containing version information and update availability</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens the GitHub releases page in the default browser.
    /// </summary>
    void OpenReleasesPage();
}

/// <summary>
/// Result of an update check operation.
/// </summary>
/// <param name="UpdateAvailable">True if a newer version is available</param>
/// <param name="CurrentVersion">Current application version</param>
/// <param name="LatestVersion">Latest version available on GitHub</param>
/// <param name="ReleaseUrl">URL to the GitHub release page</param>
public record UpdateCheckResult(
    bool UpdateAvailable,
    string? CurrentVersion,
    string? LatestVersion,
    string? ReleaseUrl
);

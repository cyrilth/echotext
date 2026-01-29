using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using EchoText.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EchoText.Services;

/// <summary>
/// Service for checking for application updates from GitHub releases.
/// </summary>
public class UpdateService : IUpdateService
{
    private const string GitHubOwner = "yourusername";  // TODO: Update with actual GitHub username
    private const string GitHubRepo = "echotext";
    private const string GitHubApiUrl = $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest";
    private const string GitHubReleasesUrl = $"https://github.com/{GitHubOwner}/{GitHubRepo}/releases";

    private readonly ILogger<UpdateService>? _logger;
    private readonly HttpClient _httpClient;

    public UpdateService(ILogger<UpdateService>? logger = null)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "EchoText");
    }

    /// <summary>
    /// Checks GitHub for the latest release and compares with current version.
    /// </summary>
    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.LogInformation("Checking for updates from GitHub...");

            // Get current version
            var currentVersion = GetCurrentVersion();
            _logger?.LogInformation("Current version: {Version}", currentVersion);

            // Fetch latest release from GitHub
            var response = await _httpClient.GetAsync(GitHubApiUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to check for updates. Status code: {StatusCode}", response.StatusCode);
                return new UpdateCheckResult(false, currentVersion, null, null);
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var release = JsonSerializer.Deserialize<GitHubRelease>(jsonContent);

            if (release?.TagName == null)
            {
                _logger?.LogWarning("Failed to parse GitHub release information");
                return new UpdateCheckResult(false, currentVersion, null, null);
            }

            // Remove 'v' prefix if present (e.g., "v1.0.0" -> "1.0.0")
            var latestVersionString = release.TagName.TrimStart('v');
            _logger?.LogInformation("Latest version: {Version}", latestVersionString);

            // Compare versions
            var updateAvailable = CompareVersions(currentVersion, latestVersionString);

            if (updateAvailable)
            {
                _logger?.LogInformation("Update available: {LatestVersion}", latestVersionString);
            }
            else
            {
                _logger?.LogInformation("Application is up to date");
            }

            return new UpdateCheckResult(
                updateAvailable,
                currentVersion,
                latestVersionString,
                release.HtmlUrl ?? GitHubReleasesUrl
            );
        }
        catch (HttpRequestException ex)
        {
            _logger?.LogError(ex, "Network error while checking for updates");
            return new UpdateCheckResult(false, GetCurrentVersion(), null, null);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Unexpected error while checking for updates");
            return new UpdateCheckResult(false, GetCurrentVersion(), null, null);
        }
    }

    /// <summary>
    /// Opens the GitHub releases page in the default browser.
    /// </summary>
    public void OpenReleasesPage()
    {
        try
        {
            _logger?.LogInformation("Opening GitHub releases page: {Url}", GitHubReleasesUrl);

            var psi = new ProcessStartInfo
            {
                FileName = GitHubReleasesUrl,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to open releases page");
        }
    }

    /// <summary>
    /// Gets the current application version from the assembly.
    /// </summary>
    private string GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        // Return version in format "1.0.0" (major.minor.patch)
        return version != null
            ? $"{version.Major}.{version.Minor}.{version.Build}"
            : "0.0.0";
    }

    /// <summary>
    /// Compares two semantic version strings.
    /// </summary>
    /// <param name="currentVersion">Current version string (e.g., "1.0.0")</param>
    /// <param name="latestVersion">Latest version string (e.g., "1.1.0")</param>
    /// <returns>True if latest version is greater than current version</returns>
    private bool CompareVersions(string currentVersion, string latestVersion)
    {
        try
        {
            // Try to parse as System.Version for proper semantic version comparison
            if (Version.TryParse(currentVersion, out var current) &&
                Version.TryParse(latestVersion, out var latest))
            {
                return latest > current;
            }

            // Fallback to string comparison
            _logger?.LogWarning("Failed to parse versions for comparison. Current: {Current}, Latest: {Latest}",
                currentVersion, latestVersion);
            return false;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error comparing versions");
            return false;
        }
    }

    /// <summary>
    /// GitHub API release response model.
    /// </summary>
    private class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("body")]
        public string? Body { get; set; }

        [JsonPropertyName("published_at")]
        public string? PublishedAt { get; set; }
    }
}

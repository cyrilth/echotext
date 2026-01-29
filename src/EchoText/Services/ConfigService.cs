using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EchoText.Models;
using EchoText.Platform;
using EchoText.Services.Interfaces;

namespace EchoText.Services;

/// <summary>
/// Manages application configuration loading and saving.
/// </summary>
public class ConfigService : IConfigService
{
    private const string ConfigFileName = "settings.json";
    private readonly string _configFilePath;
    private readonly ILogger<ConfigService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private AppSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="configFilePath">Optional custom path for the config file. If null, uses the default platform-specific path.</param>
    public ConfigService(ILogger<ConfigService> logger, string? configFilePath = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configFilePath = configFilePath ?? Path.Combine(PlatformInfo.ConfigDirectory, ConfigFileName);
        _settings = new AppSettings();

        _logger.LogInformation("ConfigService initialized, config file: {ConfigPath}", _configFilePath);
    }

    /// <inheritdoc/>
    public AppSettings Settings => _settings;

    /// <inheritdoc/>
    public event EventHandler? SettingsChanged;

    /// <inheritdoc/>
    public async Task LoadAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _logger.LogInformation("Loading configuration from {ConfigPath}", _configFilePath);

            // Ensure config directory exists
            var configDirectory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(configDirectory) && !Directory.Exists(configDirectory))
            {
                _logger.LogInformation("Creating config directory: {ConfigDirectory}", configDirectory);
                Directory.CreateDirectory(configDirectory);
            }

            // Load settings if file exists, otherwise create default
            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                var settings = JsonSerializer.Deserialize(json, AppSettingsJsonContext.Default.AppSettings);
                if (settings != null)
                {
                    _settings = settings;
                    _logger.LogInformation("Configuration loaded successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize configuration, using defaults");
                }
            }
            else
            {
                _logger.LogInformation("Configuration file not found, creating default settings");
                // Create default settings file
                _settings = new AppSettings();
                await SaveInternalAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load configuration, using defaults");
            _settings = new AppSettings();
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _logger.LogInformation("Saving configuration to {ConfigPath}", _configFilePath);
            await SaveInternalAsync();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
            _logger.LogInformation("Configuration saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration");
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Internal save method (not thread-safe, caller must hold lock).
    /// </summary>
    private async Task SaveInternalAsync()
    {
        // Ensure config directory exists
        var configDirectory = Path.GetDirectoryName(_configFilePath);
        if (!string.IsNullOrEmpty(configDirectory) && !Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
        }

        // Serialize and write settings
        var json = JsonSerializer.Serialize(_settings, AppSettingsJsonContext.Default.AppSettings);
        await File.WriteAllTextAsync(_configFilePath, json);
    }
}

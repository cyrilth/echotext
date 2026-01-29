using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly SemaphoreSlim _lock = new(1, 1);
    private AppSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigService"/> class.
    /// </summary>
    /// <param name="configFilePath">Optional custom path for the config file. If null, uses the default platform-specific path.</param>
    public ConfigService(string? configFilePath = null)
    {
        _configFilePath = configFilePath ?? Path.Combine(PlatformInfo.ConfigDirectory, ConfigFileName);
        _settings = new AppSettings();
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
            // Ensure config directory exists
            var configDirectory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(configDirectory) && !Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            // Load settings if file exists, otherwise create default
            if (File.Exists(_configFilePath))
            {
                var json = await File.ReadAllTextAsync(_configFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json, GetJsonOptions());
                if (settings != null)
                {
                    _settings = settings;
                }
            }
            else
            {
                // Create default settings file
                _settings = new AppSettings();
                await SaveInternalAsync();
            }
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
            await SaveInternalAsync();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
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
        var json = JsonSerializer.Serialize(_settings, GetJsonOptions());
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    /// <summary>
    /// Gets JSON serialization options with proper formatting.
    /// </summary>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };
    }
}

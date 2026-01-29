using System;
using System.Threading.Tasks;
using EchoText.Models;

namespace EchoText.Services.Interfaces;

/// <summary>
/// Service for loading and saving application configuration.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Gets the current application settings.
    /// </summary>
    AppSettings Settings { get; }

    /// <summary>
    /// Loads settings from disk. Creates default settings if file doesn't exist.
    /// </summary>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    Task LoadAsync();

    /// <summary>
    /// Saves the current settings to disk.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync();

    /// <summary>
    /// Fired when settings are changed and saved.
    /// </summary>
    event EventHandler? SettingsChanged;
}

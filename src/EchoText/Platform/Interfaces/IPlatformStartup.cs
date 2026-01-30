using System;

namespace EchoText.Platform.Interfaces;

/// <summary>
/// Platform-specific interface for managing application startup with the system.
/// </summary>
public interface IPlatformStartup : IDisposable
{
    /// <summary>
    /// Gets whether the application is configured to start with the system.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Enables the application to start automatically when the system starts.
    /// </summary>
    /// <param name="executablePath">Path to the application executable.</param>
    /// <returns>True if successfully enabled, false otherwise.</returns>
    bool Enable(string executablePath);

    /// <summary>
    /// Disables the application from starting automatically with the system.
    /// </summary>
    /// <returns>True if successfully disabled, false otherwise.</returns>
    bool Disable();
}

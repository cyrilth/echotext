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
}

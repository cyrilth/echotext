using Avalonia.Controls;
using EchoText.ViewModels;

namespace EchoText.Views;

/// <summary>
/// Recording overlay window code-behind.
/// Displays recording duration, audio level, and cancel button.
/// Always on top, appears when recording starts.
/// </summary>
public partial class RecordingOverlay : Window
{
    public RecordingOverlay()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="viewModel">The recording overlay view model</param>
    public RecordingOverlay(RecordingOverlayViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Start duration updates when window is opened
        Opened += (sender, args) =>
        {
            viewModel.StartDurationUpdates();
        };
    }
}

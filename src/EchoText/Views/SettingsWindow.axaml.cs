using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EchoText.Models;
using EchoText.ViewModels;

namespace EchoText.Views;

/// <summary>
/// Settings window code-behind
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Handle the Close button click.
    /// </summary>
    private void OnCloseClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Constructor for dependency injection
    /// </summary>
    /// <param name="viewModel">The settings view model</param>
    public SettingsWindow(SettingsViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // Handle key events for hotkey capture
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm || !vm.IsCapturingHotkey)
            return;

        // Ignore modifier-only key presses
        if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
            e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
            e.Key == Key.LWin || e.Key == Key.RWin)
        {
            return;
        }

        // Handle Escape to cancel
        if (e.Key == Key.Escape)
        {
            vm.CancelHotkeyCapture();
            e.Handled = true;
            return;
        }

        // Convert Avalonia key modifiers to our KeyModifiers
        var modifiers = Models.KeyModifiers.None;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Control))
            modifiers |= Models.KeyModifiers.Ctrl;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Shift))
            modifiers |= Models.KeyModifiers.Shift;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Alt))
            modifiers |= Models.KeyModifiers.Alt;
        if (e.KeyModifiers.HasFlag(Avalonia.Input.KeyModifiers.Meta))
            modifiers |= Models.KeyModifiers.Meta;

        // Convert key to string
        var keyString = ConvertKeyToString(e.Key);
        if (!string.IsNullOrEmpty(keyString))
        {
            vm.CaptureHotkey(modifiers, keyString);
            e.Handled = true;
        }
    }

    private static string ConvertKeyToString(Key key)
    {
        return key switch
        {
            Key.Space => "Space",
            Key.A => "A",
            Key.B => "B",
            Key.C => "C",
            Key.D => "D",
            Key.E => "E",
            Key.F => "F",
            Key.G => "G",
            Key.H => "H",
            Key.I => "I",
            Key.J => "J",
            Key.K => "K",
            Key.L => "L",
            Key.M => "M",
            Key.N => "N",
            Key.O => "O",
            Key.P => "P",
            Key.Q => "Q",
            Key.R => "R",
            Key.S => "S",
            Key.T => "T",
            Key.U => "U",
            Key.V => "V",
            Key.W => "W",
            Key.X => "X",
            Key.Y => "Y",
            Key.Z => "Z",
            Key.D0 => "0",
            Key.D1 => "1",
            Key.D2 => "2",
            Key.D3 => "3",
            Key.D4 => "4",
            Key.D5 => "5",
            Key.D6 => "6",
            Key.D7 => "7",
            Key.D8 => "8",
            Key.D9 => "9",
            Key.F1 => "F1",
            Key.F2 => "F2",
            Key.F3 => "F3",
            Key.F4 => "F4",
            Key.F5 => "F5",
            Key.F6 => "F6",
            Key.F7 => "F7",
            Key.F8 => "F8",
            Key.F9 => "F9",
            Key.F10 => "F10",
            Key.F11 => "F11",
            Key.F12 => "F12",
            _ => key.ToString()
        };
    }
}

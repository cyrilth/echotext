using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace EchoText.Views;

/// <summary>
/// Converts a percentage (0-100) to a width value for the audio level meter.
/// Assumes the container is 250 pixels wide.
/// </summary>
public class PercentageToWidthConverter : IValueConverter
{
    public static readonly PercentageToWidthConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double percentage)
        {
            // Container is 250 pixels wide (280 - padding)
            const double maxWidth = 250.0;
            return (percentage / 100.0) * maxWidth;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

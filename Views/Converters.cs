using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ZootekniPro.App.Views;

public static class Converters
{
    public static readonly IValueConverter SelectedBackgroundConverter = new SelectedBackgroundConverterImpl();
}

public class SelectedBackgroundConverterImpl : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is string paramStr && int.TryParse(paramStr, out int targetIndex))
        {
            return selectedIndex == targetIndex 
                ? new SolidColorBrush(Color.Parse("#6366F1"))
                : new SolidColorBrush(Colors.Transparent);
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InvestmentPortfolioManager.WPF.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        // Konwertuje Object -> Visibility
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Jeśli parametr to "Inverted", zwracamy Visible dla null (dla placeholderów)
            bool isInverted = parameter?.ToString() == "Inverted";

            if (value == null)
                return isInverted ? Visibility.Visible : Visibility.Collapsed;

            return isInverted ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeflexPro.Converters
{
    // Returns "TabButtonActiveStyle" or "TabButtonStyle" key string for DynamicResource lookup.
    // Used in MainWindow to style sub-page tab buttons based on active state.
    public class BoolToTabButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is true
                ? Application.Current.FindResource("TabButtonActiveStyle")
                : Application.Current.FindResource("TabButtonStyle");

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            DependencyProperty.UnsetValue;
    }
}

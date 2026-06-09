using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DeflexPro.Converters
{
    class TempToImgConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not double temp)
                return Binding.DoNothing;

            string resourceKey = temp switch
            {
                <= 0 => "temp1Img",
                < 20 => "temp2Img",
                < 30 => "temp3Img",
                < 40 => "temp4Img",
                _ => "temp5Img"
            };

            return App.Current.FindResource(resourceKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}

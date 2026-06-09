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
            BitmapImage bmp = null;

            if (value is double)
            {
                double temp = (double)value;

                if (temp <= 0)
                    bmp = (BitmapImage)App.Current.FindResource("temp1Img");
                else
                    if (temp > 0 && temp < 20)
                        bmp = (BitmapImage)App.Current.FindResource("temp2Img");
                    else
                        if (temp >= 20 && temp < 30)
                            bmp = (BitmapImage)App.Current.FindResource("temp3Img");
                        else
                            if (temp >= 30 && temp < 40)
                                bmp = (BitmapImage)App.Current.FindResource("temp4Img");
                            else
                                if (temp > 40)
                                    bmp = (BitmapImage)App.Current.FindResource("temp5Img");
            }
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }

        #endregion
    }
}

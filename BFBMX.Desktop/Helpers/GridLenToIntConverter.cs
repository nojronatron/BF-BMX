using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace BFBMX.Desktop.Helpers
{
    [ValueConversion(typeof(int), typeof(GridLength))]
    public class GridLenToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            GridLength gl = (GridLength)value;
            return (int)gl.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double newVal = (double)value;
            GridLength gl = new GridLength(newVal);
            return gl;
        }
    }
}

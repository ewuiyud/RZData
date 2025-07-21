using System;
using System.Windows.Data;
using System.Windows.Media;

namespace RZData.Converters
{
    // 用于根据IsModified属性设置行背景色的转换器
    public class BoolToColorGrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isModified && isModified)
            {
                return new SolidColorBrush(Colors.LightGray);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

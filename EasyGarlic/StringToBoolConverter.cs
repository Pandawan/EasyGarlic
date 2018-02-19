using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace EasyGarlic {
    public class StringToBoolConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is string))
            {
                return false;
            }
            return !String.IsNullOrWhiteSpace((string)value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string))
            {
                return false;
            }
            return !String.IsNullOrWhiteSpace((string)value);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

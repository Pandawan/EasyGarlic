using System;
using System.Globalization;
using System.Windows.Data;

namespace EasyGarlic {
    public class MinerToIDStringConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Miner))
            {
                return "Unkown";
            }
            return ((Miner)value).GetID();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Miner))
            {
                return "Unkown";
            }
            return ((Miner)value).GetID();
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

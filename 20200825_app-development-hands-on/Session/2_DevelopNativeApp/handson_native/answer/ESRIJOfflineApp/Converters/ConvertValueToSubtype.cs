using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;

namespace ESRIJOfflineApp.Converters
{
    // 不使用
    class ConvertValueToSubtype : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null && values[1] != null)
            {
                var CodedValues = values[0];
                return CodedValues;
            }
            return null;
        }

        /// <summary>
        /// Takes in a CodedValue object and returns the code for the object
        /// </summary>
        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[2] { null, null };
        }
    }
}

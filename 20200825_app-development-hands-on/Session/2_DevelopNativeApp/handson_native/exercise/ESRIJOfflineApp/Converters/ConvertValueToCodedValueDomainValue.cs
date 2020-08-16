using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using Esri.ArcGISRuntime.Data;

namespace ESRIJOfflineApp.Converters
{
    class ConvertValueToCodedValueDomainValue : IMultiValueConverter
    {
        /// <summary>
        /// Takes in a list of CodedValue objects and a code and returns the CodedValue object represented by that code
        /// </summary>
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // values[0] is the list of all the CodedValue objects available for that field
            // values[1] is the code for the actual CodedValue of the field 
            if (values[0] != null && values[0] is IReadOnlyList<CodedValue> && values[1] != null)
            {
                var CodedValues = values[0] as IReadOnlyList<CodedValue>;
                return CodedValues.Where(x => x.Code.ToString() == values[1].ToString()).Select(x => x).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// Takes in a CodedValue object and returns the code for the object
        /// </summary>
        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            // object[2] is an array of 2 objects because ConvertBack expects to return a multivalue
            // but only the actual code is needed so the first value is set to null
            if (value != null && value is CodedValue)
            {
                return new object[2] { null, ((CodedValue)value).Code };
            }
            return new object[2] { null, null };
        }
    }
}

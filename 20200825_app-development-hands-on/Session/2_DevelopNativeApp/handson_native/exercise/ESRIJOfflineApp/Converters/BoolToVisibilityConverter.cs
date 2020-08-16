using System;
using System.Windows.Data;
using System.Windows;
using CustomCultureInfo = System.Globalization.CultureInfo;

namespace ESRIJOfflineApp.Converters
{
    class BoolToVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CustomCultureInfo culture)
        {
            if (value is bool)
            {
                // Handle bool(true) to visibility and bool(false) (inverse) to visibility
                if (parameter != null && parameter.ToString() == "Inverse")
                {
                    //if value is false, visibility = visible (inverse)
                    return ((bool)value == false) ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    //if value is false, visibility is collapsed 
                    return ((bool)value == false) ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            else
                return Visibility.Collapsed;
        }

        /// <summary>
        /// Handle the conversion from a visibility value to a boolean value
        /// </summary>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CustomCultureInfo culture)
        {
            if (value is Visibility)
            {
                // Handle visibility to boolean conversion
                if (parameter != null && parameter.ToString() == "Inverse")
                {
                    //if visibility is collapsed return true, otherwise false (inverse)
                    return ((Visibility)value == Visibility.Collapsed);
                }
                else
                {
                    //if visibility is collapsed return false, otherwise true
                    return ((Visibility)value != Visibility.Collapsed);
                }
            }
            else
                return false;
        }
    }
}

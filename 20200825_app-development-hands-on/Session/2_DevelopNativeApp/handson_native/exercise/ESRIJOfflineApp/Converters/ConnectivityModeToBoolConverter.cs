using ESRIJOfflineApp.Models;
using System;
using System.Windows.Data;
using System.Windows;
using CustomCultureInfo = System.Globalization.CultureInfo;

namespace ESRIJOfflineApp.Converters
{
    class ConnectivityModeToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Handle the conversion from an ConnectivityMode value to a visibility value
        /// </summary>
        object IValueConverter.Convert(object value, Type targetType, object parameter, CustomCultureInfo culture)
        {
            if (value is ConnectivityMode)
            {
                // Handle ConnectivityMode.Online to visibility and ConnectivityMode.Offline (inverse) to visibility
                if (parameter != null && parameter.ToString() == "Inverse")
                {
                    //if value is ConnectivityMode.Offline, visibility = visible (inverse)
                    return ((ConnectivityMode)value == ConnectivityMode.Offline) ? true : false;
                }
                else
                {
                    //if value is ConnectivityMode.Offline, visibility is collapsed 
                    return ((ConnectivityMode)value == ConnectivityMode.Offline) ? false : true;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Handle the conversion from a visibility value to a ConnectivityMode value
        /// </summary>
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CustomCultureInfo culture)
        {
            if (value is bool)
            {
                // Handle visibility to ConnectivityMode conversion
                if (parameter != null && parameter.ToString() == "Inverse")
                {
                    //if visibility is collapsed return ConnectivityMode.Online, otherwise ConnectivityMode.Offline (inverse)
                    return ((bool)value == false) ? ConnectivityMode.Online : ConnectivityMode.Offline;
                }
                else
                {
                    //if visibility is collapsed return ConnectivityMode.Offline, otherwise ConnectivityMode.Online
                    return ((bool)value != true) ? ConnectivityMode.Offline : ConnectivityMode.Online;
                }
            }
            else
                return false;
        }
    }
}

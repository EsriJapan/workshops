using Esri.ArcGISRuntime.Data;
using ESRIJOfflineApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ESRIJOfflineApp
{
    class AttributeEditorDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (element != null && item != null && item is FieldContainer popupFieldValue)
            {
                if (popupFieldValue.OriginalField.FieldType == FieldType.Date)
                {
                    return element.FindResource("DateTemplate") as DataTemplate;
                }
                else if (popupFieldValue.OriginalField.Domain != null && popupFieldValue.OriginalField.Domain is CodedValueDomain)
                {
                    return element.FindResource("CodedValueDomainTemplate") as DataTemplate;
                }
                else if (popupFieldValue.OriginalField.Domain != null && popupFieldValue.OriginalField.Domain is RangeDomain<int>)
                {
                    return element.FindResource("IntegerRangeDomainTemplate") as DataTemplate;
                }
                else if (popupFieldValue.OriginalField.Domain != null && popupFieldValue.OriginalField.Domain is RangeDomain<float>)
                {
                    return element.FindResource("DoubleRangeDomainTemplate") as DataTemplate;
                }
                else if (popupFieldValue.OriginalField.FieldType == FieldType.Float32 || popupFieldValue.OriginalField.FieldType == FieldType.Float64)
                {
                    return element.FindResource("DoubleTemplate") as DataTemplate;
                }
                else if (popupFieldValue.OriginalField.FieldType == FieldType.Int16 || popupFieldValue.OriginalField.FieldType == FieldType.Int32)
                {
                    if(popupFieldValue.PopupFieldValue.FormattedValue == popupFieldValue.PopupFieldValue.OriginalValue.ToString())
                    {
                        return element.FindResource("IntTemplate") as DataTemplate;
                    }
                    else
                    {
                        return element.FindResource("SubtypeTemplate") as DataTemplate;
                    }
                }
                else
                {
                    return element.FindResource("StringTemplate") as DataTemplate;
                }
            }
            return null;
        }

        public static bool TryParse<T>(string str, out T outval)
        {
            outval = default(T);    //0
            if (string.IsNullOrEmpty(str)) return true;

            try
            {
                var conv = TypeDescriptor.GetConverter(typeof(T));
                if (conv == null) return false;
                outval = (T)conv.ConvertFromString(str);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

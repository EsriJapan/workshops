using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping.Popups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESRIJOfflineApp.Models
{
    public struct FieldContainer
    {
        public PopupFieldValue PopupFieldValue { get; set; }
        public Esri.ArcGISRuntime.Data.Field OriginalField { get; set; }
        public string SubtypeField { get; set; }

        public static IEnumerable<FieldContainer> GetFields(PopupManager popupManager)
        {
            

            /// <summary>
            /// Gets the underlying Field property for the PopupField in order to retrieve FieldType and Domain
            /// This is a workaround until Domain and FieldType are exposed on the PopupManager
            /// </summary>
            if (popupManager != null)
            {
                string subtypeField = "";
                // サブタイプ対応
                foreach (var f in popupManager.EditableDisplayFields)
                {
                    if (f.FormattedValue != null && f.OriginalValue != null)
                    {
                        foreach (var ff in ((Feature)popupManager.Popup.GeoElement).FeatureTable.Fields)
                        {
                            if (f.Field.FieldName == ff.Name && ff.Domain == null && f.FormattedValue != f.OriginalValue.ToString())
                            {
                                if (ff.FieldType == FieldType.Int16 || ff.FieldType == FieldType.Int32)
                                {
                                    subtypeField = f.FormattedValue;
                                    break;
                                }
                            }
                        }
                    }                   
                }

                return popupManager.EditableDisplayFields.Join(((Feature)popupManager.Popup.GeoElement).FeatureTable.Fields, i =>
                i.Field.FieldName, i => i.Name, (i, j) => new FieldContainer() { PopupFieldValue = i, OriginalField = j, SubtypeField = subtypeField });
            }
            return null;
        }
    }
}

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESRIJOfflineApp.Models
{
    public class MapDataStore
    {
        private static Map _onlineMap;
        public Map OnlineMap
        {
            get { return _onlineMap; }
            set { _onlineMap = value; }
        }

        private static Map _offlineMap;
        public Map OfflineMap
        {
            get { return _offlineMap; }
            set { _offlineMap = value; }
        }
    }
}

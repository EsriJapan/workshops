using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;


using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;

namespace sample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        // ArcGIS Online フィーチャ レイヤーサービスの URL  
        private const string FEATURELAYER_SERVICE_URL = "https://services.arcgis.com/wlVTGRSYTzAbjjiC/arcgis/rest/services/urayasushi_hoikuen_yochien/FeatureServer";
        private FeatureLayer featureLayer;
        private Map myMap;

        private SyncGeodatabaseParameters syncParams;

        public MainWindow()
        {
            InitializeComponent();

            Initialize();
        }

        public void Initialize()
        {
            myMap = new Map();

            TileCache tileCache = new TileCache(@"D:\workshops\offlinemap-app-hands-on\samples\SampleData\public_map.tpk");
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tileCache);

            LayerCollection baseLayers = new LayerCollection();
            baseLayers.Add(tiledLayer);
            myMap.Basemap.BaseLayers = baseLayers;

            // 主題図の表示
            addFeatureLayer();
            
            MyMapView.Map = myMap;

        }

        /**
        * 主題図の表示をする
        **/
        public void addFeatureLayer()
        {
            // 主題図用のフィーチャ レイヤー（フィーチャ サービス）の表示
            // フィーチャ サービスの URL を指定してフィーチャ テーブル（ServiceFeatureTable）を作成する
            // フィーチャ サービスの URL はレイヤー番号（〜/FeatureServer/0）まで含める
            var serviceUri = new Uri(FEATURELAYER_SERVICE_URL + "/0");
            ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceUri);
            // フィーチャ テーブルからフィーチャ レイヤーを作成
            featureLayer = new FeatureLayer(featureTable);
            // マップにフィーチャ レイヤーを追加
            myMap.OperationalLayers.Add(featureLayer);
        }

    }
}

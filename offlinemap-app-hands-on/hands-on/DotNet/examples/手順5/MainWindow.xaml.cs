using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

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

            MyMapView.Map = myMap;

            MyMapView.GeoViewTapped += OnMapViewTapped;

            // PC内の geodatabase ファイル作成パスを取得する
            getGeodatabasePath();

        }

        private void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // get the click point in geographic coordinates
                var mapClickPoint = e.Location;
                addPoint(mapClickPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sample error", ex.ToString());
            }
        }

        ////////////////////////////////////////////////////////////////
        // 追加
        ////////////////////////////////////////////////////////////////
        /**
         * 新しいポイントを追加する
         * From touch eventから
         **/
        private void addPoint(MapPoint structureLocation)
        {
            MapPoint wgs84Point = (MapPoint)GeometryEngine.Project(structureLocation, SpatialReferences.Wgs84);
            addFeature(wgs84Point);
        }

        /**
         * ローカルgeodatabaseにポイントを追加する
         **/
        private async void addFeature(MapPoint pPoint)
        {
            if (!mGdbFeatureTable.CanAdd())
            {
                // Deal with indicated error
                return;
            }

            // 項目にデータを入れる
            var attributes = new Dictionary<string, object>();
            attributes.Add("name", "ESRIジャパンnow！");

            Feature addedFeature = mGdbFeatureTable.CreateFeature(attributes, pPoint);

            await mGdbFeatureTable.AddFeatureAsync(addedFeature);

            FeatureQueryResult results = await mGdbFeatureTable.GetAddedFeaturesAsync();

            foreach (var r in results)
            {
                Console.WriteLine("add point geodatabase : '" + r.Attributes["name"]);
            }
        }
 

        ////////////////////////////////////////////////////////////////////////////////////////
        // 端末ローカルのパスまわり
        ////////////////////////////////////////////////////////////////////////////////////////
        /**
        * geodatabaseファイル作成のパスを取得する
        **/
        private String mGeodatabasePath;
        private void getGeodatabasePath()
        {
            // カレントディレクトリの取得
            string stCurrentDir = System.Environment.CurrentDirectory;

            // カレントディレクトリを表示する
            //MessageBox.Show(stCurrentDir);

            mGeodatabasePath = stCurrentDir + "\\" + "orglayer.geodatabase";
        }
        
        private void OnDonwloadButton(object sender, RoutedEventArgs e)
        {
            // すでにランタイムコンテンツが作成されているかチェックする
            chkGeodatabase();
        }

        /**
        * ローカルファイルをMapViewへ追加する
        * */
        private void chkGeodatabase()
        {
            // カレントディレクトリの取得
            string stCurrentDir = System.Environment.CurrentDirectory;

            mGeodatabasePath = stCurrentDir + "\\" + "orglayer.geodatabase";

            if (System.IO.File.Exists(mGeodatabasePath))
            {
                // 存在する場合は、既存のgeodatabaseから読み込む
                readGeoDatabase();
            }
            else
            {
                // ファイル作成メソッドをcallする
                createGeodatabaseSyncTask();
            }
        }

        /**
         * 既存 GeoDatabase から読み込む
         ****/
        private GeodatabaseFeatureTable mGdbFeatureTable;
        private FeatureLayer mFeatureLayer;
        private Geodatabase geodatabase;
        private async void readGeoDatabase()
        {
            geodatabase = await Geodatabase.OpenAsync(mGeodatabasePath);

            if (geodatabase.GeodatabaseFeatureTables.Count > 0)
            {
                // データベース内の最初のテーブルを取得する
                mGdbFeatureTable = geodatabase.GeodatabaseFeatureTables.FirstOrDefault();

                await mGdbFeatureTable.LoadAsync();

                if (mGdbFeatureTable.LoadStatus == LoadStatus.Loaded)
                {
                    mFeatureLayer = new FeatureLayer(mGdbFeatureTable);

                    myMap.OperationalLayers.Add(mFeatureLayer);
                }
            }
        }

        ////////////////////////////////////////////////////////////////
        // ローカルフォルダにランタイムコンテンツ(*.geodatabase)作成
        // 【概要】
        //　① 同期させたいArcGIS Online の Feature Layer でタスクを作成する
        //　② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
        //　③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
        ////////////////////////////////////////////////////////////////

        private GeodatabaseSyncTask geodatabaseSyncTask;
        private GenerateGeodatabaseParameters generateParams;
        private GenerateGeodatabaseJob generateJob;
        /**
         * GeoDatabaseを新規に作成する
         * ① 同期させたいArcGIS Online の Feature Layer でタスクを作成する
         ****/
        private async void createGeodatabaseSyncTask()
        {
            // TODO 同期させたいレイヤーで geodatabase 作成 タスクオブジェクトを作成する
            var featureServiceUri = new Uri(FEATURELAYER_SERVICE_URL);
            geodatabaseSyncTask = await GeodatabaseSyncTask.CreateAsync(featureServiceUri);

            // ② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
            generateGeodatabaseParameters();
        }

        /**
         * GeoDatabaseを新規に作成する
         * ② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
         **/
        private async void generateGeodatabaseParameters()
        {
            // TODO geodatabase 作成のためのパラメータを取得する
            Envelope extent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
            generateParams = await geodatabaseSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // TODO レイヤーごとに同期を設定する 
            generateParams.SyncModel = SyncModel.Layer;

            // TODO 添付ファイルは返さない
            generateParams.ReturnAttachments = false;

            // ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
            generateGeodatabase();
        }

        /**
         * GeoDatabaseを新規に作成する
         * ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
         **/
        private void generateGeodatabase()
        {
            // TODO geodatabaseファイル作成ジョブオブヘジェクトを作成する
            generateJob = geodatabaseSyncTask.GenerateGeodatabase(generateParams, mGeodatabasePath);

            // JobChanged イベントを処理してジョブのステータスをチェックする
            generateJob.JobChanged += (s, e) =>
            {
                // report error (if any)
                if (generateJob.Error != null)
                {
                    Console.WriteLine("Error creating geodatabase: " + generateJob.Error.Message);
                    return;
                }

                // check the job status
                if (generateJob.Status == JobStatus.Succeeded)
                {
                    // ジョブが成功した場合はローカルデータをマップに追加する
                    readGeoDatabase();
                }
                else if (generateJob.Status == JobStatus.Failed)
                {
                    // report failure
                    Console.WriteLine("Unable to create local geodatabase.");
                }
                else
                {
                    // job is still running, report last message
                    Console.WriteLine(generateJob.Messages[generateJob.Messages.Count - 1].Message);
                }

            };

            generateJob.ProgressChanged += ((object sender, EventArgs e) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    MyProgressBar.Value = generateJob.Progress / 1.0;
                });
            });

            // ジョブを開始し、ジョブIDをコンソール上に表示
            generateJob.Start();

            Console.WriteLine("Submitted job #" + generateJob.ServerJobId + " to create local geodatabase");
        }

    }
}

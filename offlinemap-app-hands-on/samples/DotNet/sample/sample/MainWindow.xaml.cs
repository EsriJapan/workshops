using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
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
        private const string FEATURELAYER_SERVICE_URL = "https://services7.arcgis.com/903opF9LxIC4unCH/arcgis/rest/services/yokohamaTripPoint/FeatureServer/";

        private Map myMap;

        private SyncGeodatabaseParameters syncParams;


        public MainWindow()
        {
            InitializeComponent();

            Initialize();

        }

        public void Initialize()
        {
            myMap = new Map(BasemapType.LightGrayCanvas, 35.3312442, 139.6202471, 10);

            MyMapView.Map = myMap;

            MyMapView.GeoViewTapped += OnMapViewTapped;

            // PC内の geodatabase ファイル作成パスを取得する
            getGeodatabasePath();

            // すでにランタイムコンテンツが作成されているかチェックする
            chkGeodatabase();

            createGeodatabaseSyncTask();
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
         * */
        private void addPoint(MapPoint structureLocation)
        {

            MapPoint wgs84Point = (MapPoint)GeometryEngine.Project(structureLocation, SpatialReferences.Wgs84);

            addFeature(wgs84Point);

        }

        /**
         * ローカルgeodatabaseにポイントを追加する
         * */
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
        private Geodatabase geodatabase;
        private String mGeodatabasePath;

        /**
         * GeoDatabaseを新規に作成する
         * ① 同期させたいArcGIS Online の Feature Layer でタスクを作成する
         * ***/
        private async void createGeodatabaseSyncTask()
        {

            var featureServiceUri = new Uri(FEATURELAYER_SERVICE_URL);
            geodatabaseSyncTask = await GeodatabaseSyncTask.CreateAsync(featureServiceUri);

            // ② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
            generateGeodatabaseParameters();

        }

        /**
         * GeoDatabaseを新規に作成する
         * ② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
         * */
        private async void generateGeodatabaseParameters()
        {
            // geodatabase 作成のためのパラメータを取得する
            Envelope extent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
            generateParams = await geodatabaseSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // レイヤーごとに同期を設定する 
            generateParams.SyncModel = SyncModel.Layer;

            // 添付ファイルは返さない
            generateParams.ReturnAttachments = false;

            // ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
            generateGeodatabase();

        }

        /**
         * GeoDatabaseを新規に作成する
         * ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
         * */
        private void generateGeodatabase()
        {

            // geodatabaseファイル作成ジョブオブヘジェクトを作成する
            generateJob = geodatabaseSyncTask.GenerateGeodatabase(generateParams, mGeodatabasePath);


            // JobChanged イベントを処理してジョブのステータスをチェックする
            generateJob.JobChanged += OnGenerateJobChanged;


            // ジョブを開始し、ジョブIDをコンソール上に表示
            generateJob.Start();
            Console.WriteLine("Submitted job #" + generateJob.ServerJobId + " to create local geodatabase");

        }

        // JobChangedイベントのハンドラ
        private void OnGenerateJobChanged(object sender, EventArgs e)
        {
            // get the GenerateGeodatabaseJob that raised the event
            var job = sender as GenerateGeodatabaseJob;


            // report error (if any)
            if (job.Error != null)
            {
                Console.WriteLine("Error creating geodatabase: " + job.Error.Message);
                return;
            }

            // check the job status
            if (job.Status == JobStatus.Succeeded)
            {
                // ジョブが成功した場合はローカルデータをマップに追加する
                readGeoDatabase();
            }
            else if (job.Status == JobStatus.Failed)
            {
                // report failure
                Console.WriteLine("Unable to create local geodatabase.");
            }
            else
            {
                // job is still running, report last message
                Console.WriteLine(job.Messages[job.Messages.Count - 1].Message);
            }
        }


        /**
         * 既存GeoDatabaseから読み込む
         ****/
        private GeodatabaseFeatureTable mGdbFeatureTable;
        private FeatureLayer mFeatureLayer;

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

        ////////////////////////////////////////////////////////////////////////////////////////
        // 端末ローカルのパスまわり
        ////////////////////////////////////////////////////////////////////////////////////////
        /**
         * geodatabaseファイル作成のパスを取得する
         * */
        private void getGeodatabasePath()
        {
            // カレントディレクトリの取得
            string stCurrentDir = System.Environment.CurrentDirectory;

            // カレントディレクトリを表示する
            //MessageBox.Show(stCurrentDir);

            mGeodatabasePath = stCurrentDir + "\\" + "orglayer.geodatabase";

        }

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

        ////////////////////////////////////////////////////////////////
        // 同期
        ////////////////////////////////////////////////////////////////
        /**
         * サーバー(AGOL)と同期する
         * ① 同期タスクを作成する
         * ② 同期パラメータを取得する
         * */
        private async void OnButtonClick(object sender, RoutedEventArgs e)
        {

            // 同期したいレイヤーでタスクオブジェクトを作成する
            geodatabaseSyncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(FEATURELAYER_SERVICE_URL));

            readGeoDatabase();

            // タスクオブジェクトから同期するためのパラメータを作成する
            syncParams = await geodatabaseSyncTask.CreateDefaultSyncGeodatabaseParametersAsync(geodatabase);

            // パラーメータを使用してgeodatabaseを同期する
            syncGeodatabase();

        }

        /**
        * サーバー(AGOL)と同期する
        * ③ 同期ジョブを作成する
        * ④ 同期する
        * */
        private SyncGeodatabaseJob syncJob;
        private void syncGeodatabase()
        {

            // 同期ジョブオブヘジェクトを作成する
            syncJob = geodatabaseSyncTask.SyncGeodatabase(syncParams, geodatabase);

            syncJob.JobChanged += (s, e) =>
            {
                // 同期ジョブが終了したときのステータスを検査する
                if (syncJob.Status == JobStatus.Succeeded)
                {
                    // 同期完了から返された値を取得する
                    var result = syncJob.GetResultAsync();
                    if (result != null)
                    {
                        // 同期結果を確認して、例えばユーザに通知する処理を作成します
                        ShowStatusMessage(result.Status.ToString());
                    }
                }
                else if (syncJob.Status == JobStatus.Failed)
                {
                    // エラーの場合
                    ShowStatusMessage(syncJob.Error.Message);
                }
                else
                {
                    var statusMessage = "";
                    var m = from msg in syncJob.Messages select msg.Message;
                    statusMessage += ": " + string.Join<string>("\n", m);

                    Console.WriteLine(statusMessage);

                }
            };

            syncJob.ProgressChanged += ((object sender, EventArgs e) =>
            {
                UpdateProgressBar();
            });

            // geodatabase 同期のジョブを開始します
            syncJob.Start();

        }

        private void ShowStatusMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void UpdateProgressBar()
        {
            this.Dispatcher.Invoke(() =>
            {
                MyProgressBar.Value = syncJob.Progress / 1.0;
            });
        }

    }
}

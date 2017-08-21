using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using System.Drawing;

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;

namespace sample
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        
        // フィーチャーレイヤの定義
        private FeatureLayer shelterLayer;

        // 最寄り施設の検出解析結果の表示用グラフィックスオーバーレイ
        private GraphicsOverlay myGraphicsOverlay = new GraphicsOverlay();

        // 最寄り施設の検出解析
        private ClosestFacilityTask closestFacilityTask;
        private ClosestFacilityParameters closestFacilityParameters;

        // 検索結果のフィーチャのリスト
        private List<Feature> facilities = new List<Feature>();

        private Credential credential = null;

        public MainWindow()
        {
            InitializeComponent();

            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateKnownCredentials);

            Initialize();

        }

        private async void Initialize()
        {

            // Web マップの Id を指定してマップを作成
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            PortalItem webmapItem = await PortalItem.CreateAsync(portal, "285f619f75e64d3681ba101b006d2f65");
            
            Map myMap = new Map(webmapItem);
            await myMap.LoadAsync();

            // マップビューのマップに設定 
            MyMapView.Map = myMap;


            // マップがロードされた際の処理
            if (MyMapView.Map.LoadStatus == LoadStatus.Loaded)
            {
                                            
                // 最寄り施設の検出解析の設定
                var uri = new Uri("https://route.arcgis.com/arcgis/rest/services/World/ClosestFacility/NAServer/ClosestFacility_World");
                closestFacilityTask = await ClosestFacilityTask.CreateAsync(uri);
                closestFacilityTask.Credential = credential;

                // パラメータの設定
                closestFacilityParameters = await closestFacilityTask.CreateDefaultParametersAsync();
                closestFacilityParameters.ReturnRoutes = true;
                closestFacilityParameters.OutputSpatialReference = MyMapView.SpatialReference;

                // マップビューのタップ イベントを登録
                MyMapView.GeoViewTapped += OnMapViewTapped;

                // 検索対象のレイヤーの取得
                foreach (FeatureLayer featureLayer in MyMapView.Map.OperationalLayers)
                {
                    if (featureLayer.Name == "室蘭市 - 避難場所")
                    {
                        shelterLayer = featureLayer;
                    }
                }

                // マップビューにグラフィック表示用のオーバレイを追加
                MyMapView.GraphicsOverlays.Add(myGraphicsOverlay);

            }

        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs evt)
        {
            try
            {

                // グラフィック オーバレイに追加したグラフィックを削除
                myGraphicsOverlay.Graphics.Clear();
                
                // シンボルの作成
                var incidentPointSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 8);
                var outLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(100, 255, 183, 51), 2);
                var bufferPolygonSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.FromArgb(75, 255, 183, 51), outLineSymbol);

                // クリック地点をマップに表示
                var point = evt.Location;
                var location = new Graphic(point, incidentPointSymbol);

                myGraphicsOverlay.Graphics.Add(location);

                // クリック地点を解析のパラメーターに設定
                var incidentOneLocation = new MapPoint(evt.Location.X, evt.Location.Y, MyMapView.SpatialReference);
                var incidentOne = new Incident(incidentOneLocation);

                var incidentList = new List<Incident>();
                incidentList.Add(incidentOne);

                closestFacilityParameters.SetIncidents(incidentList);

                // タップした地点から1000メートルのバッファーの円を作成し、グラフィックとして表示する
                var buffer = GeometryEngine.Buffer(evt.Location, 1000);
                var graphic = new Graphic(buffer, null, bufferPolygonSymbol);

                myGraphicsOverlay.Graphics.Add(graphic);

                // フィーチャの検索用のパラメーターを作成
                var queryParams = new QueryParameters();
                // 検索範囲を作成したバファーの円に指定
                queryParams.Geometry = buffer;

                // 検索範囲とフィーチャの空間的な関係性を指定（バファーの円の中にフィーチャが含まれる）
                queryParams.SpatialRelationship = SpatialRelationship.Contains;
                // フィーチャの検索を実行
                FeatureQueryResult queryResult = await shelterLayer.FeatureTable.QueryFeaturesAsync(queryParams);

                // 検索結果のフィーチャのリストを取得
                var queryList =  queryResult.ToList();
                var facilities = new List<Facility>();

                for (int i = 0; i < queryList.Count; ++i)
                {
                    facilities.Add(new Facility((MapPoint)queryList[i].Geometry));
                }

                // パラメーターを設定し、最寄り施設検出解析を実行
                closestFacilityParameters.SetFacilities(facilities);
                closestFacility();

            }
            catch (Exception ex)
            {
                MessageBox.Show("解析の実行エラー " + ex.Message);

            }

        }

        private async void closestFacility()
        {
            // クエリの実行
            ClosestFacilityResult solveResult = await closestFacilityTask.SolveClosestFacilityAsync(closestFacilityParameters);

            var routePolylineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.FromArgb(100, 89, 95, 35), 4);

            // 解析の実行
            for (int incidentIndex = 0; incidentIndex < solveResult.Incidents.Count; incidentIndex++)
            {
                IReadOnlyList<int> rankedFacilitiesIndexes = solveResult.GetRankedFacilityIndexes(incidentIndex);
                foreach (var facilityIndex in rankedFacilitiesIndexes)
                {
                    ClosestFacilityRoute closestFacilityRoute = solveResult.GetRoute(facilityIndex, incidentIndex);
                    Graphic RouteGraphics = new Graphic(closestFacilityRoute.RouteGeometry, routePolylineSymbol);
                    // 結果を表示
                    myGraphicsOverlay.Graphics.Add(RouteGraphics);
                }
            }
        }

        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            try
            {

                string username = "***********";
                string password = "***********";

                credential = await AuthenticationManager.Current.GenerateCredentialAsync
                                        (info.ServiceUri,
                                         username,
                                         password,
                                         info.GenerateTokenOptions);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Access to " + info.ServiceUri.AbsoluteUri + " denied. " + ex.Message, "Credential Error");
            }

            return credential;

        }
    }
}

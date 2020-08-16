using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Offline;
using ESRIJOfflineApp.BindingSupport;
using ESRIJOfflineApp.Commands;
using ESRIJOfflineApp.Models;
using ESRIJOfflineApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace ESRIJOfflineApp.ViewModels
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MapViewModel : BaseViewModel
    {

        #region App.config の値を取得
        // - 認証するポータルの URL
        private string _serverUrl = ConfigurationManager.AppSettings["ServerUrl"];
        // - サーバに登録したクライアント ID.
        private string AppClientId = ConfigurationManager.AppSettings["AppClientId"];
        // - アプリケーションの秘密キー
        private string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        // - リダイレクトされる URL.
        private string OAuthRedirectUrl = ConfigurationManager.AppSettings["OAuthRedirectUrl"];
        // - サーバにホスティングしている Web マップアイテムの ID.
        private string WebMapId = ConfigurationManager.AppSettings["WebMapId"];
        // - ダウンロードしたオフラインデータの保存先.
        private string _offlineDataFolder = ConfigurationManager.AppSettings["OfflineDataFolder"];
        // - アプリケーションのモード（Online or Offline）.
        private string _mode = ConfigurationManager.AppSettings["Mode"];
        // - 最後に表示していたオフラインデータの名前.
        private string _offlineDataName = ConfigurationManager.AppSettings["OfflineDataName"];
        #endregion

        #region クラス変数
        private PopupManager _popupManager;
        private Feature _feature;
        private OfflineMapTask _offlineMapTask;
        private MobileMapPackage _mobileMapPackage;
        // 編集可能なレイヤー
        private readonly ReadOnlyCollection<string> _isUpdateEnabledLayers = new ReadOnlyCollection<string>(new List<string>() { "漏水" });
        private Esri.ArcGISRuntime.Location.Location _lastLocation;
        public static ListView IdentifiedFeatureListView;
        public static Button SaveBtn;
        private Configuration _config;
        #endregion

        #region 共通プロパティを扱うためのインスタンス
        private OfflineAreaPanelModel _offlineAreaPanelModel = new OfflineAreaPanelModel();
        private OAuthModel oAuthModel = new OAuthModel();
        private MapDataStore _mapDataStore = new MapDataStore();
        #endregion


        #region コンストラクタ
        public MapViewModel()
        {
            // identify controller の初期化
            IdentifyController = new IdentifyController();
            IdentifyController.IdentifyCompleted += IdentifyController_IdentifyCompleted;

            _offlineAreaPanelModel.PropertyChanged += RaisePropertyChanged;

            // Web マップのインスタンス作成
            _mapDataStore.OnlineMap = new Map(new Uri(WebMapId));

            // 現在のモード取得
            ConnectivityMode = _mode == "Online" ? ConnectivityMode.Online : ConnectivityMode.Offline;

            // アプリケーションのモードによって処理を変更
            if (_mode != "Offline")
            {
                // オンラインモード
                oAuthModel.SetOAuthInfo();

                // マップの初期化
                InitializeOnline();
            }
            else
            {
                // オフラインモード
                InitializeOffline();
            }
        }
        #endregion

        /// <summary>
        /// イニシャライズ
        /// </summary>
        private async void InitializeOnline()
        {
            try
            {
                // ライセンスキーを登録して Lite ライセンスの認証を行う
                //string licenseKey = "runtimelite,1000,rud8329497369,none,TRB3LNBHPB6J2T8AG010";
                //ArcGISRuntimeEnvironment.SetLicense(licenseKey);

                await LoadMaps();

                // 基準縮尺をなくす
                Map.ReferenceScale = 0;

                OperationalLayers = Map.OperationalLayers.Reverse();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
                Application.Current.Shutdown();
            }
        }

        private async void InitializeOffline()
        {
            try
            {
                await LoadOfflineMaps();

                Map.ReferenceScale = 0;

                OperationalLayers = Map.OperationalLayers.Reverse();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// マップの表示
        /// </summary>
        /// <returns></returns>
        private async Task LoadMaps()
        {            
            Map = _mapDataStore.OnlineMap;
            Map.LoadStatusChanged += Map_LoadStatusChanged;

            // Initialize the location data source for device location
            LocationDataSource = new SystemLocationDataSource();
            LocationDataSource.LocationChanged += (s, l) =>
            {
                _lastLocation = l;
                //IsLocationStarted = true;
            };

            _offlineMapTask = await OfflineMapTask.CreateAsync(Map);
            IReadOnlyList<PreplannedMapArea> preplannedAreas = await _offlineMapTask.GetPreplannedMapAreasAsync();

            foreach (PreplannedMapArea area in preplannedAreas)
            {
                await area.LoadAsync();
                AreasList.Add(area);
            }
        }

        private async Task LoadOfflineMaps()
        {
            // マップ パッケージをダウンロードするフォルダ パスを作成します。
            string path = System.IO.Path.Combine(_offlineDataFolder, _offlineDataName);
            // 既にダウンロードされている場合は、そのエリアを開きます。
            if (Directory.Exists(path))
            {
                try
                {
                    // ダウンロード済みのモバイルマップパッケージを開きます。
                    _mobileMapPackage = await MobileMapPackage.OpenAsync(path);

                    // Show the first map.
                    _mapDataStore.OfflineMap = _mobileMapPackage.Maps.First();
                    Map = _mapDataStore.OfflineMap;
                    Map.LoadStatusChanged += Map_LoadStatusChanged;

                    // Initialize the location data source for device location
                    LocationDataSource = new SystemLocationDataSource();
                    LocationDataSource.LocationChanged += (s, l) =>
                    {
                        _lastLocation = l;
                        //IsLocationStarted = true;
                    };

                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    MessageBox.Show(e.Message, "Couldn't open offline area. Proceeding to take area offline.");
                }
            }
        }

        #region プロパティ
        /// <summary>
        /// マップを取得または設定します。
        /// </summary>
        public Map Map
        {
            get { return _offlineAreaPanelModel.Map; }
            set { _offlineAreaPanelModel.Map = value; OnPropertyChanged(nameof(Map)); }
        }

        /// <summary>
        /// Gets or sets the current area of interest
        /// </summary>
        private Viewpoint _areaOfInterest;
        public Viewpoint AreaOfInterest
        {
            get { return _areaOfInterest; }
            set
            {
                if (_areaOfInterest != value)
                {
                    _areaOfInterest = value;
                    OnPropertyChanged(nameof(AreaOfInterest));
                }
            }
        }

        /// <summary>
        /// Gets or sets the current location data source
        /// </summary>
        private LocationDataSource _locationDataSource;
        public LocationDataSource LocationDataSource
        {
            get { return _locationDataSource; }
            set
            {
                if (_locationDataSource != value)
                {
                    _locationDataSource = value;
                    OnPropertyChanged(nameof(LocationDataSource));
                }
            }
        }


        /// <summary>
        /// Gets or sets the state of the application (online or offline)
        /// </summary>
        public ConnectivityMode ConnectivityMode
        {
            get { return _offlineAreaPanelModel.ConnectivityMode; }
            set
            {
                if (_offlineAreaPanelModel.ConnectivityMode != value)
                {
                    _offlineAreaPanelModel.ConnectivityMode = value;
                    OnPropertyChanged(nameof(ConnectivityMode));
                }
            }
        }

        /// <summary>
        /// Identify コントローラを取得または設定します。
        /// </summary>
        public IdentifyController IdentifyController { get; private set; }

        /// <summary>
        /// 選択したフィーチャのポップアップマネージャを取得します
        /// </summary>
        public PopupManager PopupManager
        {
            get { return _popupManager; }
            set
            {
                _popupManager = value;
                OnPropertyChanged(nameof(PopupManager));
            }
        }

        /// <summary>
        /// オフラインパネルの Visibility ステータス
        /// </summary>
        public bool OfflinePanaleVisibility
        {
            get { return _offlineAreaPanelModel.OfflinePanaleVisibility; }
            set
            {
                _offlineAreaPanelModel.OfflinePanaleVisibility = value;
                OnPropertyChanged(nameof(OfflinePanaleVisibility));
            }
        }

        /// <summary>
        /// レイヤー一覧の Visibility ステータス
        /// </summary>
        private bool _layerListVisibility;
        public bool LayerListVisibility
        {
            get { return _layerListVisibility; }
            set
            {
                _layerListVisibility = value;
                OnPropertyChanged(nameof(LayerListVisibility));
            }
        }


        /// <summary>
        /// 作図モードの Visiblity ステータス
        /// </summary>
        private bool _NewFeatureOverlayVisiblity;
        public bool NewFeatureOverlayVisiblity
        {
            get { return _NewFeatureOverlayVisiblity; }
            set
            {
                _NewFeatureOverlayVisiblity = value;
                OnPropertyChanged(nameof(NewFeatureOverlayVisiblity));
            }
        }

        /// <summary>
        /// Busyパネルの Visibility ステータス
        /// </summary>
        public bool BusyIndicatorVisibility
        {
            get 
            { return _offlineAreaPanelModel.BusyIndicatorVisibility; }
            set
            {
                _offlineAreaPanelModel.BusyIndicatorVisibility = value;
                OnPropertyChanged(nameof(BusyIndicatorVisibility));
            }
        }

        /// <summary>
        /// オフラインエリアのリスト
        /// </summary>
        public List<PreplannedMapArea> AreasList
        {
            get { return _offlineAreaPanelModel.AreasList; }
            set 
            {
                _offlineAreaPanelModel.AreasList = value; 
                OnPropertyChanged(nameof(AreasList));
            }
        }

        /// <summary>
        /// オフラインエリアの選択した値を取得します。
        /// </summary>
        public PreplannedMapArea MySelectedItem
        {
            get { return _offlineAreaPanelModel.MySelectedItem; }
            set
            {
                _offlineAreaPanelModel.MySelectedItem = value;
                AreasList_SelectionChanged(MySelectedItem);
                OnPropertyChanged(nameof(MySelectedItem));

            }
        }

        /// <summary>
        /// ポップアップタイトル
        /// </summary>
        private string _popupTitle;
        public string PopupTitle
        {
            get { return _popupTitle; }
            set
            {
                _popupTitle = value;
                OnPropertyChanged(nameof(PopupTitle));
            }
        }

        /// <summary>
        /// ポップアップフィールド
        /// </summary>
        private IEnumerable<FieldContainer> _fields;
        public IEnumerable<FieldContainer> Fields
        {
            get { return _fields; }
            set
            {
                _fields = value;
                OnPropertyChanged(nameof(Fields));
            }
        }

        // 定義済みのアタッチメント取得用
        private string _attachmentList;

        /// <summary>
        /// View(画面)へ反映する変数を定義する
        /// </summary>
        public string AttachmentList
        {
            get { return _attachmentList; }
            set
            {
                _attachmentList = value;
                OnPropertyChanged(nameof(AttachmentList));
            }
        }

        // 定義済みのアタッチメント取得用
        private BitmapImage _attachment;

        /// <summary>
        /// View(画面)へ反映する変数を定義する
        /// </summary>
        public BitmapImage Attachment
        {
            get { return _attachment; }
            set
            {
                _attachment = value;
                // 画面に値を反映する
                OnPropertyChanged(nameof(Attachment));
            }
        }
        
        private bool _isEnabled;
        /// <summary>
        /// アイテムを非活性にするプロパティ
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private bool _isEnabledDell;
        /// <summary>
        /// 削除ボタンを非活性にするプロパティ
        /// </summary>
        public bool IsEnabledDell
        {
            get { return _isEnabledDell; }
            set
            {
                _isEnabledDell = value;
                OnPropertyChanged(nameof(IsEnabledDell));
            }
        }

        // 定義済みのアタッチメント取得用
        private IEnumerable<Layer> _operationalLayers;

        /// <summary>
        /// View(画面)へ反映する変数を定義する
        /// </summary>
        public IEnumerable<Layer> OperationalLayers
        {
            get { return _operationalLayers; }
            set
            {
                _operationalLayers = value;
                // 画面に値を反映する
                OnPropertyChanged(nameof(OperationalLayers));
            }
        }
        #endregion

        #region コマンド

        /// <summary>
        /// 作図関連のコマンド
        /// </summary>
        private ICommand _SketchClick;

        public ICommand SketchClick
        {
            get
            {
                return _SketchClick ?? (_SketchClick = new DelegateCommand(
                    (x) =>
                    {
                        //Mouse.OverrideCursor = Cursors.Cross;
                        NewFeatureOverlayVisiblity = true;
                    }));
            }
        }

        /// <summary>
        /// 新規フィーチャ作成
        /// </summary>
        private ICommand _saveNewFeatureCommand;

        public ICommand SaveNewFeatureCommand
        {
            get
            {
                return _saveNewFeatureCommand ?? (_saveNewFeatureCommand = new DelegateCommand(
                    (x) =>
                    {
                        foreach (var layer in Map.OperationalLayers)
                        {
                            if (layer.Name.Contains(_isUpdateEnabledLayers[0]))
                            {
                                IsEnabled = true;
                                IsEnabledDell = false;
                                try
                                {
                                    AttachmentList = null;
                                    Attachment = null;

                                    // create new feature 
                                    _feature = ((FeatureLayer)layer).FeatureTable.CreateFeature();

                                    // set feature geometry as the mapview's center
                                    var newFeatureGeometry = AreaOfInterest?.TargetGeometry.Extent.GetCenter() as MapPoint;
                                    _feature.Geometry = newFeatureGeometry;

                                    PopupManager = new PopupManager(new Popup(_feature, _feature.FeatureTable.PopupDefinition));
                                    PopupTitle = _feature.FeatureTable.DisplayName;
                                    Fields = FieldContainer.GetFields(PopupManager);

                                    PopupManager.StartEditing();

                                    NewFeatureOverlayVisiblity = false;
                                }
                                catch (Exception ex)
                                {
                                    // エラー処理
                                }
                                break;
                            }
                        }
                    }));
            }
        }
        #endregion

        #region Identify パネルのコマンド
        private SaveCommand _editPopupCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public SaveCommand EditPopupCommand
        {
            get
            {
                return _editPopupCommand
                    ?? (_editPopupCommand = new SaveCommand(
                    async () =>
                    {
                        await SavePopup();
                        PopupManager = null;
                    }));
            }
        }

        private SaveCommand _closeCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public SaveCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new SaveCommand(
                    () =>
                    {
                        PopupManager = null;
                    }));
            }
        }

        private SaveCommand _deletePopupCommand;

        /// <summary>
        /// Gets the MyCommand.
        /// </summary>
        public SaveCommand DeletePopupCommand
        {
            get
            {
                return _deletePopupCommand
                    ?? (_deletePopupCommand = new SaveCommand(
                    async () =>
                    {
                        await DeletePopup(_feature.FeatureTable, _feature);
                        PopupManager = null;

                    }));
            }
        }

        /// <summary>
        /// コマンド（ボタン選択時の処理）のための変数
        /// wrote by esrijapan
        /// </summary>
        // アタッチメントをアプリに一時保存するためのコマンド
        private ICommand _attachmentCommand;

        /// <summary>
        /// アタッチメント選択ボタンを押されたときに行う処理
        ///  1.エクスプローラを開く
        ///  2.PopupManagerのAttachmentManagerで、アタッチメントを追加する
        /// </summary>
        public ICommand AttachmentCommand
        {
            get
            {
                return _attachmentCommand ?? (_attachmentCommand = new DelegateCommand(
                    (x) =>
                    {
                        // エクスプローラーを開く
                        System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                        ofd.Title = "JPEGファイルを開く";
                        //複数ファイルを選択できるようにする
                        ofd.Multiselect = false;
                        // [ファイルの種類] ボックスに表示される選択肢を設定する
                        ofd.Filter = "JPEGファイル(*.jpg;*.jpeg)|*.jpg;*.jpeg";

                        //ダイアログを表示
                        string[] files = null;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            //選択されたファイル名を変数にセット
                            files = ofd.FileNames;
                            ////////////////////////////////////////////////////////////////////////
                            // ポップアップマネージャーのアタッチメントマネージャーをのAddAttachment()メソッドを使用してアタッチメントを追加する
                            // ■引数
                            // ①files[0] >file配列の0番目
                            // ②"image/png" > アタッチメント画像の種別
                            PopupManager.AttachmentManager.AddAttachment(files[0], "image/png");
                            ////////////////////////////////////////////////////////////////////////
                            // 表示ファイル名の取得
                            String _addFileName = Path.GetFileName("@" + files[0]);
                            AttachmentList = _addFileName;

                            // サムネイルを表示する
                            var source = new BitmapImage();
                            source.BeginInit();
                            source.UriSource = new Uri(files[0]);
                            source.EndInit();
                            Attachment = source;

                        }
                        else
                        {
                            // 「キャンセル」が選択されたときの処理
                            // なにもしない
                        }
                    }));
            }
        }
        #endregion

        #region Expander コマンド
        /// <summary>
        /// オフラインパネルの起動
        /// </summary>
        private ICommand _ShowOfflinePanel;
        public ICommand ShowOfflinePanel
        {
            get
            {
                return _ShowOfflinePanel ?? (_ShowOfflinePanel = new DelegateCommand(
                    (x) =>
                    {
                        OfflinePanaleVisibility = true;
                    }));
            }
        }

        /// <summary>
        /// レイヤー一覧の起動
        /// </summary>
        private ICommand _showLayerList;
        public ICommand ShowLayerList
        {
            get
            {
                return _showLayerList ?? (_showLayerList = new DelegateCommand(
                    (x) =>
                    {
                        LayerListVisibility = true;
                    }));
            }
        }

        /// <summary>
        /// アプリケーションの終了
        /// </summary>
        private ICommand _CloseApp;
        public ICommand CloseApp
        {
            get
            {
                return _CloseApp ?? (_CloseApp = new DelegateCommand(
                    (x) =>
                    {
                        Application.Current.Shutdown();
                    }));
            }
        }
        #endregion

        #region オフラインパネル コマンド
        private ICommand _ShowOnline;

        public ICommand ShowOnline
        {
            get
            {
                return _ShowOnline ?? (_ShowOnline = new DelegateCommand(
                    async (x) =>
                    {
                        try
                        {
                            await _offlineAreaPanelModel.ShowOnline();
                            OperationalLayers = Map.OperationalLayers.Reverse();
                        }
                        catch (Exception e)
                        {
                            Map = _mapDataStore.OfflineMap;
                            Debug.WriteLine(e);
                        }
                    }));
            }
        }

        private ICommand _DownloadMapArea;

        public ICommand DownloadMapArea
        {
            get
            {
                return _DownloadMapArea ?? (_DownloadMapArea = new DelegateCommand(
                    async  (x) =>
                    {
                        await _offlineAreaPanelModel.DownloadMapArea(_offlineMapTask);
                        OperationalLayers = Map.OperationalLayers.Reverse();
                    }));
            }
        }

        private ICommand _DeleteAllMapAreas;

        public ICommand DeleteAllMapAreas
        {
            get
            {
                return _DeleteAllMapAreas ?? (_DeleteAllMapAreas = new DelegateCommand(
                    async (x) =>
                    {
                        await _offlineAreaPanelModel.DeleteAllMapAreasAsync();
                        OperationalLayers = Map.OperationalLayers.Reverse();
                    }));
            }
        }

        private ICommand _UploadMapAreas;
        public ICommand UploadMapAreas
        {
            get
            {
                return _UploadMapAreas ?? (_UploadMapAreas = new DelegateCommand(
                    (x) =>
                    {
                        _offlineAreaPanelModel.UpdateMapAreasAsync();
                    }));
            }
        }

        private ICommand _CloseOfflineAreaPanel;

        public ICommand CloseOfflineAreaPanel
        {
            get
            {
                return _CloseOfflineAreaPanel ?? (_CloseOfflineAreaPanel = new DelegateCommand(
                    (x) =>
                    {
                        //OfflinePanaleVisibility = null;
                        _offlineAreaPanelModel.CloseOfflineAreaPanel();
                    }));
            }
        }

        private ICommand _closeLayerList;

        public ICommand CloseLayerList
        {
            get
            {
                return _closeLayerList ?? (_closeLayerList = new DelegateCommand(
                    (x) =>
                    {
                        LayerListVisibility = false;
                        OnPropertyChanged(nameof(LayerListVisibility));
                    }));
            }
        }
        #endregion


        private async Task SavePopup()
        {
            await PopupManager.FinishEditingAsync();

            // オンラインかオフラインかで処理を分岐
            if (_feature.FeatureTable is ServiceFeatureTable serviceFeatureTable)
            {
                ServiceFeatureTable featureTable = (ServiceFeatureTable)_feature.FeatureTable;
                await featureTable.ApplyEditsAsync();
            }
            //PopupManager.StartEditing();
        }

        private async Task DeletePopup(FeatureTable featureTable, Feature feature)
        {
            if (featureTable is ServiceFeatureTable serviceFeatureTable)
            {
                await serviceFeatureTable.DeleteFeatureAsync(feature);
                await serviceFeatureTable.ApplyEditsAsync();
            }
            else if (featureTable is GeodatabaseFeatureTable geodatabaseFeatureTable)
            {
                await geodatabaseFeatureTable.DeleteFeatureAsync(feature);
            }
        }


        /// <summary>
        /// マップのロード ステータスが変更されたときに呼び出されます。
        /// IdentifyControllerにターゲットレイヤーを設定するには、マップがロードされるまで待つ必要があります。
        /// </summary>
        private async void Map_LoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            // reload map if an error occured
            if (e.Status == LoadStatus.FailedToLoad)
                await Map.RetryLoadAsync();

            else if (e.Status == LoadStatus.Loaded && IdentifyController != null)
            {

                // 位置情報サービスを起動
                try
                {
                    await LocationDataSource.StartAsync();
                }
                catch
                { // 位置情報サービスを開始できない場合は、何もしないでください、ボタンはただ無効になります。
                }

                //specifying the points layer as the target of the identify operation
                foreach (FeatureLayer layer in Map.OperationalLayers)
                {
                    // load layer if it isn't loaded
                    if (layer.LoadStatus == LoadStatus.NotLoaded)
                        await layer.LoadAsync();
                }
            }
        }

        /// <summary>
        /// Identify 操作が完了したときに呼び出されます。
        /// </summary>
        private void IdentifyController_IdentifyCompleted(object sender, IdentifyEventArgs e)
        {
            //This code only retrieves the first identified result. 
            //TODO: change this to handle all results returned fromt he identified operation (if needed)
            // get first identified layer from the collection 
            if (e.LayerResults.Count > 0)
            {
                var identifyLayerResult = e.LayerResults.First();

                // get the first identified feature
                if (identifyLayerResult.Popups.Count() > 0)
                {
                    // get feature
                    _feature = identifyLayerResult.Popups.First().GeoElement as ArcGISFeature;

                    // set up popup manager
                    if (_feature != null)
                    {
                        PopupManager = new PopupManager(new Popup(_feature, _feature.FeatureTable.PopupDefinition));
                        PopupTitle = _feature.FeatureTable.DisplayName;
                        Fields = FieldContainer.GetFields(PopupManager);

                        // 漏水ポイントのみ編集可能
                        if (PopupTitle.Contains(_isUpdateEnabledLayers[0]))
                        {
                            IsEnabled = true;
                            IsEnabledDell = true;
                        }
                        else
                        {
                            IsEnabled = false;
                            IsEnabledDell = false;
                        }

                        CheckAttachment(_feature);
                        PopupManager.StartEditing();
                    }
                }
            }
            else
            {
                PopupManager = null;
            }
        }

        /// <summary>
        /// フィーチャのアタッチメントの有無をチェックする
        /// 
        /// by esrijapan 
        /// </summary>
        /// <param name="pFeature"></param>
        public async void CheckAttachment(Feature pFeature)
        {
            AttachmentList = null;
            Attachment = null;

            ArcGISFeature arcGISFeature = (ArcGISFeature)pFeature;
            IReadOnlyList<Attachment> attachments = await arcGISFeature.GetAttachmentsAsync();

            if (attachments.Count() > 0)
            {
                // アタッチメントが存在するなら、最初の1つだけを表示する(複数ある場合は"他"をつける)
                String _fullName = attachments.First().Name;

                // サムネイルを表示する
                Stream x = await attachments.First().GetDataAsync();
                var source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = x;
                source.EndInit();
                Attachment = source;

                AttachmentList += _fullName;
                if (attachments.Count() > 1)
                {
                    AttachmentList += "、他";
                }
            }
            else
            {
                //ない場合は、ない旨のメッセージを詰める
                AttachmentList = "添付ファイル無";
            }
        }

        private void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// リストビューで選択されたオフラインエリアのダウンロード状態によって処理変更
        /// </summary>
        /// <param name="MySelectedItem"></param>
        private void AreasList_SelectionChanged(PreplannedMapArea MySelectedItem)
        {
            //// Update the download button to reflect if the map area has already been downloaded.
            //if (Directory.Exists(System.IO.Path.Combine(_offlineDataFolder, (AreasList.SelectedItem as PreplannedMapArea).PortalItem.Title)))
            //{
            //    DownloadButton.Content = "View downloaded area";
            //}
            //else
            //{
            //    DownloadButton.Content = "Download preplanned area";
            //}
        }


        /// <summary>
        /// アプリ起動時にネットワーク状況とモードを考慮して起動する
        /// </summary>
        /// <returns></returns>
        //private async Task<Map> GetMap()
        //{
        //    // オンラインウェブマップへの接続性をテスト
        //    // デバイスが完全にオフラインの場合、これ
        //    try
        //    {
        //        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(WebMapId);
        //        HttpWebResponse response;

        //        using (response = (HttpWebResponse)request.GetResponse())
        //        {
        //            // force app state to offline if the app cannot find the webmap
        //            if (response.StatusCode == HttpStatusCode.NotFound)
        //            {
        //                _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //                _config.AppSettings.Settings["Mode"].Value = "Offline";
        //                _config.Save();
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //        _config.AppSettings.Settings["Mode"].Value = "Offline";
        //        _config.Save();
        //    }

        //    // mmpkを取得し、まだロードされていない場合はそれを取得します。
        //    if (Mmpk == null)
        //    {
        //        try
        //        {
        //            Mmpk = await MobileMapPackage.OpenAsync(DownloadPath);
        //            OfflineMap = Mmpk.Maps.FirstOrDefault();
        //        }
        //        catch
        //        {
        //            // オフラインマップが取得できない場合は、アプリをオンラインモードに切り替えてください。
        //            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //            config.AppSettings.Settings["Mode"].Value = "Online";
        //            config.Save();
        //        }
        //    }

        //    // Choose online or offline map based on app state         
        //    return (_config.AppSettings.Settings["Mode"].Value == "Online") ?
        //        new Map(new Uri(WebMapId)) :
        //        OfflineMap;
        //}
    }
}

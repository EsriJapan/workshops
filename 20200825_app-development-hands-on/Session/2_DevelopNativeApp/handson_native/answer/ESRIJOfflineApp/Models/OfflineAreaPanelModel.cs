using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ESRIJOfflineApp.Models
{
    public class OfflineAreaPanelModel:BaseModel
    {

        #region App.Config の パラメータ取得
        // - ダウンロードしたオフラインデータの保存先.
        private string _offlineDataFolder = ConfigurationManager.AppSettings["OfflineDataFolder"];
        // - サーバにホスティングしている Web マップアイテムの ID.
        private string WebMapId = ConfigurationManager.AppSettings["WebMapId"];
        // - フィーチャサービスの URL.
        private string _featureServiceUrl = ConfigurationManager.AppSettings["FeatureServiceUrl"];
        #endregion

        private OfflineMapTask _offlineMapTask;
        private MobileMapPackage _mobileMapPackage;
        private Geodatabase _localGeodatabase;
        private string _statusMessage;
        private Configuration _config;
        private OAuthModel oAuthModel = new OAuthModel();

        #region 共通プロパティを扱うためのインスタンス
        private MapDataStore _mapDataStore = new MapDataStore();
        #endregion

        #region プロパティ
        public bool OfflinePanaleVisibility { get; set; }
        public bool BusyIndicatorVisibility { get; set; }
        public List<PreplannedMapArea> AreasList { get; set; } = new List<PreplannedMapArea>();
        public Map Map { get; set; }
        public PreplannedMapArea MySelectedItem { get; set; }
        public ConnectivityMode ConnectivityMode { get; set; }
        #endregion

        #region コンストラクタ
        public OfflineAreaPanelModel()
        {

        }
        #endregion

        /// <summary>
        /// オンラインマップ表示イベント
        /// </summary>
        public async Task ShowOnline()
        {
            await ShowOnlineMapAsync();
            Map.ReferenceScale = 0;
            OnPropertyChanged(nameof(Map));
        }

        /// <summary>
        /// オフラインマップダウンロードイベント
        /// </summary>
        /// <param name="offlineMapTask"></param>
        public async Task DownloadMapArea(OfflineMapTask offlineMapTask)
        {
            await DownloadMapAreaAsync(MySelectedItem, offlineMapTask);
            Map.ReferenceScale = 0;
            OnPropertyChanged(nameof(Map));
        }

        /// <summary>
        /// オフラインマップ削除イベント
        /// </summary>
        public async Task DeleteAllMapAreasAsync()
        {
            await DeleteMapAreasAsync(MySelectedItem);

        }

        /// <summary>
        /// オフラインマップのアップロードイベント
        /// </summary>
        public async void UpdateMapAreasAsync()
        {
            await OfflineMapUp(MySelectedItem);
        }

        /// <summary>
        /// オフラインエリアパネルクローズイベント
        /// </summary>
        public void CloseOfflineAreaPanel()
        {
            OfflinePanaleVisibility = false;
            OnPropertyChanged(nameof(OfflinePanaleVisibility));
        }


        /// <summary>
        /// オンラインマップの表示
        /// </summary>
        private async Task ShowOnlineMapAsync()
        {
            oAuthModel.SetOAuthInfo();

            Map = new Map(new Uri(WebMapId));
            //Map = _mapDataStore.OnlineMap;

            // 現在開いているモバイルパッケージを閉じます。
            _mobileMapPackage?.Close();

            _offlineMapTask = await OfflineMapTask.CreateAsync(Map);
            IReadOnlyList<PreplannedMapArea> preplannedAreas = await _offlineMapTask.GetPreplannedMapAreasAsync();

            AreasList.Clear();

            foreach (PreplannedMapArea area in preplannedAreas)
            {
                await area.LoadAsync();
                AreasList.Add(area);
            }

            // 現在のモードをコンフィグに記載
            ConfigSet(ConnectivityMode.Online, "");
        }

        /// <summary>
        /// オフライン プレプランマップダウンロード処理
        /// </summary>
        /// <param name="mapArea"></param>
        /// <param name="offlineMapTask"></param>
        /// <returns></returns>
        private async Task DownloadMapAreaAsync(PreplannedMapArea mapArea, OfflineMapTask offlineMapTask)
        {

            if (mapArea == null)
            {
                MessageBox.Show("ダウンロードするエリアを選択してください。");
                return;
            }

            //// 現在開いているモバイルパッケージを閉じます。
            //_mobileMapPackage?.Close();

            // ダウンロード用のUIを設定します。
            BusyIndicatorVisibility = true;
            OnPropertyChanged(nameof(BusyIndicatorVisibility));

            // マップ パッケージをダウンロードするフォルダ パスを作成します。
            string path = System.IO.Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            // 既にダウンロードされている場合は、そのエリアを開きます。
            if (Directory.Exists(path))
            {
                try
                {
                    // ダウンロード済みのモバイルマップパッケージを開きます。
                    _mobileMapPackage = await MobileMapPackage.OpenAsync(path);

                    // マップを表示.
                    _mapDataStore.OfflineMap = _mobileMapPackage.Maps.First();
                    Map = _mapDataStore.OfflineMap;

                    // UIを更新します。
                    BusyIndicatorVisibility = false;
                    OnPropertyChanged(nameof(BusyIndicatorVisibility));

                    // 現在のモードをコンフィグに記載
                    ConfigSet(ConnectivityMode.Offline, mapArea.PortalItem.Title);
                    return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    MessageBox.Show(e.Message, "Couldn't open offline area. Proceeding to take area offline.");
                }
            }

            // ダウンロードパラメータを作成します。
            DownloadPreplannedOfflineMapParameters parameters = await offlineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync(mapArea);

            // アップデートモードを設定 (https://developers.arcgis.com/net/latest/wpf/api-reference/html/T_Esri_ArcGISRuntime_Tasks_Offline_PreplannedUpdateMode.htm)
            parameters.UpdateMode = PreplannedUpdateMode.SyncWithFeatureServices;

            // ジョブの作成
            DownloadPreplannedOfflineMapJob job = offlineMapTask.DownloadPreplannedOfflineMap(parameters, path);

            try
            {
                // エリアのダウンロード
                DownloadPreplannedOfflineMapResult results = await job.GetResultAsync();

                // モバイルマップパッケージをセットします
                _mobileMapPackage = results.MobileMapPackage;
                _mapDataStore.OfflineMap = _mobileMapPackage.Maps.First();

                // エラーがあれば処理してマップを表示します
                if (results.HasErrors)
                {
                    // Accumulate all layer and table errors into a single message.
                    string errors = "";

                    foreach (KeyValuePair<Layer, Exception> layerError in results.LayerErrors)
                    {
                        errors = $"{errors}\n{layerError.Key.Name} {layerError.Value.Message}";
                    }

                    foreach (KeyValuePair<FeatureTable, Exception> tableError in results.TableErrors)
                    {
                        errors = $"{errors}\n{tableError.Key.TableName} {tableError.Value.Message}";
                    }

                    // Show the message.
                    MessageBox.Show(errors, "Warning!");
                }

                // ダウンロードした地図を表示
                Map = _mapDataStore.OfflineMap;

                // 現在のモードをコンフィグに記載
                ConfigSet(ConnectivityMode.Offline, mapArea.PortalItem.Title);
            }
            catch (Exception ex)
            {
                // Report any errors.
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Downloading map area failed.");
            }
            finally
            {
                BusyIndicatorVisibility = false;
                OnPropertyChanged(nameof(BusyIndicatorVisibility));                
            }
        }

        /// <summary>
        /// オフライン マップをオンラインと同期する
        /// </summary>
        /// <param name="mapArea"></param>
        /// <returns></returns>
        public async Task OfflineMapUp(PreplannedMapArea mapArea)
        {

            if (mapArea == null)
            {
                MessageBox.Show("アップロードするエリアを選択してください。");
                return;
            }

            // マップ パッケージをダウンロードするフォルダ パスを作成します。
            string path = System.IO.Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

            if (Directory.Exists(path))
            {
                // MainWindow  Busy パネル のステータスを変更
                BusyIndicatorVisibility = true;
                OnPropertyChanged(nameof(BusyIndicatorVisibility));

                // ダウンロード済みのモバイルマップパッケージを開きます。
                _mobileMapPackage = await MobileMapPackage.OpenAsync(path);

                // マップオブジェクト作成。
                Map upMap = _mobileMapPackage.Maps.First();

                // 新しいオフライン マップ同期タスクの作成
                OfflineMapSyncTask offlineMapSyncTask = await OfflineMapSyncTask.CreateAsync(upMap);

                // オフライン マップ同期パラメータの作成
                OfflineMapSyncParameters parameters = new OfflineMapSyncParameters
                {
                    RollbackOnFailure = true,
                    SyncDirection = SyncDirection.Bidirectional
                };

                // 同期パラメータを使用して同期ジョブのインスタンスを作成します。
                OfflineMapSyncJob offlineMapSyncJob = offlineMapSyncTask.SyncOfflineMap(parameters);

                // ジョブステータスを通知するためのリスナーを作成
                offlineMapSyncJob.JobChanged += (s, e) =>
                {
                    // ジョブステータスの変更を報告する
                    if (offlineMapSyncJob.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
                    {
                        // 同期成功
                        _statusMessage = "同期が完了しました。";
                        BusyIndicatorVisibility = false;
                        OnPropertyChanged(nameof(BusyIndicatorVisibility));
                        MessageBox.Show(_statusMessage);
                        // 現在開いているモバイルパッケージを閉じます。
                        _mobileMapPackage?.Close();
                    }
                    else if (offlineMapSyncJob.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
                    {
                        //同期失敗
                        _statusMessage = offlineMapSyncJob.Error.Message;
                        BusyIndicatorVisibility = false;
                        OnPropertyChanged(nameof(BusyIndicatorVisibility));
                        MessageBox.Show(_statusMessage);
                        // 現在開いているモバイルパッケージを閉じます。
                        _mobileMapPackage?.Close();
                    }
                    else
                    {
                        _statusMessage = "Sync in progress ...";
                    }
                };

                // 同期開始
                offlineMapSyncJob.Start();
            }
            else
            {
                MessageBox.Show("アップロードするデータがありません。");
            }

        }

        /// <summary>
        /// オフライン プレプランマップの削除処理
        /// </summary>
        /// <param name="mapArea"></param>
        /// <returns></returns>
        private async Task DeleteMapAreasAsync(PreplannedMapArea mapArea)
        {
            if (mapArea == null)
            {
                MessageBox.Show("削除するエリアを選択してください。");
                return;
            }

            try
            {            
                // 削除するマップ パッケージフォルダ パスを作成します。
                string path = System.IO.Path.Combine(_offlineDataFolder, mapArea.PortalItem.Title);

                if (Directory.Exists(path))
                {
                    // MainWindow  Busy パネル のステータスを変更
                    BusyIndicatorVisibility = true;
                    OnPropertyChanged(nameof(BusyIndicatorVisibility));

                    string[] mgdb = Directory.GetFiles(path, "*.geodatabase", SearchOption.AllDirectories);

                    // オンライン フィーチャ サービスを使用して、新しい GeodatabaseSyncTask を作成します。
                    GeodatabaseSyncTask gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(_featureServiceUrl));

                    _localGeodatabase = await Geodatabase.OpenAsync(mgdb[0]);

                    // サービスから生成されたローカル ジオデータベースの登録を解除します。
                    await gdbSyncTask.UnregisterGeodatabaseAsync(_localGeodatabase);
                    _localGeodatabase.Close();

                    // 開いているモバイルマップパッケージを閉じる
                    _mobileMapPackage?.Close();

                    // フォルダに保存されている該当のマップパッケージを削除する
                    Directory.Delete(path, true);

                    // 現在のモードをコンフィグに記載
                    ConfigSet(ConnectivityMode.Online, "");

                    MessageBox.Show("削除が完了しました。");
                }
                else
                {
                    MessageBox.Show("選択されたオフラインデータはありません。");
                    return;
                }                
            }
            catch (Exception ex)
            {
                // エラーメッセージ
                MessageBox.Show(ex.Message, "Deleting map area failed.");
            }
            finally
            {
                BusyIndicatorVisibility = false;
                OnPropertyChanged(nameof(BusyIndicatorVisibility));
            }
        }

        // オンラインサービスとの同期解除処理
        private async Task UnRegisterGdbAsync(string mgdb)
        {
            // オンライン フィーチャ サービスを使用して、新しい GeodatabaseSyncTask を作成します。
            GeodatabaseSyncTask gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(_featureServiceUrl));

            try
            {
                _localGeodatabase = await Geodatabase.OpenAsync(mgdb);
                // サービスから生成されたローカル ジオデータベースの登録を解除します。
                await gdbSyncTask.UnregisterGeodatabaseAsync(_localGeodatabase);
                _localGeodatabase.Close();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Error while unregistering database '" + _localGeodatabase.Path + "'" + exp.Message);
            }
        }

        /// <summary>
        /// AppConfig に現在のモードを記録してオフラインパネルを閉じる
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="title"></param>
        public void ConfigSet(ConnectivityMode mode, string title)
        {
            _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            _config.AppSettings.Settings["Mode"].Value = mode.ToString();
            _config.AppSettings.Settings["OfflineDataName"].Value = title;
            _config.Save();
            ConnectivityMode = mode;
            OnPropertyChanged(nameof(ConnectivityMode));
            CloseOfflineAreaPanel();
        }
    }
}

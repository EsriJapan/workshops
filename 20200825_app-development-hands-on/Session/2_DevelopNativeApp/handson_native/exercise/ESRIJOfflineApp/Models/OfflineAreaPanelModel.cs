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

        }

        /// <summary>
        /// オフライン マップをオンラインと同期する
        /// </summary>
        /// <param name="mapArea"></param>
        /// <returns></returns>
        public async Task OfflineMapUp(PreplannedMapArea mapArea)
        {

        }

        /// <summary>
        /// オフライン プレプランマップの削除処理
        /// </summary>
        /// <param name="mapArea"></param>
        /// <returns></returns>
        private async Task DeleteMapAreasAsync(PreplannedMapArea mapArea)
        {
          
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

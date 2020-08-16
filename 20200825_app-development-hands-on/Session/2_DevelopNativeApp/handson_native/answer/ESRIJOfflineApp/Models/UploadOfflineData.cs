using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ESRIJOfflineApp.Models
{
    public class UploadOfflineData : BaseModel
    {

        private string _statusMessage;

        #region プロパティ
        public string BusyIndicatorVisibility { get; set; }
        #endregion

        #region コンストラクタ
        public UploadOfflineData()
        {

        }
        #endregion

        /// <summary>
        /// オフライン マップをオンラインと同期する
        /// </summary>
        public async void UploadOfflineMapAsync(Map map)
        {
            await OfflineMapUp(map);
        }


        public async Task OfflineMapUp(Map map)
        {

            // 新しいオフライン マップ同期タスクの作成
            OfflineMapSyncTask offlineMapSyncTask = await OfflineMapSyncTask.CreateAsync(map);

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
                    _statusMessage = "Synchronization is complete!";
                }
                else if (offlineMapSyncJob.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Failed)
                {
                    //同期失敗
                    _statusMessage = offlineMapSyncJob.Error.Message;
                }
                else
                {
                    _statusMessage = "Sync in progress ...";
                }
                MessageBox.Show(_statusMessage);
            };

            // 同期開始
            offlineMapSyncJob.Start();
        }
    }
}

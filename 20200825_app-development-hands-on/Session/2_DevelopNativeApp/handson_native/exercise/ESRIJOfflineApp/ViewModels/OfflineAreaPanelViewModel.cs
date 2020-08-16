using ESRIJOfflineApp.Commands;
using ESRIJOfflineApp.Models;
using System.Windows.Input;

namespace ESRIJOfflineApp.ViewModels
{
    public class OfflineAreaPanelViewModel : BaseViewModel
    {
        private OfflineAreaPanelModel _offlineAreaPanelModel = new OfflineAreaPanelModel();

        #region コンストラクタ
        public OfflineAreaPanelViewModel()
        {
            // オフラインマップの情報を取得する処理を記述 (前作ったプレプランツールを参考に)
        }
        #endregion

        #region コマンド
        private ICommand _ShowOnline;

        public ICommand ShowOnline
        {
            get
            {
                return _ShowOnline ?? (_ShowOnline = new DelegateCommand(
                    (x) =>
                    {
                        _offlineAreaPanelModel.ShowOnline();
                    }));
            }
        }

        private ICommand _DownloadMapArea;

        public ICommand DownloadMapArea
        {
            get
            {
                return _DownloadMapArea ?? (_DownloadMapArea = new DelegateCommand(
                    (x) =>
                    {
                        _offlineAreaPanelModel.DownloadMapArea();
                    }));
            }
        }

        private ICommand _DeleteAllMapAreas;

        public ICommand DeleteAllMapAreas
        {
            get
            {
                return _DeleteAllMapAreas ?? (_DeleteAllMapAreas = new DelegateCommand(
                    (x) =>
                    {
                        _offlineAreaPanelModel.DeleteAllMapAreas();
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
                        _offlineAreaPanelModel.CloseOfflineAreaPanel();
                    }));
            }
        }
        #endregion
    }
}

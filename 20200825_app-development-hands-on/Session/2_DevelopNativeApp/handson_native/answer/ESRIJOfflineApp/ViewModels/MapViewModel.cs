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

        #region App.config �̒l���擾
        // - �F�؂���|�[�^���� URL
        private string _serverUrl = ConfigurationManager.AppSettings["ServerUrl"];
        // - �T�[�o�ɓo�^�����N���C�A���g ID.
        private string AppClientId = ConfigurationManager.AppSettings["AppClientId"];
        // - �A�v���P�[�V�����̔閧�L�[
        private string ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
        // - ���_�C���N�g����� URL.
        private string OAuthRedirectUrl = ConfigurationManager.AppSettings["OAuthRedirectUrl"];
        // - �T�[�o�Ƀz�X�e�B���O���Ă��� Web �}�b�v�A�C�e���� ID.
        private string WebMapId = ConfigurationManager.AppSettings["WebMapId"];
        // - �_�E�����[�h�����I�t���C���f�[�^�̕ۑ���.
        private string _offlineDataFolder = ConfigurationManager.AppSettings["OfflineDataFolder"];
        // - �A�v���P�[�V�����̃��[�h�iOnline or Offline�j.
        private string _mode = ConfigurationManager.AppSettings["Mode"];
        // - �Ō�ɕ\�����Ă����I�t���C���f�[�^�̖��O.
        private string _offlineDataName = ConfigurationManager.AppSettings["OfflineDataName"];
        #endregion

        #region �N���X�ϐ�
        private PopupManager _popupManager;
        private Feature _feature;
        private OfflineMapTask _offlineMapTask;
        private MobileMapPackage _mobileMapPackage;
        // �ҏW�\�ȃ��C���[
        private readonly ReadOnlyCollection<string> _isUpdateEnabledLayers = new ReadOnlyCollection<string>(new List<string>() { "�R��" });
        private Esri.ArcGISRuntime.Location.Location _lastLocation;
        public static ListView IdentifiedFeatureListView;
        public static Button SaveBtn;
        private Configuration _config;
        #endregion

        #region ���ʃv���p�e�B���������߂̃C���X�^���X
        private OfflineAreaPanelModel _offlineAreaPanelModel = new OfflineAreaPanelModel();
        private OAuthModel oAuthModel = new OAuthModel();
        private MapDataStore _mapDataStore = new MapDataStore();
        #endregion


        #region �R���X�g���N�^
        public MapViewModel()
        {
            // identify controller �̏�����
            IdentifyController = new IdentifyController();
            IdentifyController.IdentifyCompleted += IdentifyController_IdentifyCompleted;

            _offlineAreaPanelModel.PropertyChanged += RaisePropertyChanged;

            // Web �}�b�v�̃C���X�^���X�쐬
            _mapDataStore.OnlineMap = new Map(new Uri(WebMapId));

            // ���݂̃��[�h�擾
            ConnectivityMode = _mode == "Online" ? ConnectivityMode.Online : ConnectivityMode.Offline;

            // �A�v���P�[�V�����̃��[�h�ɂ���ď�����ύX
            if (_mode != "Offline")
            {
                // �I�����C�����[�h
                oAuthModel.SetOAuthInfo();

                // �}�b�v�̏�����
                InitializeOnline();
            }
            else
            {
                // �I�t���C�����[�h
                InitializeOffline();
            }
        }
        #endregion

        /// <summary>
        /// �C�j�V�����C�Y
        /// </summary>
        private async void InitializeOnline()
        {
            try
            {
                // ���C�Z���X�L�[��o�^���� Lite ���C�Z���X�̔F�؂��s��
                //string licenseKey = "runtimelite,1000,rud8329497369,none,TRB3LNBHPB6J2T8AG010";
                //ArcGISRuntimeEnvironment.SetLicense(licenseKey);

                await LoadMaps();

                // ��k�ڂ��Ȃ���
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
        /// �}�b�v�̕\��
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
            // �}�b�v �p�b�P�[�W���_�E�����[�h����t�H���_ �p�X���쐬���܂��B
            string path = System.IO.Path.Combine(_offlineDataFolder, _offlineDataName);
            // ���Ƀ_�E�����[�h����Ă���ꍇ�́A���̃G���A���J���܂��B
            if (Directory.Exists(path))
            {
                try
                {
                    // �_�E�����[�h�ς݂̃��o�C���}�b�v�p�b�P�[�W���J���܂��B
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

        #region �v���p�e�B
        /// <summary>
        /// �}�b�v���擾�܂��͐ݒ肵�܂��B
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
        /// Identify �R���g���[�����擾�܂��͐ݒ肵�܂��B
        /// </summary>
        public IdentifyController IdentifyController { get; private set; }

        /// <summary>
        /// �I�������t�B�[�`���̃|�b�v�A�b�v�}�l�[�W�����擾���܂�
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
        /// �I�t���C���p�l���� Visibility �X�e�[�^�X
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
        /// ���C���[�ꗗ�� Visibility �X�e�[�^�X
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
        /// ��}���[�h�� Visiblity �X�e�[�^�X
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
        /// Busy�p�l���� Visibility �X�e�[�^�X
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
        /// �I�t���C���G���A�̃��X�g
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
        /// �I�t���C���G���A�̑I�������l���擾���܂��B
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
        /// �|�b�v�A�b�v�^�C�g��
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
        /// �|�b�v�A�b�v�t�B�[���h
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

        // ��`�ς݂̃A�^�b�`�����g�擾�p
        private string _attachmentList;

        /// <summary>
        /// View(���)�֔��f����ϐ����`����
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

        // ��`�ς݂̃A�^�b�`�����g�擾�p
        private BitmapImage _attachment;

        /// <summary>
        /// View(���)�֔��f����ϐ����`����
        /// </summary>
        public BitmapImage Attachment
        {
            get { return _attachment; }
            set
            {
                _attachment = value;
                // ��ʂɒl�𔽉f����
                OnPropertyChanged(nameof(Attachment));
            }
        }
        
        private bool _isEnabled;
        /// <summary>
        /// �A�C�e����񊈐��ɂ���v���p�e�B
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
        /// �폜�{�^����񊈐��ɂ���v���p�e�B
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

        // ��`�ς݂̃A�^�b�`�����g�擾�p
        private IEnumerable<Layer> _operationalLayers;

        /// <summary>
        /// View(���)�֔��f����ϐ����`����
        /// </summary>
        public IEnumerable<Layer> OperationalLayers
        {
            get { return _operationalLayers; }
            set
            {
                _operationalLayers = value;
                // ��ʂɒl�𔽉f����
                OnPropertyChanged(nameof(OperationalLayers));
            }
        }
        #endregion

        #region �R�}���h

        /// <summary>
        /// ��}�֘A�̃R�}���h
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
        /// �V�K�t�B�[�`���쐬
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
                                    // �G���[����
                                }
                                break;
                            }
                        }
                    }));
            }
        }
        #endregion

        #region Identify �p�l���̃R�}���h
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
        /// �R�}���h�i�{�^���I�����̏����j�̂��߂̕ϐ�
        /// wrote by esrijapan
        /// </summary>
        // �A�^�b�`�����g���A�v���Ɉꎞ�ۑ����邽�߂̃R�}���h
        private ICommand _attachmentCommand;

        /// <summary>
        /// �A�^�b�`�����g�I���{�^���������ꂽ�Ƃ��ɍs������
        ///  1.�G�N�X�v���[�����J��
        ///  2.PopupManager��AttachmentManager�ŁA�A�^�b�`�����g��ǉ�����
        /// </summary>
        public ICommand AttachmentCommand
        {
            get
            {
                return _attachmentCommand ?? (_attachmentCommand = new DelegateCommand(
                    (x) =>
                    {
                        // �G�N�X�v���[���[���J��
                        System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
                        ofd.Title = "JPEG�t�@�C�����J��";
                        //�����t�@�C����I���ł���悤�ɂ���
                        ofd.Multiselect = false;
                        // [�t�@�C���̎��] �{�b�N�X�ɕ\�������I������ݒ肷��
                        ofd.Filter = "JPEG�t�@�C��(*.jpg;*.jpeg)|*.jpg;*.jpeg";

                        //�_�C�A���O��\��
                        string[] files = null;
                        if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            //�I�����ꂽ�t�@�C������ϐ��ɃZ�b�g
                            files = ofd.FileNames;
                            ////////////////////////////////////////////////////////////////////////
                            // �|�b�v�A�b�v�}�l�[�W���[�̃A�^�b�`�����g�}�l�[�W���[����AddAttachment()���\�b�h���g�p���ăA�^�b�`�����g��ǉ�����
                            // ������
                            // �@files[0] >file�z���0�Ԗ�
                            // �A"image/png" > �A�^�b�`�����g�摜�̎��
                            PopupManager.AttachmentManager.AddAttachment(files[0], "image/png");
                            ////////////////////////////////////////////////////////////////////////
                            // �\���t�@�C�����̎擾
                            String _addFileName = Path.GetFileName("@" + files[0]);
                            AttachmentList = _addFileName;

                            // �T���l�C����\������
                            var source = new BitmapImage();
                            source.BeginInit();
                            source.UriSource = new Uri(files[0]);
                            source.EndInit();
                            Attachment = source;

                        }
                        else
                        {
                            // �u�L�����Z���v���I�����ꂽ�Ƃ��̏���
                            // �Ȃɂ����Ȃ�
                        }
                    }));
            }
        }
        #endregion

        #region Expander �R�}���h
        /// <summary>
        /// �I�t���C���p�l���̋N��
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
        /// ���C���[�ꗗ�̋N��
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
        /// �A�v���P�[�V�����̏I��
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

        #region �I�t���C���p�l�� �R�}���h
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

            // �I�����C�����I�t���C�����ŏ����𕪊�
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
        /// �}�b�v�̃��[�h �X�e�[�^�X���ύX���ꂽ�Ƃ��ɌĂяo����܂��B
        /// IdentifyController�Ƀ^�[�Q�b�g���C���[��ݒ肷��ɂ́A�}�b�v�����[�h�����܂ő҂K�v������܂��B
        /// </summary>
        private async void Map_LoadStatusChanged(object sender, LoadStatusEventArgs e)
        {
            // reload map if an error occured
            if (e.Status == LoadStatus.FailedToLoad)
                await Map.RetryLoadAsync();

            else if (e.Status == LoadStatus.Loaded && IdentifyController != null)
            {

                // �ʒu���T�[�r�X���N��
                try
                {
                    await LocationDataSource.StartAsync();
                }
                catch
                { // �ʒu���T�[�r�X���J�n�ł��Ȃ��ꍇ�́A�������Ȃ��ł��������A�{�^���͂��������ɂȂ�܂��B
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
        /// Identify ���삪���������Ƃ��ɌĂяo����܂��B
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

                        // �R���|�C���g�̂ݕҏW�\
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
        /// �t�B�[�`���̃A�^�b�`�����g�̗L�����`�F�b�N����
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
                // �A�^�b�`�����g�����݂���Ȃ�A�ŏ���1������\������(��������ꍇ��"��"������)
                String _fullName = attachments.First().Name;

                // �T���l�C����\������
                Stream x = await attachments.First().GetDataAsync();
                var source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = x;
                source.EndInit();
                Attachment = source;

                AttachmentList += _fullName;
                if (attachments.Count() > 1)
                {
                    AttachmentList += "�A��";
                }
            }
            else
            {
                //�Ȃ��ꍇ�́A�Ȃ��|�̃��b�Z�[�W���l�߂�
                AttachmentList = "�Y�t�t�@�C����";
            }
        }

        private void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// ���X�g�r���[�őI�����ꂽ�I�t���C���G���A�̃_�E�����[�h��Ԃɂ���ď����ύX
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
        /// �A�v���N�����Ƀl�b�g���[�N�󋵂ƃ��[�h���l�����ċN������
        /// </summary>
        /// <returns></returns>
        //private async Task<Map> GetMap()
        //{
        //    // �I�����C���E�F�u�}�b�v�ւ̐ڑ������e�X�g
        //    // �f�o�C�X�����S�ɃI�t���C���̏ꍇ�A����
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

        //    // mmpk���擾���A�܂����[�h����Ă��Ȃ��ꍇ�͂�����擾���܂��B
        //    if (Mmpk == null)
        //    {
        //        try
        //        {
        //            Mmpk = await MobileMapPackage.OpenAsync(DownloadPath);
        //            OfflineMap = Mmpk.Maps.FirstOrDefault();
        //        }
        //        catch
        //        {
        //            // �I�t���C���}�b�v���擾�ł��Ȃ��ꍇ�́A�A�v�����I�����C�����[�h�ɐ؂�ւ��Ă��������B
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

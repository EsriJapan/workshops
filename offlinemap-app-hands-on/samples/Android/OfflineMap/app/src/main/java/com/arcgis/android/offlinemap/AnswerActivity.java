package com.arcgis.android.offlinemap;

import android.os.Bundle;
import android.os.Environment;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.esri.arcgisruntime.ArcGISRuntimeException;
import com.esri.arcgisruntime.concurrent.Job;
import com.esri.arcgisruntime.concurrent.ListenableFuture;
import com.esri.arcgisruntime.data.Feature;
import com.esri.arcgisruntime.data.FeatureTable;
import com.esri.arcgisruntime.data.Geodatabase;
import com.esri.arcgisruntime.data.GeodatabaseFeatureTable;
import com.esri.arcgisruntime.data.ServiceFeatureTable;
import com.esri.arcgisruntime.data.TileCache;
import com.esri.arcgisruntime.geometry.Envelope;
import com.esri.arcgisruntime.geometry.GeometryEngine;
import com.esri.arcgisruntime.geometry.Point;
import com.esri.arcgisruntime.geometry.SpatialReferences;
import com.esri.arcgisruntime.layers.ArcGISTiledLayer;
import com.esri.arcgisruntime.layers.FeatureLayer;
import com.esri.arcgisruntime.loadable.LoadStatus;
import com.esri.arcgisruntime.mapping.ArcGISMap;
import com.esri.arcgisruntime.mapping.Basemap;
import com.esri.arcgisruntime.mapping.view.DefaultMapViewOnTouchListener;
import com.esri.arcgisruntime.mapping.view.MapView;
import com.esri.arcgisruntime.tasks.geodatabase.GenerateGeodatabaseJob;
import com.esri.arcgisruntime.tasks.geodatabase.GenerateGeodatabaseParameters;
import com.esri.arcgisruntime.tasks.geodatabase.GeodatabaseSyncTask;
import com.esri.arcgisruntime.tasks.geodatabase.SyncGeodatabaseJob;
import com.esri.arcgisruntime.tasks.geodatabase.SyncGeodatabaseParameters;
import com.esri.arcgisruntime.tasks.geodatabase.SyncLayerResult;

import java.io.File;
import java.util.HashMap;
import java.util.List;
import java.util.concurrent.ExecutionException;

/**
 * OfflineApp Hands on @Android
 *
 * 2017/09/04 write by wakanasato
 * Esri Japan Corporation All Rughts Reserved
 * Answer Activity
 * */
public class AnswerActivity extends AppCompatActivity {

    /** map */
    MapView mMapView;
    ArcGISMap mArcGISmap;

    /** Android Local path */
    String mLocalFilePath;

    /** debug用 */
    String TAG = "☆esrijapan_offlineApp☆";

    /** ランタイムコンテンツのダウンロードボタン */
    Button mBottun_DL;
    Button mBottun_Sync;

    /** 使用するサービスURL */
    String mArcGISFeatureServiceUrl = "https://services.arcgis.com/wlVTGRSYTzAbjjiC/ArcGIS/rest/services/urayasushi_hoikuen_yochien/FeatureServer";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Android 端末のローカルパスを取得します
        mLocalFilePath = Environment.getExternalStorageDirectory().getPath() + getResources().getString(R.string.local_path);;

        // ランタイムコンテンツダウンロードボタンを作成する
        mBottun_DL = (Button)findViewById(R.id.button_DL);
        mBottun_DL.setEnabled(false);
        mBottun_Sync = (Button)findViewById(R.id.button_Sync);
        mBottun_Sync.setEnabled(false);

        // 地図の表示
        mMapView = (MapView) findViewById(R.id.mapView);
        // AGOL(ArcGIS Online) のベースマップ(STREETS)を読み込む
        mArcGISmap = new ArcGISMap(Basemap.Type.STREETS, 35.6539486,139.9133403, 13);
        mMapView.setMap(mArcGISmap);

        // TODO 2.タイルパッケージを読み込んで背景地図を表示する※メソッド内を実装します。
        readTilePkg();

        // TODO 3.主題図の表示
        readFeatureLayer();

        // ボタンを選択したときにフィーチャ サービスを読み込んでランタイムコンテンツを作成する
        mBottun_DL.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                File geodatabase = new File(mLocalFilePath + getResources().getString(R.string.runtimecontents_name));
                if(geodatabase.exists()){
                    // 既存のgeodatabaseをreadする
                    readGeoDatabase();
                }else{
                    // TODO 4.フィーチャ サービスのデータのダウンロード
                    downloadFeatureService();
                }
            }
        });

        // 地図上の任意の場所をタップしたときにポイントを追加する(編集)
        mMapView.setOnTouchListener(new DefaultMapViewOnTouchListener(this,mMapView){
            @Override
            public boolean onSingleTapConfirmed(MotionEvent motionEvent) {
                Log.d(TAG, "setOnTouchListener: " + motionEvent.toString());
                // タッチされたポイントを作成
                android.graphics.Point sreenPoint = new android.graphics.Point(Math.round(motionEvent.getX()), Math.round(motionEvent.getY()));
                // TODO 5.フィーチャの編集（ポイント追加）
                addFeatures(sreenPoint);
                return true;
            }
        });

        mBottun_Sync.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // TODO 6.編集結果をフィーチャ サービスと同期
                syncFeatureService();
            }
        });
    }

    /**
     * タイルパッケージを読み込んで背景地図を表示する
     * */
    private void readTilePkg(){

        /**
         * コードの前にファイルをダウンロードする
         * ※デフォルトのダウンロード先は/sdcard/Download
         * Local_tpk_pathにはDownloadを指定している
         *
         * 【権限チェック】
         * Andrioid 6.0以上の端末はアプリの権限があるかどうかみる設定＞アプリ＞インストールしたapp＞権限
         * */

        String tpkpath  = mLocalFilePath + getResources().getString(R.string.tpk_name);
        // 存在チェック
        File tpkfile = new File(mLocalFilePath);
        if(!tpkfile.exists()){
            Log.d(TAG, tpkpath + ":" + tpkfile.exists());
        }else{
            // TODO tpkファイルがある場合はレイヤーとして表示する
            TileCache tileCache = new TileCache(tpkpath);
            ArcGISTiledLayer tiledLayer = new ArcGISTiledLayer(tileCache);
            mArcGISmap.getOperationalLayers().add(tiledLayer);
        }
    }

    /**
     * 主題図の表示
     *
     * */
    private void readFeatureLayer(){

        String FeatureServiceURL = mArcGISFeatureServiceUrl + "/0"; // レイヤーはここにリスト状に定義されているので順番を指定します
        FeatureTable featureTable = new ServiceFeatureTable(FeatureServiceURL);
        final FeatureLayer featureLayer = new FeatureLayer(featureTable);
        mArcGISmap.getOperationalLayers().add(featureLayer);

        featureLayer.addDoneLoadingListener(new Runnable() {
            @Override
            public void run() {
                if(featureLayer.getLoadStatus() == LoadStatus.LOADED){
                    // toast出してボタンの有効化
                    Log.d(TAG, "FeatureLayer Loaded");
                    Toast.makeText(getApplicationContext(), "FeatureLayer Loaded / ボタンの活性化", Toast.LENGTH_SHORT).show();
                    mBottun_DL.setEnabled(true);
                }
            }
        });
    }

    ////////////////////////////////////////////////////////////////
    // geodatabase作成
    ////////////////////////////////////////////////////////////////

    /** ローカルジオデータベース(*.geodatabase)関連 */
    // 新規作成または編集・同期に使用する
    GeodatabaseSyncTask mGeodatabaseSyncTask;
    GenerateGeodatabaseParameters generateParams;
    GenerateGeodatabaseJob generateJob;
    Geodatabase geodatabase;
    GeodatabaseFeatureTable mGdbFeatureTable;
    // ArcGIS Online または ArcGIS Enterprise との同期
    static SyncGeodatabaseParameters mSyncParameter;
    static SyncGeodatabaseJob mSyncGeodatabaseJob;

    /**
     * フィーチャ サービスのデータのダウンロード
     * */
    private void downloadFeatureService(){

        ////////////////////////////////////////////////////////////////
        // ローカルフォルダにランタイムコンテンツ(*.geodatabase)作成
        // 【手順】
        //　① ランタイムコンテンツを作成したい ArcGIS Online の Feature Layer でタスクを作成する
        //　② ランタイムコンテンツを作成したい ArcGIS Online の Feature Layer のパラメータを取得する
        //　③ ランタイムコンテンツを作成したい ArcGIS Online の Feature Layer でローカル geodatabase を作成する
        ////////////////////////////////////////////////////////////////

        // ① ランタイムコンテンツを作成したい ArcGIS Online の Feature Layer でタスクオブジェクト(GeodatabaseSyncTask)を作成する
        mGeodatabaseSyncTask = new GeodatabaseSyncTask(mArcGISFeatureServiceUrl);
        // タスクオブジェクトのロードを行う
        mGeodatabaseSyncTask.addDoneLoadingListener(new Runnable() {
            @Override public void run() {
                // ロードのステータスを検査する
                if (mGeodatabaseSyncTask.getLoadStatus() == LoadStatus.FAILED_TO_LOAD) {
                    Log.e(TAG,mGeodatabaseSyncTask.getLoadError().toString());
                    Toast.makeText(getApplicationContext(), "Created GeodatabaseSyncTask failed!", Toast.LENGTH_SHORT).show();
                } else {
                    // Load に成功
                    Log.d(TAG, "Created GeodatabaseSyncTask");
                    Toast.makeText(getApplicationContext(), "Created GeodatabaseSyncTask Success!", Toast.LENGTH_SHORT).show();
                    // ② ロードができたら Feature Layer のパラメータを取得する
                    generateGeodatabaseParameters();
                }
            }
        });
        // タスクのロードを開始する
        mGeodatabaseSyncTask.loadAsync();

    }

    /**
     * GeoDatabaseを新規に作成する
     * ② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
     * */
    private void generateGeodatabaseParameters() {

        // geodatabase 作成のためのパラメータを取得する
        Envelope generateExtent = mMapView.getVisibleArea().getExtent();
        final ListenableFuture<GenerateGeodatabaseParameters> generateParamsFuture =
                mGeodatabaseSyncTask.createDefaultGenerateGeodatabaseParametersAsync(generateExtent);
        generateParamsFuture.addDoneListener(new Runnable() {
            @Override
            public void run() {
                try {
                    generateParams = generateParamsFuture.get();
                    Log.d(TAG, "Created GeodatabaseParameters");
                    Toast.makeText(getApplicationContext(), "Created GeodatabaseParameters Success!", Toast.LENGTH_SHORT).show();
                    // ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
                    generateGeodatabase();
                }
                catch (InterruptedException | ExecutionException e) {
                    Log.d(TAG, "Created GeodatabaseParameters failed");
                    Toast.makeText(getApplicationContext(), "Created GeodatabaseParameters failed!", Toast.LENGTH_SHORT).show();
                    e.printStackTrace();
                }
            }
        });
    }

    /**
     * GeoDatabaseを新規に作成する
     * ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
     * */
    private void generateGeodatabase() {

        // geodatabaseファイル作成ジョブオブヘジェクトを作成する
        String runtimecontentspath  = mLocalFilePath + getResources().getString(R.string.runtimecontents_name);
        generateJob = mGeodatabaseSyncTask.generateGeodatabaseAsync(generateParams, runtimecontentspath);

        // データダウンロードのステータスをチェックする
        generateJob.addJobChangedListener(new Runnable() {
            @Override
            public void run() {

                // 作成中のステータスをチェックする
                if (generateJob.getError() != null) {
                    Log.e(TAG,generateJob.getError().toString());
                } else {
                    // ダウンロードの進行状況：メッセージを確認したり、ログやユーザーインターフェイスで進行状況を更新します
                }
            }
        });

        // ダウンロードとgeodatabaseファイル作成が終了したときのステータスを取得します
        generateJob.addJobDoneListener(new Runnable() {
            @Override
            public void run() {

                // 作成ジョブが終了したときのステータスを検査する
                String status = generateJob.getStatus().toString();
                if ((generateJob.getStatus() != Job.Status.SUCCEEDED) || (generateJob.getError() != null)) {
                    Log.e(TAG,generateJob.getError().toString());
                } else {
                    // ランタイムコンテンツ作成成功！
                    Log.d(TAG, "Created RuntimeContents success!");
                    Toast.makeText(getApplicationContext(), "Created RuntimeContents success!", Toast.LENGTH_SHORT).show();

                    if (generateJob.getResult() instanceof Geodatabase) {
                        Geodatabase syncResultGdb = (Geodatabase) generateJob.getResult();
                        geodatabase = syncResultGdb;
                        // 作成したgeodatabaseからフィーチャ レイヤーを表示する
                        // それまでのフィーチャーレイヤーは削除して、新たにランタイムコンテンツから表示させる
                        readGeoDatabase();
                    }
                }
            }
        });
        // geodatabase 作成のジョブを開始します
        generateJob.start();
    }

    /**
     * 既存GeoDatabaseから読み込む
     * ***/
    private void readGeoDatabase(){

        geodatabase = new Geodatabase(mLocalFilePath + getResources().getString(R.string.runtimecontents_name));
        geodatabase.loadAsync();
        geodatabase.addDoneLoadingListener(new Runnable() {
            @Override
            public void run() {

                // geodatabaseの読込 地図追加
                if(geodatabase.getLoadStatus() == LoadStatus.LOADED ){
                    if(geodatabase.getGeodatabaseFeatureTables().size() > 0){
                        // 今回読み込むレイヤーは１つ=0
                        mGdbFeatureTable = geodatabase.getGeodatabaseFeatureTables().get(0);
                        try{
                            FeatureLayer featureLayer = new FeatureLayer(mGdbFeatureTable);
                            mArcGISmap.getOperationalLayers().add(featureLayer);
                        }catch (Exception e){
                            e.printStackTrace();
                        }
                    }else{
                        Log.e(TAG, geodatabase.getLoadStatus() + "/ FeatureTables.size=" + geodatabase.getGeodatabaseFeatureTables().size());
                    }
                }else if(geodatabase.getLoadStatus() == LoadStatus.FAILED_TO_LOAD){
                    Log.e(TAG,geodatabase.getLoadStatus().toString());
                }else if(geodatabase.getLoadStatus() == LoadStatus.NOT_LOADED){
                    Log.e(TAG,geodatabase.getLoadStatus().toString());
                }else if(geodatabase.getLoadStatus() == LoadStatus.LOADING){
                    Log.e(TAG,geodatabase.getLoadStatus().toString());
                }
            }
        });
    }


    ////////////////////////////////////////////////////////////////
    // ポイント追加(編集)
    ////////////////////////////////////////////////////////////////

    /**
     * フィーチャの編集（ポイント追加）
     * */
    private void addFeatures(android.graphics.Point pScreenPoint){

        // 同期ボタンを有効にする
        mBottun_Sync.setEnabled(true);

        // 変換した座標からArcGISのジオメトリ(point)を作成する
        Point mapPoint = mMapView.screenToLocation(pScreenPoint);
        // ポイントの座標変換
        final Point wgs84Point = (Point) GeometryEngine.project(mapPoint, SpatialReferences.getWgs84());

        // ポイントと一緒に設定したい属性項目のデータ定義します。
        final java.util.Map<String, Object> attributes = new HashMap<String, Object>();
        attributes.put("name","ESRIジャパンnow！"); // 使用するFeature Layerにはあらかじめ"name"の項目を作成しています。
        // ローカルのランタイムコンテンツのフィーチャ テーブルをもとに新しいポイントと属性情報のフィーチャを作成します。
        Feature addedFeature = mGdbFeatureTable.createFeature(attributes, wgs84Point);
        // ローカルのランタイムコンテンツに新しいポイント情報を追加します。
        final ListenableFuture<Void> addFeatureFuture = mGdbFeatureTable.addFeatureAsync(addedFeature);
        addFeatureFuture.addDoneListener(new Runnable() {
            @Override
            public void run() {
                try {
                    // ポイント追加の成功をチェックする
                    addFeatureFuture.get();
                    Toast.makeText(getApplicationContext(), "add point geodatabase", Toast.LENGTH_SHORT).show();

                } catch (InterruptedException | ExecutionException e) {
                    // executionException may contain an ArcGISRuntimeException with edit error information.
                    if (e.getCause() instanceof ArcGISRuntimeException) {
                        ArcGISRuntimeException agsEx = (ArcGISRuntimeException)e.getCause();
                        Log.e(TAG, agsEx.toString());
                    } else {
                        Log.e(TAG, "other error");
                    }
                }
            }
        });
    }

    ////////////////////////////////////////////////////////////////
    // 同期
    ////////////////////////////////////////////////////////////////

    /**
     * 編集結果をフィーチャ サービスと同期する
     * ① 同期タスクを作成する
     * ② 同期パラメータを取得する
     * */
    private void syncFeatureService() {

        // タスクオブジェクトから同期するためのパラメータを作成する
        final ListenableFuture<SyncGeodatabaseParameters> syncParamsFuture = mGeodatabaseSyncTask.createDefaultSyncGeodatabaseParametersAsync(geodatabase);
        syncParamsFuture.addDoneListener(new Runnable() {
            @Override
            public void run() {
                try {
                    // パラメータを取得
                    mSyncParameter = syncParamsFuture.get();
                    // パラーメータを使用してgeodatabaseを同期する
                    syncGeodatabase();
                }catch (InterruptedException | ExecutionException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    /**
     * サーバー(AGOL)と同期する
     * ③ 同期ジョブを作成する
     * ④ 同期する
     * */
    private void syncGeodatabase() {

        // 同期ジョブオブヘジェクトを作成する
        mSyncGeodatabaseJob = mGeodatabaseSyncTask.syncGeodatabaseAsync(mSyncParameter, geodatabase);

        // 同期中のステータスをチェックする
        mSyncGeodatabaseJob.addJobChangedListener(new Runnable() {
            @Override
            public void run() {
                if (mSyncGeodatabaseJob.getError() != null) {
                    // 同期中にエラーがある場合
                    Log.e(TAG, mSyncGeodatabaseJob.getError().toString());
                } else {
                    // 同期の進行状況：メッセージを確認したり、ログやユーザーインターフェイスで進行状況を更新します
                }
            }
        });

        // 同期が終了したときのステータスを取得します
        mSyncGeodatabaseJob.addJobDoneListener(new Runnable() {
            @Override
            public void run() {
                // 同期ジョブが終了したときのステータスを検査する
                if ((mSyncGeodatabaseJob.getStatus() != Job.Status.SUCCEEDED) || (mSyncGeodatabaseJob.getError() != null)) {
                    // エラーの場合
                    Log.e(TAG, mSyncGeodatabaseJob.getError().toString());
                } else {
                    // 同期完了から返された値を取得する
                    List<SyncLayerResult> syncResults = (List<SyncLayerResult>) mSyncGeodatabaseJob.getResult();
                    if (syncResults != null) {
                        // 同期結果を確認して、例えばユーザに通知する処理を作成します
                        Toast.makeText(getApplicationContext(), "Sync Success!" , Toast.LENGTH_SHORT).show();
                    }
                }
            }
        });
        // geodatabase 同期のジョブを開始します
        mSyncGeodatabaseJob.start();
    }
}

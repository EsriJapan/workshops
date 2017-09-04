package com.arcgis.android.offlinemap;

import android.os.Bundle;
import android.os.Environment;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;

import com.esri.arcgisruntime.data.Geodatabase;
import com.esri.arcgisruntime.data.GeodatabaseFeatureTable;
import com.esri.arcgisruntime.mapping.ArcGISMap;
import com.esri.arcgisruntime.mapping.Basemap;
import com.esri.arcgisruntime.mapping.view.DefaultMapViewOnTouchListener;
import com.esri.arcgisruntime.mapping.view.MapView;
import com.esri.arcgisruntime.tasks.geodatabase.GenerateGeodatabaseJob;
import com.esri.arcgisruntime.tasks.geodatabase.GenerateGeodatabaseParameters;
import com.esri.arcgisruntime.tasks.geodatabase.GeodatabaseSyncTask;
import com.esri.arcgisruntime.tasks.geodatabase.SyncGeodatabaseJob;
import com.esri.arcgisruntime.tasks.geodatabase.SyncGeodatabaseParameters;

import java.io.File;

/**
 * OfflineApp Hands on @Android
 *
 * 2017/09/04 write by wakanasato
 * Esri Japan Corporation All Rughts Reserved
 * */
public class MainActivity extends AppCompatActivity {

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


    }

    /**
     * 主題図の表示
     *
     * */
    private void readFeatureLayer(){


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



    }

    /**
     * GeoDatabaseを新規に作成する
     * ② 同期させたいArcGIS Online の Feature Layer のパラメータを取得する
     * */
    private void generateGeodatabaseParameters() {


    }

    /**
     * GeoDatabaseを新規に作成する
     * ③ 同期させたいArcGIS Online の Feature Layer でローカル geodatabase を作成する
     * */
    private void generateGeodatabase() {


    }

    /**
     * 既存GeoDatabaseから読み込む
     * ***/
    private void readGeoDatabase(){


    }


    ////////////////////////////////////////////////////////////////
    // ポイント追加(編集)
    ////////////////////////////////////////////////////////////////

    /**
     * フィーチャの編集（ポイント追加）
     * */
    private void addFeatures(android.graphics.Point pScreenPoint){


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


    }

    /**
     * サーバー(AGOL)と同期する
     * ③ 同期ジョブを作成する
     * ④ 同期する
     * */
    private void syncGeodatabase() {


    }

}

package com.arcgis.android.handsonapp;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.graphics.drawable.BitmapDrawable;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.MotionEvent;

import com.esri.arcgisruntime.concurrent.ListenableFuture;
import com.esri.arcgisruntime.data.Feature;
import com.esri.arcgisruntime.data.FeatureQueryResult;
import com.esri.arcgisruntime.data.QueryParameters;
import com.esri.arcgisruntime.data.ServiceFeatureTable;
import com.esri.arcgisruntime.geometry.GeometryEngine;
import com.esri.arcgisruntime.geometry.Point;
import com.esri.arcgisruntime.geometry.Polygon;
import com.esri.arcgisruntime.layers.FeatureLayer;
import com.esri.arcgisruntime.loadable.LoadStatus;
import com.esri.arcgisruntime.mapping.ArcGISMap;
import com.esri.arcgisruntime.mapping.LayerList;
import com.esri.arcgisruntime.mapping.view.DefaultMapViewOnTouchListener;
import com.esri.arcgisruntime.mapping.view.Graphic;
import com.esri.arcgisruntime.mapping.view.GraphicsOverlay;
import com.esri.arcgisruntime.mapping.view.MapView;
import com.esri.arcgisruntime.portal.Portal;
import com.esri.arcgisruntime.security.AuthenticationManager;
import com.esri.arcgisruntime.security.DefaultAuthenticationChallengeHandler;
import com.esri.arcgisruntime.symbology.PictureMarkerSymbol;
import com.esri.arcgisruntime.symbology.SimpleFillSymbol;
import com.esri.arcgisruntime.symbology.SimpleLineSymbol;
import com.esri.arcgisruntime.symbology.SimpleMarkerSymbol;
import com.esri.arcgisruntime.tasks.networkanalysis.ClosestFacilityParameters;
import com.esri.arcgisruntime.tasks.networkanalysis.ClosestFacilityResult;
import com.esri.arcgisruntime.tasks.networkanalysis.ClosestFacilityRoute;
import com.esri.arcgisruntime.tasks.networkanalysis.ClosestFacilityTask;
import com.esri.arcgisruntime.tasks.networkanalysis.Facility;
import com.esri.arcgisruntime.tasks.networkanalysis.Incident;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.concurrent.ExecutionException;


/**
 * Esriジャパン ハンズオンアプリ
 * 避難場所を表示するマップ。クリックした地点から最寄りの避難場所への経路を表示する機能を提供
 *
 * version:Runtime SDK for Android 100.1
 * wrote by satowaka@esrijapan
 * 2017/08/18
 *
 * */
public class MainActivity extends AppCompatActivity {

    // ArcGIS の必須オブジェクト
    public MapView mMapView;
    public ArcGISMap mArcGISMap;

    // デバッグタグ
    public String TAG = "☆esrijapan☆";

    // 表示用Overlay
    public GraphicsOverlay mPointRouteOverlay = new GraphicsOverlay();
    public GraphicsOverlay mBufferGraphicOverlay = new GraphicsOverlay();

    // 使用するURL
    /** WebMap URL */
    public String mWebMapURL = "https://www.arcgis.com/home/item.html?id=285f619f75e64d3681ba101b006d2f65";
    /** 最寄り施設解析 URL */
    public String mClosestFacilityTask = "http://route.arcgis.com/arcgis/rest/services/World/ClosestFacility/NAServer/ClosestFacility_World";
    /** ArcGIS Online ポータルサイト URL */
    public String mPortalURL = "http://www.arcgis.com";

    // 解析するレイヤーのインデックス番号(検索結果)
    public int mAnalysisLayerIndex;
    // 画面をタッチしたポイント(表示および解析に使用する)
    public Point mTouchPoint;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mMapView = (MapView) findViewById(R.id.mapView);
        // 課題のwebmapを表示する
        callwebMap();
        // レイヤに対しての動作を実装する
        operation4Map();
    }

    /**
     * 課題のwebmapを表示する
     * */
    public void callwebMap(){

        // WebMap URL
        mArcGISMap = new ArcGISMap(mWebMapURL);
        mMapView.setMap(mArcGISMap);
        mArcGISMap.addDoneLoadingListener(new Runnable() {
            @Override
            public void run() {
                if(mArcGISMap.getLoadStatus() == LoadStatus.LOADED){
                    // 操作対象レイヤーを求める
                    selectOperationalLayer();
                }else {
                    Log.d(TAG, "callwebMap: " + mArcGISMap.getLoadStatus());
                }
            }
        });
        mArcGISMap.loadAsync();
    }

    /**
     * 該当レイヤーを検索する
     * */
    private void selectOperationalLayer(){

        LayerList layerList = mArcGISMap.getOperationalLayers();
        for(int i  =  0 ; i < layerList.size() ; i++){
            String layername = layerList.get(i).getName();
            Log.d(TAG, "selectOperationalLayer: " + layername);
            if(layername.equals("室蘭市 - 避難場所")){
                mAnalysisLayerIndex = i;
            }
        }

    }

    /**
     * 地図に対する操作を定義する
     *
     * */
    private void operation4Map() {

        // クリックした地点から 1km のバッファーを作成、表示
        mMapView.setOnTouchListener(new DefaultMapViewOnTouchListener(this, mMapView) {

            @Override
            public boolean onSingleTapConfirmed(MotionEvent motionEvent) {
                Log.d(TAG, "onSingleTapConfirmed: " + motionEvent.toString());

                // Overlayの初期化
                if((mPointRouteOverlay.getGraphics().size() != 0) || (mBufferGraphicOverlay.getGraphics().size() != 0)){
                    mPointRouteOverlay.getGraphics().clear();
                    mBufferGraphicOverlay.getGraphics().clear();
                }else{
                    mMapView.getGraphicsOverlays().add(mPointRouteOverlay);
                    mMapView.getGraphicsOverlays().add(mBufferGraphicOverlay);
                }

                // Android の画面位置を取得
                android.graphics.Point screenPoint = new android.graphics.Point(Math.round(motionEvent.getX()),Math.round(motionEvent.getY()));
                // ArcGISのポイントに変換する
                mTouchPoint = mMapView.screenToLocation(screenPoint);
                // タッチしたポイントを表示する
                Bitmap bitmap = BitmapFactory.decodeResource(getResources(), R.mipmap.ic_launcher);
                // new するやり方はAPI4から非推奨:new BitmapDrawable(bitmap);
                BitmapDrawable bd = new BitmapDrawable(getResources(), bitmap);
                PictureMarkerSymbol pms = new PictureMarkerSymbol(bd);
                pms.loadAsync();
                Graphic touchpoint = new Graphic(mTouchPoint, pms);
                mPointRouteOverlay.getGraphics().add(touchpoint);

                // ポイントからバッファを作成する:単位=m
                Polygon bufferPolygon = GeometryEngine.buffer(mTouchPoint, 1000);
                // bufferの表示
                SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbol.Style.SOLID, Color.argb(50, 230, 0, 51), null);// 薄い赤だったと思う
                Graphic bufferGraphic = new Graphic(bufferPolygon, fillSymbol);
                // bufferを地図に追加する
                mBufferGraphicOverlay.getGraphics().add(bufferGraphic);

                // レイヤーからバッファ内のフィーチャを検索する
                findFeaturefromBuffer(bufferPolygon);

                return true;
            }
        });
    }

    /**
     * レイヤーからバッファ内のフィーチャを検索する
     *
     * */
    private void findFeaturefromBuffer(Polygon pPolygon){
        Log.d(TAG, "findFeaturefromBuffer ");

        // 検索パラメータの作成
        QueryParameters queryParameter = new QueryParameters();
        queryParameter.setGeometry(pPolygon);
        queryParameter.setSpatialRelationship(QueryParameters.SpatialRelationship.CONTAINS);

        // 表示用シンボル定義
        final SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbol.Style.CIRCLE, Color.RED, 10);
        // 最寄り施設解析用のオブジェクト
        final List<Incident> incidentList = new ArrayList();
        // bufferとフィーチャレイヤの計算
        FeatureLayer targetLayer = (FeatureLayer)mArcGISMap.getOperationalLayers().get(mAnalysisLayerIndex);
        ServiceFeatureTable serviceFeatureTable = (ServiceFeatureTable)targetLayer.getFeatureTable();
        final ListenableFuture<FeatureQueryResult>  queryResult = serviceFeatureTable.queryFeaturesAsync(queryParameter);
        queryResult.addDoneListener(new Runnable() {
            @Override
            public void run() {
                // 計算結果を取得する
                FeatureQueryResult result = null;
                try {
                    result = queryResult.get();
                    // 表示用
                    Graphic resultPointGraphic = null;
                    // 結果を処理する
                    for (Iterator<Feature> features = result.iterator(); features.hasNext();) {
                        Feature feature = features.next();
                        incidentList.add(new Incident((Point)feature.getGeometry()));
                        // 表示処理
                        resultPointGraphic = new Graphic(feature.getGeometry(),markerSymbol);// ポイントだと信じている
                        mPointRouteOverlay.getGraphics().add(resultPointGraphic);
                    }
                } catch (InterruptedException e) {
                    e.printStackTrace();
                } catch (ExecutionException e) {
                    e.printStackTrace();
                }
                //　解析機能を呼び出す
                callPortalLogin4Analytics(incidentList);
            }
        });
    }

    /**
     * 解析のためにポータルにログインする
     *
     * */
    public void callPortalLogin4Analytics(final List<Incident> pIncident){
        Log.d(TAG, "callPortalLogin4Analytics ");

        // ユーザー アカウント情報で ArcGIS Online / ArcGISポータルへログインし認証情報を取得します
        // ログインのための入力フォームが表示されます。
        DefaultAuthenticationChallengeHandler handler = new DefaultAuthenticationChallengeHandler(this);
        AuthenticationManager.setAuthenticationChallengeHandler(handler);

        final Portal portal = new Portal(mPortalURL,true);
        portal.loadAsync();
        portal.addDoneLoadingListener(new Runnable() {
            @Override
            public void run() {

                if(portal.getLoadStatus() == LoadStatus.LOADED){
                    // アイテムの呼び出し
                    searchNearestPoint(pIncident,portal);
                }
            }
        });
    }

    /**
     * 検索したフィーチャを対象に最寄り施設解析を実行
     *
     * */
    private void searchNearestPoint(final List<Incident> pIncident,final Portal portal){
        Log.d(TAG, "searchNearestPoint ");

        // 最寄り施設解析を実装するためのtaskを作成し、実行のためのパラメータを取得する
        final ClosestFacilityTask closestFacilityTask = new ClosestFacilityTask(this, mClosestFacilityTask);
        closestFacilityTask.setCredential(portal.getCredential());
        final ListenableFuture<ClosestFacilityParameters> paramsFuture = closestFacilityTask.createDefaultParametersAsync();
        paramsFuture.addDoneListener(new Runnable() {
            @Override
            public void run() {
                try {
                    // 実行パラメータを取得する
                    final ClosestFacilityParameters closestFacilityParameters = paramsFuture.get();

                    // 表示の準備
                    final SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbol.Style.SOLID, Color.RED, 2);

                    // タッチしたポイントがFacility(このポイントを基準に最寄り施設を解析する)
                    List<Facility> facilities = new ArrayList();
                    facilities.add(new Facility(mTouchPoint));
                    closestFacilityParameters.setFacilities(facilities);

                    // バッファーから抽出したポイントがIncidents(解析先のポイント)
                    closestFacilityParameters.setIncidents(pIncident);

                    // 解析の実行
                    final ListenableFuture resultFuture = closestFacilityTask.solveClosestFacilityAsync(closestFacilityParameters);
                    resultFuture.addDoneListener(new Runnable() {
                        @Override
                        public void run() {
                            try {
                                // 実行結果からルートをラインシンボルとして表示する
                                ClosestFacilityResult result = (ClosestFacilityResult) resultFuture.get();
                                ClosestFacilityRoute closestFacilityRoute = result.getRoute(0, 0);
                                mPointRouteOverlay.getGraphics().add(new Graphic(closestFacilityRoute.getRouteGeometry(),lineSymbol));
                            } catch (Throwable e) {
                                Log.e(TAG,e.toString());
                            }
                        }
                    });
                } catch (Throwable e) {
                    Log.e(TAG,e.toString());
                }
            }
        });
    }
}

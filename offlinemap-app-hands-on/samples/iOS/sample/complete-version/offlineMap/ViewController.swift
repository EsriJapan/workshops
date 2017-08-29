//
//  ViewController.swift
//  hinanjyoMap
//
//  Created by esrij on H29/08/16.
//  Copyright © 平成29年 com.esrij. All rights reserved.
//

import UIKit
import ArcGIS

class ViewController: UIViewController, AGSGeoViewTouchDelegate, AGSPopupsViewControllerDelegate {

    @IBOutlet var mapView: AGSMapView!
    @IBOutlet weak var downloadBtn: UIBarButtonItem!
    @IBOutlet weak var syncBtn: UIBarButtonItem!

    private var syncTask:AGSGeodatabaseSyncTask!
    private var generateJob:AGSGenerateGeodatabaseJob!
    private var generatedGeodatabase:AGSGeodatabase!
    private var geodatabaseFeatureTable:AGSGeodatabaseFeatureTable!
    private var syncJob:AGSSyncGeodatabaseJob!
    
    private var popupsVC:AGSPopupsViewController!


    private let FEATURE_SERVICE_URL = "https://services.arcgis.com/wlVTGRSYTzAbjjiC/ArcGIS/rest/services/urayasushi_hoikuen_yochien/FeatureServer"

    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // 背景用のタイル レイヤー（タイル パッケージ）を表示
        // AGSTileCache のパラメーターには .tpk ファイルの名前を指定
        let localTiledLayer = AGSArcGISTiledLayer(tileCache: AGSTileCache(name: "public_map"))
        // タイル レイヤーを背景としてマップを作成
        let map = AGSMap(basemap: AGSBasemap(baseLayer: localTiledLayer))
        // マップビュー（地図画面）にマップを表示
        self.mapView.map = map
        
        
        // 主題図用のフィーチャ レイヤー（フィーチャ サービス）の表示
        // フィーチャ サービスの URL を指定してフィーチャ テーブル（AGSServiceFeatureTable）を作成する
        // フィーチャ サービスの URL はレイヤー番号（〜/FeatureServer/0）まで含める
        let featureTable = AGSServiceFeatureTable(url: URL(string: FEATURE_SERVICE_URL + "/0")!)
        // フィーチャ テーブルからフィーチャ レイヤーを作成
        let featureLayer = AGSFeatureLayer(featureTable: featureTable)
        // マップにフィーチャ レイヤーを追加
        self.mapView.map?.operationalLayers.add(featureLayer)
        
        // フィーチャ レイヤーの読み込み完了時の処理
        featureLayer.load(completion: { (error) in
            if error == nil {
                // フィーチャ レイヤーの全体表示領域にマップをズーム
                // 全体表示領域は Rest エンドポイント（Full Extent:）で確認可能
                // https://services.arcgis.com/wlVTGRSYTzAbjjiC/ArcGIS/rest/services/urayasushi_hoikuen_yochien/FeatureServer
                let viewPoint = AGSViewpoint(targetExtent: featureLayer.fullExtent!)
                self.mapView.setViewpoint(viewPoint)
                
                // ダウンロード ボタンのタップを有効化
                self.downloadBtn.isEnabled = true
            }
        })
        
        // フィーチャ サービス URL を使用して同期タスク（AGSGeodatabaseSyncTask）を作成
        self.syncTask = AGSGeodatabaseSyncTask(url: URL(string: FEATURE_SERVICE_URL)!)
        
        // マップのタッチ操作のデリゲートを設定
        self.mapView.touchDelegate = self
        
    }
    
    
    //MARK: - IBAction（ダウンロード ボタンのタップ）
    @IBAction func download(_ sender: UIBarButtonItem) {
        
        // 同期タスクのダウンロード パラメーターを作成
        let params = AGSGenerateGeodatabaseParameters()
        // ダウンロードするレイヤー番号の指定（最上位のレイヤー）
        let layerOption = AGSGenerateLayerOption(layerID: 0)
        params.layerOptions = [layerOption]
        // ダウンロードする範囲の指定（マップの現在の表示範囲）
        params.extent = self.mapView.visibleArea

        // ダウンロードするファイル（.geodatabase）のファイル名を指定（任意の名前）
        // タイムスタンプを使用して現在時刻のユニークなファイル名を設定する
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ssZ"
        let path = NSSearchPathForDirectoriesInDomains(.documentDirectory, .userDomainMask, true)[0]
        let fullPath = "\(path)/\(dateFormatter.string(from: Date())).geodatabase"
        
        // ダウンロード（AGSGenerateGeodatabaseJob）を実行
        generateJob =  self.syncTask.generateJob(with: params, downloadFileURL: URL(string: fullPath)!)
        self.generateJob.start(statusHandler: { (status: AGSJobStatus) -> Void in
            
            // 同期タスクの進捗状況を表示
            print(self.statusString(withStatus: status))
            
        }) { [weak self] (object: AnyObject?, error: Error?) -> Void in
            if let error = error {
                
                // ダウンロードのエラー時にエラー メッセージを表示
                print(error.localizedDescription)
            }
            else {
                
                // ダウンロードの成功時の処理を実装
                // ダウンロードされたファイル（.geodatabase）を取得
                self?.generatedGeodatabase = object as! AGSGeodatabase
                // .geodatabase を読み込む
                self?.generatedGeodatabase.load(completion: { [weak self] (error:Error?) -> Void in
                    if let error = error {
                        
                        // 読み込みのエラー時にエラー メッセージを表示
                        print(error.localizedDescription)
                        
                    }
                    else {
                        
                        // 読み込みの成功時の処理を実装
                        // マップ上に現在表示しているフィーチャ レイヤーを削除
                        self?.mapView.map?.operationalLayers.removeAllObjects()
                        // ダウンロードした .geodatabase をマップ上に表示
                        // .geodatabase からフィーチャ テーブル（AGSGeodatabaseFeatureTable）を作成
                        self?.geodatabaseFeatureTable = self?.generatedGeodatabase.geodatabaseFeatureTables[0]
                        // フィーチャ テーブルからフィーチャ レイヤーを作成
                        let featureLayer = AGSFeatureLayer(featureTable: (self?.geodatabaseFeatureTable)!)
                        // フィーチャ レイヤーをマップに追加
                        self?.mapView.map?.operationalLayers.add(featureLayer)
                        
                        // 同期ボタンのタップを有効化
                        self?.syncBtn.isEnabled = true

                    }
                })
            }
        }
        
    }
    
    
    //MARK: - AGSGeoViewTouchDelegate（マップ上をタップ）
    func geoView(_ geoView: AGSGeoView, didTapAtScreenPoint screenPoint: CGPoint, mapPoint: AGSPoint) {
        
        // ダウンロードしたデータを表示している時だけ実行
        if self.geodatabaseFeatureTable != nil {
            
            
            //=================================================================
            // コードで属性を編集する場合
            
            // フィーチャ テーブルのスキーマを使用して空のフィーチャ（ポイント）を作成
            let feature = self.geodatabaseFeatureTable.createFeature()
            // 作成したフィーチャのジオメトリをマップ上をタップした位置に指定
            feature.geometry = mapPoint
            
            // フィーチャの属性情報を編集（name フィールド（String型）に文字列を指定）
            feature.attributes["name"] = "何か値を入力"
            // フィーチャ テーブルにフィーチャを追加
            self.geodatabaseFeatureTable.add(feature) { (error: Error?) -> Void in
                if let error = error {
                    
                    // フィーチャ追加のエラー時にエラー メッセージを表示
                    print(error.localizedDescription)
                }
                else {
                    
                    // フィーチャ追加の成功
                    print("Successful edit")
                    
                }
            }
            
            //=================================================================
 
            
            /*
            //=================================================================
            // ポップアップを使用して属性を編集する場合
            
            // フィーチャ テーブルのスキーマを使用して空のフィーチャ（ポイント）を作成
            let feature = self.geodatabaseFeatureTable.createFeature()
            // 作成したフィーチャのジオメトリをマップ上をタップした位置に指定
            feature.geometry = mapPoint
            
            // ポップアップ（フィーチャの編集画面）を作成
            let popup = AGSPopup(geoElement: feature)
            
            // ポップアップの画面をカスタマイズ
            // フィーチャの削除を不可に設定
            popup.popupDefinition.allowDelete = false
            // フィーチャのジオメトリの編集を不可に設定
            popup.popupDefinition.allowEditGeometry = false
            // フィーチャのアタッチメント（添付ファイル）の編集を不可に設定
            popup.popupDefinition.showAttachments = false
            
            // ポップアップを使用してポップアップ ビュー コントローラーを作成
            self.popupsVC = AGSPopupsViewController(popups: [popup], containerStyle: .navigationBar)
            // ポップアップの操作のデリゲートを設定
            self.popupsVC.delegate = self
            // ポップアップ ビュー コントローラーを表示
            self.present(self.popupsVC, animated: true, completion: nil)
            
            //=================================================================
            */
            
        }
        
    }

    //MARK: - IBAction（同期ボタンのタップ）
    @IBAction func sync(_ sender: UIBarButtonItem) {
        
        // 同期タスクの同期 パラメーターを作成
        let params = AGSSyncGeodatabaseParameters()
        // ダウンロードするレイヤー番号の指定（最上位のレイヤー）
        let layerOption = AGSSyncLayerOption(layerID: 0)
        params.layerOptions = [layerOption]
        // 同期方向の指定（端末で変更されたデータをフィーチャ サービスに更新）
        params.geodatabaseSyncDirection = .upload
        
        // 同期（AGSSyncGeodatabaseJob）を実行
        self.syncJob = self.syncTask.syncJob(with: params, geodatabase: self.generatedGeodatabase)
        self.syncJob.start(statusHandler: { (status: AGSJobStatus) -> Void in
            
            // 同期タスクの進捗状況を表示
            print(self.statusString(withStatus: status))
            
        }, completion: { (results: [AGSSyncLayerResult]?, error: Error?) -> Void in
            if let error = error {
                
                // 同期のエラー時にエラー メッセージを表示
                print(error.localizedDescription)
            }
            else {
                
                // 同期に成功
                print("Successful sync")
                
            }
        })
        
    }
    

    
    //MARK: - AGSPopupsViewControllerDelegate（ポップアップ操作の終了）
    // ポップアップを使用して属性を編集する場合
    func popupsViewControllerDidFinishViewingPopups(_ popupsViewController: AGSPopupsViewController) {
        // ポップアップ ビュー コントローラーを非表示にする
        self.dismiss(animated: true, completion:nil)
        self.popupsVC = nil
    }
    
    
    
    
    // 同期タスクの進捗状況を表示（AGSJobStatus のステータスに応じて文字列を返す）
    // AGSJobStatus: https://developers.arcgis.com/ios/latest/api-reference/_a_g_s_enumerations_8h.html#a92fcced7af54d0d8f9164773fe64ade7
    func statusString(withStatus status: AGSJobStatus) -> String {
        switch status {
        case .notStarted:
            return "Not started" //処理中
        case .started:
            return "Started" //開始
        case .paused:
            return "Paused" //停止
        case .succeeded:
            return "Succeeded" //成功
        case .failed:
            return "Failed" //失敗
        }
    }

    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }

}


//
//  ViewController.swift
//  hinanjyoMap
//
//  Created by esrij on H29/08/16.
//  Copyright © 平成29年 com.esrij. All rights reserved.
//

import UIKit
import ArcGIS

class ViewController: UIViewController {

    @IBOutlet var mapView: AGSMapView!
    @IBOutlet weak var downloadBtn: UIBarButtonItem!
    @IBOutlet weak var syncBtn: UIBarButtonItem!

    private var syncTask:AGSGeodatabaseSyncTask!
    private var generateJob:AGSGenerateGeodatabaseJob!
    private var generatedGeodatabase:AGSGeodatabase!
    private var geodatabaseFeatureTable:AGSGeodatabaseFeatureTable!
    private var syncJob:AGSSyncGeodatabaseJob!
    
    private let FEATURE_SERVICE_URL = "https://services.arcgis.com/wlVTGRSYTzAbjjiC/ArcGIS/rest/services/urayasushi_hoikuen_yochien/FeatureServer"

    override func viewDidLoad() {
        super.viewDidLoad()

        
    }
    
    
    //MARK: - IBAction（ダウンロード ボタンのタップ）
    @IBAction func download(_ sender: UIBarButtonItem) {
        
    }

    
    //MARK: - AGSGeoViewTouchDelegate（マップ上をタップ）
    func geoView(_ geoView: AGSGeoView, didTapAtScreenPoint screenPoint: CGPoint, mapPoint: AGSPoint) {
        
    }
    

    //MARK: - IBAction（同期ボタンのタップ）
    @IBAction func sync(_ sender: UIBarButtonItem) {
        
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


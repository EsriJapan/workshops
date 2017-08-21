//
//  ViewController.swift
//  hinanjyoMap
//
//  Created by esrij on H29/08/16.
//  Copyright © 平成29年 com.esrij. All rights reserved.
//

import UIKit
import ArcGIS

class ViewController: UIViewController, AGSGeoViewTouchDelegate {

    @IBOutlet var mapView: AGSMapView!
    
    private var featureTable:AGSFeatureTable!
    private var closestFacilityTask: AGSClosestFacilityTask!
    private var closestFacilityParameters:AGSClosestFacilityParameters!
    private var graphicsOverlay = AGSGraphicsOverlay()

    
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // Web マップの URL を指定してマップを作成
        // https://www.arcgis.com/home/item.html?id=<作成した Web マップの ID>
        // 例: https://www.arcgis.com/home/item.html?id=285f619f75e64d3681ba101b006d2f65
        self.mapView.map = AGSMap(url: URL(string: "<Web マップの URL>")!)
        
        // マップの操作を無効にする
        self.mapView.isUserInteractionEnabled = true
        
        // 検索対象のレイヤーの取得
        self.mapView.map?.load(completion: { (error) in
            if error == nil {
                for layer in (self.mapView.map?.operationalLayers)! {
                    let featureLayer = layer as! AGSFeatureLayer
                    if featureLayer.name == "室蘭市 - 避難場所" {
                        self.featureTable = featureLayer.featureTable
                    }
                }
            }
        })
        
        // 最寄り施設の検出解析の設定
        // ArcGIS Online にサインイン
        let portal = AGSPortal(url: URL(string: "https://www.arcgis.com")!, loginRequired: true)
        portal.load { (error) in
            if let error = error {
                print(error.localizedDescription)
            }
            else {
                self.closestFacilityTask = AGSClosestFacilityTask(url: URL(string: "https://route.arcgis.com/arcgis/rest/services/World/ClosestFacility/NAServer/ClosestFacility_World")!)
                self.closestFacilityTask.credential = portal.credential

                // デフォルトのパラメーターを取得
                self.getDefaultParameters()
            }
        }
        
        // マップビューのタッチ操作のデリゲート
        self.mapView.touchDelegate = self

        // 検索結果のグラフィックを表示するオーバレイを追加
        self.mapView.graphicsOverlays.add(graphicsOverlay)
        
        
    }
    
    private func getDefaultParameters() {
        
        self.closestFacilityTask.defaultClosestFacilityParameters { [weak self] (parameters: AGSClosestFacilityParameters?, error: Error?) in
            
            guard error == nil else {
                print(error?.localizedDescription ?? "")
                return
            }
            
            // デフォルトのパラメーターを検索用のパラメーターとして設定
            self?.closestFacilityParameters = parameters
            
            // マップの操作を有効にする
            self?.mapView.isUserInteractionEnabled = true
            
        }
        
    }
    
    //MARK: - AGSGeoViewTouchDelegate
    func geoView(_ geoView: AGSGeoView, didTapAtScreenPoint screenPoint: CGPoint, mapPoint: AGSPoint) {
        
        // 前回の結果のグラフィックを消去
        self.graphicsOverlay.graphics.removeAllObjects()

        // マップ上をタップした場所にグラフィックを表示
        let incidentSymbol = AGSSimpleMarkerSymbol(style: .circle, color: .blue, size: 10)
        let incidentGraphic = AGSGraphic(geometry: mapPoint, symbol: incidentSymbol, attributes: nil)
        self.graphicsOverlay.graphics.add(incidentGraphic)
        
        // マップ上をタップした場所を最寄り施設検出の解析地点に設定
        let incident = AGSIncident(point: mapPoint)
        self.closestFacilityParameters.setIncidents([incident])

        // マップ上をタップした場所から1km圏内を解析対象エリアにする
        // マップ上をタップした場所から1kmのバッファーを作成
        let queryParams = AGSQueryParameters()
        let buffer = AGSGeometryEngine.bufferGeometry(mapPoint, byDistance: 1000)
        queryParams.geometry = buffer
        queryParams.returnGeometry = true
        // バッファー内にある施設を検索
        self.featureTable.queryFeatures(with: queryParams, completion: { [weak self] (result: AGSFeatureQueryResult?, error: Error?) in
            
            guard error == nil else {
                print(error?.localizedDescription ?? "")
                return
            }
            
            if (result?.featureEnumerator().allObjects.count)! > 0 {
                
                // 検索結果の施設を最寄り施設検出の対象施設に設定
                var facilities: [AGSFacility] = []
                
                for graphic in (result?.featureEnumerator().allObjects)! {
                    if let point = graphic.geometry as? AGSPoint {
                        let facility = AGSFacility(point: point)
                        facilities.append(facility)
                    }
                }

                // パラメーターを設定し、最寄り施設検出解析を実行
                self?.closestFacilityParameters.setFacilities(facilities)
                self?.closestFacility()
            
            }
        })
        
    }
    
    
    func closestFacility () {
        
        self.closestFacilityTask.solveClosestFacility(with: self.closestFacilityParameters, completion: { [weak self] (result: AGSClosestFacilityResult?, error: Error?) in
            
            guard error == nil else {
                print(error?.localizedDescription ?? "")
                return
            }
            
            // 結果から最も近い施設のインデックスを取得
            if let rank = result?.rankedFacilityIndexes(forIncidentIndex: 0) {
                let facilityIncident = rank[0]
                // 最も近い施設までのルートを取得
                let route = result?.route(forFacilityIndex: Int(facilityIncident), incidentIndex: 0)
                // ルートのグラフィックを表示
                let lineSymbol = AGSSimpleLineSymbol(style: .solid, color: UIColor.green, width: 5)
                let graphic = AGSGraphic(geometry: route?.routeGeometry, symbol: lineSymbol, attributes: nil)
                self?.graphicsOverlay.graphics.add(graphic)
            }
            
        })
        
    }


    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }


}


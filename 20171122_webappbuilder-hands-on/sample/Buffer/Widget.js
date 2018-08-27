define([
  'dojo/_base/declare',
  'jimu/BaseWidget',
  'dijit/_WidgetsInTemplateMixin',
  'dojo/_base/lang',
  'dojo/_base/array',
  'jimu/LayerInfos/LayerInfos',
  'esri/geometry/geometryEngine',
  'esri/symbols/SimpleFillSymbol',
  'esri/symbols/SimpleLineSymbol',
  'esri/symbols/SimpleMarkerSymbol',
  'esri/Color',
  'esri/graphic',
  'esri/tasks/query',
  'dijit/form/Select'
], function(declare, BaseWidget, _WidgetsInTemplateMixin, lang, array, LayerInfos, geometryEngine, SimpleFillSymbol, SimpleLineSymbol, SimpleMarkerSymbol, Color, Graphic, Query) {
  return declare([BaseWidget, _WidgetsInTemplateMixin], {

    baseClass: 'jimu-widget-buffer',

    // ウィジェットのパネルを開いたときの処理
    onOpen: function() {
      // レイヤーの一覧をパネルに表示
      this._createLayerlist();

      // マップをクリックしたときのイベントを作成
      this.mapClickFunc = this.map.on('click', lang.hitch(this, this._mapClickEvent));
    },

    // パネルを閉じたときの処理
    onClose: function() {
      // マップ上のグラフィックをクリア
      this.map.graphics.clear();

      // マップのクリック イベントを削除
      this.mapClickFunc.remove();
    },

    _createLayerlist: function() {
      // マップ コンストラクタを取得
      var map = this.map;

      // マップ上のレイヤーを取得し、レイヤー一覧を selectlayerNode に表示
      LayerInfos.getInstance(map, map.itemInfo).then(lang.hitch(this, function(layerInfosObj) {
        var infos = layerInfosObj.getLayerInfoArray();
        var options = [];
        array.forEach(infos, function(info) {
          if (info.originOperLayer.layerType === 'ArcGISFeatureLayer') {
            options.push({
              label: info.title,
              value: info.id
            });
          }
        });
        this.layerSelectNode.set('options', options);
      }));

      this.layerSelectNode.on('change', lang.hitch(this, function(value) {
        this.layerId = value;
      }));
    },

    // マップクリック時のイベントリスナー
    _mapClickEvent: function(evt) {
      // マップ コンストラクタを取得
      var map = this.map;

      // マップのグラフィックスをクリア
      map.graphics.clear();

      // distanceInputNode に入力されたバッファーの距離を取得
      var distance = this.distanceInputNode.value;
      // 構成画面で選択された単位を config.json から取得
      var lengthUnit = this.config.measurement.lengthUnit;

      // バッファー用のジオメトリを作成
      var bufferGeometry = geometryEngine.buffer(evt.mapPoint, distance, lengthUnit);

      // バッファーのグラフィックを作成し、マップのグラフィックス レイヤーに追加
      var sfs = new SimpleFillSymbol(SimpleFillSymbol.STYLE_SOLID,
        new SimpleLineSymbol(SimpleLineSymbol.STYLE_DASHDOT, new Color([255, 0, 0]), 2),
        new Color([255, 255, 0, 0.25])
      );
      var graphic = new Graphic(bufferGeometry, sfs);
      map.graphics.add(graphic);

      // バッファーにあるフィーチャを検索
      var query = new Query();
      query.geometry = bufferGeometry;
      query.spatialRelationship = Query.SPATIAL_REL_CONTAINS;

      // マップ上からレイヤー ID を指定してフィーチャ レイヤーを取得
      var layer = map.getLayer(this.layerId);

      // フィーチャ レイヤーに対してクエリを実行
      layer.queryFeatures(query, function(featureSet) {
        var highlightSymbol;

        // 取得したフィーチャに対して処理を行う
        for (var i=0; i<featureSet.features.length; i++) {

          // ポイント、ライン、ポリゴンごとにシンボルを設定
          if (layer.geometryType === 'esriGeometryPoint') {
            highlightSymbol = new SimpleMarkerSymbol().setColor(new Color('#f00'));
          } else if (layer.geometryType == 'esriGeometryPolygon') {
            highlightSymbol = new SimpleFillSymbol(SimpleFillSymbol.STYLE_SOLID,
              new SimpleLineSymbol(SimpleLineSymbol.STYLE_SOLID, new Color([255,0,0]), 3),
              new Color([125, 125, 125, 0.5])
            );
          } else {
            highlightSymbol = new SimpleLineSymbol(SimpleLineSymbol.STYLE_SOLID,
              new Color([255, 0, 0, 0.5]),
              6
            );
          }

          // クエリ結果をグラフィックス レイヤーに追加
          var queryGraphic = new Graphic(featureSet.features[i].geometry, highlightSymbol);
          map.graphics.add(queryGraphic);
        }
      });
    }

  });
});

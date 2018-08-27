# タスク

フィーチャ レイヤーからフィーチャをクエリする <a href="https://developers.arcgis.com/javascript/3/jsapi/featurelayer-amd.html#queryfeatures" target="_blank">layer.queryFeatures()</a> を使って、バッファー内に含まれるフィーチャを取得してみましょう。  
layer.queryFeatures() の第1引数には、手順 7 で作成したクエリ オブジェクト、第2引数には、クエリ完了時に実行するコールバック関数を渡します。  
コールバック関数に返ってきたクエリの結果結果からフィーチャを取得し、ハイライトするシンボルを<a href="https://developers.arcgis.com/javascript/3/jsapi/simplemarkersymbol-amd.html" target="_blank">ポイント</a>、<a href="https://developers.arcgis.com/javascript/3/jsapi/simplelinesymbol-amd.html" target="_blank">ライン</a>、<a href="https://developers.arcgis.com/javascript/3/jsapi/simplefillsymbol-amd.html" target="_blank">ポリゴン</a>ごとに作成します。  
取得したフィーチャと作成したシンボルを使用して、<a href="https://developers.arcgis.com/javascript/3/jsapi/graphic-amd.html">グラフィック</a>を作成、<a href="https://developers.arcgis.com/javascript/3/jsapi/map-amd.html#graphics" target="_blank">マップに表示</a>します。

# 回答例

```js
_mapClickEvent: function(evt) {
  // ...

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
```

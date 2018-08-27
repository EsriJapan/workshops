# タスク

1. クライアント側でジオメトリ演算を行う <a href="https://developers.arcgis.com/javascript/3/jsapi/esri.geometry.geometryengine-amd.html#buffer" target="_blank">geometryEngine.buffer()</a> を使って、クリック地点からバッファーを作成してみましょう。  
バッファーの距離は distance、距離の単位は lengthUnit が参照する値を使用します。

2. 作成したバッファーをマップ上に表示してみましょう。  
マップに表示するには<a href="https://developers.arcgis.com/javascript/3/jsapi/graphic-amd.html">グラフィック</a>を作成します。  
グラフィックには、表示する位置の情報を持つジオメトリと、表示するスタイル情報を持つシンボルが含まれます。  
ジオメトリは、作成したバッファーです。シンボルは、<a href="https://developers.arcgis.com/javascript/3/jsapi/simplefillsymbol-amd.html" target="_blank">塗りつぶし シンボル</a>を使用して作成します。  
最後に、作成したグラフィックを、<a href="https://developers.arcgis.com/javascript/3/jsapi/map-amd.html#graphics" target="_blank">マップのグラフィックス レイヤー</a>に追加します。

# 回答例

```js
_mapClickEvent: function(evt) {
  // ...

  // バッファー用のジオメトリを作成
  var bufferGeometry = geometryEngine.buffer(evt.mapPoint, distance, lengthUnit);

  // バッファーのグラフィックを作成し、マップのグラフィックス レイヤーに追加
  var sfs = new SimpleFillSymbol(SimpleFillSymbol.STYLE_SOLID,
    new SimpleLineSymbol(SimpleLineSymbol.STYLE_DASHDOT, new Color([255, 0, 0]), 2),
    new Color([255, 255, 0, 0.25])
  );
  var graphic = new Graphic(bufferGeometry, sfs);
  map.graphics.add(graphic);

  // ...
}
```

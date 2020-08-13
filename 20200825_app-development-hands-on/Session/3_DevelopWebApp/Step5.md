## Step5：属性検索の実装  
### 1. 属性検索用のウィジェットを作成
### 2. フィーチャ テーブル ウィジェットに検索結果を表示

<br/>

<b>1. 属性検索 ウィジェットの実装</b>  
水道マップで用意されている各属性を検索します。属性検索用のウィジェットを実装します。

main.js の 255 行目以降にある mapView.when() 内で、Todo: Step5 に対して以下のコードを記述します。
```JavaScript
overView.when(() => {
  mapView.when(() => {
    // Todo: Step4 概観図を表示（概観図ウィジェット）
    overViewMapSet();
    // Todo: Step5 属性検索の設定（属性検索ウィジェット）
    queryTaskSet();
    // Todo: Step5 検索結果の表示（フィーチャ テーブルウィジェット）
    searchFeatureTable();
    // ポップアップの設定
    settingPopupTemplate();
  });
});
```

View の [when()](https://developers.arcgis.com/javascript/latest/api-reference/esri-views-View.html#when) を使用することで、overView と mapView のインスタンスが作成されたあとに queryTaskSet() と searchFeatureTable() を実行するようにしています。  
queryTaskSet() の処理を呼び出しているファイルは、query-task.js となり、searchFeatureTable() の処理を呼び出しているファイルは、search-feature-table.js になります。

次に over-view-map.js を開いて、overViewMapSet() 関数内に以下のコードを記述します。

次にメインの地図の表示を変更 (拡大、縮小など) するたびに表示範囲の枠が連動するように以下のコードを記述します。

```JavaScript
watchUtils.init(mapView, "extent", (extent) => {
  // mapView が更新（地図の表示を変更）した場合は、概観図と同期する
  if (mapView.updating) {
    overView
      .goTo({
        center: mapView.center,
        scale: 
        mapView.scale *
          2 *
          Math.max(
            mapView.width / overView.width,
            mapView.height / overView.height
          )
      })
      .catch((error) => {
        // goto-interrupted エラーは無視する
        if (error.name != "view:goto-interrupted") {
          console.error(error);
        }
      });
  }
  extent3Dgraphic.geometry = extent;
});
```

watchUtils の [init](https://developers.arcgis.com/javascript/latest/api-reference/esri-core-watchUtils.html#init) で、プロパティの変更を監視し、プロパティの初期値で呼び出すようにします。mapView の地図の表示が変更になった場合に mapView の表示範囲 (extent) を取得して、extent3Dgraphic の geometry に extent を設定します。  
また、mapView が 更新 ([updating](https://developers.arcgis.com/javascript/latest/api-reference/esri-views-MapView.html#updating)) された場合に、メインの地図の表示に合わせて、概観図を [goTo](https://developers.arcgis.com/javascript/latest/api-reference/esri-views-MapView.html#goTo) で移動するように設定します。


以下の画面のようにメインの地図と連動した概観図が作成されます。
|![step4_2](./img/app_step4_2.png)|
|:-:|

サンプルの [Overview map](https://developers.arcgis.com/javascript/latest/sample-code/overview-map/index.html) も併せてご参照ください。

Step 4 はここまでです。  

次の Step5 は、「[Step5：属性検索の実装](./Step5#Step5：属性検索の実装)」をご参照ください。

## Step2：地図メイン部の実装
### 演習の目的
- Web マップを使用した地図の作成方法の習得
  - Map と View の関係を理解し、地図の表示を行います。

### 1. Web マップの設定

Web マップを設定して、地図を表示します。
Map と View を作成します。  
main.js を開いて、以下のコードを記述します。
```JavaScript
// TODO: Map の作成
map = new WebMap({
  portalItem: {
    id: "<Web マップ の ID>"
  }
});

// TODO: View の作成
mapView = new MapView({
  container: "mapViewDiv",
  map: map,
  padding: {
    top: 50,
    bottom: 0
  }
});
```

上記の portalItem の id には「[ArcGIS Pro を使用した Web マップの作成](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISPro#arcgis-pro-%E3%82%92%E4%BD%BF%E7%94%A8%E3%81%97%E3%81%9F-web-%E3%83%9E%E3%83%83%E3%83%97%E3%81%AE%E4%BD%9C%E6%88%90)」のハンズオンで作成した Web マップ の ID を指定します。

- Web マップは以下の2パターンで作成しているかと思いますので目的に応じて Web マップの ID を指定してください。

  - ArcGIS Pro をお持ちの方 (ArcGIS Pro を使用した Web マップの作成)
https://bit.ly/2WCVHyQ

  - ArcGIS Pro をお持ちでない方 (ArcGIS Online を使用した Web マップの作成)
https://bit.ly/2Dl74EB

もし、事前に作成ができていない方は、以下の Web マップ の ID（ハンズオン限定で公開）を使用してください。

- 8e285147abe044cb851fbec6a1bed5cd

Map と View を設定したら Live Server を使用して地図を表示します。

### 2. Live Server の起動

Visual Studio Code で、index.html を選択して、マウスの左クリックをクリックすると、一覧が表示されますので、一覧から [Open with Live Server] を右クリックをクリックして、Live Server を起動します。
|<img src="./img/app_step1_1.png" width="600">|
|:-:|

Live Server が起動することで、ローカルでサーバーを立ち上げることができます。
以下の URL でアプリが表示されます。   
http://127.0.0.1:5500/calcite-maps/index.html

ここで、ArcGIS Online にログインするため、ご自身のユーザー名、パスワードを入力して、[OK] ボタンをクリックします。
|<img src="./img/app_step1_2.png" width="600">|
|:-:|

すると、Web Map を参照して地図が表示されます。
|<img src="./img/app_step1_3.png" width="600">|
|:-:|

Step 2 はここまでです。  

### Step2 のまとめ
Map と View の関係は、Map はレイヤーを管理し、View はスクリーン上に Map を描画します。マップは、作成した Map を View に設定することでページに表示されます。  
また、Map の作成には、Map クラスを使用してベースマップなどを指定して
作成する場合と上記のように Web マップを指定して利用する方法があります。

```JavaScript
// コードで Map を作成
const map = new Map({
    basemap: "topo",
    layers: [......]
});

// Web マップを利用
// ポータル（ArcGIS Online/ArcGIS Enterprise）と連携
const map = new WebMap({
    portalItem: {
        id: "<Web マップ の ID>"
    }
});
```

マップの作成に関しての詳細は、API リファレンスの Map、WebMap、View をご参照ください。

- API リファレンス
  - [Map](https://developers.arcgis.com/javascript/latest/api-reference/esri-Map.html)
  - [WebMap](https://developers.arcgis.com/javascript/latest/api-reference/esri-WebMap.html)
  - [View](https://developers.arcgis.com/javascript/latest/api-reference/esri-views-MapView.html) 

<br/>

参照している Web マップは、演習の [Python スクリプトによるホスト フィーチャ レイヤーのアップデート](https://github.com/EsriJapan/workshops/blob/master/20200825_app-development-hands-on/Session/1_BuildData/Python/0826/update_fl.md#%E6%BC%94%E7%BF%92%E3%81%AE%E7%9B%AE%E7%9A%84)で行ったようにデータ更新の自動化・定期実行でデータ完全性を保っているため、Web アプリで参照するデータは常に最新のデータを参照することができます。

次の Step3 は、「[Step3：標準ウィジェットの実装](./Step3.md#Step3標準ウィジェットの実装) 」をご参照ください。
   

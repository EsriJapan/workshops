## Step1：ハンズオンの準備
### 演習の目的
- 開発環境の構築方法の習得
  - ハンズオンで使用するソースコードの配置を行い、Visual Studio Code で開いてソースコードの表示を行います。

### 1. 開発の準備

開発には、Visual Studio Code を使用し、拡張機能の Live Server を使用します。開発環境の構築には、環境構築の [ArcGIS プラットフォームを活用したWebアプリ開発ハンズオン](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Environment#arcgis-プラットフォームを活用したwebアプリ開発ハンズオン)をご参照ください。

### 2. アプリの配置

[本ハンズオンで使用するアプリはこちらでダウンロードできます](https://github.com/tkamiya0625/workshops/raw/master/20200825_app-development-hands-on/Session/3_DevelopWebApp/HandsOn_WebApp.zip)

上記リンクでダウンロードする HandsOn_WebApp.zip を任意の場所（作業フォルダ等）に配置して解凍してください。HandsOn_WebApp.zip 内の calcite-maps フォルダがプロジェクトです。  

- ハンズオン用のアプリ
  - HandsOn_WebApp.zip\source\exercise\calcite-maps

- 解答用のアプリ
  - HandsOn_WebApp.zip\source\answer\calcite-maps

上記のダウンロードしたアプリ (ハンズオン用) を Visual Studio Code を使用して開きます。

① ファイル＞フォルダーを開くを選択します。
|<img src="./img/app_deployment_1.png" width="600">|
|:-:|

② ダウンロードした Hands-On > source > exercise を選択して、フォルダーの選択をクリックして、開きます。
|<img src="./img/app_deployment_2.png" width="600">|
|:-:|

③ ./calcite-maps/js/main.js を開いてください。
|<img src="./img/app_deployment_3.png" width="600">|
|:-:|

これで準備は完了です。

### 3. Calcite-Maps ​の構成

- calcite-maps ​
  - css・・・bootstrap を定義した CSS ファイル、本アプリで使用するために定義した CSS ファイル​
  - fonts・・・ bootstrap が使用できる fonts ファイル​
  - js​
    - jquery ・・・・・・・・・bootstrap のコンポーネント群​
    - main.js ・・・ ・・・・地図メイン部（地図表示、標準ウィジェットの作成）​
    - over-view-map.js ・・・・概観図の設定と表示​
    - query-task.js ・・・ ・・・属性検索用のウィジェットを作成​
    - search-feature-table.js ・・属性検索の実行と結果をフィーチャテーブル ウィジェットに表示​
    - setting-popup-template.js ・・ポップアップの設定
  - index.html・・・ bootstrap の要素を適用した HTML ファイル

### Step1 のまとめ
開発環境の構築でご紹介した Visual Studio Code は、多くの便利なツールがございます。GeoNet ブログの [Web マッピング アプリ開発に便利なツールをご紹介：～Visual Studio Code で開発環境を構築～](https://community.esri.com/docs/DOC-13101)でも紹介していますのでご参照ください。

次の Step2 は、「[Step2：地図メイン部の実装](./Step2.md#Step2地図メイン部の実装)」をご参照ください。

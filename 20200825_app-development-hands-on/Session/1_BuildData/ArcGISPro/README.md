# ArcGIS Pro を使用した Web マップの作成

<span style="color: red; ">※ArcGIS Pro をお持ちでない方は[ArcGIS API for Python を使用した Web マップの作成](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISPro/PythonAPIVersion)をご参照ください。</span>

## ArcGIS Pro 起動

1. EJWater フォルダにある EJWater.aprx をダブルクリックします。

   <img src="./img/project.png" width="300px">  

2. ArcGIS Pro が起動します。

   <img src="./img/pro_initial_state.png" width="500px">

## Web マップの作成

1. 「共有」タブ内の「Webマップ」ボタンを押下します。

   <img src="./img/pro_share.png" width="500px">

2. 「Web マップとして共有」パネルの「概要」「タグ」項目を入力し（内容は任意）、「共有」ボタンを押下します。

   ※「名前」項目を変更しても構いません。

   <img src="./img/pro_webmap_upload.png" width="300px"></br>

3. Web マップの作成が完了します。

   <img src="./img/pro_webmap_upload_finish.png" width="300px">

## ArcGIS Online で Web マップを参照

1. [ArcGIS Online](https://www.esrij.com/products/arcgis-online/) にアクセスします。

   <img src="./img/agol.png" width="500px">

2. 画面上部の「コンテンツ」を押下します。

   <img src="./img/agol_menu.png" width="800px">

3. アップロードした Web マップを押下します。

   <img src="./img/agol_content.png" width="500px">  

4. Web マップの概要画面に遷移後、サムネイルを押下します。

   <img src="./img/agol_web_map.png" width="500px">  

5. Web マップ が参照可能になります。

   <img src="./img/agol_view.png" width="500px">

## まとめ
ArcGIS Pro を使用することで FGDB などのローカルデータから簡単に Web マップを作成することができます。また、ArcGIS API for Python, ArcGIS Runtime SDK, ArcGIS API for JavaScript などの API を使用すれば 作成した Web マップを参照することができます。次のセッションでは ArcGIS API for Python を使用して Web マップ を扱ってみようと思います。

### １日目(2020/8/25)
作成した Web マップ と ArcGIS API for Python を使用して現地調査用データを作成

### 2日目(2020/8/26)
ArcGIS API for Python を使用して作成した Web マップ を更新
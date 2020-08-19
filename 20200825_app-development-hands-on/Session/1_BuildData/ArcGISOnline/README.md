# Web GIS 基礎 ~ ArcGIS Online を使ってみよう ~

## 演習の目的
1. ArcGIS Online にデータをアップロードする
2. ArcGIS Online のサービスの一つである、Living Atlas を使ってみる

## ①CSV ファイルをアップロードしてみよう
### ArcGIS Online へアクセス
1. [ArcGIS Online](https://www.arcgis.com/home/index.html) にアクセスして「サインイン」を押下してください。

   <img src="./img/agol.png" width="500px">

2. ログイン情報を入力し、ArcGIS Onlineにログインしてください。

   <img src="./img/agol_login.png" width="300px">

### フィーチャ レイヤーの作成
マップにレイヤーが格納されているように、Web マップにも同じようにレイヤーを格納する必要があります。Web マップを作成する前にまずはレイヤーを作ってみましょう。

1. [アップロードするデータ](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISOnline/data/%E5%90%84%E5%BA%97%E8%88%97%E5%A3%B2%E4%B8%8A.zip) をダウンロードして、解凍してください。

2. 「アイテムの追加」 > 「コンピューターから」を押下します。

   <img src="./img/add_item.png" width="300px">

3. 以下を入力し、「アイテムの追加」ボタンを押下します。

   「ファイル」・・・「1」で解凍したデータ（csv）  
   「タイトル」・・・各店舗売上  
   「タグ」・・・各店舗売上  
   「フィーチャの検索」・・・座標  
   「場所フィールド（フィールド名：経度）」・・・経度  
   「場所フィールド（フィールド名：緯度）」・・・緯度  

   <img src="./img/add_item2.png" width="300px"></br>
  
   <img src="./img/add_item3.png" width="300px">  

### フィーチャ レイヤーの参照
1. フィーチャ レイヤー 作成後、サムネイルを押下します。

   <img src="./img/thumnail.png" width="300px">

2. フィーチャ レイヤーを参照することができます。

   <img src="./img/viewer.png" width="300px">

### フィーチャ レイヤーのフィルタリング
1. 「詳細」を押下します。

   <img src="./img/viewer_detail.png" width="300px">

2. 「フィルター」を押下します。

   <img src="./img/filter.png" width="300px">

3. 「売上＝3000000」となるようにフィルターの設定をし、「フィルターの適用ボタン」を押下します。

   <img src="./img/filter2.png" width="300px">

4. 「テーブルの表示」を押下します。

   <img src="./img/filter3.png" width="300px">

5. フィルターが正しく設定されていることがわかります。

   <img src="./img/filter4.png" width="300px">

## ②Living Atlas を使ってみよう
### Living Atlas へアクセス
1. 画面右上の「Living Atlas」ボタンを押下します。

   <img src="./img/agol_living_atlas.png" width="300px">

2. Living Atlas 表示後、検索バーに「全国市区町村界マップ2020」と入力し、Enterキーを押下します。

   <img src="./img/living_atlas_japan.png" width="300px">

### Web マップ参照
1. 検索結果の表示後、「全国市区町村界マップ2020」を押下します。

   <img src="./img/living_atlas_japan_search.png" width="300px">

2. 「全国市区町村界マップ2020」 Web マップが起動します。

   <img src="./img/agol_japan_mapview.png" width="300px">

### Web マップのレンダリング
1. 「コンテンツ」を押下します。

   <img src="./img/map_content.png" width="300px">

2. 「スタイルの変更」を押下します。

   <img src="./img/change_style.png" width="300px">

3. 「表示する属性を選択」で「P_NUM」(人口数)を選択します。

   <img src="./img/change_style2.png" width="300px">

4. 数値分類でフィーチャがレンダリングされていることがわかります。

   <img src="./img/change_style3.png" width="300px">

5. 「オプション」を押下します。

   <img src="./img/change_style4.png" width="300px">

6. 「シンボル」を押下します。

    <img src="./img/change_style5.png" width="300px">

7.  任意の色を選択して「OK」を押下してください。

    <img src="./img/change_style6.png" width="300px">

8. シンボルの色が変更されたことがわかります。
   
   <img src="./img/change_style7.png" width="300px">

9. 下にある「全国都道府県界データ2020」のレイヤーを非表示にします。

   <img src="./img/layer_off.png" width="300px">

10. 「表示する縮尺範囲」を以下のように設定します。

   <img src="./img/scale_filter.png" width="300px">

### Web マップの保存
1. 「名前を付けて保存」を押下します。

   <img src="./img/save.png" width="300px">

2. 以下を入力し、「マップの保存」ボタンを押下します。

   「タイトル」・・・全国市区町村界マップ2020_開発塾   
   「サマリー」・・・ 全国市区町村界マップ2020にレンダリングを設定  

3. 「コンテンツ」を押下します。

   <img src="./img/content.png" width="300px">

4. 「全国市区町村界マップ2020_開発塾 」がWebマップとして作成されます。これを押下します。

   <img src="./img/webmap.png" width="300px">

### データ共有設定の変更
1. 「編集」を押下します。

   <img src="./img/sharing.png" width="300px">


2. 「すべての人に公開(パブリック)」を選択し、「保存」を押下します。

   <img src="./img/private2public.png" width="300px">

3. 共有が「すべての人に公開(パブリック)」になっていることを確認します。

   <img src="./img/public.png" width="300px">

## まとめ
ArcGIS Online を使用すれば、簡単に Web GIS 上にデータをアップロードしたり、パブリックに公開されているデータを自由に使用することができます。また、ArcGIS Online にアップロードしたデータを ArcGIS API for Python, ArcGIS Runtime SDK, ArcGIS API for JavaScript などを API で操作することもできます。  

次のセッションでは Web マップの作成を実際に行ってみようと思います。
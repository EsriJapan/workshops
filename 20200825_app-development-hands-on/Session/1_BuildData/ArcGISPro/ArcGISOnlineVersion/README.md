# ArcGIS Online を使用した Web マップの作成

<span style="color: red; ">※ArcGIS Pro をお持ちの方は[ArcGIS Pro を使用した Web マップの作成](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISPro)をご参照ください。</span>

## 演習の目的
1. FGDB をもとにした ArcGIS 上での Web マップの作成方法の習得
2. 作成した Web マップを ArcGIS Online で参照する方法の習得

## ArcGIS Online にアクセス

1. [ArcGIS Online](https://www.arcgis.com/home/index.html) にアクセスして「サインイン」を押下してください。

   <img src="./img/agol.png" width="300px">

2. 画面上部の「コンテンツ」を押下します。

   <img src="./img/agol_menu.png" width="500px">

## フィーチャ レイヤーの作成
Web マップに格納するためのレイヤーを作成します。

1. 「作成」ボタンを押下します。

   <img src="./img/create_item.png" width="300px">

2. 「フィーチャ レイヤー」を押下します。

   <img src="./img/create_featurelayer.png" width="300px">
   
3. 「URLから」を選択し、「URL」に以下を入力し、「次へ」ボタンを押下します。

   「URL」・・・`https://services.arcgis.com/wlVTGRSYTzAbjjiC/arcgis/rest/services/%E6%97%A5%E5%90%89%E6%B0%B4%E9%81%93%E3%83%9E%E3%83%83%E3%83%97_WFL1/FeatureServer`  

   ※入力する URL はあらかじめパブリックに公開しておいたデータです。これをテンプレートにして空のフィーチャ レイヤーを作成します。  
   ※この作業をすることで、複数のレイヤーをレンダリング設定を含めて一括でコピーすることができます。

   <img src="./img/create_featurelayer2.png" width="500px">

4. 「次へ」ボタンを押下します。

   <img src="./img/create_featurelayer3.png" width="500px">

5. 以下を入力し「次へ」ボタンを押下します。

   「左」・・・139.636  
   「右」・・・139.652  
   「上」・・・35.556  
   「下」・・・35.552  

   <img src="./img/create_featurelayer4.png" width="300px">

6. 以下を入力し「完了」ボタンを押下します。

   「タイトル」・・・日吉水道マップ_WFL1  
   「タグ」・・・日吉水道マップ  

   <img src="./img/create_featurelayer5.png" width="300px">

7. フィーチャレイヤーが作成されます。

   <img src="./img/create_featurelayer6.png" width="300px">

## フィーチャレイヤーにデータを追加
1. [GDB](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISPro/ArcGISOnlineVersion/data/EJWater.gdb.zip)をダウンロードします。

2. 「データ更新」＞「データをレイヤーに追加」を押下します。

   <img src="./img/add_data.png" width="500px">

3. 以下を入力し、「アップロードと継続」ボタンを押下します。

   「ファイル名」・・・「1」でダウンロードした zip  
   「コンテンツ」・・・ファイルジオデータベース

   <img src="./img/data_append.png" width="500px">

   ※以下エラーが出てきたら「OK」を押してください（特に問題はありません）。

   <img src="./img/append_error.png" width="300px">

4. 「データの追加先のレイヤーを選択」「更新済みデータを含むEJWWater.gdb.zipからレイヤーを選択」に同じ値を設定し、「更新の適用」ボタンを押下します。

   <img src="./img/select_layer.png" width="500px">

   ※以下エラーが出てきたら「OK」を押してください（特に問題はありません）。

   <img src="./img/append_error.png" width="300px">

5. 「2」~「4」の手順を以下レイヤーに対して繰り返します。

   「漏水」  
   「メータ」  
   「弁」  
   「管」  
   「給水管」  
   「図郭_500」  

   ※本手順ではハンズオンの時間の関係上、レイヤーを抜粋していますが、時間に余裕がありましたら全レイヤーに対して処理していただいても構いません。

## Web マップを作成
1. サムネイルを押下します。

   <img src="./img/select_thumnail.png" width="300px">

2. 「名前を付けて保存」を押下します。

   <img src="./img/save_webmap.png" width="300px">

3. 以下を入力し、「マップの保存」を押下します。

   「タイトル」・・・日吉水道マップ    
   「タグ」・・・開発塾2020    
   「サマリー」・・・日吉水道マップ  

   <img src="./img/save_map.png" width="300px">

4. 「コンテンツ」を押下します。

   <img src="./img/home.png" width="300px">

5. Web マップが作成されていることを確認します。

   <img src="./img/webmap.png" width="300px">

## Web マップを参照
1. アップロードした Web マップを押下します。

   <img src="./img/agol_content.png" width="300px">  

2. 後のセッションで使用するので、赤枠の部分をコピーして控えておいてください（=より後の部分をコピーしてください）。

   <img src="./img/web_id.png" width="500px">  

3. Web マップの概要画面に遷移後、サムネイルを押下します。

   <img src="./img/agol_web_map.png" width="300px">  

4. Web マップ が参照可能になります。

   <img src="./img/agol_view.png" width="300px">

## まとめ
ArcGIS Online 上で FGDB などから簡単に Web マップを作成することができます。そして、ArcGIS API for Python, ArcGIS Runtime SDK, ArcGIS API for JavaScript などの API を使用すれば作成した Web マップを参照することができます。

次のセッションでは ArcGIS API for Python を使用して Web マップ を扱ってみようと思います。

※Web マップが作成できなかった場合、「ArcGIS Runtime SDK for .NET を使用して現地調査アプリを開発してみよう！」（1日目）、「ArcGIS API for JavaScript を使用して Web アプリを開発してみよう！」(2日目) のセッションでは、こちらで用意している Web マップを使用していただきますが、以下のセッションにつきましては聴講のみとさせていただきますので、ご了承ください。

### １日目(2020/8/25)
作成した Web マップ と ArcGIS API for Python を使用して現地調査用データを作成

### 2日目(2020/8/26)
ArcGIS API for Python を使用して作成した Web マップ を更新
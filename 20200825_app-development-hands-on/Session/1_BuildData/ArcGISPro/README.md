# ArcGIS Pro を使用した Web マップの作成

<span style="color: red; ">※ArcGIS Pro をお持ちでない方は[ArcGIS Online を使用した Web マップの作成](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISPro/ArcGISOnlineVersion)をご参照ください。</span>

## 演習の目的
1. ArcGIS Pro を使用した Web マップ作成方法の習得
2. 作成した Web マップを ArcGIS Online で参照する方法の習得

## ArcGIS Pro 起動

1. EJWater フォルダにある EJWater.aprx をダブルクリックします。

   <img src="./img/project.png" width="300px">  

2. ArcGIS Pro が起動します。

   <img src="./img/pro_initial_state.png" width="500px">

3. サインインしていない場合はサインインしてください。

   <img src="./img/signin.png" width="150">

   ※もし、サインインができない場合は、[ArcGIS Online を使用した Web マップの作成](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Session/1_BuildData/ArcGISPro/ArcGISOnlineVersion)の方で Web マップを作成してください。</span>

## Web マップの作成

1. 「共有」タブ内の「Webマップ」ボタンを押下します。

   <img src="./img/pro_share.png" width="500px">

2. 「Web マップとして共有」パネルの「概要」「タグ」項目を以下のように入力し、「共有」ボタンを押下します。

   「概要」・・・日吉水道マップ  
   「タグ」・・・開発塾2020  

   <img src="./img/pro_webmap_upload.png" width="300px"></br>

   ※同一組織内で同じアイテムを公開する場合は名前を変更する必要があります。

3. Web マップの作成が完了します。

   <img src="./img/pro_webmap_upload_finish.png" width="300px">

## ArcGIS Online で Web マップを参照
1. [ArcGIS Online](https://www.arcgis.com/home/index.html) にアクセスして「サインイン」を押下してください。

   <img src="./img/agol.png" width="500px">

2. ログイン情報を入力し、ArcGIS Online にログインしてください。

   <img src="./img/login.png" width="300px">

3. 画面上部の「コンテンツ」を押下します。

   <img src="./img/agol_menu.png" width="500px">

4. アップロードした Web マップを押下します。

   <img src="./img/agol_content.png" width="500px">  

5. 「作成した Web マップ と ArcGIS API for Python を使用して現地調査用データを作成」（1日目）、「ArcGIS API for Python を使用して作成した Web マップ を更新」（2日目）、「ArcGIS API for JavaScript を使用して Web アプリを開発してみよう！」(2日目) のセッションで使用するので、赤枠の部分をコピーして控えておいてください（=より後の部分をコピーしてください）。

   <img src="./img/web_id.png" width="500px">  

6. Web マップの概要画面に遷移後、サムネイルを押下します。

   <img src="./img/agol_web_map.png" width="500px">  

7. Web マップ が参照可能になります。

   <img src="./img/agol_view.png" width="500px">

   ※もし演習が早く終わった場合は ArcGIS Online の各種機能をお試しください。

## まとめ
ArcGIS Pro を使用することで FGDB などのローカルデータから簡単に Web マップを作成することができます。そして、ArcGIS API for Python, ArcGIS Runtime SDK, ArcGIS API for JavaScript などの API を使用すれば 作成した Web マップを参照することができます。次のセッションでは ArcGIS API for Python を使用して Web マップ を扱ってみようと思います。

※Web マップが作成できなかった場合、「ArcGIS Runtime SDK for .NET を使用して現地調査アプリを開発してみよう！」（1日目）「ArcGIS API for JavaScript を使用して Web アプリを開発してみよう！」(2日目)のセッションでは、こちらで用意している Web マップを使用していただきますが、以下のセッションにつきましては聴講のみとさせていただきますので、ご了承ください。

### １日目(2020/8/25)
作成した Web マップ と ArcGIS API for Python を使用して現地調査用データを作成

### 2日目(2020/8/26)
ArcGIS API for Python を使用して作成した Web マップ を更新
# 環境構築
ArcGIS 開発者のための最新アプリ開発塾 2020 にご参加する方は以下手順に沿って環境構築をお願いします。

## ArcGIS プラットフォームを活用したデータ構築
### データの配置
1. [ハンズオン用データ](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/HandsOn_Data.zip)をダウンロードし、解凍してください。

2. 解凍したデータをDドライブ直下に配置してください（D:\EJWater となるように配置してください）。

   ※Cドライブや任意のディレクトリに配置していただいても構いませんが、配置先にあわせて EJWater フォルダにある「project.config」の4,6行目を書き換えて頂く必要があります（以下画像参照）。

   <img src="./img/data_config2.png" width="500px">

3. 「project.config」ファイルの 5行目の value をご自身の Python 環境に合わせて変更してください（例：C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\pythonw.exe）

   <img src="./img/data_config.png" width="500px">

### ArcGIS Pro の環境設定
1. ArcGIS Pro をインストールしてください。

   ArcGIS Pro をお持ちでない方は[トライアルライセンス](https://www.esrij.com/form/arcgis/trials/)を配布していますので、そちらを使ってインストールしてください。ArcGIS Pro をすでにお持ちの方はそちらをご使用いただいて構いません（ただし、バージョンは2.5にしていただく必要があります）。

2. [ArcGIS Pro SDK](https://pro.arcgis.com/en/pro-app/sdk/) で作成された[アドイン](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/Environment/Addin.zip)をダウンロードし、解凍してください。

3. アドインファイルをダブルクリックし、インストールしてください。

   <img src="./img/prosdk.png" width="500px">

4. D:\EJWater\EJWater.aprx をダブルクリックし、起動確認をしてください。

   <img src="./img/pro_boot.png" width="500px">

5. 「オフラインデータ」タブが存在し、リボン上のアイテムが活性状態であることを確認してください。

   <img src="./img/pro_addin.png" width="500px">

6. また、「図郭」コンボボックスに以下のように値が格納されていることを確認してください。

   <img src="./img/pro_zukaku.png" width="500px">

### ArcGIS Online へのアクセス確認
1. [ArcGIS Online](https://www.esrij.com/products/arcgis-online/) にアクセスしてください。

   <img src="./img/agol.png" width="500px">

2. ログイン情報を入力し、ログインができることを確認してください。

   ※ログイン情報の作成に関しては[ArcGIS for Developers 開発者アカウント](https://esrijapan.github.io/arcgis-dev-resources/guide/create-map/get-dev-account/)参照してください。

   <img src="./img/agol_login.png" width="300px">

### ArcGIS API for Python の環境設定

## ArcGIS プラットフォームを活用した現地調査アプリ開発ハンズオン

### ArcGIS Runtime SDK for .NET の環境設定
ハンズオンでは ArcGIS Runtime SDK for .NET Ver 100.8 を使用いたします。  
ArcGIS Runtime SDK for .NET Ver 100.8 のシステム要件につきましては以下のサイトに記載されておりますのでご確認ください。  
・[ArcGIS Runtime SDK システム要件](https://developers.arcgis.com/net/latest/wpf/guide/system-requirements.htm)  

ArcGIS Runtime SDK for .NET につきましては、開発環境としてVisual Studio が必要となります。  
以下サイトよりダウンロード、およびインストールをお願いいたします。  
・[Visual Studio ダウンロードページ](https://visualstudio.microsoft.com/ja/downloads/)

## ArcGIS プラットフォームを活用したWebアプリ開発ハンズオン

### ArcGIS API for JavaScript の環境設定
1. [Visual Studio Code](https://code.visualstudio.com/) をインストールしてください。

2. [Google Chrome](https://www.google.com/chrome/?brand=CHBD&gclid=EAIaIQobChMIqszkqJLO6gIVCFdgCh1CbgnjEAAYASAAEgIfY_D_BwE&gclsrc=aw.ds)(最新バージョン)をインストールしてください。

# 環境構築
ArcGIS 開発者のための最新アプリ開発塾 2020 にご参加する方は以下手順に沿って環境構築をお願いします。

## ArcGIS プラットフォームを活用したデータ構築
### ①ArcGIS Online へのアクセス確認
1. [ArcGIS Online](https://www.esrij.com/products/arcgis-online/) にアクセスしてください。

   <img src="./img/agol.png" width="500px">

2. ログイン情報を入力し、ログインができることを確認してください。

   ※ログイン情報の作成に関しては[ArcGIS for Developers 開発者アカウント](https://esrijapan.github.io/arcgis-dev-resources/guide/create-map/get-dev-account/)参照してください。

   <img src="./img/agol_login.png" width="300px">

3. 日本語表示にされる方は Settings ボタン押下語、Language を 日本語に変更してください(ハンズオンは日本語表示で進めます)。  

   <img src="./img/agol_setting.png" width="300px"></br>
  
   <img src="./img/agol_language.png" width="300px">

## ArcGIS Pro をお持ちの方のみ「②」「③」「④」をご対応願います。
### ②データの配置
1. [ハンズオン用データ](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/Environment/HandsOn_Data.zip)をダウンロードし、解凍してください。

   ※データは差し替える可能性がありますので、あらかじめご了承願います。

2. 解凍したデータをDドライブ直下に配置してください（D:\EJWater となるように配置してください）。

   ※Cドライブや任意のディレクトリに配置していただいても構いませんが、配置先にあわせて EJWater フォルダにある「project.config」の4,6行目を書き換えて頂く必要があります（以下画像参照）。

   <img src="./img/data_config2.png" width="500px">

3. 「project.config」ファイルの 5行目の value をご自身の Python 環境に合わせて変更してください（例：C:\Program Files\ArcGIS\Pro\bin\Python\envs\arcgispro-py3\pythonw.exe）

   <img src="./img/data_config.png" width="500px">

### ③ArcGIS Pro の環境設定
1. ArcGIS Pro 2.5 を用意してください。

2. [ArcGIS Pro SDK](https://pro.arcgis.com/en/pro-app/sdk/) で作成された[アドイン](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/Environment/Addin.zip)をダウンロードし、解凍してください。

3. アドインファイルをダブルクリックし、インストールしてください。

   <img src="./img/prosdk.png" width="500px">

4. D:\EJWater\EJWater.aprx をダブルクリックし、ArcGIS Pro が起動することを確認してください。

   <img src="./img/pro_boot.png" width="500px">

5. 「オフラインデータ」タブが存在し、リボン上のアイテムが活性状態であることを確認してください。

   <img src="./img/pro_addin.png" width="500px">

6. また、「図郭」コンボボックスに以下のように値が格納されていることを確認してください。

   <img src="./img/pro_zukaku.png" width="500px">

### ④ArcGIS API for Python の環境設定
1. ArcGIS Pro をインストールし起動したら [設定] をクリックします

   <img src="./img/pro_setting.png" width="500px">

2. [Python] をクリックし、Python パッケージ マネージャーを開きます

   <img src="./img/pro_setting_python.png" width="500px">

3. [環境の管理] をクリックし、デフォルト環境である [arcgispro-py3] をクローンします。任意の環境名を指定 (ここでは [app-dev-2020]) して、クローンを開始しましょう。

   <img src="./img/pro_clone.png" width="500px">

4. クローンが完了したら、作成された環境のラジオボタンを押して環境を切り替え、ArcGIS Pro を閉じましょう。

   <img src="./img/pro_change_env.png" width="500px">

5. 次にPython コマンド プロンプトを開きます。スタートメニュー>すべてのプログラム> ArcGIS> Python コマンドプロンプトから開くことができます。

   <img src="./img/python_cmd.png" width="500px">

6. Python コマンド プロンプトが開いたら、環境が先ほど作成した app-dev-2020 であることを確認したら以下のコマンドを実行します。

   `conda install -c esri arcgis=1.8.1 --no-pin`

   <img src="./img/install-python.png" width="500px">

   各種パッケージのインストールについて確認されるので、y + Enter でインストールを進めます。
   
8. インストールが完了したらディレクトリを任意の場所に移動し、次のコマンドで Jupyter Notebook を起動します。

   `jupyter notebook`

   Chrome、Firefox、Chromium 版の Edge 等のモダン ブラウザーを使用してください。
   IE 等で開いてしまった場合は以下画像部分の URL を Chrome 等の アドレスバーにコピーして開いてください。

   <img src="./img/jupyter-notebook.png" width="500px">

9. Jupyter Notebook が開いたら以下画像の順にクリックし、新しいノートブックを開きます。

   <img src="./img/open-jupyter-nb.png" width="500px">

   ノートブックが開いたら、セルに次のコードを入力後、Shift + Enter で実行して、マップが表示されることを確認してください。
   ```
      from arcgis.gis import GIS
      gis = GIS("https://www.arcgis.com/", "ユーザー名", "パスワード")
      m = gis.map()
      m
   ```
   マップが表示されたら正常に環境がこうちくされています。
   <img src="./img/map.png" width="500px">

## ArcGIS プラットフォームを活用した現地調査アプリ開発ハンズオン

### ⑤ArcGIS Runtime SDK for .NET の環境設定
ハンズオンでは ArcGIS Runtime SDK for .NET バージョン 100.8 を使用いたします。  
ArcGIS Runtime SDK for .NET バージョン 100.8 のシステム要件につきましては以下のサイトに記載されておりますのでご確認ください。  
・[ArcGIS Runtime SDK システム要件](https://www.esrij.com/products/arcgis-runtime-sdk-for-dotnet/environments/100_8_0/)  

ArcGIS Runtime SDK for .NET バージョン 100.8 につきましては、開発環境として Visual Studio 2019 16.0 以上（ [.NETデスクトップ開発] ワークロード のインストール）が必要となります。  
以下サイトよりダウンロード、およびインストールをお願いいたします。  
・[Visual Studio ダウンロードページ](https://visualstudio.microsoft.com/ja/downloads/)  
・[Visual Studio インストール手順](https://docs.microsoft.com/ja-jp/visualstudio/install/install-visual-studio?view=vs-2019)  

## ArcGIS プラットフォームを活用したWebアプリ開発ハンズオン

### ⑥ArcGIS API for JavaScript の環境設定
#### 1. 開発環境

ハンズオンでは [Visual Studio Code](https://azure.microsoft.com/ja-jp/products/visual-studio-code/) を使用します。  
ご使用の開発環境に応じて[ダウンロード](https://code.visualstudio.com/download)行い、インストールを行ってください。

また、本ハンズオンでは、Visual Studio Code の拡張機能を使用しますので、以下の手順に従って Live Server のインストールを行ってください。

#### 2. Live Server
Live Server を使用することによって、簡単にローカルサーバを立てることができます。また、ファイルを更新すると自動的にブラウザを更新します。

※ 既にインストール済みの方は不要です。

1．Visual Studio Code を起動してください。
<img src="./img/vs_code_start.png" width="600px"></br>

2．拡張機能ボタンを押下してください。
<img src="./img/vs_code_extension.png" width="600px"></br>

3．検索バーに Liver Server と入力し、Live Server のインストールをしてください。
<img src="./img/vs_code_liveServer.png" width="600px"></br>

#### 3. ArcGIS API for JavaScript Snippets 
ArcGIS API for JavaScript のコード スニペットとして、ArcGIS API for JavaScript Snippets を提供しています。開発を効率に行う上でも便利な機能ですので、必須ではありませんが、インストールをお勧めします。

※ 既にインストール済みの方は不要です。

ArcGIS API for JavaScript Snippets のインストール方法や使い方に関しては、GeoNet ブログの [Web マッピング アプリ開発に便利なツールをご紹介（Visual Studio Code のコード スニペット）](https://community.esri.com/docs/DOC-13834) をご参照ください。
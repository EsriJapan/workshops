# アプリのデプロイ
DataCollection アプリは、参照するデータや、ArcGIS Online  へ接続するための情報といった利用者や用途によって変更が必要な情報を 設定ファイル（Configration.xml）として定義しています。この設定ファイルに記載されている Web マップの情報などを変更することにより、他の業務アプリとして、汎用的にアプリを利用することも可能です。

以下が、Configration.xml に設定を行う情報となります。
- 参照する Web マップの URL
- ArcGIS Online から発行された クライアント ID
- ArcGIS Online から発行された リダイレクト URL

このドキュメントはアプリをデプロイするための以下の手順を示しています。
1. [Web マップ の URL 取得と設定](#1-web-マップの-url-取得と設定)
1. [認証情報の取得](#2-認証情報の取得と設定)
    - 2_1. [アプリの登録](#2_1-アプリの登録)
    - 2_2. [クライアント ID とリダイレクト URL の発行](#2_2-クライアント-id-とリダイレクト-url-の発行)
1. [アプリのビルド](#3アプリのビルド)

## 1. Web マップの URL 取得と設定
ArcGIS Online にアクセスして、コンテンツ画面から Web マップの作成で作成したデータの概要ページを開きURLをコピーします。

![ArcGIS Online アイテム詳細画面](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_webmapUrl.png)

コピーしたURL を Configration.xml に設定します。

ファイルの保存先：HandsOn_Data\02_Data_Collection_Application\data-collection-dotnet-master_ejLocalized\src\DataCollection.Shared\Properties\ Configuration.xml
Configration.xmlをテキストエディター等で開き、上記でコピーしたURLを、次の XML タグに設定します。

- \<WebmapURL> Web マップの URL \</WebmapURL>

## 2. 認証情報の取得と設定
ArcGIS for Developers で「アプリの登録」を行い、認証に必要な クライアント ID と リダイレクト URL を発行します。アプリの登録は、ArcGIS for Developers のダッシュボード画面から行います。

### 2_1. アプリの登録
ユーザーアイコンの左側にある「App Launcher」から「Developers」のアイコンを選択し、「New Application」ボタンを選択します。

![ダッシュボード画面へ遷移](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_devLauncher.gif)

Create New Applicationのページでは、次の 3 つを入力し「Register New Application」ボタンを押下します。 アプリの登録を完了し、クラアイント ID を取得します。

※入力例
- title：DataCollecton
- tag：test
- Description：データコレクションアプリ

注意：Description 以外は半角英数字で入力します

![アプリの登録画面](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_registration.gif)

### 2_2. クライアント ID とリダイレクト URL の発行
アプリの登録完了の画面から、「Authentication」タブを選択して、「Redirect URIs」の項目に以下を入力し、「Add」を選択します。

※入力例
DataCollection://oauth

![リダイレクト URL の登録](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_redirectUrl.gif)

緑のチェックが表示されれば、利用可能な URL として登録ができます。

注意：ブラウザの翻訳機能は使用しないで URL の登録を行ってください。

画面に表示されている Client ID と Current Redirect URIs をConfigration.xml に設定します。

![ID と URL のコピー](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_credentialInf.png)

Configration.xml をテキストエディター等で開き、上記でコピーした「クライアント ID」と「リダイレクト URL」をそれぞれ次の XML タグに設定します。
- \<AppClientID> クライアント ID \</AppClientID>
- \<RedirectURL> リダイレクト URL \</RedirectURL>

## 3.アプリのビルド
DataCollection.sln を Visual Studio 2017 で開きます。

Configration.xml を開き次の XML タグに Web マップの URL 等が設定されていることを確認してください。
- \<WebmapURL> Web マップの URL \</WebmapURL>
- \<AppClientID> クライアント ID \</AppClientID>
- \<RedirectURL> リダイレクト URL \</RedirectURL>

![Visual Studio の画面](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_settingInfo.png)

設定されていることが確認出来たら、ビルドを行います。実行後にログインの認証画面が表示された場合は、開発者アカウントのユーザー ID とパスワードを入力します。

初めてアプリを実行すると、以下のパスに設定ファイルが作成されます。
- C:\Users\ \<YourUsername>\AppData\Roaming\DataCollectionSettings.xml

参照する Web マップを変更する場合は、アプリを再構築する必要はなく、上記の設定ファイルに記載されている WebmapURL タグの URL を更新することで Web マップを変更することが可能です。なお、最終的に完成したアプリを配置する場合は、アプリは任意の場所に配置、設定ファイルは指定パス（デフォルトは上記のパス）に配置されます。

注意：２ 回目以降の起動は 上記の設定ファイルを読込み起動するため、プロジェクト内の Configration.xml を変更しても反映されません。Configration.xml の設定を反映したい場合は、実行前に、出力されている設定ファイルを削除してください。

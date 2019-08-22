# Data Collection for .NET

<div align="center">
<img src="https://developers.arcgis.com/example-apps/data-collection-dotnet/img/featured-img.png" width=70%>
</div>

# このアプリについて

このアプリは、Web GIS に構築した Webマップと GIS データを使用して、[Collector for ArcGIS](https://www.esrij.com/products/collector-for-arcgis/) のように、現地調査の場面でデータを収集または編集するためのアプリです。オンラインまたはオフラインに対応しています。

アプリをご利用いただく場合は、次の GitHub サイトから、ソースコードをダウンロードして、ご自身の開発環境で一度デプロイする必要がございます。

* [data-collection-dotnet](https://github.com/Esri/data-collection-dotnet) (GitHub)
* 【参考】[アプリケーションについてのサイト（米国ESRI ガイド（英語））](https://developers.arcgis.com/example-apps/data-collection-dotnet/)

動作要件はこのドキュメント末尾の [動作要件](#動作要件) をご参照ください。

上記の GitHub および、アプリケーションについてのサイトでは、森林調査を想定して作成した Web マップやフィーチャサービスに対しての挙動を解説しています。（英語）</br>
もちろん自分自身で作成したデータを適用してアプリをご利用いただくことが可能です。[アプリを使うために](#アプリを使うために)を参考に、各種設定を適用ください。

# アプリの機能

このアプリでは、以下の機能をご利用いただくことができます。</br>
すべての機能をご利用いただく場合は、アプリの設定だけでなく Web マップやフィーチャ サービスなどの設定も考慮していただく必要がございます。

* Webマップとデータをオフラインで利用する
* オフラインとオンラインの両方でデータを編集する
* オフライン編集の同期する
* ポップアップを使ってデータを表示、編集する
* フィーチャ、ポップアップの操作
* フィーチャテーブルの編集と照会
* フィーチャの関連データを扱う
* 現在地表示
* OAuth によるポータル認証
* World Geocoder サービスを使用する
* ポップアップ設定を使用してアプリの動作を促進する

クラスを各！

# アプリを使うために

アプリをご利用いただくために、次の 5 つのことをご確認ください。

* [アカウントについて](#アカウントについて)
* [日本語で使用したい](#日本語で使用したい)
* [アプリが参照する Web GIS を設定する](#アプリが参照する-web-gis-を設定する)
* [OAuthの設定]()
* [データを作成する](#データを作成する)

## アカウントについて

 [Data Collection for .NET](https://developers.arcgis.com/example-apps/data-collection-dotnet/) で提供するアプリを起動して、レコードを編集したり Web マップをオフラインで利用するためには、ArcGIS Online Developerアカウント、またはArcGIS Online アカウントが必要です。</br>
 ご自身で作成した Web マップを使用するには、ArcGIS Onlineの組織アカウントが必要です。</br>
 [ArcGIS for Developer](https://developers.arcgis.com/) では無償アカウントが作成できます。作成方法は[開発者アカウントの作成](http://esrijapan.github.io/arcgis-dev-resources/get-dev-account/)をご参照ください。


## 日本語で使用したい

このアプリ中で表示される言語は、デフォルトでは英語になっています。
日本語でご利用いただきたい場合は、以下の [Data Collection for .NET](https://developers.arcgis.com/example-apps/data-collection-dotnet/) からGitHubにアクセスして、[Resources.resx]() ファイルを適用してください</br>
ソースコードは [Open source apps using the ArcGIS Platform](https://developers.arcgis.com/example-apps/) の [Data Collection for .NET](https://developers.arcgis.com/example-apps/data-collection-dotnet/) から入手できます。


## アプリが参照する Web GIS を設定する

 [Configuration.xml](https://github.com/Esri/data-collection-dotnet/blob/master/src/DataCollection.Shared/Properties/Configuration.xml) には、アプリがアクセスする Web GIS や　Web マップなどの設定リソースを定義しています。

ご自身の組織サイトおよびデータを次の項目に設定します。

``WebmapURL``：アプリで利用する Web マップの URL。アプリは一度に 1 つの Web マップでのみ動作します。作成方法は[Web マップ作成]()をご参照ください</br>
``ArcGISOnlineURL``：OAuth 認証に使用されます。https ://www.arcgis.com/sharing/rest</br>
``DefaultZoomScale``：現在位置ボタンが押されたときにズームインする距離を設定する整数</br>
``AppClientID``：OAuth認証に使用されます。</br>
``RedirectURL``：OAuth認証に使用されます</br>

定義例

``` 
<ArcGISOnlineURL>https://www.arcgis.com/sharing/rest</ArcGISOnlineURL>
<WebmapURL>https://ej.maps.arcgis.com/home/item.html?　id=e873bd862b2a4d7fa4ed0102547d3ac...</WebmapURL>
<DefaultZoomScale>5000</DefaultZoomScale>
<AppClientID>CsWtcMl4ao3Y8B...</AppClientID>
<RedirectURL>data-collection://auth</RedirectURL>
``` 

その他、``<TreeDatasetWebmapUrl>`` 以下の内容は、デフォルトのデータセットで使用される設定のため変更不要です。


## OAuth の設定

### OAuth の設定の目的

* アプリを開発して運用に移すときには登録が必要
* アクセス制限がかかったデータにアクセスする場合に、ユーザ情報の入力画面と結果を問い合わせて、アプリに結果を返す
* クレジットを消費するアプリを開発した場合は、アプリを登録したユーザーアカウントから消費する

### OAuth の設定方法

1. アプリケーションを登録する

    ArcGIS Portalに新しいアプリケーションを作成してクライアントIDを取得し、リダイレクトURLを設定します。</br>
    クライアントIDは、ご自身で開発されたアプリだと ArcGIS が信頼することができ、ログイン時や、アクセス制限のかかったコンテンツにアクセスし認証するための画面を提供します。リダイレクトURLは、認証が完了すると、アプリに戻ることができます。

    
    1.  自分のArcGIS組織アカウントまたはArcGIS開発者アカウントを使用してhttps://developers.arcgis.comにログインします。
    1. 新しいアプリケーションを登録します。

        ![アプリ登録画面](https://user-images.githubusercontent.com/20545379/48228207-6885e500-e359-11e8-99dd-fe528dc50875.png "アプリ登録画面")


    1.  [認証]タブで、クライアントIDをメモし、リダイレクトURLを追加します。</br>
    例えば、このアプリでは ``data-collection：//auth`` と入力します。以下のプロジェクトの設定セクションでこのURLを使用します。

        ![アプリ登録画面](https://user-images.githubusercontent.com/20545379/48228212-6de32f80-e359-11e8-9404-aa50858f7cb3.png "アプリ登録画面")

1.  プロジェクトに反映する

    登録したアプリの画面から、クライアントID（clientID）とrいダイレクトURL（Current Redirect URLs）を [アプリが参照する Web GIS を設定する](#アプリが参照する-Web-GIS-を設定する)で編集した設定ファイル（Configuration.xml）に定義します。    


## アプリで使用する Web マップとフィーチャ サービス作成方法

アプリで使用するあなた自身のWeb マップとフィーチャサービスの作成方法または設定方法をご紹介します。

```
注意！
フィーチャレイヤーと関連テーブルを設定変更を行ったときは、Web マップの保存も再度行ってください。
```

### *Web マップの作成*

[Web マップ](https://www.esrij.com/gis-guide/web-gis/web-map/) を作成する方法は、[はじめての Web マッピングアプリケーション開発：Web マップの作成・表示編](https://community.esri.com/docs/DOC-12505) の記事をご参照ください。</br>
Web マップ作成についてもっと詳細に学びたい方は [Learn ArcGIS](https://learn.arcgis.com/ja/) で[はじめてのマップ ビューアー](https://learn.arcgis.com/ja/projects/get-started-with-map-viewer/)をご覧ください。


### *Web マップのタイトル*

アプリで使用する Web マップのタイトルは、マップビューのナビゲーションバーのマップのタイトルになります。画面サイズを考慮し、簡潔でわかりやすいタイトルを推奨します。

### *編集対象のフィーチャ レイヤー*

Web マップに定義したフィーチャ レイヤーのうち、以下のレイヤーが表示および編集対象となります。

 * ポイントデータのフィーチャ サービスであること
 * フィーチャ サービスのレイヤーインデックス ID が 0 であること

ポイントデータのフィーチャ レイヤーのみが対象です。</br>
また、フィーチャ サービスには、空間的なデータを持たないテーブルデータを関連データ（関連テーブル）として定義し、このアプリで編集することができます。関連テーブルが定義されている場合は、編集のためのポップアップを開くことができます。(関連テーブルの有無でアプリ自体の動作に影響はありません)


### *フィーチャ レイヤーの表示範囲*

マップビューワーでは、フィーチャ サービスの表示・非表示を地図へのズームレベルにより指定できます。このアプリで編集できるフィーチャは表示されているデータに限ります。

### *フィーチャレイヤーとテーブルの編集を有効にする*

フィーチャ レイヤーとその関連テーブルには、それぞれ編集を有効にするか無効にするかを設定する項目があります。</br>
仮に、関連テーブルの編集可能設定が無効の場合は、そのテーブルを更新することはできません。

### *フィーチャレイヤーとテーブルでポップアップを有効にする*

アプリは、フィーチャや関連テーブルの属性情報を編集するためにポップアップを利用します。</br>
アプリでポップアップを表示できるかどうかは、Web マップでのポップアップ設定に依存しています。アプリは、ポップアップ対応のフィーチャ レイヤーとテーブルのみを編集できます。
ポップアップ有効化については[ポップアップの構成](https://enterprise.arcgis.com/ja/portal/10.5/use/configure-pop-ups.htm)をご参照ください。

### *フィーチャレイヤとテーブルにポップアップで表示する項目を設定する*

ポップアップが有効になっていれば、ポップアップを構成する属性情報の項目等を設定することができます。ポップアップの構成設定は、Web マップで定義している各レイヤーの[・・・]を選択し、[ポップアップの構成]から設定画面を開きます。

* ポップアップのタイトル</br>
設定するポップアップのタイトルは、アプリからフィーチャを選択してポップアップを表示させたときに先頭に表示される項目です。</br>
ポップアップタイトルは、固定の文字列で設定することも、属性情報でフォーマットして表示することもできます。画面サイズを考慮して設定することを推奨します。
例えば、”保存樹木”という固定の文字列に、属性情報のIDを組み合わせて表示したい場合は次のように定義します。</br></br>
``` 保存樹木: {OBJECTID}``` 

    ![アプリ登録画面](https://learn.arcgis.com/ja/projects/get-started-with-map-viewer/arcgis-online/lessons/GUID-12FA673C-E500-4424-90F9-6A1BCC29F0B8-web.png "アプリ登録画面")


* ポップアップ コンテンツ</br>
表示または編集できる属性情報の項目をカスタムすることができます。</br>
``` 表示：``` のドロップダウンで ```属性フィールドのリスト（A list of field attributes）``` を選択すると、これの下のウィンドウに表示する属情報の定義が表示されます。任意の属性情報を一度選択してから、右横の上下の矢印を操作すると、選択した属性情報の項目の表示順番を指定することができます。

    ![アプリ登録画面](https://learn.arcgis.com/ja/projects/get-started-with-map-viewer/arcgis-online/lessons/GUID-70D71446-C56C-4839-B992-F149BBA59788-web.png "アプリ登録画面")
    
    ```属性フィールドの構成（Configure Attributes）``` では、表示または編集する属性項目を詳細に設定することができます。フィーチャ サービスでは、フィーチャの更新日時や編集者の情報を記録するための属性情報があらかじめ作成されています。この情報もこの構成画面から表示できるように設定することができます。

    ![アプリ登録画面](https://learn.arcgis.com/ja/projects/get-started-with-map-viewer/arcgis-online/lessons/GUID-D26FC2D5-66DD-4BBC-BB50-9A787592E9FD-web.png "アプリ登録画面")
    
## 動作要件

アプリは、.NETフレームワーク(MVVMモデル)に基づいて設計され動作するようになっています。このアプリは次の動作要件でデプロイしてください。

### 開発環境
* Visual Studio 2017
* ArcGIS Runtime SDK for .NET（100.5）
* Esri.ArcGISRuntime.Toolkit（100.4）

その他、詳しい動作環境についての詳細は Esriジャパンの [ArcGIS Runtime SDK for .NET](https://www.esrij.com/products/arcgis-runtime-sdk-for-dotnet/environments/) の動作環境をご覧ください。

※ArcGIS のライブラリのバージョンは、Esriジャパンでの動作確認現在時のバージョンです。Nuget似て更新バージョンがリリースされましたら、バージョンアップしてご利用ください。または復元時に最新バージョンをご利用ください。


## 免責事項

このアプリの内容や動作に関しては弊社の開発者サポート対象外となります


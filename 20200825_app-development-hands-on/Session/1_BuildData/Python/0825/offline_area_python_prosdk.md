#  ArcGIS API for Python と ArcGIS Pro SDK for .NET を使用したオフライン エリアの作成

## 演習の目的
- ここでは、ArcGIS API for Python のスクリプトを ArcGIS Pro SDK for .NET でカスタマイズしたアドインから実行してオフライン エリアを作成することで、以下の 2 点について理解を深めていただきます。
  - ArcGIS API for Python による Web GIS の操作
  - ArcGIS Pro SDK for .NET による業務ワークフローに合わせた ArcGIS Pro のカスタマイズ

  ※ このハンズオンは ArcGIS Pro をご利用いただいている方のみ実施いただきます。ArcGIS Online で参加されている方は、参考資料として御覧ください。

## 設定ファイルの編集

1. 環境構築時にダウンロードした[ハンズオンデータ](https://github.com/EsriJapan/workshops/blob/master/20200825_app-development-hands-on/Environment/README.md#%E3%83%87%E3%83%BC%E3%82%BF%E3%81%AE%E9%85%8D%E7%BD%AE)の EJWater\script\config フォルダにある「config.ini」をメモ帳で開きます。
    
    <img src="./img/dir.png" width="300px">

1. 次の画像のように設定を編集して保存します。

   <img src="./img/config.png" width="550px">

    ①: ハンズオン データを配置したパス</br>
    ②: ArcGIS Online のユーザー名とパスワード</br>
    ③: Web マップのアイテム ID</br>
    ④: ホスト フィーチャ レイヤーのサービス定義ファイルのアイテム ID</br>
    ⑤: 図郭_500 レイヤーのサービス URL</br>
    ⑥: 図郭_1000 レイヤーのサービス URL</br>

    ※: 青枠内は変更不要です
    
    アイテム ID については[アイテム ID の確認方法](#アイテムIDの確認方法)、レイヤーのサービス URL については[レイヤーのサービス URL の確認方法](#レイヤーのサービスURLの確認方法)をそれぞれ参照してください。

### アイテムIDの確認方法
アイテム ID は Web GIS 上のアイテムを一意に識別する ID です。次の方法で確認できます。
1. ArcGIS Online にサインインし、[コンテンツ] をクリックし、一覧から対象のアイテムをクリックし、アイテム ページを開きます。

  <img src="./img/host_fl_setting.png" width="400px">
  
1. アイテム ページの URL の`id=`以降の英数字がアイテム IDです。コピーして設定ファイルに貼り付けてください。
      
  <img src="./img/item-id.png" width="400px">

* Web マップ、ホスト フィーチャ レイヤー、ホスト フィーチャ レイヤーのサービス定義ファイルをID を混同しないよう注意してください。

  <img src="./img/items.png" width="400px">

### レイヤーのサービスURLの確認方法
2つの図郭レイヤーのサービス URL は、次の方法でそれぞれコピー、ペーストしてください
1. ホスト フィーチャ レイヤーのアイテム ページを開き、画面をスクロールすると図郭_500と図郭_100レイヤーが表示されるので、クリックします。

  <img src="./img/layers.png" width="400px">

1. クリックしたレイヤーの詳細画面に切り替わるので、右側のサービス URL のコピーボタンをクリックし、設定ファイルに URL を貼り付けます。

  <img src="./img/copy-url.png" width="400px">

## ArcGIS Pro SDK for .NET で拡張したアドインを使用したオフライン エリアの作成
1. 設定ファイルを編集・保存したら EJWater フォルダにある EJWater.aprx をダブルクリックして ArcGIS Pro のプロジェクトを開きます。

1. ArcGIS Pro が開いたら画面上部の [オフラインデータ タブ] をクリックし、図郭_500 をコンボボックスから選択しましょう。

  <img src="./img/prosdk-1.png" width="400px">

1. [図郭選択] をクリック後、マップ上で日吉駅付近の図郭をクリックして選択し、[オフラインデータ作成] をクリックします。

  <img src="./img/prosdk-2.png" width="400px">

1. オフライン エリアの名前を入力し、作成をクリックします。確認のプロンプトが出るので、[はい] をクリックして作成を開始します。

  オフライン エリアの名前は ArcGIS Online の UI 操作で作成したものとは別の名前にしてください。

  <img src="./img/prosdk-3.png" width="400px">

  作成には数分かかるので、[オフライン エリア作成の仕組み](オフライン エリア作成の仕組み)へと進みましょう。

## オフライン エリア作成の仕組み
ArcGIS Pro を ArcGIS Pro SDK for .NET を使って拡張し、[オフライン エリア]カスタムのアドインを追加しています。

カスタムのアドインでは、選択した図郭の属性情報 (図郭固有の番号等) を取得し、Python スクリプトへパラメーターとして渡して実行します。

Python スクリプト内では ArcGIS API for Python を使用して、渡されたパラメーターに基づいて ArcGIS Online へオフライン エリア作成のリクエストを送っています。

ArcGIS Pro をカスタマイズすることで、ArcGIS Pro を業務ワークフローに合わせて拡張することができるほか、ArcGIS API for Python と組み合わせることで、ArcGIS Pro の UI 操作を介して ArcGIS Online の操作を実行することができます。

  <img src="./img/prosdk-pythonapi-agol.png" width="400px">

以上で ArcGIS API for Python と ArcGIS Pro SDK for .NET を使用したオフライン エリアの作成は終了です。

時間がある方は EJWater\script\src の中の preplan.py ファイルを見てみましょう。

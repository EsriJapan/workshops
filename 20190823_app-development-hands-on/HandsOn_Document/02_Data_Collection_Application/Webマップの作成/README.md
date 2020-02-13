# Web マップの作成

Data Collection for .NET アプリ (以下、アプリ) で使う Web マップを作成します。 

Web マップ作成手順は以下の通りです。

1. [ホスト フィーチャ レイヤーの作成](#1-ホスト-フィーチャ-レイヤーの作成)
    - 1_1.  [ファイル ジオデータベースからホスト フィーチャ レイヤーを作成](#1_1-ファイル-ジオデータベースからホスト-フィーチャ-レイヤーを作成)
    - 1_2.  [アプリで利用するための設定変更](#1_2-アプリで利用するための設定変更)
        - 1_2_1. [添付ファイルの有効化](#1_2_1-添付ファイルの有効化)
        - 1_2_2. [オフライン設定](#1_2_2-オフライン設定)
        - 1_2_3. [ドメインの設定](#1_2_3-ドメインの設定)
        - 1_2_4. [シンボルの設定](#1_2_4-シンボルの設定)
1. [Web マップの作成](#2-web-マップの作成)
    - 2_1. [レイヤー順序の変更](#2_1-レイヤー順序の変更)
    - 2_2. [Web マップの保存](#2_2-web-マップの保存)
    - 2_3. [Web マップのオフライン設定](#2_3-web-マップのオフライン設定)

---

## 1. ホスト フィーチャ レイヤーの作成

### 1_1. ファイル ジオデータベースからホスト フィーチャ レイヤーを作成

**ArcGIS for Developers へログイン**

[ArcGIS for Developers](https://developers.arcgis.com/) へログインします。ダッシュボード画面が表示されたら、[Manage Content] ボタンをクリックし、ArcGIS Online の [マイ コンテンツ] 画面を開きます。
既にログインしている場合は画面中程の左側に [View Dashboard] と記載されているボタンがあるので、クリックしてダッシュボード画面に移動し、[Manage Content] ボタンをクリックしてください。

![ArcGIS for Developers ログイン](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_login.gif)

※言語表示を日本語に変更する方法は [ArcGIS Online 逆引きガイド](https://www.esrij.com/cgi-bin/wp/wp-content/uploads/documents/ArcGISOnline_user_guide.pdf)の「1-1. ArcGIS Onlineへサインイン」に記載されている [マイ プロフィールの設定] で、[言語] を日本語に設定してください。

**ホスト フィーチャ レイヤーの作成**

[マイ コンテンツ] のページの画面左上にある [アイテムの追加] をクリックし、[コンピューターから] を選択します。

ダイアログが表示されるので、[ファイルを選択] をクリックして、ハンズオン用のデータから tree_inspection.gdb.zip を選択します。

[コンテンツ] のドロップダウンから [ファイル ジオデータベース] を選択し、アイテム検索に用いるタグを [タグ] 欄に入力します。任意の値で構いませんが、ここでは「test」と入力しましょう。

[このファイルをホスト レイヤーとして公開します] にチェックが入っていることを確認し、[アイテムの追加] をクリックします。

追加された ファイル ジオデータベースを元にホスト フィーチャ レイヤーが作成され、当該ホスト フィーチャ レイヤーのアイテム詳細ページが開きます。

![ホスト フィーチャ レイヤーの作成](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_createLayer.gif)

### 1_2. アプリで利用するための設定変更

前項で作成したホスト フィーチャ レイヤーに対して以下の設定を行います。
* 添付ファイルの有効化
* オフライン設定
* ドメインの設定
* シンボルの設定


#### 1_2_1. 添付ファイルの有効化

前項で作成したホスト フィーチャ レイヤーのアイテム詳細ページにて、trees レイヤーの [添付ファイルの有効化]をクリックします。ダイアログが表示されたら [はい] をクリックします。

![添付ファイルの有効化](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_activeAttachment.gif)


#### 1_2_2. オフライン設定

アイテム詳細ページ右上の [設定] タブをクリックします。左上の [Feature Layer (ホスト)] をクリックし、以下の項目についてチェックボックスをクリックして有効化します。

* 編集の有効化
* 作成および更新されたフィーチャを記録
* フィーチャの作成者および最終更新者を記録
* 同期を有効化
* 他のユーザーが別の形式にエクスポートすることを許可します。

最後に [保存] ボタンをクリックして設定を保存します。


![オフライン設定](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_offlineSetting.gif)

#### 1_2_3. ドメインの設定

[ドメイン](https://doc.arcgis.com/ja/arcgis-online/manage-data/define-attribute-lists-and-ranges.htm)を以下の手順で設定します。
* [データ] タブをクリックします。
* 左端の [レイヤー] のドロップダウンから [trees] をクリックします。
* 右端の [フィールド] をクリックします。
* trees レイヤーの属性フィールドの一覧が表示されたら、Condition フィールドをクリックします。
* 右端に表示される [リストの作成] ボタンをクリックし、リスト入力画面を表示して次のように入力します。

    ラベル|コード
    ------|---
    良好|1
    普通|2
    不調|3
    枯死|4

* 右下の [保存] をクリックします。

![ドメイン設定](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_createDomainList.gif)

#### 1_2_4. シンボルの設定

アイテム詳細画面から [マップビューアーで開く] をクリックし、マップを開きます。 次の 2 つのレイヤーに対してそれぞれシンボルを設定します。

* trees：調査を想定した街路樹のポイントデータ
* neighbourhoods：調査範囲を想定したポリゴンデータ

左側のペインから trees レイヤーの [スタイルの変更] アイコンをクリックします。[①表示する属性を選択] のドロップダウンから Condition を選択し、[完了] ボタンをクリックします。

レイヤーの [その他のオプション]アイコン (…) をクリックし、[レイヤーの保存]を選択して、変更を保存します。これにより、ホスト フィーチャ レイヤーにシンボルの設定が保存されます。

neighbourhoods レイヤーに対しても同じ手順でシンボルを変更・保存します。ただし、[①表示する属性を選択] のドロップダウンリストでは NAME を選択してください。

![シンボルの設定](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_chSymbol00sized.gif)

## 2. Web マップの作成

### 2_1. レイヤー順序の変更

レイヤーの表示順序を変更するために、tree レイヤーの左端をドラッグして最上部に移動させます。

![レイヤー順序の変更](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_chLayerOrder.gif)


### 2_2. Web マップの保存

画面中央上部の [保存] ボタンをクリックし、選択肢の中から続いて[名前を付けて保存] をクリックします。 ダイアログが現れたら以下を入力して [マップの保存] をクリックします。

* タイトル：街路樹調査アプリ
* タグ：test

![Web マップの保存](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_saveMap.gif)


### 2_3. Web マップのオフライン設定

画面左のペインから [情報]タブ (「i」のアイコン) をクリックし、続いて[詳細…] をクリックします。
保存した Web マップのアイテム詳細ページが開くので、 [設定] タブをクリックします。[オフライン]のセクションで、[オフラインモードの有効化] をクリックして有効化したら設定を保存します。

![Web マップのオフライン設定](https://s3-ap-northeast-1.amazonaws.com/apps.esrij.com/arcgis-dev/github/img/workshop/DataCollection/dc_offlinemap.gif)

以上で Web マップの作成は終了です。

# Python スクリプトによるホスト フィーチャ レイヤーのアップデート 

## 演習の目的
- ここでは、事前に用意してある Python スクリプトを実行し、前のステップで編集したローカルのデータを元に、既存の ArcGIS Online 上のホスト フィーチャ レイヤーをアップデートします。演習を通じて次の事項について習得していただきます。
  - Python コマンド プロンプトの使用方法 (Python 環境の一覧取得・切り替え、カレント ディレクトリの移動、スクリプトの実行)
  - Python スクリプトによるホスト フィーチャ レイヤーの更新

## 同期の有効化の解除
一日目のハンズオンに参加されて[ホスト フィーチャ レイヤーの同期の有効化](../0825/web_map_configuration.md)の設定を行っている場合は無効化します。
※ 一日目のハンズオンに参加されていない方は [Python コマンド プロンプトの操作](#Python-コマンド-プロンプトの操作)に進みましょう。

1. ArcGIS Online にサインインし、[コンテンツ] をクリックし、アイテムの一覧の中から今回のハンズオンで作成したホスト フィーチャ レイヤーをクリックします。

    <img src="../0825/img/host_fl_setting.png" width="550px">  

1. ホスト フィーチャ レイヤーのアイテム ページが開いたら、右上の [設定] をクリックします。

    <img src="../0825/img/host_fl_setting_2.png" width="400px">

1.  画面をスクロールし、[Feature Layer (ホスト)] の中のチェックボックスのうち、[同期の有効化 (オフラインでの使用及び共同作業のために必要)] のチェックボックスを外します。同期の無効化に係る確認のプロンプトが表示されるので [はい] をクリックして無効化します。

    <img src="./img/deactivate-sync.png" width="400px">

## Python コマンド プロンプトの操作
Python コマンド プロンプトを起動すると、自動的に ArcGIS Pro が現在アクティブ化している Python 環境でコマンド プロンプトを開いてくれるため、ArcGIS Pro の Python 環境でスクリプトを実行したい場合は通常のコマンド プロンプトではなく、Python コマンド プロンプトを利用することをお勧めします。

1. Windows の [スタート] をクリックし、[ArcGIS]、[Python コマンド プロンプト] の順にクリックして Python コマンド プロンプトを起動します。

    <img src="./img/python-cmd.png" width="550px">

1. Python コマンド プロンプトが起動すると左端の () 内に現在アクティブになっている Python 環境名が表示されます。

    <img src="./img/current-env.png" width="550px">

1. 以下のコマンドを実行すると作成されている Python 環境の一覧が表示されます。

      `conda info -e`

1. 以下のコマンドを実行するとアクティブな Python 環境を切り替えることが可能です。

      `proswap 任意の環境名` (例: proswap arcgispro-py3)
      
      事前の環境構築の際に今回のハンズオン用に作成した環境に切り替えておきましょう。[環境構築手順](https://github.com/EsriJapan/workshops/tree/master/20200825_app-development-hands-on/Environment#arcgis-api-for-python-%E3%81%AE%E7%92%B0%E5%A2%83%E8%A8%AD%E5%AE%9A)に記載のとおり作成された場合、環境名は app-dev-2020 です。

## Python スクリプトの実行
事前に用意された Python スクリプトを実行します。
1. `cd` コマンドで EJWater\script\src にディレクトリを移動します。以下のコマンドは D ドライブにハンズオン データを配置した場合の例です。ご自身の任意のディレクトリにデータを配置した場合はそのディレクトリに移動してください。

    `d:`
    
    `cd EJWater\script\src`

1. ディレクトリを移動したら src ディレクトリ内にある web_map_sync_main.py ファイルを実行しましょう。

    `python web_map_sync_main.py`
    
    <img src="./img/execute-python.png" width="550px">

    実行完了までしばらく時間がかかるので、余裕があればテキストエディタ等で src ディレクトリ内の web_map_sync.py ファイルを開き、ソースコードを確認してみましょう。

1. 実行が完了するとPython コマンド プロンプトに `フィーチャ サービスの更新が完了しました` と表示されます。完了したら Web マップを開いて確認しましょう。

    1. ArcGIS Online にサインインし、[コンテンツ] をクリックして [マイ コンテンツ] の中から公開した Web マップをクリックします。

        <img src="../0825/img/web_map_setting.png" width="550px">

    1. サムネイル画像をクリックし、Web マップを開きます。

        <img src="../0825/img/open-mapviewer.png" width="450px">
        
    1. ArcGIS Pro で追加したポイント データが反映されていることを確認しましょう。

この演習では Python コマンド プロンプトの基本的な使用方法、Python スクリプトの実行などを行いました。

タスクスケジューラなどを使うことでPython スクリプトの実行を自動で定期的に行うことも可能です。詳しくは[ArcGIS for Developer 開発リソース集](http://esrijapan.github.io/arcgis-dev-resources/tips/python/automate-tasks/)をご覧ください。

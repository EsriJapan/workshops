# ステップ 1 ：コンフィグの設定と起動確認

## 演習の目的
ステップ 1 では、ハンズオンで使用するアプリケーションが動作するために必要な AppConfig ファイルの設定とアプリケーションの起動の確認を行います。

## 手順
- ① アプリケーションのダウンロード
- ② コンフィグの設定
- ③ アプリケーションの起動確認

### ① アプリケーションのダウンロード
1. ハンズオンで使用するアプリケーションを以下よりダウンロードしてください。  
[EJWater_NativeApp.zip](https://github.com/EsriJapan/workshops/raw/master/20200825_app-development-hands-on/Session/2_DevelopNativeApp/EJWater_NativeApp.zip)

2. 任意の場所に配置して解凍する  
例：D：\EJWater_NativeApp<br><br>
zip を解凍すると「answer」と「exercise」フォルダが格納されています。  
ハンズオンでは、「exercise」フォルダにあるプロジェクトを使用します。  
「answer」フォルダにはすでに完成したアプリケーションが格納されているので、コードや動作を確認する際にご利用ください。  

### ② コンフィグの設定  
アプリケーションの起動に必要な情報を「App.Config ファイル」に設定する必要があります。 App.Config ファイルの内容の確認と、設定に必要な情報を作成し、設定しましょう。  

1. ① でダウンロードしたフォルダ内にある「ESRIJOfflineApp.sln」を起動します。    
<img src="./img/solution_file.png" width="500px"><br>

2. ソリューションエクスプローラーから「App.Config ファイル」を開きます。  
<img src="./img/appconfig1.png" width="500px"><br>
「App.config ファイル」には、以下のパラメータが定義してあります。<br>
    - ServerUrl：ポータルのURL
    - AppClientId：アプリケーションID
    - ClientSecret：アプリケーションの秘密キー
    - OAuthRedirectUrl：リダイレクトURL
    - WebMapId：WebマップのURL
    - FeatureServiceUrl：FeatureServiceのURL
    - OfflineDataFolder：オフラインデータの保存フォルダパス
    - Mode：起動時のアプリケーションのモード（Online/Offline）
    - OfflineDataName：アプリケーション終了時に表示していたオフラインデータの名前<br>
    
以下より、パラメーターに必要な情報の作成と設定を行います。<br>

3. 「ServerUrl」を設定  
ServerUrl の Value にポータルのURLを設定する。  
例：<br>
    ```xml
    <add key="ServerUrl" value="https://www.arcgis.com/sharing/rest"/>
    ```  

4. 「AppClientID」、「OAuthRedirectUrl」を設定  
まずは、アプリケーションID と リダイレクトURLを発行します。<br>
    - [ArcGIS Online](https://www.arcgis.com/home/index.html) にアクセスして「サインイン」を押下してください。  
    <img src="./img/agol.png" width="500px"><br>

    - ログイン情報を入力し、ArcGIS Online にログインしてください。  
    <img src="./img/agol_login.png" width="250px"><br>

    - 画面上部の「コンテンツ」を押下します。  
    <img src="./img/agol_menu.png" width="700px"><br>

    - 「アイテムの追加」 > 「アプリケーション」を押下します。  
    <img src="./img/agol_item_add.png" width="250px"><br>

    - 以下を入力し、「アイテムの追加」ボタンを押下します。  
      ・タイプ：アプリケーションにチェック  
      ・タイトル：漏水調査アプリ  
      ・タグ：開発塾  
    <img src="./img/agol_item_add2.png" width="500px"><br>

    - 登録したアプリ (漏水調査アプリ) の概要画面が開かれるので、設定タブを押下して設定画面に遷移する。  
    ※自動で移動しなかった場合はコンテンツ画面から漏水調査アプリのリンクをクリックする。  
    <img src="./img/agol_app_gaiyou.png" width="700px"><br>

    - 設定画面の「Application」枠の「登録情報」ボタンを押下する。  
    <img src="./img/agol_app_setting.png" width="500px"><br>

    - 画面に「アプリケーションID」と「リダイレクトURL」が表示されます。  
    <img src="./img/agol_app_reg.png" width="500px"><br>
    上記で発行した「アプリケーションID」と「リダイレクトURL」を App.Config ファイルの「AppClientID」、「OAuthRedirectUrl」に設定します。<br>
    例：<br>
        ```xml
        <add key="AppClientId" value="M3R8sklRa37pnAUN"/>
        <add key="OAuthRedirectUrl" value="urn:ietf:wg:oauth:2.0:oob"/>
        ```

5. 「WebMapId」を設定  
コンテンツ画面より今回使用する Web マップ (前のセッションで作成した Web マップ) をクリックして、Web マップの概要ページを開き、URL をコピー、「WebMapId」に設定します。  
<img src="./img/agol_webmap_id.png" width="500px"><br>
※この前のセッションで Web マップを作成できなかった方は、今回のハンズオン用に事前に作成済みの Web マップがあるので下記のURLをそのまま使用してください。  
例：<br>
    ```xml
    <add key="WebMapId" value="https://ej.maps.arcgis.com/home/item.html?id=8e285147abe044cb851fbec6a1bed5cd"/>
    ```  

6. 「FeatureServiceUrl」を設定  
    - Web マップの概要画面に表示されているレイヤーから任意のモノをクリックする。  
    <img src="./img/agol_webmap_layer.png" width="500px"><br>

    - FeatureLayer の概要ページが表示されるので、画面の右下にあるURLをコピーします。  
    <img src="./img/agol_layer_url.png" width="500px"><br>
    FeatureLayer の URL を App.config ファイルの「FeatureServiceUrl」に設定する。<br>
    ※この前のセッションで Web マップを作成できなかった方は、今回のハンズオン用に事前に作成済みの FeatureLayer があるので下記のURLをそのまま使用してください。<br>
    例：<br>
        ```xml
        <add key="FeatureServiceUrl" value="https://services.arcgis.com/wlVTGRSYTzAbjjiC/arcgis/rest/services/%E6%97%A5%E5%90%89%E6%B0%B4%E9%81%93%E3%83%9E%E3%83%83%E3%83%97_WFL1/FeatureServer"/>
        ```   

7. 「OfflineDataFolder」と「Mode」に以下を設定する  
    - OfflineDataFolder：任意のフォルダ (例： D:/temp)  
    - Mode：Online を設定
    <br><br>
    すべて設定すると次のようなコンフィグになります。<br>
    <img src="./img/appconfig.png" width="500px"><br>
    
    ※「ClientSecret」、「OfflineDataName」の設定は不要です。


### ③ アプリケーションの起動確認
1. Visual Studio の画面「ビルド」タブから「ソリューションのクリーン」を選択します。  
<img src="./img/vs_clean.png" width="500px"><br>

2. Visual Studio の画面「ビルド」タブから「ソリューションのビルド」を選択します。  
<img src="./img/vs_build.png" width="500px"><br>

3. 「開始」を選択して、アプリケーションを起動します。  
<img src="./img/vs_jikkou.png" width="500px"><br>

4. 以下画面が起動すれば完了です。  
※アプリケーションがうまく起動できない場合、もう一度 Visual Studio の画面「ビルド」タブから「ソリューションのクリーン」を選択し、再度「ソリューションのビルド」を実行してください。  
<img src="./img/app.png" width="500px"><br>

※時間がある方は、実際にログインして動作を確認してみてください。  
アプリケーションの機能については、[Step0：漏水調査アプリの機能説明](./Step0.md) に記載しています。

## 次のステップ：
[Step2：ダウンロード機能の実装](./Step2.md)

# Data Collection for .NET アプリ ハンズオン

![Data Collection for .NET](https://camo.githubusercontent.com/1ec24340c1b992c3b1e8a10fefa33b5268ec2f18/68747470733a2f2f646576656c6f706572732e6172636769732e636f6d2f6578616d706c652d617070732f646174612d636f6c6c656374696f6e2d646f746e65742f696d672f66656174757265642d696d672e706e67)

## 概要
このドキュメントは、2019年8月に開催した「ArcGIS  開発者のための最新アプリ開発塾 2019」で使用した現地調査に活用できるData Collection for .NET アプリ（以下、アプリ）を起動するまでの手順を示しています。

## ハンズオンで使用する環境 および アプリ
このハンズオン（2019/8月現在）で使用した環境 およびアプリは以下になります。

- ArcGIS for Developers 開発者アカウント ※1
- Visual Studio 2017
- ArcGIS Runtime SDK for .NET 100.5
- サンプルプロジェクト（Data Collection for .NETアプリ）※2
- ダミーデータ ※2

※1 ArcGIS for Developers 開発者アカウントをお持ちでない方は「[開発者アカウントの作成](http://esrijapan.github.io/arcgis-dev-resources/guide/create-map/get-dev-account/)」から作成して、ご準備ください。アカウントは無償で作成できます。

※2 ローカライズ済みのサンプルプロジェクト及びダミーデータは HandsOn_Data.zip からダウンロードできます。 [HandsOn_Data.zip](https://github.com/EsriJapan/workshops/raw/master/20190823_app-development-hands-on/HandsOn_Data.zip) を解凍すると以下の構成となっています。

| HandsOn_Data フォルダ配下 | 格納ファイルまたはプロジェクト | 備考 |
| --- | --- | --- |
| 02_Data_Collection_Application | 190823_dev_school.ipynb | ここでは使用しません |
|  | tree_inspection.gdb.zip | 使用します：ダミーデータ |
|  | data-collection-dotnet-master_ejLocalized | 使用します：サンプルプロジェクト |
| 03_Widget_Development | Hello World | ここでは使用しません |
|  | TypeScript | ここでは使用しません |

## ハンズオンの手順
上記のサンプル プロジェクト、データ等を使用し、次の順番でアプリを構築します。

1. [Web マップの作成](https://github.com/EsriJapan/workshops/tree/master/20190823_app-development-hands-on/HandsOn_Document/02_Data_Collection_Application/Webマップの作成)
2. [アプリのデプロイ](https://github.com/EsriJapan/workshops/tree/master/20190823_app-development-hands-on/HandsOn_Document/02_Data_Collection_Application/アプリのデプロイ)

## ハンズオンで使用したアプリについて
ハンズオンで使用したアプリ（サンプルプロジェクト）は、米国Esri 社が [GitHub](https://github.com/Esri/data-collection-dotnet) で公開する [Data Collection for .NET](https://developers.arcgis.com/example-apps/data-collection-dotnet/) アプリを元に、ESRIジャパンにてローカライズ および カスタマイズしたものです。

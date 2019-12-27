# ArcGISでデータサイエンスしよう ～より高度で自由な地理空間分析へ～ デモソースコード

これは第 15 回 GISコミュニティフォーラムのプレフォーラム・セミナー「「ArcGISでデータサイエンスしよう ～より高度で自由な地理空間分析へ～」で行ったデモのソースコードです。

** 本ツールはデモ用であり、実際の運用環境でご使用になる場合は、必要な処理を加えたうえで十分にテストを行うことをお勧めいたします。**


## 使用している製品・プロジェクト
* ArcGIS API for Python を使ったディープ ラーニング
    * [ArcGIS Pro](https://pro.arcgis.com/ja/pro-app/arcpy/get-started/what-is-arcpy-.htm)
    * [ArcGIS API for Python](https://www.esrij.com/products/arcgis-api-for-python/)
    * [ArcGIS Image Analyst](https://www.esrij.com/products/image-analyst/)

* R と ArcGIS を連携した空間誤差モデルでの解析
    * [ArcGIS Pro](https://pro.arcgis.com/ja/pro-app/arcpy/get-started/what-is-arcpy-.htm)
    * [R-ArcGIS Bridge](https://github.com/R-ArcGIS/r-bridge)
    * [R](https://cran.ism.ac.jp/)

## 動作環境

* ArcGIS API for Python を使ったディープ ラーニング
    * [ArcGIS Pro](https://pro.arcgis.com/ja/pro-app/arcpy/get-started/what-is-arcpy-.htm) 2.3.2 以上
    * [ArcGIS API for Python](https://www.esrij.com/products/arcgis-api-for-python/) 1.6.0 以上
    * [ArcGIS Image Analyst](https://www.esrij.com/products/image-analyst/)
    * [pytorch](https://pytorch.org/) 1.0.0
    * [torchvision](https://pytorch.org/docs/stable/torchvision/index.html#module-torchvision) 0.2.2
    * [fastai](https://docs.fast.ai/index.html) 1.0.39
    * [spacy](https://spacy.io/) 2.1.3

* R と ArcGIS を連携した空間誤差モデルでの解析
    * [ArcGIS Pro](https://www.esrij.com/products/arcgis-desktop/environments/arcgis-pro/) 2.3.2 以上
    * [R-ArcGIS Bridge](https://github.com/R-ArcGIS/r-bridge) 1.0.1.232
    * [R](https://cran.ism.ac.jp/) 3.5.3 (64bit)

## リソース

* ArcGIS API for Python を使ったディープ ラーニング
    * [ディープ ラーニングを使用したヤシの木の健康状態の評価](https://learn.arcgis.com/ja/projects/use-deep-learning-to-assess-palm-tree-health/)
    * [ArcGIS API for Python の API リファレンス (英語)](https://esri.github.io/arcgis-python-api/apidoc/html/)
    * [Object Detection Workflow with arcgis.learn (英語)](https://developers.arcgis.com/python/guide/object-detection/)
    * [How we did it: End-to-end deep learning in ArcGIS (英語)](https://medium.com/geoai/how-we-did-it-end-to-end-deep-learning-in-arcgis-dd5b10d87b8)

* R と ArcGIS を連携した空間誤差モデルでの解析
    * [Install the R-ArcGIS Bridge (英語)](https://github.com/R-ArcGIS/r-bridge-install)
    * [R-ArcGIS Bridge Tutorial Notebooks (英語)](https://github.com/R-ArcGIS/R-Bridge-Tutorial-Notebooks)
    * [統計情報および R-ArcGIS Bridge を使用した犯罪の解析 (Learn ArcGIS)](https://learn.arcgis.com/ja/projects/analyze-crime-using-statistics-and-the-r-arcgis-bridge/)
    * [アフリカ水牛の生態的地位の識別 (Learn ArcGIS)](https://learn.arcgis.com/ja/projects/identify-an-ecological-niche-for-african-buffalo/)

## ライセンス
Copyright 2019 Esri Japan Corporation.

Apache License Version 2.0（「本ライセンス」）に基づいてライセンスされます。あなたがこのファイルを使用するためには、本ライセンスに従わなければなりません。本ライセンスのコピーは下記の場所から入手できます。

> http://www.apache.org/licenses/LICENSE-2.0

適用される法律または書面での同意によって命じられない限り、本ライセンスに基づいて頒布されるソフトウェアは、明示黙示を問わず、いかなる保証も条件もなしに「現状のまま」頒布されます。本ライセンスでの権利と制限を規定した文言については、本ライセンスを参照してください。

ライセンスのコピーは本リポジトリの[ライセンス ファイル](./LICENSE)で利用可能です。


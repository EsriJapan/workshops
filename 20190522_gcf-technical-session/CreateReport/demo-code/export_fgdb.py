import zipfile
from arcgis.gis import GIS

#　ArcGIS Online にサイン イン
username = "ユーザー名"
password = "パスワード"
gis = GIS("https://www.arcgis.com", username, password)

#　ArcGIS Online のフィーチャ レイヤーを FGDB にエクスポート
item = gis.content.get("フィーチャ レイヤーのID") 
exportItem = item.export('demo_fgdb','File Geodatabase')

# エクスポートした FGDB をダウンロード
dlresult = exportItem.download(save_path="保存場所のパス")

# ZIPファイルを解凍
with zipfile.ZipFile(dlresult) as existing_zip:
    existing_zip.extractall("C:\data\GCF_demo")
import arcpy

workspace = r"ワークスペースのパス"

arcpy.env.workspace = workspace

# arcpy を使用してジオプロセシングツールを実行:(https://pro.arcgis.com/ja/pro-app/tool-reference/geocoding/geocode-addresses.htm)
in_table = r"CSV ファイルのパス"
address_fields = '住所 所在地 VISIBLE NONE'
geocode_result = "ジオコーディングで作成されるフィーチャ クラス名"
address_locator = "住所テーブルのジオコーディングに使用する住所ロケーター"
arcpy.GeocodeAddresses_geocoding(in_table, address_locator, address_fields, geocode_result, 'STATIC')
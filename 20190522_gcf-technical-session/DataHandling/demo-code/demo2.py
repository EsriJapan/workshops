import arcpy
import pandas as pd

infile = r'CSV ファイルのパス'
out_fc = r'出力フィーチャクラスのパス'
# pandas を使って CSV ファイルを読み込む
df = pd.read_csv(infile, engine='python' ,sep =  ",",
            usecols=['観測所番号', '観測所名', '緯度(度)','緯度(分)','経度(度)','経度(分)'],
            dtype = {'観測所番号':'<i4', '観測所名':'<U7', '緯度(度)':'<f8', '緯度(分)':'<f8', '経度(度)':'<f8', '経度(分)':'<f8'})
# 新しい列を定義して、計算結果を格納
df['緯度'] = df['緯度(度)']  + (df['緯度(分)']  / 60)
df['経度'] = df['経度(度)']  + (df['経度(分)']  / 60)
# pandas の dataframe を numpy の配列に変換 ※ column_dtypes のオプションは pandas 0.24.0 Version から搭載
records = df.to_records(column_dtypes={'観測所名':'<U7'})
# arcpy のメソッドを使用してフィーチャ クラスを作成
arcpy.da.NumPyArrayToFeatureClass(records, out_fc, ['経度', '緯度'], arcpy.SpatialReference(4269))

import arcpy
import pandas as pd

df1file = r'CSV ファイルのパス1'
df2file = r'CSV ファイルのパス2'
out_fc = r'出力フィーチャクラスのパス'

# pandas を使って CSV ファイルを読み込む
df1 = pd.read_csv(df1file, engine='python', sep = ",",
            usecols=['観測所番号', '観測所名', '緯度(度)','緯度(分)','経度(度)','経度(分)'],
            dtype={'観測所番号':'<i4', '観測所名':'<U7', '緯度(度)':'<f8', '緯度(分)':'<f8', '経度(度)':'<f8', '経度(分)':'<f8'})
df2 = pd.read_csv(df2file, engine='python', sep=',',
            usecols=['観測所番号', '累積降雪量（cm）'],
            dtype={'観測所番号':'<i4', '累積降雪量（cm）':'<f8'})
# 観測所番号をキーにして ２ つの CSV ファイルを結合する
df = pd.merge(df1, df2, on = r"観測所番号")
# 新しい列を定義して、計算結果を格納
df['緯度'] = df['緯度(度)']  + (df['緯度(分)']  / 60)
df['経度'] = df['経度(度)']  + (df['経度(分)']  / 60)
# pandas の dataframe を numpy の配列に変換 ※ column_dtypes のオプションは pandas 0.24.0 Version から搭載
records = df.to_records(column_dtypes={'観測所名':'<U7'})
# arcpy のメソッドを使用してフィーチャ クラスを作成
arcpy.da.NumPyArrayToFeatureClass(records, out_fc, ['経度', '緯度'], arcpy.SpatialReference(4269))

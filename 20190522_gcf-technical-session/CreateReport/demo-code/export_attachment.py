import arcpy
from arcpy import da
import os

#　アタッチメント テーブルのパス画像の出力先を指定
inTable = r"アタッチメント テーブルのパス"
fileLocation = r"添付ファイルの出力パス"

# アタッチメント テーブルから添付ファイルを抽出して出力
with da.SearchCursor(inTable, ['DATA', 'ATT_NAME', 'ATTACHMENTID']) as cursor:
    for item in cursor:
        attachment = item[0]
        filenum = "ATT" + str(item[2]) + "_"
        filename = filenum + str(item[1])
        open(fileLocation + os.sep + filename, 'wb').write(attachment.tobytes())
        # オブジェクトの削除
        del item
        del filenum
        del filename
        del attachment



        
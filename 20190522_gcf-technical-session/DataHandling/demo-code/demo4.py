import xml.etree.ElementTree as ET
import arcpy

#固定値
WORK_SPACE = r"ワークスペースのパス"
SHAPE_FILE = r"D:\ｘｘ\地震情報／細分区域.shp"
XML_FILE = r"D:\ｘｘ\sample.xml"
FEATURE_CLASS_NAME = "地震情報／細分区域"
COLUMN_NAME = "震度"

def update(maxInt, code):
    """
    フィーチャクラスを更新します
    """
    with arcpy.da.UpdateCursor(FEATURE_CLASS_NAME,
                               COLUMN_NAME,
                               "code = %s" % "'" + code + "'") as cursor:
        for row in cursor:
            row[0] = maxInt #震度カラムを更新
            cursor.updateRow(row)

def read_xml():
    """
    XMLをパースします。Area要素内のCodeがフィーチャクラスに存在する場合、
    MaxIntを使用してフィーチャクラスの震度カラムを更新します。
    """
    tree  = ET.parse(XML_FILE)
    elem = tree.getroot()
    for elem in elem.getiterator():
        if elem.tag == "{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}Pref":
            for el in list(elem):
                if el.tag == "{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}Area":
                    for e in list(el):
                        code = el.find("{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}Code") #コード取得
                        maxInt = el.find("{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}MaxInt") #震度取得
                        update(maxInt.text, code.text) #フィーチャクラスの更新


if __name__ == '__main__':
    #ワークスペース指定
    arcpy.env.workspace = WORK_SPACE

    #Shapeファイルをフィーチャクラスに変換
    arcpy.CopyFeatures_management(SHAPE_FILE, FEATURE_CLASS_NAME)

    #フィーチャクラスにカラム追加
    arcpy.AddField_management(FEATURE_CLASS_NAME, COLUMN_NAME, "Text")

    #座標系変換
    arcpy.DefineProjection_management(FEATURE_CLASS_NAME, arcpy.SpatialReference(4612))

    #XML読込
    read_xml()

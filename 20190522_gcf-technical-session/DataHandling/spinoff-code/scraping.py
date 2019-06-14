import xml.etree.ElementTree as ET
import urllib.request
import arcpy

WORK_SPACE = r"D:\data\DemoProject\DemoProject.gdb"
SHAPE_FILE = r"D:\data\DemoProject\20190125_AreaForecastLocalE_GIS\地震情報／細分区域.shp"

def read_xml(_xml_):
    """
    XMLをパースします。Area要素内のCodeがフィーチャクラスに存在する場合、
    MaxIntを使用してフィーチャクラスの震度カラムを更新します。
    """
    elem = ET.fromstring(_xml_)

    for elem in elem.iter():
        if elem.tag == "{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}OriginTime":
            s_originTime = elem.text #地震発生時刻を取得
            #フィーチャクラスの名前作成
            outfc = arcpy.ValidateTableName("地震情報_" + s_originTime)
            #フィーチャクラスの作成
            create_flg = create_feature(outfc)
            if create_flg == False:
                break
                
        if elem.tag == "{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}Pref":
            for el in list(elem):
                if el.tag == "{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}Area":
                    for e in list(el):
                        code = el.find("{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}Code") #コード取得
                        maxInt = el.find("{http://xml.kishou.go.jp/jmaxml1/body/seismology1/}MaxInt") #震度取得
                        update(maxInt.text, code.text, outfc) #フィーチャクラスの更新

def create_feature(outfc):
    #ワークスペースにすでに同一名のフィーチャクラスがないかチェック
    if arcpy.Exists(outfc):
        return False
    #Shapeファイルをフィーチャクラスに変換
    arcpy.CopyFeatures_management(SHAPE_FILE, outfc)
    #フィーチャクラスにカラム追加
    arcpy.AddField_management(outfc, "震度", "Text")
    #座標系変換
    arcpy.DefineProjection_management(outfc, arcpy.SpatialReference(4612))
    return True

def update(maxInt, code, outfc):
    """
    フィーチャクラスを更新します
    """
    with arcpy.da.UpdateCursor(outfc,
                               "震度",
                               "code = %s" % "'" + code + "'") as cursor:
        for row in cursor:
            row[0] = maxInt #震度カラムを更新
            cursor.updateRow(row)

def load_xml():
    """
    気象庁のサイトよりXMLの情報を取得する
    """
    url = 'http://www.data.jma.go.jp/developer/xml/feed/eqvol.xml'

    xml_string = download(url)
    elem = ET.fromstring(xml_string)
    target_dict = {}
    atom_ns = 'http://www.w3.org/2005/Atom'

    for e in elem.findall('.//{%s}entry' % atom_ns):
        s_title = e.find('./{%s}title' % atom_ns).text
        s_updated = e.find('./{%s}updated' % atom_ns).text
        s_url = e.find('./{%s}link' % atom_ns).attrib
        s_content = e.find('./{%s}content' % atom_ns).text

        if s_title=='震源・震度に関する情報':
            target_dict[s_content] = [s_url, s_updated]

    # コンテンツごとのURLを作成
    for key in target_dict.keys():
        d = target_dict[key]
        _url = d[0]['href']
        _xml_string = download(_url)
        #XML読込
        read_xml(_xml_string)

def download(url):
    """
    URL で指定したXMLを取得する
    """
    req = urllib.request.Request(url)
    with urllib.request.urlopen(req) as response:
        xml_string = response.read()

    return xml_string

if __name__ == '__main__':
    #ワークスペース指定
    arcpy.env.workspace = WORK_SPACE
    #XMLダウンロード
    load_xml()

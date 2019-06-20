import arcpy
import os

# ArcPy でレポートを PDF に出力

# PDF の出力先を指定
pdfPath = r"C:\data\output\report.pdf"
if os.path.exists(pdfPath):
    os.remove(pdfPath)

# プロジェクトファイルをPDFに出力
output = r"C:\data\output"  # 出力フォルダのパス
aprx = arcpy.mp.ArcGISProject("プロジェクトファイルのパス")
aprx.listLayouts()[0].mapSeries.exportToPDF(output + r"\map.pdf")

# PDFを結合
pdfDoc = arcpy.mp.PDFDocumentCreate(pdfPath)
pdfDoc.appendPages(r"C:\data\output\title.pdf")  # 表紙となる PDF
pdfDoc.appendPages(r"C:\data\output\map.pdf")  # マップ シリーズで出力した PDF

#保存とオブジェクトの削除
pdfDoc.saveAndClose()
del pdfDoc
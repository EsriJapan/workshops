/** 4-1 ウィジェットの画面用モジュールの設定 Start */
/** @jsx jsx */
import { React, AllWidgetProps, css, jsx, DataSourceManager, QueriableDataSource, QueryParams, MessageManager, DataRecordsSelectionChangeMessage } from "jimu-core";
import { JimuMapViewComponent, JimuMapView } from "jimu-arcgis";
import { Label, Select, Option, NumericInput, Checkbox, Table } from "jimu-ui";
/** 4-4 使用する ArcGIS API for JavaScript のモジュールをインポート Start */
import Point from "esri/geometry/Point";
import Polygon from "esri/geometry/Polygon";
import geometryEngine from "esri/geometry/geometryEngine";
import SimpleFillSymbol from "esri/symbols/SimpleFillSymbol";
import Graphic from "esri/Graphic";
import Query from "esri/rest/support/Query";
import LayerView from "esri/views/layers/LayerView";
import Collection from "esri/core/Collection";
/** 4-4 使用する ArcGIS API for JavaScript のモジュールをインポート End */
export default class Widget extends React.PureComponent<AllWidgetProps<any>, any> {

    /** 4-2 State の定義 Start */
    // 処理内で利用する変数を定義
    state = {
        jimuMapView: null, /** 対象 Webマップ */
        webmapLayers: [], /** Web マップのレイヤー情報 */
        selectLayer: null, /** 選択したレイヤー情報 */
        distance: 0, /** バッファーの半径距離 */
        widgetEnable: false, /** バッファー処理実行フラグ */
        selLayerSource: null, /** 選択したレイヤーのデータソース */
    };
    /** 4-2 State の定義 End */

    /** 4-6 マップ ウィジェット イベント追加 Start */
    // マップ ウィジェットが変更されたときにマップ情報とクリックイベントの設定
    activeViewChangeHandler = (jmv: JimuMapView) => {
        if (jmv) {
            this.setState({
                jimuMapView: jmv,
                webmapLayers: this.setLayerList(jmv.view.layerViews)
            });
            // 対象のマップをクリック イベントを取得
            jmv.view.on("click", (evt) => {
                // バッファー処理はチェックがオンになっている時のみ実行
                if (this.state.widgetEnable) {
                    // 必須項目入力チェック
                    let msg = this.eventErrorCheck();
                    if (msg.length != 0) {
                        // 必須項目が入力されていない場合はエラー
                        alert(msg);
                        return;
                    }
                    // バッファー検索の本処理を実行
                    this.doBufferSerch(evt);
                }
            });
        }
    };
    /** 4-6 マップ ウィジェット イベント追加 End */

    /** 4-8 必須項目チェック追加 Start */
    // 必須項目入力チェック
    eventErrorCheck = () => {
        let requrirdMsg = ""; /** エラーメッセージ格納用 */

        // レイヤーが選択されていない場合はエラー
        if (this.state.selectLayer.length == 0) {
            requrirdMsg = "対象のレイヤーを選択してください。\n";
        }

        // バッファー距離半径が 0 以下の場合はエラー
        if (this.state.distance <= 0) {
            requrirdMsg = requrirdMsg + "バッファーの距離（半径）は 0 より大きい値を入力してください。";
        }

        return requrirdMsg;
    }
    /** 4-8 必須項目チェック追加 End */

    /** 4-9 バッファー検索の本処理追加 Start */
    // バッファー検索の本処理
    doBufferSerch = (evt: Object) => {

        // 前回の実行結果があれば削除する。
        this.state.jimuMapView.view.graphics.removeAll();

        // マップをクリックした地点の位置情報を取得
        const point: Point = this.state.jimuMapView.view.toMap({
            x: evt.x,
            y: evt.y
        });

        // 指定した条件でバッファーを作成
        const buffer = geometryEngine.geodesicBuffer(point, this.state.distance, this.props.config.distanceUnit, false);

        // バッファーの作成が正常に終了した場合に描画処理を実行
        if (buffer) {
            // バッファーのグラフィックを定義
            let bufGraphic = new Graphic({
                geometry: buffer,
                symbol: this.setBufferSymbol()
            });
            // バッファーのグラフィックをマップに表示
            this.state.jimuMapView.view.graphics.add(bufGraphic);

            // バッファー内のレイヤー取得および選択、連携処理実行
            this.layerGetAndMarking(buffer);
        }
    }
    /** 4-9 バッファー検索の本処理追加 End */

    /** 4-10 バッファー内に含まれるレイヤーの取得および選択、連携の処理追加 Start */
    // バッファー内のレイヤー取得および選択、連携の処理
    layerGetAndMarking = async (buffer: Polygon) => {
        // プルダウンで選択したレイヤーをウェブマップから取得
        const targetLayer = this.state.jimuMapView.view.map.findLayerById(this.state.selectLayer);
        // 取得したレイヤータイプがフィーチャレイヤーの場合描画処理を実行
        if (targetLayer.type == "feature") {
            // バッファー内にある対象のレイヤーを取得
            const query = new Query({ returnGeometry: true, outFields: ["*"] });
            query.geometry = buffer;
            query.spatialRelationship = "contains";

            // 選択した対象レイヤーのデータソースでクエリを取得できる方で宣言。
            const dataSource = this.state.selLayerSource as QueriableDataSource;

            // バッファー内にあるオブジェクトを取得
            const objList = await targetLayer.queryFeatures(query).then(results => {
                let objList = [];
                // バッファー内にあるオブジェクトをリストに格納
                for (let idx = 0; idx < results.features.length; idx++) {
                    objList.push(results.features[idx].attributes.OBJECTID);
                }
                return objList;
            })
                .catch(error => {
                    console.log("targetLayer.queryFeatures error:", error.messagae);
                    return [];
                });
            
            // Where 句を作成
            const selectedQuery =
                objList && objList.length > 0
                    ? `OBJECTID IN (${objList
                        .map(item => {
                            return item
                        })
                        .join()})`
                    : '1=2';
            
            // データソースから対象のクエリを取得
            dataSource.query({
                where: selectedQuery,
                returnGeometry: true
            } as QueryParams)
                .then(result => {
                    // 実行結果からレコード属性を取得
                    const records = result?.records
                    if (records) {
                        // 他のウィジェットへ選択対象を連携する。
                        MessageManager.getInstance().publishMessage(
                            new DataRecordsSelectionChangeMessage(this.props?.widgetId, result.records)
                        )
                        if (records.length > 0) {
                            // 対象データを選択する。
                            dataSource.selectRecordsByIds(
                                records.map(record => record.getId()),
                                records
                            )
                        } else {
                            dataSource.clearSelection()
                        }
                    }
                })
        }
    }
    /** 4-10 バッファー内に含まれるレイヤーの取得および選択、連携の処理追加 End */

    /** 4-11 バッファー用のシンボル定義を追加 Start */
    // バッファー用のシンボル（ポリゴン）を定義
    setBufferSymbol = () => {
        return new SimpleFillSymbol({
            color: [51, 51, 204, 0.2],
            style: "solid",
            outline: {
                color: "black",
                width: 1
            }
        });
    }
    /** 4-11 バッファー用のシンボル定義を追加 End */

    /** 4-7 レイヤー情報の取得処理追加 Start */
    // マップ ウィジェットに設定されいてるレイヤー情報の取得
    setLayerList = (layers: Collection<LayerView>) => {
        const list = [];
        for (let idx = layers.length; 0 < idx; idx--) {
            const layer = layers.items[idx - 1];
            list[idx] = { id: layer.layer.id, name: layer.layer.title }
        }
        return list
    }
    /** 4-7 レイヤー情報の取得処理追加 End */

    /** 4-5 UI コンポーネント用ファンクション Start */
    // マーキング対象のレイヤーを設定
    selLayer = (selected: React.FormEvent<HTMLInputElement>) => {
        // アプリに設定した Web マップ、FeatureLayer のデータソース一覧を取得
        let dsManager = DataSourceManager.getInstance();
        // マップ ウィジェットに設定した Web マップのレイヤー一覧を取得
        const layerViews = this.state.jimuMapView.jimuLayerViews;
        let dataSource = null;
        // 検索対象としたレイヤーのデータソースを取得する。
        for (let layerView in layerViews) {
            let layer = layerViews[layerView];
            if (layer.jimuLayerId === selected.currentTarget.value) {
                dataSource = dsManager.getDataSource(layer.layerDataSourceId);
            }
        }
        this.setState({
            selectLayer: selected.currentTarget.value,
            selLayerSource: dataSource
        });
    };

    // バッファーの半径を設定
    chgBufDst = (numVal: Number) => {
        this.setState({
            distance: numVal
        });
    };

    // 処理実行可否の設定
    chgWdgEbl = () => {
        const ebl = this.state.widgetEnable;
        if (ebl) {
            this.setState({ widgetEnable: false });
            this.state.jimuMapView.view.graphics.removeAll();
            this.state.selLayerSource.selectRecordsByIds([]);
        } else {
            this.setState({ widgetEnable: true });
        }
    };
    /** 4-5 UI コンポーネント用ファンクション End */

    /** 4-3 画面構成の定義 Start */
    // UI 情報レンダリング
    render() {
        // UI のデザインを設定
        const widgetStyle = css`
    background-color: var(--white);
    padding: 10px;
    height: 200px;
    `
        const tableStyle = css`
    background-color: var(--white);
    `

        // レイヤーリストをプルダウンに設定
        const { webmapLayers } = this.state;
        let layerList = webmapLayers.length > 0
            && webmapLayers.map((item, idx) => {
                return (
                    <Option key={idx} value={String(item.id)}>{item.name}</Option>
                )
            }, this);

        return (
            <div className="widget-starter jimu-widget" css={widgetStyle}>
                {this.props.hasOwnProperty("useMapWidgetIds") &&
                    this.props.useMapWidgetIds &&
                    this.props.useMapWidgetIds[0] && (
                        <JimuMapViewComponent
                            useMapWidgetId={this.props.useMapWidgetIds?.[0]}
                            onActiveViewChange={this.activeViewChangeHandler}
                        />
                    )
                }
                <h3>バッファー検索</h3>
                <Table css={tableStyle}>
                    <tr>
                        <Label>
                            レイヤー：
                            <Select
                                onChange={this.selLayer}
                                autoWidth={true}
                                placeholder="対象とするレイヤーを選択してください。">
                                {layerList}
                            </Select>
                        </Label>
                    </tr>
                    <tr>
                        <Label>
                            バッファーの距離（半径）[距離単位：{this.props.config.distanceUnitName}]：
                            <NumericInput defaultValue={Number(0)} onChange={this.chgBufDst} />
                        </Label>
                    </tr>
                    <tr>
                        <Label>
                            <Checkbox onChange={this.chgWdgEbl} checked={this.state.widgetEnable} /> バッファーを有効にする。
                        </Label>
                    </tr>
                </Table>
            </div>
        );
    }
    /** 4-3 画面構成の定義 End */
}
/** 4-1 ウィジェットの画面用モジュールの設定 End */
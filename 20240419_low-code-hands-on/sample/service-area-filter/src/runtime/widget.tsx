/**
 * ※本ウィジェットを使用する際の前提条件
 * 　Web マップに FeatureLayer を設定する際に GroupLayer にしていると正しく動作しません。
 */

/** 3-1 ウィジェットの画面用モジュールの設定 Start */
/** @jsx jsx */
import { React, AllWidgetProps, css, jsx, DataSourceManager, QueriableDataSource, MessageManager, DataSourceFilterChangeMessage } from "jimu-core";
import { JimuMapViewComponent, JimuMapView } from "jimu-arcgis";
import { Label, Select, Option, NumericInput, Table, Button } from "jimu-ui";

/** 3-4 使用する ArcGIS Maps SDK for JavaScript のモジュールをインポート Start */
import Polygon from "esri/geometry/Polygon";
import SimpleFillSymbol from "esri/symbols/SimpleFillSymbol";
import Graphic from "esri/Graphic";
import Query from "esri/rest/support/Query";
import LayerView from "esri/views/layers/LayerView";
import Collection from "esri/core/Collection";
import FeatureLayerView from "esri/views/layers/FeatureLayerView";
import FeatureFilter from "esri/layers/support/FeatureFilter";
import serviceArea from "esri/rest/serviceArea";
import ServiceAreaParameters from "esri/rest/support/ServiceAreaParameters";
import networkService from "esri/rest/networkService";
import TravelMode  from "esri/rest/support/TravelMode"
import FeatureSet from "esri/rest/support/FeatureSet";
import Search from "esri/widgets/Search";
import WebStyleSymbol from "esri/symbols/WebStyleSymbol";
/** 3-4 使用する ArcGIS Maps SDK for JavaScript のモジュールをインポート End */

export default class Widget extends React.PureComponent<AllWidgetProps<any>, any> {

    /** 3-2 State の定義 Start */
    // 処理内で利用する変数を定義
    state = {
        jimuMapView: null, /** 対象 Webマップ */
        webmapLayers: [], /** Web マップのレイヤー情報 */
        selectLayer: null, /** 選択したレイヤー情報 */
        breaks: 0, /** 到達圏を作成する大きさ */
        beforTargetLayer: null, /** 直前に実行対象としたレイヤー */
        serviceAreaUrl: null, /** 到達圏解析用 Url */
        networkDescription: null, /** ネットワークの説明 */
        travelModes: [], /** 到達圏解析条件一覧 */
        selTravelMode: null, /** 選択した到達圏解析条件 */
        serviceAreaPolygon: null /** 到達圏解析の結果ポリゴン */
    };
    /** 3-2 State の定義 End */

    /** 3-6 マップ ウィジェット イベント追加 Start */
    // マップ ウィジェットが変更されたときにマップ情報と検索ウィジェットの設定
    activeViewChangeHandler = async (jmv: JimuMapView) => {
        if (jmv) {
            this.state.serviceAreaUrl = this.props.config.serviceUrl ? this.props.config.serviceUrl : "https://route-api.arcgis.com/arcgis/rest/services/World/ServiceAreas/NAServer/ServiceArea_World";
            this.state.networkDescription = await networkService.fetchServiceDescription(this.state.serviceAreaUrl);
            this.setState({
                jimuMapView: jmv,
                webmapLayers: this.setLayerList(jmv.view.layerViews),
                travelModes: this.setTravelList(this.state.networkDescription.supportedTravelModes)
            });
            const searchWidget: Search = new Search({
                view: jmv.view,
                popupEnabled: false,
                container: document.getElementById("searchArea")
            });
            const pinSymbol = new WebStyleSymbol({
                name: "Pushpin 3",
                styleName: "EsriIconsStyle"
            })

            searchWidget.viewModel.defaultSymbols = {
                point: pinSymbol
            } 
        }
    };
    /** 3-6 マップ ウィジェット イベント追加 End */

    /** 3-8 必須項目チェック追加 Start */
    // 必須項目入力チェック
    eventErrorCheck = () => {
        let requrirdMsg = ""; /** エラー メッセージ格納用 */

        const point = this.state.jimuMapView.view.graphics.find((graphic : Graphic) => {
            return graphic.geometry.type == "point"
        });
        if (!point) {
            requrirdMsg = "起点が設定されていません。\n";
        }

        // レイヤーが選択されていない場合はエラー
        if (!this.state.selectLayer || this.state.selectLayer.length == 0) {
            requrirdMsg = requrirdMsg + "対象のレイヤーを選択してください。\n";
        }

        // 解析条件が選択されていない場合はエラー
        if (!this.state.selTravelMode || this.state.selTravelMode.length == 0) {
            requrirdMsg = requrirdMsg + "解析条件を選択してください。\n";
        }

        // 到達距離または到達時間が 0 以下の場合はエラー
        if (this.state.breaks <= 0) {
            requrirdMsg = requrirdMsg + "到達距離または時間は 0 より大きい値を入力してください。";
        }

        return requrirdMsg;
    }
    /** 3-8 必須項目チェック追加 End */

    /** 3-9 到達圏フィルターの本処理追加 Start */
    // 到達圏フィルターの本処理
    doServiceAreaSerch = async () => {

        // 前回の実行結果があれば削除する。
        if (this.state.serviceAreaPolygon) {
            if (this.state.beforTargetLayer.id !== this.state.selectLayer) this.doFilterAction(this.state.beforTargetLayer, "1=1");
            this.state.jimuMapView.view.graphics.remove(this.state.serviceAreaPolygon);
            this.state.serviceAreaPolygon = null;
        }

        // 指定した条件で到達圏を作成
        const travelMode = this.state.networkDescription.supportedTravelModes.find(
            (travelMode) => (travelMode.name === this.state.selTravelMode)
        );
        const basePoint: Graphic = this.state.jimuMapView.view.graphics.find((graphic : Graphic) => {
            return graphic.geometry.type == "point"
        });
        const serviceAreaParameters: ServiceAreaParameters = new ServiceAreaParameters({
            facilities: new FeatureSet({
                features: [basePoint]
            }),
            defaultBreaks: [this.state.breaks],
            travelMode,
            travelDirection: "from-facility",
            outSpatialReference: this.state.jimuMapView.view.spatialReference,
            trimOuterPolygon: true
        });

        // 到達圏解析を実行
        const results = await serviceArea.solve(this.state.serviceAreaUrl, serviceAreaParameters);
        const area: any = results.serviceAreaPolygons.features[0]?.geometry;
        // 到達圏の作成が正常に終了した場合に描画処理を実行
        if (area) {
            // 到達圏のグラフィックを定義
            let areaGraphic = new Graphic({
                geometry: area,
                symbol: this.setAreaSymbol()
            });
            // 到達圏のグラフィックをマップに表示
            this.state.jimuMapView.view.graphics.add(areaGraphic);
            this.state.serviceAreaPolygon = areaGraphic;

            // 到達圏内のレイヤー取得およびフィルタリング、連携処理実行
            this.layerGetAndMarking(area);
        }

    }
    /** 3-9 到達圏フィルターの本処理追加 End */

    /** 3-10 到達圏内に含まれるレイヤーの取得およびフィルタリング連携処理の呼び出し追加 Start */
    // 到達圏内のレイヤー取得およびフィルタリング、連携の処理
    layerGetAndMarking = async (area: Polygon) => {
        // プルダウンで選択したレイヤーを Web マップから取得
        const targetLayer: any = this.state.jimuMapView.view.map.findLayerById(this.state.selectLayer);
        let primaryKey: string;
        // 取得したレイヤータイプがフィーチャ レイヤーの場合描画処理を実行
        if (targetLayer.type == "feature") {
            // 到達圏内にある対象のレイヤーを取得
            const query = new Query({ returnGeometry: true, outFields: ["*"] });
            query.geometry = area;
            query.spatialRelationship = "contains";

            primaryKey = targetLayer.objectIdField;
            // 到達圏内にあるオブジェクトを取得
            const objList = await targetLayer.queryFeatures(query).then(results => {
                let objList = [];
                // 到達圏内にあるオブジェクトをリストに格納
                for (let idx = 0; idx < results.features.length; idx++) {
                    objList.push(results.features[idx].attributes[primaryKey]);
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
                    ? primaryKey + ` IN (${objList
                        .map(item => {
                            return item
                        })
                        .join()})`
                    : '1=2';

            // フィルター処理を実行
            this.doFilterAction(targetLayer, selectedQuery);

            this.state.jimuMapView.view.goTo(area);
            this.state.beforTargetLayer = targetLayer;
        }
    }
    /** 3-10 到達圏内に含まれるレイヤーの取得およびフィルタリング連携処理の呼び出し追加 End */

    /** 3-11 フィルター処理の追加  Start */
    doFilterAction = async (targetLayer: any, wherePhrase: string) => {
        // フィルター条件の設定
        const featureFilter: FeatureFilter = new FeatureFilter({
            where: wherePhrase
        });
        const queryParams = { where: wherePhrase } as any

       // アプリに設定した Web マップ、FeatureLayer のデータソース一覧を取得
       let dsManager = DataSourceManager.getInstance();
       // マップ ウィジェットに設定した Web マップのレイヤー一覧を取得
       const layerViews = this.state.jimuMapView.jimuLayerViews;
       let dataSource = null;
       // 検索対象としたレイヤーのデータソースを取得する。
       for (let layerView in layerViews) {
           let layer = layerViews[layerView];
           if (layer.jimuLayerId === targetLayer.id) {
               dataSource = dsManager.getDataSource(layer.layerDataSourceId);
           }
       }

        // フィルター対象の LayerView を設定、フィルター
        let targetLayerView: FeatureLayerView;
        await this.state.jimuMapView.view.whenLayerView(targetLayer).then(layerview => {
            targetLayerView = layerview;
        });
        targetLayerView.filter = featureFilter;

        // フィルター処理の実施
        if (dataSource) {
            // ウィジェット間連携（フィルターされているデータを連携）
            (dataSource as QueriableDataSource).updateQueryParams?.(queryParams, this.props.id);
            MessageManager.getInstance().publishMessage(new DataSourceFilterChangeMessage(this.props.id, [dataSource.id]));
        }
    }
    /** 3-11 フィルター処理の追加  End  */

    /** 3-12 到達圏用のシンボル定義を追加 Start */
    // 到達圏用のシンボル（ポリゴン）を定義
    setAreaSymbol = () => {
        return new SimpleFillSymbol({
            color: [51, 51, 204, 0.2],
            style: "solid",
            outline: {
                color: "black",
                width: 1
            }
        });
    }
    /** 3-12 到達圏用のシンボル定義を追加 End */

    /** 3-7 レイヤー情報の取得処理および移動条件情報の取得処理追加 Start */
    // マップ ウィジェットに設定されいてるレイヤー情報の取得
    setLayerList = (layers: Collection<LayerView>) => {
        const list = [];
        for (let idx = layers.length; 0 < idx; idx--) {
            const layer = layers.items[idx - 1];
            list[idx] = { id: layer.layer.id, name: layer.layer.title }
        }
        return list;
    }

    setTravelList = (modes: TravelMode[]) => {
        const list = [];
        for (let idx = 0; idx < modes.length; idx++) {
            const mode = modes[idx];
            list[idx] = {name: mode.name}
        }
        return list;
    }
    /** 3-7 レイヤー情報の取得処理および移動条件情報の取得処理 End */

    /** 3-5 UI コンポーネント用ファンクション Start */
    // マーキング対象のレイヤーを設定
    selLayer = (selected: React.FormEvent<HTMLInputElement>) => {
        this.setState({
            selectLayer: selected.currentTarget.value
        });
    };
    
    // マーキング対象のレイヤーを設定
    selTravelMode = (selected: React.FormEvent<HTMLInputElement>) => {
        this.setState({
            selTravelMode: selected.currentTarget.value
        });
    };

    // 到達距離および時間を設定
    chgServiceTime = (numVal: Number) => {
        this.setState({
            breaks: numVal
        });
    };

    // メイン処理を実行
    doAction = () => {
        const msg = this.eventErrorCheck();
        if (!msg) {
            this.doServiceAreaSerch();
        } else {
            alert(msg);
        }
    }

    // クリア処理の実行
    doClear = () => {
        this.state.jimuMapView.view.graphics.removeAll();
        this.state.serviceAreaPolygon = null;

        this.state.jimuMapView.view.map.layers.forEach(layer => {
            this.doFilterAction(layer, "1=1");    
        })
    }
    /** 3-5 UI コンポーネント用ファンクション End */

    /** 3-3 画面構成の定義 Start */
    // UI 情報レンダリング
    render() {
        // UI のデザインを設定
        const widgetStyle = css`
    background-color: var(--white);
    padding: 10px;
    margin: 2px;
    `
        const tableStyle = css`
    background-color: var(--white);
    `

        // レイヤーリストをプルダウンに設定
        const { webmapLayers, travelModes } = this.state;
        let layerList = webmapLayers.length > 0
            && webmapLayers.map((item, idx) => {
                return (
                    <Option key={idx} value={String(item.id)}>{item.name}</Option>
                )
            }, this);

        let  travelList = travelModes.length > 0
                && travelModes.map((item, idx) => {
                    return(
                        <Option key={idx} value={String(item.name)}>{item.name}</Option>
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
                <h3>到達圏フィルター</h3>
                <Table css={tableStyle}>
                    <tr>
                        <Label>
                            起点：
                            <div id="searchArea"></div>
                        </Label>
                    </tr>
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
                            解析条件：
                            <Select
                                onChange={this.selTravelMode}
                                autoWidth={true}
                                placeholder="対象とする解析条件を選択してください。">
                                {travelList}
                            </Select>
                            <NumericInput defaultValue={Number(0)} onChange={this.chgServiceTime} />
                        </Label>
                    </tr>
                    <tr>
                        <Button size="lg" onClick={this.doAction} type="primary">実行</Button>
                        <Button size="lg" onClick={this.doClear} type="default">クリア</Button>
                    </tr>
                </Table>
            </div>
        );
    }
    /** 3-3 画面構成の定義 End */
}
/** 3-1 ウィジェットの画面用モジュールの設定 End */
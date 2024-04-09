/** 2-1 設定画面用モジュールの設定 Start */
/** @jsx jsx */
import { React, jsx } from "jimu-core";
import { AllWidgetSettingProps } from "jimu-for-builder";
import { TextInput } from "jimu-ui";
import { MapWidgetSelector } from "jimu-ui/advanced/setting-components";

export default class Setting extends React.PureComponent<AllWidgetSettingProps<any>, any> {
    /** 2-3 コンポーネントのイベント処理追記 Start */
    // 対象のマップを設定
    onMapWidgetSelected = (useMapWidgetIds: string[]) => {
        this.props.onSettingChange({
            id: this.props.id,
            useMapWidgetIds: useMapWidgetIds
        });
    };

    // 使用するネットワーク解析サービス URL を設定
    onNetworkAnalysisServiceUrlSet = (url: React.FormEvent<HTMLInputElement>) => {
        this.props.onSettingChange({
            id: this.props.id,
            config: this.props.config.set('serviceUrl', url.currentTarget.value)
        })
    };
    /** 2-3 コンポーネントのイベント処理追記 End */

    /** 2-2 設定画面の画面構成追加 Start */
    render() {
        return <div className="widget-setting-demo">
            使用マップ
            <MapWidgetSelector
                useMapWidgetIds={this.props.useMapWidgetIds}
                onSelect={this.onMapWidgetSelected}
            />
            ネットワーク解析サービス URL
            <TextInput 
                type="text" 
                placeholder="【任意】お持ちのネットワーク サービスを利用したい方は入力してください。" 
                defaultValue={this.props.config.serviceUrl} 
                onChange={this.onNetworkAnalysisServiceUrlSet}>
            </TextInput>
        </div>;
    }
    /** 2-2 設定画面の画面構成追加 End */
}
/** 2-1 設定画面用モジュールの設定 End */
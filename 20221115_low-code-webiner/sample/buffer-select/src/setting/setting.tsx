/** @jsx jsx */
import { React, jsx } from "jimu-core";
import { AllWidgetSettingProps } from "jimu-for-builder";
import { MapWidgetSelector } from "jimu-ui/advanced/setting-components";
import { Select, Option } from 'jimu-ui';

export default class Setting extends React.PureComponent<AllWidgetSettingProps<any>, any> {

    // 対象のマップを設定
    onMapWidgetSelected = (useMapWidgetIds: string[]) => {
        this.props.onSettingChange({
            id: this.props.id,
            useMapWidgetIds: useMapWidgetIds
        });
    };

    // 距離単位の名称と値を設定
    setUint = (distanceUnit: React.FormEvent<HTMLInputElement>) => {
        console.log("setUint");
        this.props.onSettingChange({
            id: this.props.id,
            config: this.props.config.set('distanceUnit', distanceUnit.currentTarget.value).set('distanceUnitName', distanceUnit.currentTarget.title)
        });
    };

    render() {
        return <div className="widget-setting-demo">
            使用マップ
            <MapWidgetSelector
                useMapWidgetIds={this.props.useMapWidgetIds}
                onSelect={this.onMapWidgetSelected}
            />
            使用距離単位
            <Select
                onChange={this.setUint}
                placeholder="距離単位を選択してください。"
                defaultValue={this.props.config.distanceUnit}
            >
                <Option header>
                    距離単位
                </Option>
                <Option value="meters" title="メートル" >
                    メートル
                </Option>
                <Option value="kilometers" title="キロメートル">
                    キロメートル
                </Option>
                <Option value="feet" title="フィート">
                    フィート
                </Option>
                <Option value="nautical-miles" title="海里マイル">
                    海里
                </Option>
                <Option value="yards" title="ヤード">
                    ヤード
                </Option>
            </Select>
        </div>;
    }
}

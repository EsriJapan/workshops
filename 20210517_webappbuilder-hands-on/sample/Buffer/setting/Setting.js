define([
  'dojo/_base/declare',
  'jimu/BaseWidgetSetting',
  'dijit/_WidgetsInTemplateMixin',
  'dijit/form/Select'
], function(declare, BaseWidgetSetting, _WidgetsInTemplateMixin) {
  return declare([BaseWidgetSetting, _WidgetsInTemplateMixin], {

    baseClass: 'jimu-widget-buffer-setting',

    postCreate: function() {
      this.setConfig(this.config);
    },

    setConfig: function(config) {
      // 構成画面の距離と単位に config.json に設定されている値を表示する
      this.selectLengthUnit.set('value', config.measurement.lengthUnit);
    },

    getConfig: function() {
      // 構成画面の値を取得する
      var lengthUnit = this.selectLengthUnit.value;
      var lengthUnitLabel = (lengthUnit === 'kilometers') ? 'キロメートル' : 'メートル';

      // 取得した値を保存する
      return {
        measurement: {
          lengthUnit: lengthUnit,
          lengthUnitLabel: lengthUnitLabel
        }
      };
    }

  });
});

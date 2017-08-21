## タスク

Web マップに含まれる避難場所レイヤーを取得して shelterLayer 変数へ代入しましょう。

## 回答例

```js
// レイヤーの取得
var shelterLayer;
webmap.then(function(){
  shelterLayer = webmap.layers.find(function(layer){
    return layer.title === "レイヤーの名前";
  });
});
```

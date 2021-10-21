// API キーを入力
const apiKey="YOUR_API_KEY";
const basemap = "OSM:Streets"; 

/* ハンズオンでは、使用しなかったベースマップの一部
"ArcGIS:Imagery:Labels" // 衛星画像にラベルを表示した地図
"ArcGIS:Navigation" // ナビゲーションに適した地図
"ArcGIS:Topographic" //　地形図を含んだ地図
"OSM:Standard" // Open Street Map をベクタータイル レイヤーとしたもの
"OSM:DarkGray" // Open Street Map を元にした白黒地図
*/

// 地図の描画設定

// 地図を描画する場所を東京駅上空に指定
const map = L.map('map', {
    minZoom: 2
}).setView([35.68109305881504, 139.76717512821057], 14);

// Esri のベクタータイルをベースマップに設定
L.esri.Vector.vectorBasemapLayer(basemap, {
  apiKey: apiKey
}).addTo(map);

/* 地名検索の機能 */

// 地名検索
const searchControl = L.esri.Geocoding.geosearch({
    position: 'topleft', // 検索窓をどこに配置するかを指定
    placeholder: '住所または場所の名前を入力',
    useMapBounds: false, // 世界中からの検索結果を出力
    providers: [L.esri.Geocoding.arcgisOnlineProvider({
      apikey: apiKey
    })]
}).addTo(map);
  
// 検索後の動作を指定。結果を地図上に描画。検索結果最上位を基本的に取得
searchControl.on('results', function (data) {
    if(data.results){
        coordinates = data.results[0].latlng;
        addstoppoint();
    }    
});

/* ルート検索の機能 */

// マップ上の検索結果をリセットするためにスタート地点とゴール地点、ルート案内のラインのレイヤーグループを作成
const startLayerGroup = L.layerGroup().addTo(map);
const endLayerGroup = L.layerGroup().addTo(map);
const routeLines = L.layerGroup().addTo(map);

let currentStep = "start"; 
let startCoords, endCoords;

// ルート検索をしたい地点を関数化
function addstoppoint(){
    if (currentStep === "start") {
      startLayerGroup.clearLayers(); 
      endLayerGroup.clearLayers(); 
      routeLines.clearLayers(); 
      L.marker(coordinates).addTo(startLayerGroup); // スタート地点にマーカーを作成
      startCoords = [coordinates.lng,coordinates.lat]; 
      currentStep = "end"; 
    } else {
      L.marker(coordinates).addTo(endLayerGroup); // ゴール地点にマーカーを作成
      endCoords = [coordinates.lng,coordinates.lat]; 
      currentStep = "start"; 
    }
  
    if (startCoords && endCoords) {
      searchRoute(); // スタート地点とゴール地点ができたらルート検索をかける
    }
}

// ルート検索の実行をする関数
function searchRoute() {
    // arcgis-rest-js を利用するための認証用の変数を用意します。
    const authentication = new arcgisRest.ApiKey({
      key: apiKey
    });
    // ルート検索
    arcgisRest
      .solveRoute({
        stops: [startCoords, endCoords], 
        endpoint: "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World/solve",
        authentication,
        params:{directionsLanguage:"ja"} // 使用言語を日本語に変更
        })
        // 結果の表示
      .then((response) => {
        L.geoJSON(response.routes.geoJson).addTo(routeLines); 
        const directionsHTML = response.directions[0].features.map((f) => f.attributes.text).join("<br/>");
        directions.innerHTML = directionsHTML;
        startCoords = null; // 最後にスタート、ゴール地点の位置情報をリセット
        endCoords = null;
      })
      // エラー時の表示
      .catch((error) => {
        console.error(error);
        alert("ルート検索に失敗しました");
        startCoords = null; // 最後にスタート、ゴール地点の位置情報をリセット
        endCoords = null;
      });
 }

 // クリックした場所の位置情報を返す
map.on("click", (e) => {
    coordinates = e.latlng;
    addstoppoint();
  });
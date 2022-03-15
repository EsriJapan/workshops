const apiKey="YOUR_API_KEYS";
const basemap = "OSM:Streets";

// RestJS を使用するための API キーを設定
const authentication = new arcgisRest.ApiKey({
  key: apiKey
});

// zoom control の位置を変えるために zoomControl には false を指定
const map = L.map('map', {
    zoomControl:false
}).setView([35.68109305881504, 139.76717512821057], 14);

// zoom のコントローラーを右上に指定
L.control.zoom( { position: 'topright' } ).addTo( map );

// Esri のベクタータイルをベースマップに設定
L.esri.Vector.vectorBasemapLayer(basemap, {
  apiKey: apiKey
}).addTo(map);

// 設定されている id の要素もしくは要素自体を取得
const search=document.getElementById("geocode");
const directions=document.getElementById("direction");
const loading=document.getElementsByTagName("calcite-loader");

// マップ上の検索結果をリセットするために Layer Group を作成
const startLayerGroup = L.layerGroup().addTo(map);
const endLayerGroup = L.layerGroup().addTo(map);

// マップ上の検索結果をリセットするために Layer Group for route lines を作成
const routeLines = L.layerGroup().addTo(map);

// 道中にあるもののポイントレイヤーグループ
const alonglayer=L.layerGroup().addTo(map);

let currentStep = "start";
let startCoords, startpoint, endCoords, endpoint

// 住所、場所検索
start_search=geocoder("start");
end_search=geocoder("end");

// 地名検索の検索ボタンの位置をアコーディオンメニュー内に入れる 
const start_container=start_search.getContainer();
search.appendChild(start_container); 
const end_container=end_search.getContainer();
search.appendChild(end_container); 

// 検索バーを開いている状態に設定する
start_container.click();  
end_container.click();

// geocode の参照元のアクセスキーを設定
const geocodeService = L.esri.Geocoding.geocodeService({
  apikey: apiKey 
});

// 使用するレイヤーの表示
population=L.esri.featureLayer({
  url: 'https://services3.arcgis.com/qcPpjnUOIagfSFbp/arcgis/rest/services/%E4%BA%BA%E6%B5%81_mesh_gdb/FeatureServer/0', //urlを消しておく
  style:function(feature){ // スタイルの設定
    if (feature.properties.人流_population<=5000){
      return  { color: 'yellow'};
    }else if(feature.properties.人流_population<=15000){
      return { color: 'gold'};    
    }else if(feature.properties.人流_population<=25000){
      return { color: 'orange'};
    }else if(feature.properties.人流_population<=35000){
      return { color: 'darkorange'};
    }else if(feature.properties.人流_population<=45000){
      return { color: 'coral'};
    }else if(feature.properties.人流_population<=55000){
      return { color: 'tomato'};
    }else if(feature.properties.人流_population<=65000){
      return { color: 'orangered'};
    }else if(feature.properties.人流_population<=75000){
      return { color: 'red'};
    }else if(85000<=feature.properties.人流_population){
      return { color: 'crimson'};
    }else{
      return { color: 'blue'};
    }
  }
}).bindTooltip(function (layer) {
  return "平均滞在人口:"+layer.feature.properties.人流_population+"人";
},{direction:"top"});

// leaflet で人流メッシュの表示、非表示をコントロール
popmesh={"人流メッシュ":population};
L.control.layers({},popmesh).addTo(map);

// 始点終点の位置情報がない場合 layergroup のレイヤーのリセットの実行する関数
function layerclear(){
  if(!startCoords && !endCoords){
    startLayerGroup.clearLayers(); 
    endLayerGroup.clearLayers(); 
    routeLines.clearLayers(); 
    alonglayer.clearLayers();
  }
}

// 住所、地名検索の関数
function geocoder(step){ // currentStep の値を指定するために引数に指定
  // placeholder の中身を始点と終点で変更する
  if(step==="start"){
    placeholder="①始点"; 
  }else{
    placeholder="②終点";
  }
  const searchControl = L.esri.Geocoding.geosearch({
    position: 'topright',
    placeholder: placeholder,
    title:'地名検索',
    collapseAfterResult:false, // 検索バーを閉じないように設定
    useMapBounds: false,
    providers: [L.esri.Geocoding.arcgisOnlineProvider({
      apikey: apiKey
    })]
  }).addTo(map);
  // 検索結果最上位を基本的に取得
  searchControl.on('results', function (data) {
    if(data.results){
      coordinates = data.results[0].latlng;
      addtostoppoint(data.results[0].text,coordinates);
    }
  });
  return searchControl;
}

// ルート検索をしたい始点終点を決めるための関数
function addtostoppoint(pointname,coordinates){ // 場所の名前と取得した位置情報を引数に設定
  if (currentStep === "start") {
    layerclear();
    L.marker(coordinates,{icon:divIcon1}).addTo(startLayerGroup).bindPopup(pointname); // スタート地点にマーカーを作成
    startCoords = [coordinates.lng,coordinates.lat];
    startpoint=pointname;
    currentStep = "end";
  } else {
    layerclear();
    L.marker(coordinates,{icon:divIcon2,popup:pointname}).addTo(endLayerGroup).bindPopup(pointname); // ゴール地点にマーカーを作成
    endCoords = [coordinates.lng,coordinates.lat]; 
    endpoint=pointname;
    currentStep = "start"; 
  }
  if (startCoords && endCoords) {
    searchRoute(); // startとendができたらルート検索をかける
  }
 }

 // ルート案内の文章に calcite-icon を指定するための関数
function adddirection(str,startpoint,endpoint){ // str: ルート案内の文章, startpoint: スタート地点の場所名, endpoint: ゴール地点の場所名
  str=str.replace("Location 1",startpoint);
  str=str.replace("Location 2",endpoint);
  str_split=str.split("<br>");
  direction=str_split[0]+"<br><hr>";
  for(i=1; i<str_split.length; i++){
    if(str_split[i].match(/[左右]|U/g)){
      str_split[i]=str_split[i].replace("左",'<br><calcite-icon icon="left" /></calcite-icon>左')
      str_split[i]=str_split[i].replace("右",'<br><calcite-icon icon="right" /></calcite-icon>右')
      str_split[i]=str_split[i].replace("U ターン",'<br><calcite-icon icon="u-turn-right" /></calcite-icon>Uターン')
    }else{
      str_split[i]='<br><calcite-icon icon="compass" /></calcite-icon>'+str_split[i];
    }
    direction+=str_split[i]+"<br><hr>";
}
  return direction;
}

// 各ポイントでの 100m 以内に存在する POIを検索する 
function addPoi(center){ 
  arcgisRest.geocode({
    params: {
        category: "Convenience Store",// POI 検索
        location: center,
        maxLocations: 5
      },
    authentication
  }).then((response) => {
    for(i=0;i<=response.candidates.length;i++){
      latlng=response.geoJson.features[i].geometry.coordinates;
      names=response.candidates[i].address;
      nearpoint=L.latLng(latlng[1],latlng[0]); 
      center_latlng=L.latLng(center[1],center[0]);
      distance=center_latlng.distanceTo(nearpoint);
      //距離が 100m 以内のもののみ表示するようにする
      if(distance<=100){
        (async()=>{
          await overlap(population,nearpoint,names);
        })();
      }else{
          break;
      }
    }

  });

}

// 人流データとの重なりを判定して、その位置にマーカーを描画
function overlap(polygon,point,placename){
  return polygon.query().intersects(nearpoint).run(function(error,response,featureCollection){
    // 店名と重なっている人流メッシュレイヤーの平均滞在人数を Tooltip で表示 
    L.marker(point,{icon:store}).bindPopup(placename).bindTooltip("<b>"+placename+"</b><br>周辺平均滞在人口:"+featureCollection.features[0].properties.人流_population+"人").addTo(alonglayer);
  });
}

// ルート検索の関数
function searchRoute() { 
  loading[0].setAttribute("active",""); // ルート検索開始後に calcite-loader を active にする
   // arcgis-rest-js のサービスを利用するために API キーを指定
   
  arcgisRest
  //　ルート検索の開始
  .solveRoute({
    stops: [startCoords, endCoords], 
    endpoint: "https://route-api.arcgis.com/arcgis/rest/services/World/Route/NAServer/Route_World/solve",
    authentication,
    params:{directionsLanguage:"ja"} // 使用言語を日本語に変更
    })
    // 結果の表示
  .then((response) => {
    geojson=L.geoJSON(response.routes.geoJson).addTo(routeLines); // geojson 化したルートを表示
    const directionsHTML = response.directions[0].features.map((f) => f.attributes.text).join("<br>");
    directions.innerHTML = adddirection(directionsHTML,startpoint,endpoint);
    startCoords = null; // 最後にスタート、ゴール地点の情報を消す
    endCoords = null;
    loading[0].removeAttribute("active"); // ルート検索終了後に calcite-loader を削除
    // Polyline の点ごとに近隣検索
    response.routes.geoJson.features[0].geometry.coordinates
    .map(point => addPoi(point));
  })
  // エラー時の表示
  .catch((error) => {
    console.error(error);
    alert("ルート検索に失敗しました<br>始点と終点の情報をリセットします");
    startCoords = null; // エラー時にも始点、終点の位置情報をリセット
    endCoords = null;
    loading[0].removeAttribute("active"); // ルート検索終了後に calcite-loader を削除
  });
}

// マーカーデザインの設定 divicon1 は スタート地点、divicon2 はゴール地点
const divIcon1 = L.divIcon({
  html: '<calcite-icon icon="number-circle-1" /></calcite-icon>',
  className: 'divicon',
  iconSize: [25,25],
  popupAnchor: [0, 0]
});

const divIcon2 = L.divIcon({
  html: '<calcite-icon icon="number-circle-2" /></calcite-icon>',
  className: 'divicon',
  iconSize: [25,25],
  popupAnchor: [0, 0]
});

// static.arcgis.com のシンボルを参照して icon を作成
const store= L.icon({
  iconUrl: 'http://static.arcgis.com/images/Symbols/PeoplePlaces/Shopping.png',
  iconSize: [24, 24],
  popupAnchor: [0, 0]
});

// クリックした場所の位置情報を返し、reverce geocoding を実行し、地名を取得。
map.on("click", (e) => {
  let coordinates = e.latlng;
  geocodeService.reverse().latlng(coordinates).run(function (error, result) {
    if (error) {
      return;
    }
    if(result.address["Match_addr"]==="日本"){
      address=coordinates.lat+","+coordinates.lng;
    }else{
      address=result.address["Match_addr"]
    }
    if(currentStep==="start"){
      start_container.firstChild.value=address;
    }else{
      end_container.firstChild.value=address;
    }
    addtostoppoint(address,coordinates);
  })
  
});

const apiKey="YOUR_API_KEY";
const basemap = "OSM:Streets";

// zoom control の位置を変えるために zoomControl には false を指定
const map = L.map('map', {
    minZoom: 2,
    zoomControl:false
}).setView([35.68109305881504, 139.76717512821057], 14);

// zoom のコントローラーを右上に指定
L.control.zoom( { position: 'topright' } ).addTo( map );

// Esri のベクタータイルをベースマップに設定
L.esri.Vector.vectorBasemapLayer(basemap, {
  apiKey: apiKey
}).addTo(map);

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

// 始点終点の位置情報がない場合 layergroup のレイヤーのリセットの実行する関数
function layerclear(){
  if(!startCoords && !endCoords){
    startLayerGroup.clearLayers(); 
    endLayerGroup.clearLayers(); 
    routeLines.clearLayers(); 
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
  currentStep=step;
  if(data.results){
    coordinates = data.results[0].latlng;
    addtostoppoint(data.results[0].text);
  }
  });
return searchControl;
}

// ルート検索をしたい始点終点を決めるための関数
function addtostoppoint(pointname){ // 場所の名前を引数に設定
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
 function add_direction(str,startpoint,endpoint){ // str: ルート案内の文章, startpoint: スタート地点の場所名, endpoint: ゴール地点の場所名
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

// 設定されている id の要素もしくは要素自体を取得
const search=document.getElementById("geocode");
const directions=document.getElementById("direction");
const loading=document.getElementsByTagName("calcite-loader");

 // マップ上の検索結果をリセットするために Layer Group を作成
const startLayerGroup = L.layerGroup().addTo(map);
const endLayerGroup = L.layerGroup().addTo(map);

 // マップ上の検索結果をリセットするために Layer Group for route lines を作成
const routeLines = L.layerGroup().addTo(map);

let currentStep = "start";
let startCoords, startpoint, endCoords, endpoint


// ルート検索の関数
function searchRoute() { 
  loading[0].setAttribute("active",""); // ルート検索開始後に calcite-loader を active にする
   // arcgis-rest-js のサービスを利用するために API キーを指定
   const authentication = new arcgisRest.ApiKey({
     key: apiKey
   });
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
       directions.innerHTML = add_direction(directionsHTML,startpoint,endpoint);
       startCoords = null; // 最後にスタート、ゴール地点の情報を消す
       endCoords = null;
     loading[0].removeAttribute("active"); // ルート検索終了後に calcite-loader を削除
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

// 住所、場所検索
start_search=geocoder("start");
end_search=geocoder("end");

// 地名検索の検索ボタンの位置をアコーディオンメニュー内に入れる 
start_container=start_search.getContainer();
search.appendChild(start_container); 
end_container=end_search.getContainer();
search.appendChild(end_container); 

// 検索バーを開いている状態に設定する
start_container.click();  
end_container.click();

const geocodeService = L.esri.Geocoding.geocodeService({
  apikey: apiKey 
});

// クリックした場所の位置情報を返し、reverce geocoding を実行し、地名を取得。
map.on("click", (e) => {
  coordinates = e.latlng;
  geocodeService.reverse().latlng(coordinates).run(function (error, result) {
    if (error) {
      return;
    }
    if(result.address["Match_addr"]=="日本"){
      address=coordinates.lat+","+coordinates.lng;
    }else{
      address=result.address["Match_addr"]
    }
    if(currentStep=="start"){
      start_container.firstChild.value=address;
    }else{
      end_container.firstChild.value=address;
    }
    addtostoppoint(address);
  })
  
});

import { useState, useRef, useEffect } from "react";
import AlfMap from "./components";
import destination from "@turf/destination";
import "./App.css";
import MapGL from "./components/MapGl";

function App() {
  /**
   * @type {{current:MapGL}}
   */
  const mapInstanceRef = useRef();
  const bearing2 = destination([103.998888, 30.699999], 10, 45);
  console.log(bearing2);

  useEffect(() => {
    setTimeout(() => {
      console.log("ddddd");
      const types = ["uav", "adsb", "fighter", "unknown"];
      const datas = [];
      const referCoords = [
        [104.0765756066678, 30.7142418297975],
        [104.07743987025782, 30.71427721213942],
        [104.07818066762167, 30.71385262318286],
        [104.07937417448562, 30.713428032357655],
        [104.08011497184947, 30.71303882245823],
        [104.08089692462204, 30.712826525487984],
        [104.08253387626479, 30.712374512590202],
        [104.08319236281119, 30.712551427593368],
        [104.0845504913114, 30.712692959363125],
        [104.08607324144788, 30.71279910805336],
        [104.08883065484201, 30.712976022426915],
        [104.08957145220597, 30.713471380530066],
        [104.09105304693361, 30.71403750096114],
        [104.09208193216136, 30.714780528983283],
        [104.093110817389, 30.715841987656674],
        [104.09451010129794, 30.71679729047281],
        [104.0950451216168, 30.717717202759772],
      ];
      for (let i = 0; i < 10; i += 1) {
        datas.push({
          id: `plane_${i}`,
          type: types[i % 4],
          coordinates: referCoords.map((item) => {
            const lng = i % 2 == 1 ? item[0] + i * 0.008 : item[0] - i * 0.008;
            const lat = i % 3 == 0 ? item[1] + i * 0.008 : item[1] - i * 0.008;
            return [lng, lat];
          }),
          description: "none",
        });
      }
      mapInstanceRef.current.drawPlanes(datas);

      setTimeout(() => {
        mapInstanceRef.current.showPalneTrack(false);
      }, 5000);
    }, 1000);
  }, []);

  // drawDevices
  useEffect(() => {
    setTimeout(() => {
      // {id:String,displayName:String,type:String,location:any,state:String}
      const devices = [
        {
          id: String(Math.random()),
          displayName: "监测",
          type: "monitoring",
          location: [103.998888, 30.699999],
          state: "idle",
        },
        {
          id: String(Math.random()),
          displayName: "测向",
          type: "directionFinding",
          location: [103.898888, 30.699999],
          state: "idle",
        },
        {
          id: String(Math.random()),
          displayName: "压制",
          type: "radioSuppressing",
          location: [103.998888, 30.599999],
          state: "idle",
        },
      ];
      console.log("draw devices");
      mapInstanceRef.current.drawDevices(devices);
    }, 1200);
  }, []);

  return (
    <div className="App">
      <div>
        <button
          onClick={() => {
            if (mapInstanceRef.current) {
              // setTimeout(() => {
              mapInstanceRef.current.setCenter([
                103.95179167727026, 30.556189685930814,
              ]);
              mapInstanceRef.current.setZoom(10);
              // }, 1000);
            }
          }}
        >
          zoomTo9
        </button>
        <button
          onClick={() => {
            if (mapInstanceRef.current) {
              mapInstanceRef.current.setZoom(14);
              setTimeout(() => {
                mapInstanceRef.current.setCenter([
                  103.95179167727026, 30.556189685930814,
                ]);
              }, 1000);
            }
          }}
        >
          zoomTo14
        </button>
      </div>
      <div style={{ flex: 1, position: "relative", margin: "8px" }}>
        <AlfMap
          tileLayers={[
            {
              name: "tdtst",
              url: `http://192.168.1.169:8182/tile?x={x}&y={y}&z={z}&ms=amap&mt=statelite`,
              saturation: -0.35,
              // rotate: -10,
              // contrast: 0.25,
            },
            {
              name: "tdtroad",
              url: `http://192.168.1.169:8182/tile?x={x}&y={y}&z={z}&ms=amap&mt=roades`,
              // url: `https://t6.tianditu.gov.cn/cia_w/wmts?SERVICE=WMTS&REQUEST=GetTile&VERSION=1.0.0&LAYER=cia&STYLE=default&TILEMATRIXSET=w&FORMAT=tiles&TILECOL={x}&TILEROW={y}&TILEMATRIX={z}&tk=9afc75c253b9c59d4159505cf0f46538`,
              opacity: 0.6,
              brightnessmax: 0.9,
              saturation: 0.1,
              // rotate: -10,
              // contrast: 0,
            },
          ]}
          zoom={11}
          onLoaded={(map) => {
            console.log("map loaded....");
            mapInstanceRef.current = map;
            map.showNavigator(true, "top-right");

            // map.showToolbar(
            //   {
            //     polygon: "面积",
            //     line_string: "测距",
            //     point: "标点",
            //     trash: "删除",
            //     measuring_angle: "角度",
            //   },
            //   true
            // );
            map.showToolbar(null);
            map.drawBearing([
              {
                coordinates: [
                  [103.998888, 30.699999],
                  bearing2.geometry.coordinates,
                ],
                color: "#00FF00",
              },
            ]);

            // setTimeout(() => {
            //   map.showLocation({
            //     latitude: 30.807777,
            //     longitude: 103.92777,
            //     heading: 45,
            //   });
            // }, 1000);
          }}
          onDrawFeature={(e) => {
            console.log("onDrawFeature :::", e);
          }}
        />
      </div>
    </div>
  );
}

export default App;

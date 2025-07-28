// import { mapImageIds } from "./loadImages.js";
import mapboxGl from "mapbox-gl";

class DrawBearing {
  /**
   * @type {mapboxGl.Map}
   */
  map;
  layerId = "bearing-layer";
  sourceId = "bearing-source";
  constructor(map) {
    this.map = map;
  }

  /**
   * type: monitoring|directionFinding|radioSuppressing|
   * @param {Array<{coordinates:Array<Number>,color:String}>} bearing
   */
  draw = (bearing) => {
    this.#drawBearings(bearing);
  };

  dispose = () => {};

  /**
   *
   * @param {Array<{coordinates:Array<Number>,color:String}>} bearing
   */
  #drawBearings = (bearing) => {
    // 绘制设备位置和状态
    const srcName = this.sourceId;
    const layerName = this.layerId;
    const geojson = {
      type: "FeatureCollection",
      features: [],
    };
    const features = bearing.map((item) => {
      return {
        type: "Feature",
        geometry: {
          type: "LineString",
          coordinates: item.coordinates,
        },
        properties: {
          color: item.color,
        },
      };
    });
    geojson.features = features;
    const source = this.map.getSource(srcName);
    if (source) {
      source.setData(geojson);
    } else {
      this.map.addSource(srcName, {
        type: "geojson",
        data: geojson,
      });

      this.map.addLayer({
        id: layerName,
        type: "line",
        source: srcName,
        layout: {
          "line-join": "round",
          "line-cap": "round",
        },
        paint: {
          "line-color": ["get", "color"],
          "line-width": 4,
        },
      });
    }
  };
}

export default DrawBearing;

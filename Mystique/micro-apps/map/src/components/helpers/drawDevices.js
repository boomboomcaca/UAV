// import { mapImageIds } from "./loadImages.js";
import mapboxGl from "mapbox-gl";

class drawDevices {
  /**
   * @type {mapboxGl.Map}
   */
  map;
  onSelectChange;
  layers = {
    selLayer: "dev_sel_layer",
    selSource: "dev_sel_source",
    deviceLayer: "dev_icon_layer",
    deviceSource: "dev_icon_source",
  };
  selectedDevice = "";
  constructor(map, onSelect) {
    this.map = map;
    this.onSelectChange = onSelect;

    const { deviceLayer } = this.layers;
    this.map.on("mouseenter", deviceLayer, this.#onMouseEnter);
    this.map.on("mouseleave", deviceLayer, this.#onMouseLeave);
    this.map.on("click", deviceLayer, this.#onMouseClickLayer);
    this.map.on("click", this.#onMouseClikBlink);
  }

  /**
   * type: monitoring|directionFinding|radioSuppressing|
   * @param {Array<{id:String,displayName:String,type:String,location:any,state:String}>} devices
   */
  draw = (devices) => {
    this.#deviceIcons(devices);
  };

  dispose = () => {
    const { deviceLayer } = this.layers;
    // 20230417 暂不处理选中
    // this.map.off("mouseenter", deviceLayer, this.#onMouseEnter);
    // this.map.off("mouseleave", deviceLayer, this.#onMouseLeave);
    // this.map.off("click", deviceLayer, this.#onMouseClickLayer);
    // this.map.off("click", this.#onMouseClikBlink);
  };

  #onMouseClikBlink = (e) => {
    if (this.map.getCanvas().style.cursor !== "pointer") {
      // if (this.onSelectChange) {
      //   this.onSelectChange();
      // }
    }
  };

  #onMouseClickLayer = (e) => {
    console.log("map device Layer clicked:::", e);
    // 处理选中
    const coordinates = e.features[0].geometry.coordinates.slice();
    const id = e.features[0].properties.id;

    if (this.onSelectChange) {
      this.onSelectChange(id);
    }
  };

  #onMouseEnter = () => {
    this.map.getCanvas().style.cursor = "pointer";
  };

  #onMouseLeave = () => {
    this.map.getCanvas().style.cursor = "";
  };

  #deviceIcons = (devices) => {
    // 绘制设备位置和状态
    const srcName = this.layers.deviceSource;
    const layerName = this.layers.deviceLayer;
    const geojson = {
      type: "FeatureCollection",
      features: [],
    };
    const features = devices.map((item) => {
      return {
        type: "Feature",
        geometry: {
          coordinates: item.location,
          type: "Point",
        },
        properties: {
          id: item.id,
          typeIcon: `${item.type}_${item.state ? "normal" : "fault"}`,
          select: "false",
          state: item.state,
          description: item.displayName,
        },
      };
    });
    console.log("device features:::", features);
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
        type: "symbol",
        source: srcName,
        layout: {
          "icon-image": ["get", "typeIcon"],
          "icon-size": 0.4, // ["interpolate", ["linear"], ["zoom"], 11, 0.4, 17, 0.8],
          // "icon-rotate": ["get", "rotate"],
          "icon-allow-overlap": true,
          "text-field": "test那个",

          // "text-allow-overlap": true,
          // "text-field": ["get", "description"],
          // "text-offset": [0, 2],
        },
        paint: {
          "text-color": "#FF0000",
        },
      });
    }
  };
}

export default drawDevices;

import mapboxGl from "mapbox-gl";
import { mapImageIds } from "./loadImages.js";
class drawPlanes {
  /**
   * @type {mapboxGl.Map}
   */
  map;
  visiblePlaneTrack = false;
  onSelectChange;
  /**
   * @type {Array<{id:String,type:String,coordinates:Arrary,description:String}>}
   */
  #prevDatas;

  #isOverview = false;

  selectedPlane;

  layers = {
    planeTrackLayer: "planegpslayer",
    planeIconLayer: "planeiconlayer",
    planeIconSource: "planeIconSource",
    pilotIconSource: "pilotIconSource",
    pilotLayer: "pilotLayer",
    linedashed: "line-dashed",
    selLayer: "plane_sel_layer",
    selSource: "plane_sel_source",
  };

  constructor(map, onSelect) {
    this.map = map;
    this.onSelectChange = onSelect;

    const { planeIconLayer } = this.layers;
    this.map.on("mouseenter", planeIconLayer, this.#onMouseEnter);
    this.map.on("mouseleave", planeIconLayer, this.#onMouseLeave);
    this.map.on("click", planeIconLayer, this.#onMouseClickLayer);
    this.map.on("click", this.#onMouseClikBlink);
  }

  /**
   *
   * @param {Array<{id:String,type:String,coordinates:Arrary,description:String}>} datas
   * @param {boolean} overView
   */
  draw = (datas, overView) => {
    // 1. 绘制轨迹
    // 2. 绘制图标
    // 3. 处理选中
    this.#prevDatas = datas;
    this.#isOverview = overView;
    if (overView) {
      this.#drawPlaneOverview(datas);
    } else {
      this.#drawPlaneTrack(datas);
      this.#drawPlaneSymbol(datas);
      this.#drawPilot(datas);
    }
  };

  selectOne = (id) => {
    this.selectedPlane = id;
    this.#drawPilot(this.#prevDatas);
    return this.#drawPlaneSymbol(this.#prevDatas);
  };

  dispose = () => {
    const { planeIconLayer } = this.layers;
    this.map.off("mouseenter", planeIconLayer, this.#onMouseEnter);
    this.map.off("mouseleave", planeIconLayer, this.#onMouseLeave);
    this.map.off("click", planeIconLayer, this.#onMouseClickLayer);
    this.map.off("click", this.#onMouseClikBlink);
  };

  #onMouseClikBlink = (e) => {
    if (this.map.getCanvas().style.cursor !== "pointer") {
      if (this.#prevDatas && this.selectedPlane) {
        this.selectedPlane = undefined;
        this.#drawPlaneSymbol(this.#prevDatas);
        this.#drawPilot(this.#prevDatas);
        if (this.onSelectChange) {
          this.onSelectChange();
        }
      }
    }
  };

  #onMouseClickLayer = (e) => {
    console.log("map plane Layer clicked:::", e);
    // 处理选中
    // const coordinates = e.features[0].geometry.coordinates.slice();
    if (!this.#isOverview) {
      const id = e.features[0].properties.id;
      this.selectedPlane = id;
      this.#drawPlaneSymbol(this.#prevDatas);
      this.#drawPilot(this.#prevDatas);
      if (this.onSelectChange) {
        this.onSelectChange(id);
      }
    }
  };

  #onMouseEnter = () => {
    this.map.getCanvas().style.cursor = "pointer";
  };

  #onMouseLeave = () => {
    this.map.getCanvas().style.cursor = "";
  };

  showPalneTrack = (visible) => {
    if (this.map.getLayer(this.layers.planeTrackLayer)) {
      this.map.setLayoutProperty(
        this.layers.planeTrackLayer,
        "visibility",
        visible ? "visible" : "none"
      );
      this.map.setLayoutProperty(
        this.layers.linedashed,
        "visibility",
        visible ? "visible" : "none"
      );
    }
    this.visiblePlaneTrack = visible;
  };

  #drawPlaneTrack = (datas) => {
    const srcName = "planegpstrack";
    const layerName = "planegpslayer";

    const geojson = {
      type: "FeatureCollection",
      features: [],
    };
    const features = datas.map((item) => {
      return {
        type: "Feature",
        properties: {},
        geometry: {
          coordinates: item.coordinates,
          type: "LineString",
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
        type: "line",
        source: srcName,
        id: layerName,
        paint: {
          "line-color": "#3ce5d3",
          "line-width": ["interpolate", ["linear"], ["zoom"], 11, 3, 17, 8],
          "line-opacity": 0.4,
        },
      });

      this.map.addLayer({
        type: "line",
        source: srcName,
        id: "line-dashed",
        paint: {
          "line-color": "#3ce5d3",
          "line-width": ["interpolate", ["linear"], ["zoom"], 11, 3, 17, 8],
          // "line-dasharray": [0, 4, 3],
        },
      });
      // this.tmrAnimatePlaneTrack = this.#animatePlaneTrack();
    }
  };

  #animatePlaneTrack = () => {
    const dashArraySequence = [
      [0, 4, 3],
      [0.5, 4, 2.5],
      [1, 4, 2],
      [1.5, 4, 1.5],
      [2, 4, 1],
      [2.5, 4, 0.5],
      [3, 4, 0],
      [0, 0.5, 3, 3.5],
      [0, 1, 3, 3],
      [0, 1.5, 3, 2.5],
      [0, 2, 3, 2],
      [0, 2.5, 3, 1.5],
      [0, 3, 3, 1],
      [0, 3.5, 3, 0.5],
    ];
    const that = this;
    return setInterval(() => {
      if (this.visiblePlaneTrack) {
        const timestamp = new Date().getTime();
        const newStep = parseInt((timestamp / 50) % dashArraySequence.length);
        that.map.setPaintProperty(
          "line-dashed",
          "line-dasharray",
          dashArraySequence[newStep]
        );
      }
    }, 100);
  };

  #drawPlaneSymbol = (datas) => {
    const srcName = this.layers.planeIconSource;
    const layerName = this.layers.planeIconLayer;
    const geojson = {
      type: "FeatureCollection",
      features: [],
    };
    let selPlaneGps;
    const features = datas.map((item) => {
      let latestCoordinates = item.coordinates[item.coordinates.length - 1];
      if (this.selectedPlane === item.id) {
        selPlaneGps = latestCoordinates;
        console.log("find seld :::", item);
      }
      return {
        type: "Feature",
        properties: {},
        geometry: {
          coordinates: latestCoordinates,
          type: "Point",
        },
        properties: {
          id: item.id,
          typeIcon:
            this.selectedPlane === item.id ? `${item.type}_sel` : item.type,
          // typeIcon: item.type,
          // select: this.selectedPlane === item.id ? "true" : "false",
          description: item.description,
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
        type: "symbol",
        source: srcName,
        layout: {
          "icon-image": [
            "match",
            ["get", "typeIcon"],
            "adsb",
            mapImageIds.adsbImageId,
            "adsb_sel",
            mapImageIds.adsbImageIdSel,
            "uav",
            mapImageIds.uavImageId,
            "uav_sel",
            mapImageIds.uavImageIdSel,
            "fighter",
            mapImageIds.fighterImageId,
            "fighter_sel",
            mapImageIds.fighterImageIdSel,
            "unknown_sel",
            mapImageIds.unknownImageIdSel,
            mapImageIds.unknownImageId,
          ],
          "icon-size": 0.4, // ["interpolate", ["linear"], ["zoom"], 11, 0.4, 17, 1],
          // "icon-rotate": ["get", "rotate"],
          "icon-allow-overlap": true,
          // "text-allow-overlap": true,
          // "text-field": ["get", "id"],
          // "text-offset": [0, 1.5],
        },
        paint: {
          "icon-opacity": 0.8,
        },
        // 单色图可用
        // paint: {
        //   "icon-color": [
        //     "match", // Use the 'match' expression: https://docs.mapbox.com/mapbox-gl-js/style-spec/#expressions-match
        //     ["get", "select"], // Use the result 'STORE_TYPE' property
        //     "true",
        //     "#3ce5d3",
        //     "#13227a", // any other store type
        //   ],
        // },
      });
    }

    return selPlaneGps;
  };

  #drawPlaneOverview = (datas) => {
    const srcName = this.layers.planeIconSource;
    const layerName = this.layers.planeIconLayer;
    const geojson = {
      type: "FeatureCollection",
      features: [],
    };
    const features = datas.map((item) => {
      return {
        type: "Feature",
        properties: {},
        geometry: {
          coordinates: item.coordinates[item.coordinates.length - 1],
          type: "Point",
        },
        properties: {
          typeIcon: item.type,
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
        type: "symbol",
        source: srcName,
        layout: {
          "icon-image": [
            "match",
            ["get", "typeIcon"],
            "uav",
            mapImageIds.uav_overview,
            "landing",
            mapImageIds.landing_overview,
            "pilot",
            mapImageIds.pilot_overview,
            mapImageIds.uav_overview,
          ],
          // "icon-size": ["interpolate", ["linear"], ["zoom"], 11, 0.35, 17, 1],
          "icon-allow-overlap": true,
        },
      });
    }
  };

  #drawPilot = (datas) => {
    const srcName = this.layers.pilotIconSource;
    const layerName = this.layers.pilotLayer;
    const geojson = {
      type: "FeatureCollection",
      features: [],
    };
    const features = datas.map((item) => {
      let pilotCoordinates = item.pilot;
      return {
        type: "Feature",
        properties: {},
        geometry: {
          coordinates: pilotCoordinates,
          type: "Point",
        },
        properties: {
          id: item.id,
          select: String(this.selectedPlane === item.id),
          // typeIcon: mapImageIds.pilot_image,
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
        type: "symbol",
        source: srcName,
        layout: {
          "icon-image": mapImageIds.pilot_image,
          // "icon-size": ["interpolate", ["linear"], ["zoom"], 11, 0.4, 17, 1],
          "icon-size": ["match", ["get", "select"], "true", 0.5, 0.35],
          // "icon-offset": [0, -40],
          "icon-allow-overlap": true,
        },
        paint: {
          "icon-opacity": ["match", ["get", "select"], "true", 1, 0.45],
        },
      });
    }
  };
}

export default drawPlanes;

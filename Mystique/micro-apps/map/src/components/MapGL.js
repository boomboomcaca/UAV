import mapboxGl from "mapbox-gl";
import "@mapbox/mapbox-gl-draw/dist/mapbox-gl-draw.css";
// import MyMapboxDraw from "./myDrawer/index.js";
import loadImages from "./helpers/loadImages.js";
import DrawRegions from "./helpers/drawRegions.js";
import DrawPlanes from "./helpers/drawPlanes.js";
import DrawDevices from "./helpers/drawDevices.js";
import MeasureTool from "./helpers/measureTool.js";
import drawWhiteRegions from "./helpers/drawWhiteRegions.js";
import DrawBearing from "./helpers/drawBearing.js";
import mapStyles, { getLayers } from "./styles";
import { Projections, eventTypes } from "./enums.js";

/**
 * @typedef {Object} MapGLOptions
 * @property {Array<Number>} center
 * @property {Number} level
 * @property {String} projection
 * @property {Array<{name:String,url:String,opacity:Number,tileSize:Number,brightnessmax:Number,contrast:Number}>} tileLayers
 * @property {string} container
 * @property {String} sourceUrl
 * @property {String} demDataUrl
 * @property {string} buildingsDataUrl
//  * @property {Function} onLoaded
//  * @property {Function} onDrawFeature
//  * @property {Function} onSelectFeature
 */

/**
 * @class
 */
class MapGL {
  /**
   * @type mapboxGl.Map
   */
  map;

  /**
   * @type {DrawRegions}
   */
  regionsDrawer;

  /**
   * @type {DrawPlanes}
   */
  planesDrawer;

  /**
   * @type {DrawDevices}
   */
  deviceDrawer;

  /**
   * @type {DrawBearing}
   */
  bearingDrawer;

  /**
   * @type {MeasureTool}
   */
  measureTool;

  mapCenter;

  /**
   * @type {MapGLOptions}
   */
  #options = {
    accessToken:
      "pk.eyJ1IjoiZ2lzb3IiLCJhIjoiY2xmdnkzdnJjMGJ0YzNkcXBsNmNsMTYyZiJ9.3mkw2ilDFTtQ2fx4uA3tjQ",
  };

  #terrainSourceId = "mapbox-dem";
  #buildingDataSourceId = "composite007";
  #add3dbuildings = "add3dbuildings";

  #controls = {
    legend: undefined,
    navigator: undefined,
    toolbar: undefined,
    // true 则只能存在一个feature，重新绘制则删除原有
    toolbarForEdit: false,
  };

  #prevTileLayer;
  #loaded = false;
  #regionOptions;
  #eventFunctions = {};

  mouseLocation = { lng: -1, lat: -1 };

  /**
   *
   * @param {MapGLOptions} props
   */
  constructor(props) {
    console.log("map props", props);
    this.#options = { ...this.#options, ...props };
    const mapStyle = mapStyles.raster(this.#options);
    this.#prevTileLayer = mapStyle;
    const initOptions = {
      style: mapStyle,
      preserveDrawingBuffer: true,
      width: 800,
      height: 600,
      zoom: 11.5,

      maxZoom: 21,
      minzoom: 4,
      localFontFamily: "sourceHan",
      // localIdeographFontFamily: "Arial",
    };
    const mapOptions = { ...initOptions, ...this.#options };
    this.map = new mapboxGl.Map(mapOptions);

    this.map.on("load", (e) => {
      // const htmlEle = document.querySelector(".mapboxgl-ctrl-logo");
      // if (htmlEle) htmlEle.parentElement.style.display = "none";
      loadImages(this.map);
      this.regionsDrawer = new DrawRegions(this.map);
      this.planesDrawer = new DrawPlanes(this.map, (e) => {
        const onSelectFeature =
          this.#eventFunctions[eventTypes.onSelectFeature];
        if (onSelectFeature)
          onSelectFeature({
            type: "plane",
            e,
          });
      });
      this.deviceDrawer = new DrawDevices(this.map, (e) => {
        const onSelectFeature =
          this.#eventFunctions[eventTypes.onSelectFeature];
        if (onSelectFeature)
          onSelectFeature({
            type: "device",
            e,
          });
      });
      this.measureTool = new MeasureTool(this.map, (e) => {
        const onDrawFeature = this.#eventFunctions[eventTypes.onDrawFeature];
        if (onDrawFeature) {
          onDrawFeature(e);
        }
      });
      this.bearingDrawer = new DrawBearing(this.map);
      this.map.on("click", (e) => {
        this.mouseLocation = e.lngLat;
      });
      const onLoaded = this.#eventFunctions[eventTypes.onLoaded];
      if (onLoaded) {
        onLoaded(this);
        this.#loaded = true;
      }

      // ######### 卫星地图+dem渲染三维，搞定
      /*      
      this.map.addSource("mapbox-dem", {
        type: "raster-dem",
        tiles: [
          // "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{z}/{x}/{y}.png",
          "https://api.mapbox.com/v4/mapbox.terrain-rgb/{z}/{x}/{y}.pngraw?access_token=pk.eyJ1IjoiZ2lzb3IiLCJhIjoiY2xmdnkzdnJjMGJ0YzNkcXBsNmNsMTYyZiJ9.3mkw2ilDFTtQ2fx4uA3tjQ",
        ],
        tileSize: 512,
        maxzoom: 15,
      });
      // add the DEM source as a terrain layer with exaggerated height
      this.map.setTerrain({ source: "mapbox-dem", exaggeration: 2.0 });
      */
      // ********* 卫星地图+dem渲染三维，搞定

      // ######### 3D 建筑物，只能使用mapbox数据？数据不全？
      /*
      this.map.addSource("composite", {
        type: "vector",
        tiles: [
          "https://api.mapbox.com/v4/mapbox.mapbox-streets-v8,mapbox.mapbox-terrain-v2,mapbox.mapbox-bathymetry-v2/{z}/{x}/{y}.vector.pbf?sku=101Ok9jcm6ljW&access_token=pk.eyJ1IjoiZ2lzb3IiLCJhIjoiY2xmdnkzdnJjMGJ0YzNkcXBsNmNsMTYyZiJ9.3mkw2ilDFTtQ2fx4uA3tjQ",
        ],
      });
      this.map.addLayer({
        id: "add-3d-buildings",
        source: "composite",
        "source-layer": "building",
        filter: ["==", "extrude", "true"],
        type: "fill-extrusion",
        minzoom: 15,
        paint: {
          "fill-extrusion-color": "#aaa",

          // Use an 'interpolate' expression to
          // add a smooth transition effect to
          // the buildings as the user zooms in.
          "fill-extrusion-height": [
            "interpolate",
            ["linear"],
            ["zoom"],
            15,
            0,
            15.05,
            ["get", "height"],
          ],
          "fill-extrusion-base": [
            "interpolate",
            ["linear"],
            ["zoom"],
            15,
            0,
            15.05,
            ["get", "min_height"],
          ],
          "fill-extrusion-opacity": 0.6,
        },
      });
*/
      // ******** 3D 建筑物，只能使用mapbox数据？数据不全？
    });

    this.map.on("remove", () => {
      if (this.deviceDrawer) {
        this.deviceDrawer.dispose();
        this.planesDrawer.dispose();
        this.measureTool.dispose();
      }
    });
    this.map.on("zoomend", () => {
      const zoomLevel = this.map.getZoom();
      console.log("zoom level:::", zoomLevel);
      if (zoomLevel < 4) this.map.zoomTo(4);
      if (zoomLevel > 20) this.map.zoomTo(19.9);
    });
  }

  on = (name, callback) => {
    this.#eventFunctions[name] = callback;
  };

  setLayerVisible = (id, visible) => {
    this.map.setLayoutProperty(
      `raster-layer-${id}`,
      "visibility",
      visible ? "visible" : "none"
    );
  };

  enable3dSatelite = (enable) => {
    if (enable) {
      const layer = this.map.getLayer(this.#add3dbuildings);
      if (layer) {
        // this.map.removeSource(this.#buildingDataSourceId);
        // this.map.removeLayer(this.#add3dbuildings);
        this.map.setLayoutProperty(this.#add3dbuildings, "visibility", "none");
      }
      // ######### 卫星地图+dem渲染三维，搞定
      const source = this.map.getSource(this.#terrainSourceId);
      if (!source) {
        this.map.addSource(this.#terrainSourceId, {
          type: "raster-dem",
          tiles: [
            // "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{z}/{x}/{y}.png",
            this.#options.demDataUrl,
            // "https://api.mapbox.com/v4/mapbox.terrain-rgb/{z}/{x}/{y}.pngraw?access_token=pk.eyJ1IjoiZ2lzb3IiLCJhIjoiY2xmdnkzdnJjMGJ0YzNkcXBsNmNsMTYyZiJ9.3mkw2ilDFTtQ2fx4uA3tjQ",
          ],
          tileSize: 512,
          maxzoom: 15,
        });
      }
      // add the DEM source as a terrain layer with exaggerated height
      this.map.setTerrain({ source: "mapbox-dem", exaggeration: 1.5 });
    } else {
      this.map.setTerrain(null);
    }
    // ********* 卫星地图+dem渲染三维，搞定
  };

  enableBuilding3D = (enable) => {
    if (enable) {
      const src = this.map.getSource(this.#terrainSourceId);
      if (src) {
        // this.map.removeSource(this.#terrainSourceId);
        this.map.setTerrain(null);
      }
      // ######### 3D 建筑物，只能使用mapbox数据？数据不全？
      const source = this.map.getSource(this.#buildingDataSourceId);
      if (!source) {
        this.map.addSource(this.#buildingDataSourceId, {
          type: "vector",
          tiles: [this.#options.buildingsDataUrl],
        });
        this.map.addLayer({
          id: this.#add3dbuildings,
          source: this.#buildingDataSourceId,
          "source-layer": "building",
          filter: ["==", "extrude", "true"],
          type: "fill-extrusion",
          maxzoom: 15,
          paint: {
            "fill-extrusion-color": "#aaa",

            // Use an 'interpolate' expression to
            // add a smooth transition effect to
            // the buildings as the user zooms in.
            "fill-extrusion-height": [
              "interpolate",
              ["linear"],
              ["zoom"],
              15,
              0,
              15.05,
              ["get", "height"],
            ],
            "fill-extrusion-base": [
              "interpolate",
              ["linear"],
              ["zoom"],
              15,
              0,
              15.05,
              ["get", "min_height"],
            ],
            "fill-extrusion-opacity": 0.6,
          },
        });
      } else {
        this.map.setLayoutProperty(
          this.#add3dbuildings,
          "visibility",
          "visible"
        );
      }
    } else {
      const source = this.map.getLayer(this.#add3dbuildings);
      if (source)
        this.map.setLayoutProperty(this.#add3dbuildings, "visibility", "none");
    }
    // ******** 3D 建筑物，只能使用mapbox数据？数据不全？
  };

  setCenter = (center) => {
    if (center) {
      this.map.setCenter(center);
      this.mapCenter = center;
    } else if (this.mapCenter) {
      this.map.setCenter(this.mapCenter);
    }
  };

  resize = () => {
    this.map.resize();
  };

  setZoom = (zoom) => {
    this.map.zoomTo(zoom);
  };

  resetNorth = () => {
    this.map.resetNorth();
  };

  showLegend = (visible, position = "bottom-right") => {
    if (visible && !this.#controls.legend) {
      const scale = new mapboxGl.ScaleControl({
        maxWidth: 80,
        unit: "metric",
      });

      this.map.addControl(scale, position);
      this.#controls.legend = scale;
    }
    if (!visible && this.#controls.legend) {
      this.map.removeControl(this.#controls.legend);
      this.#controls.legend = undefined;
    }
  };

  // 'top-right' | 'top-left' | 'bottom-right' | 'bottom-left'
  showNavigator = (visible, position = "bottom-left") => {
    if (visible && !this.#controls.navigator) {
      const navi = new mapboxGl.NavigationControl({
        showCompass: true,
        showZoom: true,
        visualizePitch: false,
      });
      this.map.addControl(navi, position);
      this.#controls.navigator = navi;
    }
    if (!visible && this.#controls.navigator) {
      this.map.removeControl(this.#controls.navigator);
      this.#controls.navigator = undefined;
    }
  };

  showToolbar = (items, editMode) => {
    if (this.measureTool) {
      this.measureTool.showToolbar(items, editMode);
    }
  };

  /**}
   *
   * {
        "id": "b4cae7f209263fc84d94624517b6ba0f",
        "type": "Feature",
        "properties": {},
        "geometry": {
            "coordinates": [
                104.12005584300692,
                30.623730799653956
            ],
            "type": "Point"
        }
    }
   * @param {{region0:any,region2:any,region3:any,region5:any}} regions
   */
  setRegions = (regions) => {
    this.regionsDrawer.setRegions(regions);
    if (this.#regionOptions) this.drawRegions(this.#regionOptions);
  };

  /**
   *
   * @param {{region0:Boolean,region2:Boolean,region3:Boolean,region5:Boolean}} options
   */
  drawRegions = (options) => {
    this.#regionOptions = options;
    this.regionsDrawer.draw(options);
  };

  drawWhiteRegions = (regions, visible) => {
    if (this.map) drawWhiteRegions(this.map, regions, "#1cffff", visible); //"#1cffff"
  };

  /**
   *
   * @param {Array<{id:String,type:String,coordinates:Arrary,description:String}>} datas
   * @param {boolean} overView
   */
  drawPlanes = (datas, overView) => {
    this.planesDrawer.draw(datas, overView);
  };

  showPalneTrack = (visible) => {
    this.planesDrawer.visiblePlaneTrack = visible;
    this.planesDrawer.showPalneTrack(visible);
  };

  setPlaneSelect = (id) => {
    const selCenter = this.planesDrawer.selectOne(id);
    if (selCenter) {
      this.map.setCenter(selCenter);
    }
  };

  /**
   *
   * @param {Array<{id:String,displayName:String,type:String,location:any,state:String}>} devices
   */
  drawDevices = (devices) => {
    this.deviceDrawer.draw(devices);
  };

  /**
   *
   * @param {Array<{coordinates:Array<Number>,color:String}>} bearings
   */
  drawBearing = (bearings) => {
    this.bearingDrawer.draw(bearings);
  };

  reloadMap = (tileLayers) => {
    if (this.#loaded) {
      this.enableBuilding3D(false);
      this.enable3dSatelite(false);
      this.#options.tileLayers = tileLayers;
      // 1. 移除老的图层和源
      this.#prevTileLayer.layers.forEach((l) => {
        this.map.removeLayer(l.id);
        this.map.removeSource(l.source);
      });
      // 获取最底下的图层
      let prevTileLayerId = undefined;
      const layersOnMap = this.map.getStyle().layers;
      if (layersOnMap.length > 0) {
        prevTileLayerId = layersOnMap[0].id;
      }
      const layers = getLayers(this.#options);
      layers.layers.forEach((l) => {
        this.map.addSource(l.source, layers.sources[l.source]);
        this.map.addLayer(l, prevTileLayerId);
      });

      this.#prevTileLayer = layers;
    }
    // this.map.moveLayer();
  };

  getImage = () => {
    return this.map.getCanvas().toDataURL();
  };

  dispose = () => {
    if (this.deviceDrawer) {
      this.deviceDrawer.dispose();
      this.planesDrawer.dispose();
      this.measureTool.dispose();
    }
  };
}

export default MapGL;

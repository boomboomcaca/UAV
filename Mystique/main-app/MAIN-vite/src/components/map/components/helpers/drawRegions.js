import mapboxgl from "mapbox-gl";
import RegionType from "../enums.js";

class drawRegions {
  regions;
  /**
   * @type {mapboxgl.Map}
   */
  map;
  #regionOptions;

  constructor(map) {
    this.map = map;
  }

  /**
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
    this.regions = regions;
    if (this.#regionOptions) {
      this.draw(this.#regionOptions);
    }
  };

  /**
   *
   * @param {{region0:Boolean,region2:Boolean,region3:Boolean,region5:Boolean}} options
   */
  draw = (options) => {
    const { region0, region2, region3, region5 } = options;

    // let show3D = false;
    // const terrain = this.map.getTerrain();
    // console.log("terrain:::", terrain);
    // if (!terrain) {
    //   const layer = this.map.getLayer("add3dbuildings");
    //   if (layer) {
    //     const building3D = this.map.getLayoutProperty(
    //       "add3dbuildings",
    //       "visibility"
    //     );
    //     console.log("building3D:::", building3D);
    //     show3D = building3D !== "none";
    //   }
    // } else {
    //   show3D = true;
    // }

    // 绘制区域
    this.#drawProtectedRegion(RegionType.region5, "#3CE5D3", region5);
    this.#drawProtectedRegion(RegionType.region3, "#00D462", region3);
    this.#drawProtectedRegion(RegionType.region2, "#FBC60D", region2);
    // this.#draw3DRegion(RegionType.region2, "#FBC60D", show3D);

    this.#drawProtectedRegion(RegionType.region0, "#FF7325", region0);
    this.#regionOptions = options;
  };

  #drawProtectedRegion = (srcName, color, visible, opacity = 0.2) => {
    if (!this.regions) return;
    const region = this.regions[srcName];
    if (!region) return;
    const { outRadius, feature } = region;
    let radiusLen = 8;
    if (srcName !== RegionType.region0) {
      const latitude = feature.geometry.coordinates[1];
      radiusLen = outRadius / 0.075 / Math.cos((latitude * Math.PI) / 180);
    }
    if (feature) {
      const layerName = `${srcName}_layer`;
      const layer = this.map.getLayer(layerName);
      if (layer) {
        this.map.setLayoutProperty(
          layerName,
          "visibility",
          visible ? "visible" : "none"
        );
        this.map.setLayoutProperty(
          `outline_${layerName}`,
          "visibility",
          visible ? "visible" : "none"
        );
        // TODO 更新数据
        const src = this.map.getSource(srcName);
        src.setData(feature);
      } else if (visible) {
        this.map.addSource(srcName, {
          type: "geojson",
          data: feature,
        });
        const isPolygon = feature.geometry.type === "Polygon";
        if (isPolygon) {
          // 1. 填充
          // 2. 边界
          this.map.addLayer({
            id: layerName,
            type: "fill",
            source: srcName, // reference the data source
            layout: {},
            paint: {
              "fill-color": color,
              "fill-opacity": opacity,
            },
          });
          this.map.addLayer({
            id: `outline_${layerName}`,
            type: "line",
            source: srcName,
            layout: {},
            paint: {
              "line-color": color,
              "line-opacity": 0.6,
              "line-width": 2,
            },
          });
        } else {
          this.map.addLayer({
            id: layerName,
            type: "circle",
            source: srcName, // reference the data source
            layout: {},
            paint: {
              "circle-radius":
                srcName === RegionType.region0
                  ? radiusLen
                  : {
                      stops: [
                        [0, 0],
                        [20, radiusLen],
                      ],
                      base: 2,
                    },
              "circle-color": color,
              "circle-opacity": opacity,
              "circle-stroke-opacity": 0.5,
              "circle-stroke-color": color,
              "circle-stroke-width": 2,
            },
          });
        }
      }
    }
  };

  #draw3DRegion = (srcName, color, visible, opacity = 0.2) => {
    if (!this.regions) return;
    const region = this.regions[srcName];
    if (!region) return;
    const { outRadius, feature } = region;
    let radiusLen = 8;
    console.log("draw 3d feature:::", feature);
    if (srcName !== RegionType.region0) {
      const latitude = feature.geometry.coordinates[1];
      radiusLen = outRadius / 0.075 / Math.cos((latitude * Math.PI) / 180);
    }
    if (feature) {
      const layerName = `${srcName}_layer3d`;
      const layer = this.map.getLayer(layerName);
      if (layer) {
        this.map.setLayoutProperty(
          layerName,
          "visibility",
          visible ? "visible" : "none"
        );
        // this.map.setLayoutProperty(
        //   `outline_${layerName}`,
        //   "visibility",
        //   visible ? "visible" : "none"
        // );
        // TODO 更新数据
        // const src = this.map.getSource(srcName);
        // src.setData(feature);
      } else if (visible) {
        const isPolygon = feature.geometry.type === "Polygon";
        if (isPolygon) {
          // 1. 填充
          // 2. 边界
          this.map.addLayer({
            id: layerName,
            type: "fill-extrusion",
            source: srcName,
            paint: {
              // Get the `fill-extrusion-color` from the source `color` property.
              "fill-extrusion-color": color,
              // Get `fill-extrusion-height` from the source `height` property.
              "fill-extrusion-height": 100,
              // Get `fill-extrusion-base` from the source `base_height` property.
              "fill-extrusion-base": 500,
              // Make extrusions slightly opaque to see through indoor walls.
              "fill-extrusion-opacity": 0.31,
            },
          });
        } else {
        }
      }
    }
  };
}

export default drawRegions;

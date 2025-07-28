/**
 *
 * @param {Array<any>} regions
 * @param {*} color
 * @param {*} visible
 */
const drawWhiteRegions = (map, regions, color, visible) => {
  const jsonData = { type: "FeatureCollection", features: regions };
  const srcName = "white_region_src";
  const layerName = "white_region_layer";
  const layer = map.getLayer(layerName);
  if (layer) {
    map.setLayoutProperty(
      layerName,
      "visibility",
      visible ? "visible" : "none"
    );
    map.setLayoutProperty(
      `outline_${layerName}`,
      "visibility",
      visible ? "visible" : "none"
    );
    // TODO 更新数据
    const src = map.getSource(srcName);
    src.setData(jsonData);
  } else if (visible) {
    map.addSource(srcName, {
      type: "geojson",
      data: jsonData,
    });
    // 1. 填充
    // 2. 边界
    map.addLayer({
      id: layerName,
      type: "fill",
      source: srcName, // reference the data source
      layout: {},
      paint: {
        "fill-color": color,
        "fill-opacity": 0.3,
      },
    });
    map.addLayer({
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
  }
};

export default drawWhiteRegions;

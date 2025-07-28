/**
 *
 * @param {{tileLayers:Array<{name:String,url:String,opacity:Number,tileSize:Number}>,sourceUrl:String}} options
 * @returns
 */
const offline_raster = (options) => {
  const { sourceUrl } = options;
  const layerStyle = getLayers(options);

  const style = {
    version: 8,
    sprite: sourceUrl
      ? `${sourceUrl}/sprite`
      : "mapbox://sprites/mapbox/bright-v8",
    glyphs: sourceUrl
      ? `${sourceUrl}/fonts/{fontstack}/{range}.pbf`
      : "mapbox://fonts/mapbox/{fontstack}/{range}.pbf",
    // sprite: "mapbox://sprites/mapbox/bright-v8",
    // glyphs: "mapbox://fonts/mapbox/{fontstack}/{range}.pbf",
    sources: layerStyle.sources,
    layers: layerStyle.layers,
  };

  return style;
};

/**
 *
 * @param {{tileLayers:Array<{name:String,url:String,opacity:Number,tileSize:Number}>,sourceUrl:String}} options
 * @returns {{sources:Object, layers:Array<{id:String,source:String}>}}
 */
const getLayers = (options) => {
  const { tileLayers } = options;
  const sources = {};
  console.log(tileLayers);
  const layers = tileLayers.map((layer) => {
    const srcName = `raster-tiles-${layer.name}`;
    sources[srcName] = {
      type: "raster",
      tiles: [layer.url],
      tileSize: layer.tileSize || 256,
    };
    const layerId = `raster-layer-${layer.name}`;
    console.log("layer name:::", layerId);
    return {
      id: layerId,
      type: "raster",
      source: srcName,
      minzoom: 4,
      maxzoom: 20,
      paint: {
        // 透明度
        "raster-opacity": layer.opacity || 1,
        // 亮度
        "raster-brightness-max": layer.brightnessmax || 1,
        // "raster-brightness-min": 1,
        // 对比度
        "raster-contrast": layer.contrast !== undefined ? layer.contrast : 0,
        // "raster-contrast": -1,
        // 色相？
        "raster-hue-rotate": layer.rotate !== undefined ? layer.rotate : 0,
        // 饱和度
        "raster-saturation":
          layer.saturation !== undefined ? layer.saturation : 0,
      },
    };
  });
  return { sources, layers };
};

export default offline_raster;
export { getLayers };

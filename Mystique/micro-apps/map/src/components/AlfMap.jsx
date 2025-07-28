import React, { useEffect, useState, useRef, memo, useCallback } from "react";
import PropTypes from "prop-types";
import MapGl from "./MapGL.js";
import styles from "./style.module.less";
// import "./main.css";
import "./mapbox-gl.css";
import { Projections, eventTypes } from "./enums.js";

// /**
//  * @typedef {Object} MapProps
//  * @property {Array<Number>} center
//  * @property {Number} level
//  * @property {String} projection
//  * @property {Array<{name:String,url:String,opacity:Number,tileSize:Number}>} tileLayers
//  * @property {Function} onLoaded
//  * @property {Function} onDrawFeature
//  * @property {Function} onSelectFeature
//  */

const AlfMap = memo(
  (props) => {
    const {
      tileLayers,
      center,
      level,
      projection,
      sourceUrl,
      buildingsDataUrl,
      demDataUrl,
      onLoaded,
      onDrawFeature,
      onSelectFeature,
    } = props;
    const mapInstanceRef = useRef();
    const mapConIdRef = useRef(`mapcon${(Math.random() * 1e16).toFixed(0)}`);

    const mapLoadedCallback = useCallback(
      (e) => {
        if (onLoaded) {
          onLoaded(e);
        }
      },
      [onLoaded]
    );

    const drawFeatureCallback = useCallback(
      (e) => {
        if (onDrawFeature) {
          onDrawFeature(e);
        }
      },
      [onDrawFeature]
    );

    const selectFeatureCallback = useCallback(
      (e) => {
        if (onSelectFeature) {
          onSelectFeature(e);
        }
      },
      [onSelectFeature]
    );

    useEffect(() => {
      console.log("map option changed:::");
      if (tileLayers) {
        if (mapInstanceRef.current) return;
        const map = new MapGl({
          tileLayers,
          center,
          level,
          projection,
          sourceUrl,
          container: mapConIdRef.current,
          buildingsDataUrl,
          demDataUrl,
        });
        map.on(eventTypes.onLoaded, mapLoadedCallback);
        map.on(eventTypes.onDrawFeature, drawFeatureCallback);
        map.on(eventTypes.onSelectFeature, selectFeatureCallback);
        mapInstanceRef.current = map;
      }
      return () => {
        if (mapInstanceRef.current) {
          console.log("dispose:::::::::");
          // mapInstanceRef.current.dispose();
          // mapInstanceRef.current = null;
        }
      };
    }, [
      tileLayers,
      center,
      level,
      projection,
      sourceUrl,
      buildingsDataUrl,
      demDataUrl,
    ]);

    useEffect(() => {
      if (tileLayers && tileLayers.length > 0 && mapInstanceRef.current) {
        mapInstanceRef.current.reloadMap(tileLayers);
      }
    }, [tileLayers]);

    return (
      <div
        style={{
          letf: 0,
          top: 0,
          width: "100%",
          height: "100%",
          position: "absolute",
          borderRadius: "inherit",
        }}
      >
        <div
          style={{
            height: "100%",
            position: "relative",
            borderRadius: "inherit",
          }}
        >
          <div
            id={mapConIdRef.current}
            style={{
              position: "absolute",
              top: 0,
              bottom: 0,
              left: 0,
              right: 0,
            }}
          />

          <div
            style={{
              position: "absolute",
              top: "10px",
              left: "10px",
              backgroundColor: "ButtonFace",
            }}
          ></div>
        </div>
      </div>
    );
  },
  (prev, next) => {
    // TODO
    return false;
  }
);

AlfMap.defaultProps = {
  center: [104, 30.6],
  level: 11.5,
  minZoom: 4,
  maxZoom: 20,
  project: Projections.gcj02,
  tileLayers: [
    {
      name: "amapst",
      url: "http://webst04.is.autonavi.com/appmaptile?style=6&x={x}&y={y}&z={z}",
      // brightnessmax: 0.7,
      contrast: -0.3,
    },
    {
      name: "amaprd",
      url: "http://webst02.is.autonavi.com/appmaptile?x={x}&y={y}&z={z}&lang=zh_cn&size=2&scale=1&style=8",
      opacity: 0.7,
      brightnessmax: 0.8,
      // contrast: -0.4,
    },
  ],
  sourceUrl: undefined,
  onLoaded: () => {},
  onDrawFeature: () => {},
  onSelectFeature: () => {},
};

AlfMap.propTypes = {
  center: PropTypes.array,
  level: PropTypes.number,
  minZoom: PropTypes.number,
  maxZoom: PropTypes.number,
  project: PropTypes.string,
  tileLayers: PropTypes.array,
  sourceUrl: PropTypes.string,
  onLoaded: () => {},
  onDrawFeature: () => {},
  onSelectFeature: () => {},
};

export default AlfMap;

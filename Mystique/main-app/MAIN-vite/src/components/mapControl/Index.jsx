import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
// import AlfMap, { RegionType } from "map-alf";
import { Checkbox, message } from "dui";

import AlfMap, { RegionType } from "../map/components/index";
import { ReactComponent as MoreIcon } from "../../assets/icons/map_more.svg";
import TypeItem from "./TypeItem/Index.jsx";
import getConf from "../../config";
import styles from "./style.module.less";

const MapControl = (props) => {
  const {
    style,
    regionBar,
    legend,
    zoom,
    zoom2Fit,
    navigator,
    region0,
    region2,
    region3,
    region5,
    devices,
    onLoaded,
    onSelectFeature,
    onDrawFeature,
    resize,
    overview,
    // 编辑的时候用
    tempRegionConfig,
    morscr,
  } = props;

  const [appConfig] = useState(getConf());
  const visibleConfRef = useRef();
  const [mouseGPS, setMouseGPS] = useState();
  const [map, setMap] = useState();
  const mapRef = useRef();
  // const [showRegion0, setShowRegion0] = useState();
  // const [showRegion2, setShowRegion2] = useState();
  // const [showRegion3, setShowRegion3] = useState();
  // const [showRegion5, setShowRegion5] = useState();
  const [regionConfig, setRegionConfig] = useState();
  const [showRegions1, setShowRegions] = useState([
    RegionType.region0,
    RegionType.region2,
    RegionType.region3,
    RegionType.region5,
  ]);

  const [showMore, setShowMore] = useState(false);
  const [mapType, setMapType] = useState("satelite");

  useEffect(() => {
    const regConfig = localStorage.getItem("regionconfig");
    if (regConfig) {
      const regionConfig = JSON.parse(regConfig);
      setRegionConfig(regionConfig);
    }
    const regionVisible = localStorage.getItem("region-visible");
    if (regionVisible) {
      // 加载上一次用户设置
      visibleConfRef.current = JSON.parse(regionVisible);
    }
  }, []);

  useEffect(() => {
    if (tempRegionConfig) setRegionConfig(tempRegionConfig);
  }, [tempRegionConfig]);

  useEffect(() => {
    if (visibleConfRef.current) {
      setShowRegions([...visibleConfRef.current]);
      // 只在第一次加载时使用
      visibleConfRef.current = null;
    } else {
      setShowRegions([
        region0 ? RegionType.region0 : "",
        region2 ? RegionType.region2 : "",
        region3 ? RegionType.region3 : "",
        region5 ? RegionType.region5 : "",
      ]);
    }
  }, [region0, region2, region3, region5]);

  // ???
  useEffect(() => {
    if (map && regionConfig) {
      console.log("change zoom level", regionConfig.center);

      map.resize();
      setTimeout(() => {
        map.setCenter(regionConfig.center);
      }, 400);
    }
  }, [map, regionConfig, regionBar, resize]);

  useEffect(() => {
    if (map && regionConfig && zoom2Fit) {
      setTimeout(() => {
        map.setZoom(zoom);
        setTimeout(() => {
          map.resetNorth();
        }, 500);
      }, 500);
    }
  }, [map, regionConfig, zoom, zoom2Fit]);

  // 图例 导航条 regionBar
  useEffect(() => {
    if (map) {
      map.showLegend(legend);
      map.showNavigator(navigator);
    }
  }, [map, legend, navigator]);

  // 设备
  useEffect(() => {
    if (map && devices) {
      console.log("draw devices::::", devices);
      // const faultSates  =["none",
      // "idle": "空闲",
      // "busy": "忙碌",
      // "deviceBusy": "设备占用",
      // "offline": "离线",
      // "fault": "故障",
      // "disabled": "禁用"
      // ]
      devices.forEach((item) => {
        if (item.location !== undefined) {
          const clone = { ...item };
          clone.state =
            clone.moduleState === "idle" || clone.moduleState === "running";
        }
      });

      // 处理一下层级问题
      setTimeout(() => {
        map.drawDevices(devices.filter((d) => d.location !== undefined));
      }, 520);
    }
  }, [map, devices]);

  useEffect(() => {
    if (map && regionConfig) {
      map.setRegions(regionConfig);
      map.resize();
      setTimeout(() => {
        map.setCenter(regionConfig.center);
      }, 500);
    }
  }, [map, regionConfig]);

  useEffect(() => {
    if (map && regionConfig) {
      map.drawRegions({
        region0: showRegions1.includes(RegionType.region0),
        region2: showRegions1.includes(RegionType.region2),
        region3: showRegions1.includes(RegionType.region3),
        region5: showRegions1.includes(RegionType.region5),
      });
    }
  }, [map, regionConfig, showRegions1]);

  //   useEffect(() => {
  //     console.log("map options changed:::::");
  //   }, [onSelectFeature, onDrawFeature]);

  const initMapLayers = (type, overview, morscr) => {
    const { webMapUrl } = appConfig;
    const stLayer = {
      name: "tdtst",
      url: `${webMapUrl}/tile?x={x}&y={y}&z={z}&ms=tianditu&mt=normal2d`,
      // google
      saturation: -0.3,
      opacity: overview ? 0.2 : 0.9,
    };
    const roadLayer = {
      name: "tdtroad",
      url: `${webMapUrl}/tile?x={x}&y={y}&z={z}&ms=tianditu&mt=roades`,
      opacity: overview ? 0.2 : 0.7,
      brightnessmax: 0.9,
    };
    if (type !== "normal2d") {
      stLayer.url = `${webMapUrl}/tile?x={x}&y={y}&z={z}&ms=tianditu&mt=statelite`;
      stLayer.saturation = 0.3;
    }
    return morscr ? [roadLayer] : [stLayer, roadLayer];
  };

  const [mapLayers, setMapLayers] = useState(
    initMapLayers(mapType, overview, morscr)
  );

  useEffect(() => {
    setMapLayers(initMapLayers(mapType, overview, morscr));
  }, [mapType]);

  return (
    <div
      className={styles.mapcon}
      style={style}
      onClick={() => {
        setMouseGPS(mapRef.current.mouseLocation);
      }}
    >
      <AlfMap
        sourceUrl={`${window.location.origin}/mapboxsource`}
        buildingsDataUrl={`${appConfig.webMapUrl}/tile?x={x}&y={y}&z={z}&ms=mapbox&mt=buildings`}
        demDataUrl={`${appConfig.webMapUrl}/tile?x={x}&y={y}&z={z}&ms=mapbox&mt=dem`}
        // "http://127.0.0.1:8182/public"
        // tileLayers={mapLayersRef.current}
        tileLayers={mapLayers}
        onLoaded={(map) => {
          mapRef.current = map;
          setMap(map);
          onLoaded(map);
        }}
        level={zoom}
        onSelectFeature={onSelectFeature}
        onDrawFeature={onDrawFeature}
      />
      {regionBar && (
        <div
          className={styles.more}
          onMouseOver={() => setShowMore(true)}
          onMouseLeave={() => setShowMore(false)}
        >
          <div
            className={styles.opbtn}
            onClick={() => {
              setShowMore(!showMore);
            }}
          >
            <MoreIcon className={styles.icon} />
          </div>
          {/* {showMore && ( */}
          <div
            className={`${styles.options} ${
              showMore ? styles.show : styles.hide
            }`}
          >
            <div className={styles.item}>
              {/* <span>防护区</span> */}
              <Checkbox.Group
                options={[
                  { label: "保护区（点）", value: RegionType.region0 },
                  { label: "识别处置区", value: RegionType.region2 },
                  { label: "警戒区", value: RegionType.region3 },
                  { label: "预警区", value: RegionType.region5 },
                ]}
                value={showRegions1}
                sort={false}
                onChange={(e) => {
                  setShowRegions(e);
                  // 存储用户设置
                  localStorage.setItem("region-visible", JSON.stringify(e));
                }}
              />
            </div>
            <div className={styles.item}>
              <TypeItem
                name="常规"
                selected={mapType === "normal2d"}
                onSelect={() => {
                  setMapType("normal2d");
                }}
                onChange={(e) => {
                  console.log(e);
                  if (map) {
                    map.enableBuilding3D(e.enable3d);
                    map.drawRegions({
                      region0: showRegions1.includes(RegionType.region0),
                      region2: showRegions1.includes(RegionType.region2),
                      region3: showRegions1.includes(RegionType.region3),
                      region5: showRegions1.includes(RegionType.region5),
                    });
                  }
                }}
              />
              <TypeItem
                name="卫星"
                allowRoad
                selected={mapType === "satelite"}
                onSelect={() => {
                  setMapType("satelite");
                }}
                onChange={(e) => {
                  if (map) {
                    map.setLayerVisible("tdtroad", e.showRoad);
                    map.enable3dSatelite(e.enable3d);
                    setTimeout(() => {
                      map.drawRegions({
                        region0: showRegions1.includes(RegionType.region0),
                        region2: showRegions1.includes(RegionType.region2),
                        region3: showRegions1.includes(RegionType.region3),
                        region5: showRegions1.includes(RegionType.region5),
                      });
                    }, 5000);
                  }
                }}
              />
            </div>
          </div>
          {/* )} */}
        </div>
      )}

      {mouseGPS && (
        <div className={styles.mouseGPS}>{`${Number(mouseGPS.lng).toFixed(
          6
        )},${Number(mouseGPS.lat).toFixed(6)}`}</div>
      )}
    </div>
  );
};

MapControl.defaultProps = {
  style: undefined,
  regionBar: false,
  legend: false,
  zoom: 11.5,
  zoom2Fit: undefined,
  navigator: false,
  region0: true,
  region2: true,
  region3: true,
  region5: true,
  devices: undefined,
  resize: undefined,
  onLoaded: () => {},
  onSelectFeature: () => {},
  onDrawFeature: () => {},
  overview: false,
  morscr: false,
};

MapControl.prototype = {
  style: PropTypes.any,
  regionBar: PropTypes.bool,
  legend: PropTypes.bool,
  zoom: PropTypes.number,
  zoom2Fit: PropTypes.any,
  navigator: PropTypes.bool,
  region0: PropTypes.bool,
  region2: PropTypes.bool,
  region3: PropTypes.bool,
  region5: PropTypes.bool,
  // showRegions: PropTypes.array,
  devices: PropTypes.array,
  onLoaded: PropTypes.func,
  resize: PropTypes.any,
  onSelectFeature: PropTypes.func,
  onDrawFeature: PropTypes.func,
  overview: PropTypes.bool,
  morscr: PropTypes.bool,
};

export default MapControl;
export { RegionType };

import React, { useEffect, useState } from "react";

// import AlfMap from "map-alf";
import MorscrHeader from "./Header/Index.jsx";
import CardContainer from "./Cards/Container/Index.jsx";
import Alarms from "./Cards/Alarms/Index.jsx";
import Devices from "./Cards/Devices/Index.jsx";
import TimeCharts from "./Cards/TimeCharts/Index.jsx";
import DateCharts from "./Cards/DateCharts/Index.jsx";
import BrandCharts from "./Cards/BrandCharts/Index.jsx";
import FrequencyCharts from "./Cards/FrequencyCharts/Index.jsx";
import MapControl from "../../components/mapControl/Index.jsx";

import styles from "./style.module.less";
import "./test.css";

const Morscr = (props) => {
  const [map, setMap] = useState();
  const [regionConfig, setRegionConfig] = useState();
  useEffect(() => {
    if (map) {
      if (regionConfig) {
        map.setRegions(regionConfig);
        map.drawRegions({
          region0: true,
          region2: true,
          region3: true,
          region5: true,
        });
        map.setCenter(regionConfig.center);
      }
      map.resize();

      // map.showPalneTrack();
    }
  }, [map, regionConfig]);

  useEffect(() => {
    const regConfig = localStorage.getItem("regionconfig");
    if (regConfig) {
      const regionConfig = JSON.parse(regConfig);
      setRegionConfig(regionConfig);
    }
  }, []);

  return (
    <div className={styles.morscrRoot}>
      <MorscrHeader />
      <div className={styles.content}>
        <div className={styles.contentLeft}>
          <div className={styles.item1}>
            <CardContainer title="事件列表">
              <Alarms />
            </CardContainer>
          </div>
          <div className={styles.item2}>
            <CardContainer title="事件时间分布图">
              <TimeCharts />
            </CardContainer>
          </div>
        </div>
        <div className={styles.contentCenter}>
          <div className={styles.centerTop}>
            <CardContainer
              title="防控区域图"
              style={{ borderRadius: "18px" }}
              fit
            >
              <div
                className={styles.mapcon}
                style={{
                  position: "relative",
                  width: "100%",
                  height: "100%",
                  borderRadius: "4PX",
                }}
              >
                <MapControl
                  legend={false}
                  zoom={11}
                  regionBar={false}
                  morscr
                  onLoaded={(map) => {
                    mapRef.current = map;
                    setTimeout(() => {
                      setMap(map);
                    }, 1000);
                  }}
                />
                {/* <AlfMap
                  mapOptions={{
                    onLoaded: (map) => {
                      map.showLegend(true);
                      // map.showNavigator(true);
                      setMap(map);
                    },
                  }}
                /> */}
              </div>
            </CardContainer>
          </div>
          <div className={styles.centerBottom}>
            <div className={styles.item1}>
              <CardContainer title="事件趋势图">
                <DateCharts />
              </CardContainer>
            </div>
            <div className={styles.item2}>
              <CardContainer title="无人机品牌统计图">
                <BrandCharts />
              </CardContainer>
            </div>
          </div>
        </div>
        <div className={styles.contentRight}>
          <div className={styles.item1}>
            <CardContainer title="站点列表">
              <Devices />
            </CardContainer>
          </div>
          <div className={styles.item2}>
            <CardContainer title="无人机频率分布图">
              <FrequencyCharts />
            </CardContainer>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Morscr;

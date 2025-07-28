import React, { useRef, useEffect, useState } from "react";
import PropTypes from "prop-types";
import { TaskStatus } from "ccesocket-webworker";
import { Table2, Loading } from "dui";
// import RadarChart from "./radarChart/RadarChart.jsx";
// import RadarChart1 from "./radarChart1/Index.jsx";
import RadarChart1 from "./radarChart1/Index1.jsx";
import { getColumns, getData } from "./demo.jsx";
import { ModuleCategory, ModuleState } from "../../../../utils/enums.js";
import { mainConf } from "../../../../config/index.js";
import styles from "./style.module.less";

/**
 *
 * @param {RadarMonitorProps} props
 * @returns
 */
const RadarMonitor = (props) => {
  const { taskStatus, devices, onMaxView, maxView } = props;
  const setterRef = useRef();
  // 设备故障信息
  const [errorTip, setErrorTip] = useState();

  useEffect(() => {
    let t;
    if (mainConf.mock) {
      t = setInterval(() => {
        if (setterRef.current) {
          const datas = {
            data: [
              {
                lng: Math.random() * 50 + 20,
                lat: Math.random() * 50 + 20,
              },
              {
                lng: Math.random() * 50 + 20,
                lat: Math.random() * 50 + 20,
              },
              {
                lng: Math.random() * 50 + 20,
                lat: Math.random() * 50 + 20,
              },
              {
                lng: Math.random() * 50 + 20,
                lat: Math.random() * 50 + 20,
              },
              {
                lng: Math.random() * 50 + 20,
                lat: Math.random() * 50 + 20,
              },
            ],
            centerGPS: { x: 50, y: 50 },
            radiusX: 50,
            radiusY: 50,
          };
          setterRef.current(datas);
        }
      }, 1000);
    }
    return () => {
      clearInterval(t);
    };
  }, []);

  /**
   * 监测设备列表
   * @type {Array<Array<Module>>}
   */
  const [deviceList, setDeviceList] = useState();
  useEffect(() => {
    if (devices && devices.length > 0) {
      const devs = devices.filter((d) =>
        d.moduleCategory.includes(ModuleCategory.radar)
      );
      setDeviceList(devs);
    }
  }, [devices]);

  useEffect(() => {
    if (deviceList && deviceList.length > 0) {
      const okDevs = deviceList.filter((d) =>
        [ModuleState.fault, ModuleState.offline, ModuleState.disabled].includes(
          d.moduleState
        )
      );
      if (okDevs.length === deviceList.length) {
        setErrorTip("设备离线或故障");
      }
    }
  }, [deviceList]);

  return (
    <div className={styles.radarRoot}>
      <div className={`${styles.content} ${maxView && styles.large}`}>
        <div
          className={
            maxView ? styles.radarContainer_large : styles.radarContainer
          }
          // style={{
          //   height: !maxView ? "100%" : undefined,
          //   flex: 1,
          // }}
        >
          <RadarChart1
            showTickLabel={false}
            tickInside
            //  datas={[]}
          />
        </div>
        {/* 状态提示、loading等 */}
        {mainConf.mock ? (
          !maxView && (
            <div
              className={styles.mask}
              onClick={() => {
                if (onMaxView) {
                  onMaxView("radar");
                }
              }}
            />
          )
        ) : (
          <>
            {(errorTip || taskStatus !== TaskStatus.running) && (
              <div className={styles.status}>{/* 当前设备离线或故障 */}</div>
            )}
            {taskStatus === TaskStatus.starting && (
              <div className={styles.status}>
                <Loading type="colorful" />
              </div>
            )}
            {!errorTip && taskStatus === TaskStatus.running && !maxView && (
              <div
                className={styles.mask}
                onClick={() => {
                  if (onMaxView) {
                    onMaxView("radar");
                  }
                }}
              />
            )}
          </>
        )}
        <div className={styles.list} style={{ flex: maxView ? 1 : 0 }}>
          {maxView && (
            <Table2
              // rowKey="kid"
              columns={getColumns()}
              showSelection={false}
              // selectRowIndex={1}
              options={{ canRowSelect: true, bordered: false }}
              data={getData()}
            />
          )}
        </div>
      </div>
    </div>
  );
};

RadarMonitor.defaultProps = {
  showList: false,
};

RadarMonitor.propTypes = {
  showList: PropTypes.bool,
};

export default RadarMonitor;

import React, { useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import { Table2 } from "dui";
import turf from "../../../../utils/myTurf";
import { ReactComponent as AlarmIcon } from "../../../../assets/icons/alarm.svg";
import { ReactComponent as AlarmArrow } from "../../../../assets/icons/alarm_arrow.svg";
import { getColumns, getData } from "./demo.jsx";
import { mainConf } from "../../../../config";
import styles from "./style.module.less";

const Alarms = (props) => {
  const { onSelectChange, fit } = props;
  const parentIdRef = useRef([
    "alarm_div_root",
    "alarm_span_left",
    "alarm_div_center",
    "alarm_span_right",
  ]);
  // 是否正在报警
  const [alarming, setAlarming] = useState(false);
  const [showAlarms, setShowAlarms] = useState(false);
  const [visibleAlarms, setVisibleAlarms] = useState(false);
  const [uavList, setUavList] = useState();
  const trackingIdRef = useRef("");
  const regionRef = useRef();

  useEffect(() => {
    if (showAlarms) {
      setVisibleAlarms(true);
    } else {
      setTimeout(() => {
        setVisibleAlarms(showAlarms);
      }, 300);
    }
  }, [showAlarms]);

  useEffect(() => {
    if (!fit) {
      setVisibleAlarms(false);
    }
  }, [fit]);

  const isInRegion = (point, region) => {
    const location = turf.point(point);
    const polygon = turf.polygon(region.feature.geometry.coordinates);

    return turf.booleanPointInPolygon(location, polygon);
  };

  const receiveAlarm = (data) => {
    // 数据处理
    // 电子指纹  型号  当前位置  飞行高度  飞手位置
    if (data) {
      if (data.details) {
        const renderList = data.details.map((item, index) => {
          // 判断飞机位置，预警、警示
          const { region2, region3 } = regionRef.current;
          const inRegion2 = isInRegion(
            [item.droneLongitude, item.droneLatitude],
            region2
          );
          const inRegion3 =
            inRegion2 ||
            isInRegion([item.droneLongitude, item.droneLatitude], region3);
          return {
            no: index + 1,
            id: item.droneSerialNum,
            col2: item.productTypeStr || "",
            col3: `${Number(item.droneLongitude).toFixed(6)},${Number(
              item.droneLatitude
            ).toFixed(6)}`,
            col4: item.height,
            col5: `${Number(item.pilotLongitude).toFixed(6)},${Number(
              item.pilotLatitude
            ).toFixed(6)}`,
            inRegion2,
            inRegion3,
            tracking: trackingIdRef.current === item.droneSerialNum,
          };
        });
        setUavList(renderList);
      }
      setAlarming(data.alarmStatus !== "allClear");
    }
  };

  useEffect(() => {
    window.onAlarm = receiveAlarm;
    return () => {
      window.onAlarm = undefined;
    };
  }, []);

  useEffect(() => {
    const regConfig = localStorage.getItem("regionconfig");
    if (regConfig) {
      const regionConfig = JSON.parse(regConfig);
      // setRegionConfig(regionConfig);
      regionRef.current = regionConfig;
    }
  }, []);

  return (
    <div
      id={parentIdRef.current[0]}
      className={`${styles.alarmroot0} ${showAlarms && styles.showing}`}
      onClick={(e) => {
        if (parentIdRef.current.includes(e.target.id)) {
          setShowAlarms(false);
        }
      }}
    >
      <span id={parentIdRef.current[1]} />
      <div id={parentIdRef.current[2]} className={`${styles.alarmroot}`}>
        <div
          className={`${styles.alarms} ${alarming && styles.alarm}`}
          onClick={() => {
            setShowAlarms(!showAlarms);
          }}
        >
          <AlarmIcon style={{ width: "20px", height: "20px" }} />
          <div className={styles.alarmCount}>
            {(uavList && uavList.length) || 0}
          </div>
          {alarming && (
            <div className={styles.latestAlarm}>
              {/* 最近一次报警信息 */}
              发现入侵飞行器
            </div>
          )}
          {/* {uavList && uavList.length > 0 && ( */}
          {/* <> */}
          <AlarmArrow
            className={`${styles.arrow} ${
              visibleAlarms ? styles.expand : styles.collpase1
            }`}
            style={{ opacity: !uavList || uavList.length === 0 ? 0 : 1 }}
          />
          {/* </>
        )} */}
        </div>
        {visibleAlarms && (
          <div
            className={`${styles.alarmList} ${
              showAlarms ? styles.show : styles.hide
            }`}
          >
            <div className={styles.tablCon}>
              <Table2
                // rowKey="kid"
                columns={getColumns((id) => {
                  trackingIdRef.current = id;
                  // TODO 1 发送锁定跟踪指令
                  // TODO 2 地图联动
                })}
                showSelection={false}
                options={{
                  canRowSelect: true,
                  bordered: { inline: false, outline: false },
                }}
                onRowSelected={(item, index, toSelect) => {
                  console.log("select uav:::::", item);
                  onSelectChange(item.id);
                }}
                data={uavList}
              />
            </div>
          </div>
        )}
      </div>
      <span id={parentIdRef.current[3]} />
    </div>
  );
};

Alarms.defaultProps = {
  onSelectChange: () => {},
};

Alarms.prototype = {
  onSelectChange: PropTypes.func,
};

export default Alarms;

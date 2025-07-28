import React from "react";
// import { IconButton } from "dui";

import styles from "./style.module.less";

const getColumns = (onTrack) => {
  // 标识
  // 型号
  // 飞行位置
  // 飞行高度
  // 飞手位置
  return [
    {
      key: "no",
      name: "序号",
      style: { width: "55px" },
    },
    {
      key: "id",
      name: "电子指纹",
      // sort: true,
      style: { width: "128px" },
    },
    {
      key: "col2",
      name: "型号",
      sort: false,
      style: { width: "110px" },
    },
    {
      key: "col3",
      name: "当前位置",
      // sort: true,
    },
    // {
    //   key: "col4",
    //   name: "飞行高度(m)",
    //   style: { width: "78px" },
    // },
    {
      key: "col6",
      name: "锁定跟踪",
      style: { width: "100px" },
      render: (d) => {
        const { tracking } = d;
        return (
          <div className={styles.trackCell}>
            <div
              className={styles.button}
              style={{ color: tracking ? "#37d0c3" : "" }}
              onClick={(e) => {
                if (onTrack) {
                  onTrack(d.col1);
                }
              }}
            >
              <div
                className={`${styles.status} ${tracking && styles.tracking}`}
              />
              锁定跟踪
            </div>
          </div>
        );
      },
    },
  ];
};

const getData = () => {
  const types = ["uav", "adsb", "fighter", "unknown"];
  const datas = [];
  let referCoords = [
    [104.0765756066678, 30.7142418297975],
    [104.07743987025782, 30.71427721213942],
    [104.07818066762167, 30.71385262318286],
    [104.07937417448562, 30.713428032357655],
    [104.08011497184947, 30.71303882245823],
    [104.08089692462204, 30.712826525487984],
    [104.08253387626479, 30.712374512590202],
    [104.08319236281119, 30.712551427593368],
    [104.0845504913114, 30.712692959363125],
    [104.08607324144788, 30.71279910805336],
    [104.08883065484201, 30.712976022426915],
    [104.08957145220597, 30.713471380530066],
    [104.09105304693361, 30.71403750096114],
    [104.09208193216136, 30.714780528983283],
    [104.093110817389, 30.715841987656674],
    [104.09451010129794, 30.71679729047281],
    [104.0950451216168, 30.717717202759772],
  ];
  referCoords = referCoords.map((it) => [it[0] + 0.12, it[1] + 0.01]);
  for (let i = 0; i < 5; i += 1) {
    datas.push({
      no: i + 1,
      id: `plane_${i}`,
      model: "Air2s",
      visible: true,
      type: types[i % 4],
      col1: "3N3BH5M00202WG",
      col2: "MavicAir 2",
      col3: "104.015268,30.178260",
      col4: "48.8",
      col5: "104015257,30177744",
      coordinates: referCoords.map((item) => {
        const lng = i % 2 == 1 ? item[0] + i * 0.008 : item[0] - i * 0.008;
        const lat = i % 3 == 0 ? item[1] + i * 0.008 : item[1] - i * 0.008;
        return [lng - 0.15, lat - 0.15];
      }),
      description: "none",
      pilot: [
        104.084 + Math.random() / 25 - 0.025,
        30.559 + Math.random() / 15 - 0.02,
      ],
    });
  }

  return datas;
};

const realMock = (callback) => {
  let referCoords = [
    [104.0765756066678, 30.7142418297975],
    [104.07743987025782, 30.71427721213942],
    [104.07818066762167, 30.71385262318286],
    [104.07937417448562, 30.713428032357655],
    [104.08011497184947, 30.71303882245823],
    [104.08089692462204, 30.712826525487984],
    [104.08253387626479, 30.712374512590202],
    [104.08319236281119, 30.712551427593368],
    [104.0845504913114, 30.712692959363125],
    [104.08607324144788, 30.71279910805336],
    [104.08883065484201, 30.712976022426915],
    [104.08957145220597, 30.713471380530066],
    [104.09105304693361, 30.71403750096114],
    [104.09208193216136, 30.714780528983283],
    [104.093110817389, 30.715841987656674],
    [104.09451010129794, 30.71679729047281],
    [104.0950451216168, 30.717717202759772],
  ];
  referCoords = referCoords.map((it) => [it[0] - 0.03, it[1] - 0.16]);
  let coordIndex = 0;
  return setInterval(() => {
    callback({
      type: "securityAlarm",
      alarmStatus: "alarming",
      mockRestart: coordIndex === referCoords.length - 1,
      details: [
        {
          droneLongitude: referCoords[coordIndex][0],
          droneLatitude: referCoords[coordIndex][1],
          droneSerialNum: "35XEDSND456DG",
          model: "air3 pro",
          height: 55.3,
          pilotLongitude: referCoords[0][0],
          pilotLatitude: referCoords[0][1],
        },
        {
          droneLongitude: referCoords[coordIndex][0] + 2 * 0.008,
          droneLatitude: referCoords[coordIndex][1] - 2 * 0.008,
          droneSerialNum: "35XEDSND455DG",
          model: "air3 pro",
          height: 55.3,
          pilotLongitude: referCoords[0][0] + 2 * 0.008,
          pilotLatitude: referCoords[0][1] - 2 * 0.008,
        },
      ],
    });
    coordIndex += 1;
    if (coordIndex === referCoords.length) {
      coordIndex = 0;
    }
  }, 2000);
};

export { getColumns, getData, realMock };

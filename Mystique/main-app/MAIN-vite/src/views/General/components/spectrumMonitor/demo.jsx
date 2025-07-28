import React from "react";
import styles from "./style.module.less";
const getColumns = () => {
  // 标识
  // 型号
  // 飞行位置
  // 飞行高度
  // 飞手位置
  return [
    {
      key: "no",
      name: "序号",
      // sort: true,
      style: { width: "80px" },
    },
    {
      key: "frequency",
      name: "频率（MHz）",
      sort: false,
      style: { width: "100px" },
    },
    {
      key: "bandwidth",
      name: "带宽（kHz）",
      // sort: true,
    },
    {
      key: "azimuth",
      name: "示向度（°）",
      style: { width: "90px" },
      render: (d) => {
        const { azimuth } = d;
        if (azimuth < 0) return Number.NaN;
        return Math.round(azimuth * 10) / 10;
      },
    },
    {
      key: "col5",
      name: "操作",
      render: (d) => {
        // const { kid } = d;
        return (
          <div className={styles.signalOp}>
            <div>跟踪</div>
            <div>压制</div>
          </div>
        );
      },
    },
  ];
};

const getData = () => {
  return [
    {
      no: 1,
      id: "1",
      frequency: "2424",
      bandwidth: "2000",
      azimuth: "48.8",
    },
    {
      no: 1,
      id: "1",
      frequency: "2424",
      bandwidth: "2000",
      azimuth: "48.8",
    },
  ];
};

const demoSignals = [
  {
    guid: "01",
    segmentIdx: 0,
    freqIdxs: [10, 190],
    azimuth: 22.5,
  },
  {
    guid: "02",
    segmentIdx: 0,
    freqIdxs: [250, 500],
    azimuth: 22.5,
  },
];
export { getColumns, getData, demoSignals };

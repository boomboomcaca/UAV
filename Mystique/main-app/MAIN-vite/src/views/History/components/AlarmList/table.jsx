import React from "react";
import {
  FundViewOutlined,
  PlaySquareOutlined,
  RocketOutlined,
} from "@ant-design/icons";

import styles from "./table.module.less";

const getColumns = (onClick) => {
  // 序号 rowid
  // 时间 time
  // 入侵区域 regions
  // 探测设备 device
  // 飞行物数量 count
  // 取证信息 datas
  // 飞行轨迹 track
  // 处置情况 handle
  return [
    {
      key: "rowid",
      name: "序号",
      style: { width: "80px" },
    },
    {
      key: "time",
      name: "发生时间(持续时长)",
      sort: true,
      //  style: { width: "190px" },
    },
    {
      key: "regions",
      name: "入侵区域",
      // sort: true,
    },
    {
      key: "device",
      name: "探测设备",
      //  style: { width: "208px" },
      render: (d) => {
        const { devices } = d;
        return (
          <div className={styles.deviceCell}>
            {devices.map((dev) => {
              return <div>{dev}</div>;
            })}
          </div>
        );
      },
    },
    {
      key: "count",
      name: "飞行物数量",
      style: { width: "100px" },
      // render: (d) => {
      //   const { kid } = d;
      //   return <div style={{ color: "orangered" }}>?kid={kid}</div>;
      // },
    },
    {
      key: "datas",
      name: "取证信息",
      style: { width: "100px" },
      render: (d) => {
        const { tracking } = d;
        return (
          <div className={styles.buttonCell}>
            <div
              className={styles.button}
              onClick={(e) => {
                if (onClick) {
                  onClick({ action: "datas" });
                }
              }}
            >
              <FundViewOutlined /> <span>查看</span>
            </div>
          </div>
        );
      },
    },
    {
      key: "track",
      name: "飞行轨迹",
      style: { width: "100px" },
      render: (d) => {
        const { tracking } = d;
        return (
          <div className={styles.buttonCell}>
            <div
              className={styles.button}
              onClick={(e) => {
                if (onClick) {
                  onClick({ action: "track" });
                }
              }}
            >
              <PlaySquareOutlined />
              <span>回放</span>
            </div>
          </div>
        );
      },
    },
    {
      key: "handle",
      name: "处置情况",
      style: { width: "100px" },
      render: (d) => {
        const { tracking } = d;
        return (
          <div className={styles.buttonCell}>
            <div
              className={styles.button}
              onClick={(e) => {
                if (onClick) {
                  onClick({ action: "handle" });
                }
              }}
            >
              <RocketOutlined />
              <span>查看</span>
            </div>
          </div>
        );
      },
    },
  ];
};

const getDemos = () => {
  // 序号 rowid
  // 时间 time
  // 入侵区域 regions
  // 探测设备 device
  // 飞行物数量 count
  // 取证信息 datas
  // 飞行轨迹 track
  // 处置情况 handle
  return [
    {
      id: "123",
      rowid: 1,
      dateStr: "2023年3月23日",
      timeStr: "10:47:30",
      keepTime: "5分30秒",
      regions: "警戒区",
      devices: ["雷达", "无线电监测", "ADS-B"],
      count: 2,
      uavs: [
        {
          coordinates: [[104.06355333208518, 30.53527624479534]],
          type: "uav",
        },
      ],
    },
    {
      id: "1243",
      rowid: 2,
      dateStr: "2023年3月23日",
      timeStr: "10:47:30",
      keepTime: "5分30秒",
      regions: "警戒区",
      devices: ["雷达", "无线电监测", "ADS-B"],
      count: 2,
      uavs: [
        {
          coordinates: [[104.03355333208518, 30.53527624479534]],
          type: "landing",
        },
      ],
    },
    {
      id: "12335",
      rowid: 3,
      dateStr: "2023年3月23日",
      timeStr: "10:47:30",
      keepTime: "5分30秒",
      regions: "警戒区",
      devices: ["雷达", "无线电监测", "ADS-B"],
      count: 2,
      uavs: [
        {
          coordinates: [[104.03355333208518, 30.53527624479534]],
          type: "uav",
        },
      ],
    },
  ];
};

export { getColumns, getDemos };

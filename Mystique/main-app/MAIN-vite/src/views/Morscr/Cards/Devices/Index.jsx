import React, { useState } from "react";
import { Tooltip } from "react-tooltip";
import styles from "./style.module.less";

const Devices = (props) => {
  const [columns] = useState([
    {
      name: "devName",
      title: "名称",
    },
    {
      name: "devModel",
      title: "型号",
    },
    {
      name: "ipAddr",
      title: "IP地址",
    },
    {
      name: "status",
      title: "状态",
    },
  ]);

  const [statusColor] = useState({
    normal: "#00ff00",
    busy: "#fbc529",
    fault: "#f31174",
  });

  const [dataList] = useState([
    {
      devName: "频谱监测接收机",
      devModel: "ALF900",
      ipAddr: "192.168.1.110",
      status: "busy",
    },
    {
      devName: "光电设备",
      devModel: "NVR100",
      ipAddr: "192.168.1.110",
      status: "normal",
    },
    {
      devName: "无人机接入设备",
      devModel: "UAVI2000",
      ipAddr: "192.168.1.110",
      status: "fault",
    },
    {
      devName: "协议解析设备",
      devModel: "UAVI2000",
      ipAddr: "192.168.1.110",
      status: "fault",
    },
    {
      devName: "雷达",
      devModel: "R1000",
      ipAddr: "192.168.1.110",
      status: "normal",
    },
    {
      devName: "信号压制",
      devModel: "CRL900",
      ipAddr: "192.168.1.110",
      status: "normal",
    },
  ]);

  return (
    <div className={styles.devRoot}>
      <div className={styles.thead}>
        {columns.map((col) => {
          return <div className={styles.tcell}>{col.title}</div>;
        })}
      </div>
      <div className={styles.tbody}>
        {dataList.map((item, index) => {
          return (
            <div
              className={styles.row}
              style={{ backgroundColor: index % 2 === 1 ? "#081f4b" : "" }}
            >
              <div
                className={styles.rcell}
                data-tooltip-id="my-tooltip2"
                data-tooltip-content={item.devName}
                data-tooltip-place="right"
              >
                {item.devName}
              </div>
              <div className={styles.rcell}>{item.devModel}</div>
              <div className={styles.rcell}>{item.ipAddr}</div>
              <div className={`${styles.rcell} ${styles.stausCell}`}>
                <div
                  className={styles.staus}
                  style={{ backgroundColor: statusColor[item.status] }}
                ></div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default Devices;

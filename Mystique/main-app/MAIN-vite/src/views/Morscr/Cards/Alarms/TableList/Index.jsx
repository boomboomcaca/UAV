import React, { useState } from "react";
import { Tooltip } from "react-tooltip";
import styles from "./style.module.less";

const TableList = (props) => {
  const { title, dataList, style, isDay } = props;

  const [columns] = useState([
    {
      name: "uavid",
      title: "电子指纹",
    },
    {
      name: "time",
      title: "发生时间",
    },
    {
      name: "keepTime",
      title: "持续时长",
    },
    {
      name: "uavmodel",
      title: "型号",
    },
  ]);

  return (
    <div className={styles.items} style={style}>
      <div className={styles.title}>{title}</div>
      {/* <div className={styles.tbody}> */}
      <div className={styles.thead}>
        {columns.map((col) => {
          return <div className={styles.tcell}>{col.title}</div>;
        })}
      </div>
      <div className={`${styles.tbody} ${isDay && styles.day}`}>
        {dataList.map((item) => {
          return (
            <div className={styles.row}>
              <div
                className={styles.rcell}
                data-tooltip-id="my-tooltip2"
                data-tooltip-content={item.uavid}
                data-tooltip-place="right"
              >
                {item.uavid}
              </div>
              <div className={styles.rcell}>{item.time}</div>
              <div className={styles.rcell}>{item.keepTime}</div>
              <div className={styles.rcell}>{item.uavmodel}</div>
            </div>
          );
        })}
      </div>
      <Tooltip id="my-tooltip2" />
    </div>
  );
};

export default TableList;

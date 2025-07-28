import React, { useState } from "react";

import TableList from "./TableList/Index.jsx";
import styles from "./style.module.less";

const Alarms = (props) => {
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

  const [demoDatas1] = useState([
    {
      uavid: "3N3BH5M00202WG",
      time: "9:32",
      keepTime: "312s",
      uavmodel: "大疆精灵3",
    },
    {
      uavid: "3N3BH5M00203WG",
      time: "9:32",
      keepTime: "312s",
      uavmodel: "大疆精灵3",
    },
    {
      uavid: "3N3BH5M00204WG",
      time: "9:32",
      keepTime: "312s",
      uavmodel: "大疆精灵3",
    },
  ]);

  const [demoDatas2] = useState([
    {
      uavid: "3N3BH5M00202WG",
      time: "9:32",
      keepTime: "312s",
      uavmodel: "大疆精灵3",
    },
    {
      uavid: "3N3BH5M00202WG",
      time: "9:32",
      keepTime: "312s",
      uavmodel: "大疆精灵3",
    },
    {
      uavid: "3N3BH5M00202WG",
      time: "9:32",
      keepTime: "312s",
      uavmodel: "大疆精灵3",
    },
  ]);

  return (
    <div className={styles.alarmRoot}>
      <TableList title="当日" dataList={demoDatas1} isDay style={{ flex: 2 }} />
      <TableList title="历史" dataList={demoDatas2} style={{ flex: 3 }} />
    </div>
  );
};

export default Alarms;

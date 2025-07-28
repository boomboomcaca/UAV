import React, { useState, useRef, useEffect } from "react";
import { OrderedListOutlined } from "@ant-design/icons";
import { useHistory } from "react-router-dom";

import Header from "../../components/Header/Index.jsx";
import RemoteControl from "./RemoteControl/Index.jsx";
import ProtectedRegion from "./ProtectedRegion/Index.jsx";
import WhiteRegions from "./WhiteRegions/WhiteRegions.jsx";
import DeviceManager from "./DeviceManager/Index.jsx";
import styles from "./style.module.less";

const SystemConfig = (props) => {
  const history = useHistory();

  const [tabItems] = useState([
    {
      name: "protectregion",
      title: "防护区",
      icon: <OrderedListOutlined />,
      // component: <RecList />,
    },
    {
      name: "remotecontrol",
      title: "设备上下电",
      icon: <OrderedListOutlined />,
      // component: <DeviceManager />,
    },
    {
      name: "whiteprotect",
      title: "屏蔽报警区",
      icon: <OrderedListOutlined />,
      // component: <SpecList />,
    },
    {
      name: "devmanager",
      title: "设备管理",
      icon: <OrderedListOutlined />,
      // component: <RadarList />,
    },
    {
      name: "usrmanager",
      title: "用户",
      icon: <OrderedListOutlined />,
      // component: <RadarList />,
    },
  ]);
  const [selItem, setSelItem] = useState(tabItems[0]);
  return (
    <div className={styles.configRoot}>
      <Header
        title="系统设置"
        onBack={() => {
          history.goBack();
        }}
      />
      <div className={styles.rootContent}>
        <div className={styles.tableItems}>
          {tabItems.map((item) => {
            return (
              <div
                className={`${styles.tableItem} ${
                  selItem.name === item.name && styles.sel
                }`}
                onClick={() => setSelItem(item)}
              >
                {item.icon}
                <span className={styles.tt}>{item.title}</span>
              </div>
            );
          })}
        </div>
        <div className={styles.content}>
          {selItem.name === "remotecontrol" && <RemoteControl />}
          {selItem.name === "protectregion" && <ProtectedRegion />}
          {selItem.name === "whiteprotect" && <WhiteRegions />}
          {selItem.name === "devmanager" && <DeviceManager />}
        </div>
      </div>
    </div>
  );
};

export default SystemConfig;

import React, { useRef, useEffect, useContext, useState } from "react";

import { MicroLoadManager, ActionType } from "gisor-qiankun-helper";
import MainContext from "../../../context/context.jsx";
import styles from "./style.module.less";

const DeviceManager = (props) => {
  // 全局缓存
  const ctx = useContext(MainContext);
  const [state = "", dispatch = null] = ctx;

  /**
   * @type {{current:MicroLoadManager}}
   */
  const microLoaderRef = useRef();
  const frontMicroRef = useRef();

  useEffect(() => {
    // 初始化子应用加载器
    microLoaderRef.current = new MicroLoadManager(8, (e) => {
      console.log("load micro message:::", e);
    });

    // 跳转子应用
    const microName = "stamgr";
    const { microApps } = state;
    const microApp = microApps[microName];

    const micro = {
      name: microName,
      entry: microApp.entry,
      container: "devicemanagercontainer",
      onMicroLoaded: (e) => {
        if (e.error) {
          console.log("load micro error::::", e);
        }
        if (e.loadType === "normal" || e.loadType === "switch") {
          // setFrontMicro(e.microId);
          frontMicroRef.current = e.microId;
        }
      },
    };
    if (microLoaderRef.current) {
      microLoaderRef.current.loadMicroOnNormal(micro);
    }
    return () => {
      microLoaderRef.current.unmountMicro(frontMicroRef.current);
    };
  }, []);

  return <div id="devicemanagercontainer" className={styles.root} />;
};

export default DeviceManager;

import React, { useEffect, useState } from "react";
import SpectrumMonitor from "../spectrumMonitor/Index.jsx";
import VideoMonitor from "../videoMonitor/Index.jsx";
import SpectrumControl from "../spectrumControl/Index.jsx";
import styles from "./style.module.less";

const MonitorLayer = (props) => {
  const [large, setLarge] = useState();
  const [components] = useState({
    left: [
      {
        name: "radar",
        element: undefined,
      },
      {
        name: "specmonitor",
        element: <SpectrumMonitor />,
      },
    ],
    right: [
      {
        name: "video",
        element: <VideoMonitor />,
      },
      {
        name: "control",
        element: <SpectrumControl />,
      },
    ],
  });

  return (
    <div className={styles.monitorRoot}>
      <div
        className={styles.monitorLeft}
        style={{ width: large ? "45%" : "0%" }}
      >
        {large && large.element}
      </div>
      <div className={styles.monitorRight}>
        <div className={styles.mrightLeft}>
          {components.left.map((item) => {
            if (!large || item.name !== large.name) {
              return (
                <div className={styles.featureItem} style={{ flex: 1 }}>
                  <div> {item.element}</div>
                  <div
                    className={styles.featureMask}
                    onClick={() => {
                      setLarge(item);
                    }}
                  />
                </div>
              );
            }
            return null;
          })}
        </div>
        <span style={{ flex: 60 }} />
        <div className={styles.mrightRight}>
          {components.right.map((item) => {
            if (!large || item.name !== large.name) {
              return (
                <div className={styles.featureItem} style={{ flex: 1 }}>
                  <div> {item.element}</div>
                  <div
                    className={styles.featureMask}
                    onClick={() => {
                      setLarge(item);
                    }}
                  />
                </div>
              );
            }
            return null;
          })}
        </div>
      </div>
    </div>
  );
};

export default MonitorLayer;

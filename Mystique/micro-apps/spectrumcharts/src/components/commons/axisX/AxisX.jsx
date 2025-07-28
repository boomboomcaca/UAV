import React from "react";
import SpectrumAxisX from "./components/SpectrumAxisX.jsx";
import ScanAxisX from "./components/Scan.jsx";
import StreamAxisX from "./components/Stream.jsx";
import styles from "./style.module.less";

const AxisX = (props) => {
  const { component, segments, streamTime } = props;
  return (
    <div className={styles.axisXRoot}>
      {component ? (
        component
      ) : segments ? (
        <ScanAxisX {...props} />
      ) : streamTime > 0 ? (
        <StreamAxisX streamTime={streamTime} {...props} />
      ) : (
        <SpectrumAxisX {...props} />
      )}
    </div>
  );
};

export default AxisX;

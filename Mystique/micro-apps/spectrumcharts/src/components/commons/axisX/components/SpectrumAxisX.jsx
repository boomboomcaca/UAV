import React, { useContext, useEffect, useState } from "react";
import styles from "./spectrum.module.less";

const SpectrumAxisX = (props) => {
  const { frequency, bandwidth, zoomInfo } = props;

  const [centerTickLeft, setCeneterTickLeft] = useState(50);
  const [labelStart, setLabelStart] = useState(0 - bandwidth / 2);
  const [labelStop, setLabelStop] = useState(bandwidth / 2);

  useEffect(() => {
    // TODO 通过context 处理缩放
    if (zoomInfo) {
      console.log("zoom chagne:::", zoomInfo, bandwidth);
      const { start, end } = zoomInfo;
      if (start !== 0 || end !== 1) {
        setLabelStart(0 - bandwidth / 2 + bandwidth * start);
        setLabelStop(bandwidth * end - bandwidth / 2);
      }
      if (start < 0.5 && end > 0.5) {
        setCeneterTickLeft(((0.5 - start) * 100) / (end - start));
      } else {
        setCeneterTickLeft(-1);
      }
    }
  }, [zoomInfo, frequency, bandwidth]);

  return (
    <div className={styles.axisYRoot}>
      {/* 占位 */}
      <div className={styles.span} />
      <div className={styles.tickLabels}>
        {/* {centerTickLeft > -1 && (
          <div className={styles.centerTick} style={{ left: `calc(${centerTickLeft}% - 1px)` }} />
        )}
        <div className={styles.ticks}></div> */}
        <div className={styles.labels}>
          <div className={styles.leftLabel}>
            {Number(labelStart).toFixed(3)}kHz
          </div>
          {centerTickLeft > 20 && centerTickLeft < 80 && (
            <div
              style={{
                position: "absolute",
                left: `calc(${centerTickLeft}% - 33px)`,
              }}
            >
              {Number(frequency).toFixed(3)}MHz
            </div>
          )}
          <div className={styles.rightLabel}>
            {Number(labelStop).toFixed(3)}kHz
          </div>
        </div>
      </div>
    </div>
  );
};

export default SpectrumAxisX;

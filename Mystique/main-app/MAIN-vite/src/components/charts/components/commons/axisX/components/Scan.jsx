import React, { memo } from "react";

import PropTypes from "prop-types";

import { frequency2String2 } from "../../../utils/utils";
import styles from "./scan.module.less";

const ScanAxisX = memo((props) => {
  const { segments, zoomInfo } = props;

  return (
    <div className={styles.scanXRoot}>
      {segments.map((item, index) => {
        const { startIndex, endIndex } = zoomInfo;
        let flag = 0;
        const segStart = item.startIndex;
        const segEnd = item.startIndex + item.pointCount - 1;
        if (startIndex >= segStart && endIndex <= segEnd) {
          flag = endIndex - startIndex;
        } else if (startIndex >= segStart && endIndex >= segEnd) {
          flag = segEnd - startIndex;
        } else if (startIndex < segStart && endIndex >= segStart) {
          flag = endIndex - segStart;
        }
        const startStr = frequency2String2(item.startFrequency);
        const stopStr = frequency2String2(item.stopFrequency);
        return (
          <div className={styles.item} style={{ flex: `${flag}` }}>
            <>{index > 0 && <span className={styles.gapLine}>|</span>}</>
            <div>{startStr}</div>
            <span style={{ flex: 1 }} />
            <div>{stopStr}</div>
          </div>
        );
      })}
    </div>
  );
});

ScanAxisX.defaultProps = {
  segments: [],
  zoomInfo: { startIndex: 0, endIndex: 1 },
};

ScanAxisX.propTypes = {
  segments: PropTypes.array,
  zoomInfo: PropTypes.any,
};

export default ScanAxisX;

import React, { memo, useEffect } from "react";

import PropTypes from "prop-types";
import styles from "./style.module.less";

const SegmentsBackground = memo((props) => {
  const { segments, zoomInfo } = props;

  return (
    <div className={styles.segbgLayer}>
      {segments.map((s, index) => {
        let startPct = -1;
        let widthPct = -1;
        const sStart = s.startIndex;
        const sStop = s.startIndex + s.pointCount;
        const zStart = zoomInfo.startIndex;
        const zStop = zoomInfo.endIndex;
        if (sStart < zStart && sStop > zStart) {
          startPct = 0;
          widthPct = ((sStop - zStart + 1) * 100) / zoomInfo.zoomLen;
        } else if (zStart >= zStart && sStop <= zStop) {
          startPct = ((sStart - zStart) * 100) / zoomInfo.zoomLen;
          widthPct = (s.pointCount * 100 + 100) / zoomInfo.zoomLen;
        } else if (sStart < zStop && sStop > zStop) {
          startPct = ((sStart - zStart) * 100) / zoomInfo.zoomLen;
          widthPct = ((zStop - sStart + 1) * 100) / zoomInfo.zoomLen;
        }
        if (startPct < 0) return null;
        return (
          <div
            className={`${styles.segbg} ${index % 2 > 0 && styles.segbgeven}`}
            key={s.id}
            style={{
              left: `${startPct}%`,
              width: `${widthPct}%`,
            }}
          />
        );
      })}
    </div>
  );
});

SegmentsBackground.defaultProps = {
  segments: [],
  zoomInfo: {},
};

SegmentsBackground.propTypes = {
  segments: PropTypes.array,
  zoomInfo: PropTypes.any,
};

export default SegmentsBackground;

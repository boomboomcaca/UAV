import React, { useContext, useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
// import { defaultBlends } from "../../../assets/colors.js";
import styles from "./style.module.less";

const Rain = (props) => {
  const { rainTickInfo, colorBlends, paddingInfo, chartGap } = props;

  return (
    <div className={styles.rain}>
      <div
        className={styles.raincaption}
        style={{
          background: `linear-gradient(${colorBlends.join()})`,
          marginTop: `${chartGap}px`,
          marginBottom: `${paddingInfo.bottom}px`,
        }}
      >
        <div className={styles.title}>瀑布图</div>
      </div>
      <div className={styles.rainTick}>
        <div></div>
        {rainTickInfo && rainTickInfo.currentPct > 0 && (
          <div
            style={{
              top:
                rainTickInfo.currentPct < 0.11
                  ? "16px"
                  : `calc(${rainTickInfo.currentPct * 100}% - 16px)`,
            }}
          >
            -{Math.round(rainTickInfo.rainTimeSpan / 100) / 10}s
          </div>
        )}
      </div>
    </div>
  );
};

Rain.defaultProps = {
  rainTickInfo: {},
  colorBlends: undefined,
};

Rain.propTypes = {
  rainTickInfo: PropTypes.any,
  colorBlends: PropTypes.array,
};

export default Rain;

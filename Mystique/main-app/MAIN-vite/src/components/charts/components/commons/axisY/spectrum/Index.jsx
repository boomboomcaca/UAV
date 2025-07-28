import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";

import { ArrowIcon, AutoRangeIcon } from "../../../assets/svgIcons.jsx";

import YTicks from "../ticks/Index.jsx";
import styles from "./style.module.less";

const Spectrum = (props) => {
  const {
    onChange,
    tickVisible,
    range,
    showCaption,
    autoRange,
    chartGap,
    paddingInfo,
    inside,
    onSetGap,
    tickGap,
    title,
    unit,
  } = props;
  return (
    <div className={styles.spectrum}>
      <div
        className={styles.left}
        style={{
          paddingTop: `${paddingInfo.top}px`,
        }}
      >
        <div className={styles.YTickButton} onClick={() => onChange("add")}>
          <ArrowIcon />
        </div>
        <div className={styles.caption}>
          <div
            className={styles.title}
            onClick={() => {
              let index = units.indexOf(unit);
              if (index < units.length - 1) {
                index += 1;
              } else {
                index = 0;
              }
              // TODO 单位切换
            }}
          >
            {showCaption
              ? `${title || "频谱图"}(${unit || "dBμV"})`
              : title || "频谱图"}
            {/* {String("频 谱 图 ( d B μ V )")
          .split(" ")
          .map((c) => (
            <div>{c}</div>
          ))} */}
          </div>
        </div>
        <div className={styles.YTickButton} onClick={() => onChange("del")}>
          <ArrowIcon rotate={180} />
        </div>
        {autoRange && (
          <div className={styles.YTickButton} onClick={() => onSetGap()}>
            {tickGap === 0 ? (
              <AutoRangeIcon />
            ) : (
              <>
                <div>{tickGap}</div>
                <div style={{ lineHeight: "12px", opacity: 0.5 }}>div</div>
              </>
            )}
          </div>
        )}
      </div>
      {tickVisible && (
        <div>
          <YTicks range={range} inside={inside} />
        </div>
      )}
    </div>
  );
};

Spectrum.defaultProps = {
  units: ["dBμV", "dBm"],
  tickVisible: true,
  onAutoRange: () => {},
  rainTickInfo: {},
  autoRange: false,
  range: { minimum: -20, maximum: 80 },
  onChange: () => {},
  onSetGap: () => {},
  tickGap: 10,
};

Spectrum.propTypes = {
  units: PropTypes.array,
  tickVisible: PropTypes.bool,
  onAutoRange: PropTypes.func,
  rainTickInfo: PropTypes.any,
  autoRange: PropTypes.bool,
  range: PropTypes.any,
  onChange: PropTypes.func,
  onSetGap: PropTypes.func,
  tickGap: PropTypes.number,
};

export default Spectrum;

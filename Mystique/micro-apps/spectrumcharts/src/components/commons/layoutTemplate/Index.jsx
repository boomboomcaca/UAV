import React, { useEffect } from "react";
import PropTypes, { string } from "prop-types";

import styles from "./style.module.less";

const ChartTemplate = (props) => {
  const {
    axisY,
    axisYInside,
    axisX,
    axisXInside,
    children,
    padding,
    lightTheme,
  } = props;

  return (
    <div className={`${styles.chartRoot} ${lightTheme && styles.lightTheme}`}>
      {/* <div className={styles.chartRoot}> */}
      <div className={styles.plotArea}>
        <div className={styles.axisYAndChart}>
          {axisY && (
            <>
              {axisYInside && <div className={styles.insideSpan} />}
              <div
                className={`${styles.axisYContainer} ${
                  axisYInside && styles.inside
                } ${axisXInside && styles.insideX}`}
                style={{ bottom: axisX && !axisXInside ? "28px" : 0 }}
              >
                {axisY}
              </div>
            </>
          )}
          <div
            className={styles.chartContainer}
            style={{ padding: padding || "8px 4px 0 0" }}
          >
            {children}
          </div>
        </div>
        {axisX && (
          <div
            className={`${styles.axisXContainer} ${
              axisXInside && styles.inside
            } ${axisYInside && styles.insideY}`}
          >
            {axisX}
          </div>
        )}
      </div>
    </div>
  );
};

ChartTemplate.defaultProps = {
  axisY: undefined,
  axisX: undefined,
  padding: undefined,
  axisYInside: false,
  axisXInside: false,
  // threshold: undefined,
  // threshold: undefined,
};

ChartTemplate.propTypes = {
  axisY: PropTypes.any,
  axisX: PropTypes.any,
  padding: PropTypes.string,
  axisYInside: PropTypes.bool,
  axisXInside: PropTypes.bool,
  // threshold: PropTypes.any,
  // threshold: PropTypes.any,
};

export default ChartTemplate;

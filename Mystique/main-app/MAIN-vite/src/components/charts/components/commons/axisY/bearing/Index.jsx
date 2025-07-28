import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";

const Bearing = (props) => {
  const { showCaption } = props;
  return (
    <div className={styles.spectrum}>
      <div className={styles.left}>
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
            {showCaption ? "示向度(°)" : ""}
            {/* {String("频 谱 图 ( d B μ V )")
          .split(" ")
          .map((c) => (
            <div>{c}</div>
          ))} */}
          </div>
        </div>
      </div>
      <div className={styles.ticks}>
        {[360, 270, 180, 90, 0].map((item) => {
          return <div>{item}</div>;
        })}
      </div>
    </div>
  );
};

Bearing.defaultProps = {
  showCaption: true,
};

Bearing.propTypes = {
  Bearing: PropTypes.bool,
};

export default Bearing;

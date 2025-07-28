import React, { useContext, useEffect, useState } from "react";
import PropTypes from "prop-types";

import styles from "./stream.module.less";

const StreamAxisX = (props) => {
  const { streamTime, inside } = props;

  /**
   *
   * @param {Number} duration s
   * @returns
   */
  function durationFormat(duration) {
    let seconds = duration;
    if (seconds > 60) {
      let minute = Math.floor(seconds / 60);
      seconds = seconds % 60;
      if (minute > 60) {
        const hour = Math.floor(minute / 60);
        minute = minute % 60;
        if (seconds > 0) {
          return `${hour}h${minute}m${seconds}s`;
        } else if (minute > 0) {
          return `${hour}h${minute}m`;
        } else {
          return `${hour}h`;
        }
      }
      if (seconds > 0) return `${minute}m${seconds}s`;
      return `${minute}m`;
    }
    return `${seconds}s`;
  }

  return (
    <div className={styles.levelRoot}>
      {!inside && <div className={styles.span} />}
      <div></div>
      <span />
      <div style={{ opacity: 0.7 }}>{durationFormat(Number(streamTime))}</div>
    </div>
  );
};

StreamAxisX.defaultProps = {
  streamTime: 10,
  inside: false,
};

StreamAxisX.propTypes = {
  streamTime: PropTypes.number,
  inside: PropTypes.bool,
};

export default StreamAxisX;

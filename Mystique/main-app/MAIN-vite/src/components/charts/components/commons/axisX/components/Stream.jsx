import React, { useContext, useEffect, useState } from "react";
import PropTypes from "prop-types";
import styles from "./stream.module.less";

const StreamAxisX = (props) => {
  const { streamTime, inside } = props;
  return (
    <div className={styles.levelRoot}>
      {!inside && <div className={styles.span} />}
      <div></div>
      <span />
      <div style={{ opacity: 0.7 }}>{Number(streamTime / 1e8).toFixed(1)}s</div>
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

import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";

const LevelBar = (props) => {
  const { unit, minimum, maximum, onlyBar } = props;
  return (
    <div className={styles.barRoot}>
      {!onlyBar && (
        <div className={styles.label}>
          <div>
            {minimum}
            <span>{unit}</span>
          </div>
          <div>
            {maximum}
            <span>{unit}</span>
          </div>
        </div>
      )}
      <div className={styles.colorBar} />
    </div>
  );
};

LevelBar.defaultProps = {
  unit: "m",
  minimum: 0,
  maximum: 2000,
};

LevelBar.propTypes = {
  unit: PropTypes.any,
  minimum: PropTypes.any,
  maximum: PropTypes.any,
};

export default LevelBar;

import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";

const Occupancy = (props) => {
  const { showCaption } = props;
  return (
    <div className={styles.spectrum}>
      <div className={styles.left}>
        <div className={styles.caption}>
          <div className={styles.title}>{showCaption ? "占用度(%)" : ""}</div>
        </div>
      </div>
      <div className={styles.ticks}>
        {[100, 80, 60, 40, 20, 0].map((item) => {
          return <div>{item}</div>;
        })}
      </div>
    </div>
  );
};

Occupancy.defaultProps = {
  showCaption: true,
};

Occupancy.propTypes = {
  Occupancy: PropTypes.bool,
};

export default Occupancy;

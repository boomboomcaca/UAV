import React, { useState, useEffect, useRef } from "react";
import PropTypes from "prop-types";

import BackgroundLayer from "./BackgoundLayer/Index.jsx";
import DataLayer from "./DataLayer/Index.jsx";
import SweepLayer from "./SweepLayer/Index.jsx";

import styles from "./style.module.less";

const RadarChart1 = (props) => {
  return (
    <div className={styles.rdchartroot}>
      <BackgroundLayer className={styles.bglayer} />
      <DataLayer />
      <SweepLayer className={styles.splayer} />
    </div>
  );
};

RadarChart1.defaultProps = {
  datas: [],
};

RadarChart1.propTypes = {
  datas: PropTypes.array,
};

export default RadarChart1;

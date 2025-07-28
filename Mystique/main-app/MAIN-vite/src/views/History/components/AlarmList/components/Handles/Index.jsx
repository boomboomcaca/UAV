import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
import { Empty } from "dui";

import styles from "./style.module.less";

const Handles = (props) => {
  return (
    <div className={styles.handleRoot}>
      <Empty emptype={Empty.UAV} />
    </div>
  );
};

export default Handles;

import React, { useState, useRef } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";
import { useEffect } from "react";

const FilterBand = (props) => {
  const { bandwidth, filterBandwidth } = props;

  const [filterWidth, setFilterWidth] = useState(50);

  useEffect(() => {
    if (bandwidth && filterBandwidth) {
      setFilterWidth((filterBandwidth * 100) / bandwidth);
    }
  }, [bandwidth, filterBandwidth]);

  return (
    <div className={styles.bandRoot}>
      {filterWidth && (
        <div
          className={styles.bandCon}
          style={{
            width: `${filterWidth}%`,
          }}
        ></div>
      )}
    </div>
  );
};

FilterBand.defaultProps = {
  bandwidth: 200,
  filterBandwidth: 100,
};

FilterBand.propTypes = {
  bandwidth: PropTypes.number,
  filterBandwidth: PropTypes.number,
};

export default FilterBand;

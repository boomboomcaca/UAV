import React, { useState, useEffect } from "react";

import styles from "./style.module.less";

const YTicks = (props) => {
  const { range, style, inside, unit } = props;
  const [renderKey] = useState(Math.round(Math.random() * 1e6));
  const [tickGap, setTickGap] = useState(10);

  const [maxY, setMaxY] = useState(80);
  useEffect(() => {
    if (range && unit) {
      const { minimum, maximum, tickGap } = range;
      let min = minimum;
      let max = maximum;
      if (unit === "dBm") {
        min = minimum - 105;
        max = maximum - 105;
      }
      setMaxY(maximum);
      setTickGap(tickGap);
    }
  }, [range, unit]);

  return (
    <div className={styles.tickRoot} style={style}>
      {[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((tick, index) => {
        let val = maxY - tick * tickGap;
        if (unit === "dBm") val -= 105;
        return (
          <div
            key={`tick_${index}_${renderKey}`}
            className={`${styles.tickItem} ${inside && styles.inside}`}
          >
            <div>{index % 2 === 0 ? val : ""}</div>
          </div>
        );
      })}
    </div>
  );
};

export default YTicks;

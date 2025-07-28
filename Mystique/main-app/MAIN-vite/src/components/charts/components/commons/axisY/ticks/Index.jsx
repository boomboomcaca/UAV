import React, { useState, useEffect } from "react";

import styles from "./style.module.less";

const YTicks = (props) => {
  const { range, style, inside } = props;

  const [tickGap, setTickGap] = useState(10);

  const [maxY, setMaxY] = useState(80);
  useEffect(() => {
    if (range) {
      const { minimum, maximum } = range;
      setMaxY(maximum);
      setTickGap((maximum - minimum) / 10);
    }
  }, [range]);

  return (
    <div className={styles.tickRoot} style={style}>
      {[0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map((tick, index) => {
        return (
          <div className={`${styles.tickItem} ${inside && styles.inside}`}>
            <div>{index % 2 === 0 ? maxY - tick * tickGap : ""}</div>
          </div>
        );
      })}
    </div>
  );
};

export default YTicks;

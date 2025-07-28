import React, { useState, useEffect } from "react";
import { Checkbox } from "dui";

import styles from "./style.module.less";

const TypeItem = (props) => {
  const { onChange, name, selected, onSelect, allowRoad } = props;

  const [showRoad, setShowRoad] = useState(true);
  const [enable3d, setEnable3D] = useState(false);

  useEffect(() => {
    if (onChange) {
      onChange({
        showRoad: showRoad,
        enable3d: enable3d,
      });
    }
  }, [showRoad, enable3d]);

  useEffect(() => {
    // 回复默认值
    setShowRoad(true);
    setEnable3D(false);
  }, [selected]);

  return (
    <div
      className={`${styles.mapopRoot} ${selected && styles.sel} ${
        name === "常规" ? styles.normal2d : styles.satelite
      }`}
      onClick={() => {
        if (onSelect) {
          onSelect();
        }
      }}
    >
      {selected && (
        <div className={styles.mapOpt}>
          {allowRoad && (
            <Checkbox.Traditional
              className={styles.check}
              checked={showRoad}
              onChange={(bl) => setShowRoad(bl)}
            >
              路网
            </Checkbox.Traditional>
          )}
          <Checkbox.Traditional
            className={styles.check}
            checked={enable3d}
            onChange={(bl) => setEnable3D(bl)}
          >
            3D
          </Checkbox.Traditional>
        </div>
      )}
      <div className={styles.mapName}>{name}</div>
    </div>
  );
};

export default TypeItem;

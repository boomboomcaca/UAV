import React from "react";
import { useState } from "react";
import { InputNumber1 } from "dui";

import Template from "../../components/Template/Index";
import LevelBar from "./components/LevelBar/Index";
import AlienTypes from "./components/AlienTypes/Index";

import styles from "./style.module.less";

const ThreatLevel = (props) => {
  const [barItems] = useState([
    {
      name: "hdistance",
      title: "水平距离",
      min: 0,
      max: 2000,
      unit: "m",
      weight: 8,
    },
    {
      name: "vdistance",
      title: "垂直高度",
      min: 1000,
      max: 0,
      unit: "m",
    },
    {
      name: "vspeed",
      title: "水平接近速度",
      min: 0,
      max: 50,
      unit: "m/s",
      weight: 7,
    },
    {
      name: "hspeed",
      title: "垂直接近速度",
      min: 0,
      max: 50,
      unit: "m/s",
    },
  ]);
  return (
    <Template title="威胁评估">
      <div className={styles.levelConfigs}>
        {barItems.map((item) => {
          return (
            <div className={styles.configItem}>
              <div className={styles.label}>{item.title}</div>
              <div className={styles.values}>
                <div className={styles.valueItem}>
                  <span>等级</span>
                  <div>
                    <LevelBar
                      minimum={item.min}
                      maximum={item.max}
                      unit={item.unit}
                    />
                  </div>
                </div>
                <div className={styles.valueItem}>
                  <span className={styles.impLabel}>权重</span>
                  <InputNumber1
                    minimum={0}
                    maximum={9}
                    value={item.weight || 5}
                  />
                </div>
              </div>
            </div>
          );
        })}

        <div className={styles.configItem1}>
          <div className={styles.label}>飞行器类别</div>
          <div className={styles.value}>
            <AlienTypes />
            <LevelBar onlyBar />
            <div className={styles.valueItem}>
              <span className={styles.impLabel}>权重</span>
              <InputNumber1 minimum={0} maximum={9} value={5} />
            </div>
          </div>
        </div>
      </div>
    </Template>
  );
};

export default ThreatLevel;

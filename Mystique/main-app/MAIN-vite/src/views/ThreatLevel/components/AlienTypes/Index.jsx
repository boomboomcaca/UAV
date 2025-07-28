import React, { useState, useEffect } from "react";
import { sortableContainer, sortableElement } from "react-sortable-hoc";

import othersIcon from "../../../../assets/images/others.png";
import adsbIcon from "../../../../assets/images/adsb.png";
import heliIcon from "../../../../assets/images/helip.png";
import uavIcon from "../../../../assets/images/uav.png";
import fighterIcon from "../../../../assets/images/fighter.png";

import styles from "./style.module.less";

const SortableItem = sortableElement(({ name, title, icon }) => (
  <div>
    <img src={icon} />
    <span>{title}</span>
  </div>
));

const SortableContainer = sortableContainer(({ children }) => {
  return <ul className={styles.sortContainer}>{children}</ul>;
});

const AlienTypes = (props) => {
  const [specialTypes, setSpecialTypes] = useState([
    {
      name: "others",
      title: "其它",
      icon: othersIcon,
    },
    {
      name: "adsb",
      title: "民航客机",
      icon: adsbIcon,
    },
    {
      name: "helicopter",
      title: "直升飞机",
      icon: heliIcon,
    },
    {
      name: "uav",
      title: "无人机",
      icon: uavIcon,
    },
    {
      name: "fighter",
      title: "战斗机",
      icon: fighterIcon,
    },
  ]);
  return (
    <div className={styles.alienRoot}>
      <SortableContainer
        axis="x"
        helperClass={styles.conHelper}
        onSortEnd={(e) => {
          const newTypes = [...specialTypes];
          const temp = newTypes[e.oldIndex];
          if (e.oldIndex < e.newIndex) {
            for (let i = e.oldIndex; i < e.newIndex; i += 1) {
              newTypes[i] = newTypes[i + 1];
            }
          } else {
            for (let i = e.oldIndex; i > e.newIndex; i -= 1) {
              newTypes[i] = newTypes[i - 1];
            }
          }
          newTypes[e.newIndex] = temp;
          setSpecialTypes(newTypes);
          console.log("on sort end:::", e.oldIndex, e.newIndex);
        }}
      >
        {specialTypes.map((item, index) => (
          <SortableItem
            key={`item-${item.name}`}
            index={index}
            name={item.name}
            title={item.title}
            icon={item.icon}
          />
        ))}
      </SortableContainer>
    </div>
  );
};

export default AlienTypes;

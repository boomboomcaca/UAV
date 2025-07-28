import React from "react";
import { useState } from "react";
// import { ReactComponent as Border } from "../../icons/entry_border.svg";
import { ReactComponent as MorScr } from "../../icons/entry_button_morscr.svg";
import { ReactComponent as BlackwhiteList } from "../../icons/engty_button_blackwhitelist.svg";
import { ReactComponent as Config } from "../../icons/engty_button_config.svg";
import { ReactComponent as Eval } from "../../icons/engty_button_evl.svg";
import { ReactComponent as History } from "../../icons/engty_button_history.svg";
import { ReactComponent as ProtectConfig } from "../../icons/engty_button_protectconfig.svg";
import EntryButton from "../EntryButton/Index";

import styles from "./style.module.less";

const Entry = (props) => {
  const [entrys] = useState([
    {
      name: "morscr",
      title: "综合大屏",
      icon: <MorScr className={styles.icon} />,
      link: "/morscr1",
      openNew: true,
    },
    {
      name: "history",
      title: "历史记录",
      icon: <History className={styles.icon} />,
      link: "/history",
    },
    {
      name: "protectconfig",
      title: "防御策略",
      link: "/defensestrategy",
      icon: <ProtectConfig className={styles.icon} />,
    },
    {
      name: "fenseeval",
      title: "威胁评估",
      link: "/threatlevel",
      icon: <Eval className={styles.icon} />,
    },
    {
      name: "bwlist",
      title: "黑白名单",
      icon: <BlackwhiteList className={styles.icon} />,
      link: "/whiteblacklist",
    },
    {
      name: "config",
      title: "系统设置",
      link: "/systemconfig",
      icon: <Config className={styles.icon} />,
    },
  ]);

  return (
    <div className={styles.entryroot}>
      <div className={styles.entryItems}>
        {entrys.map((item) => {
          return (
            // <Link to={item.link}>
            <EntryButton
              key={`key_${item.name}`}
              title={item.title}
              hash={item.link}
              openNew={item.openNew}
            >
              {item.icon}
            </EntryButton>
            // </Link>
          );
        })}
      </div>

      <svg>
        <defs>
          <clipPath id="indexBj" clipPathUnits="objectBoundingBox">
            <path
              //     fill-rule="evenodd"
              transform="scale(0.0005208,0.0062893)"
              d="M1924 187L1647.74 45.8173C1591.38 17.0171 1529 2 1465.71 2H454.287C391 2 328.615 17.0171 272.26 45.8172L-4 187"
            />
          </clipPath>
        </defs>
      </svg>
    </div>
  );
};

export default Entry;

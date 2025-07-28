import React, { useState } from "react";

import { ReactComponent as IdentryIcon } from "../../../../../assets/icons/history_identy.svg";
import { ReactComponent as TrackIcon } from "../../../../../assets/icons/history_track.svg";
import { ReactComponent as HandleIcon } from "../../../../../assets/icons/history_handle.svg";
import styles from "./style.module.less";

const RecordItem = (props) => {
  const { recordInfo, onDetail } = props;

  const [buttons] = useState([
    {
      name: "identy",
      title: "取证信息",
      icon: <IdentryIcon className={styles.icon} />,
    },
    {
      name: "trace",
      title: "飞行轨迹",
      icon: <TrackIcon className={styles.icon} />,
    },
    {
      name: "handle",
      title: "处置情况",
      icon: <HandleIcon className={styles.icon} />,
    },
  ]);

  return (
    <div className={styles.itemRoot}>
      <div className={styles.header}>
        <div className={styles.title}>
          {recordInfo?.dateStr}
          <span>{recordInfo?.timeStr}</span>
        </div>
        <div className={styles.buttons}>
          {buttons.map((item) => {
            return (
              <div
                className={styles.button}
                onClick={() => {
                  if (onDetail) {
                    onDetail(item.name);
                  }
                }}
              >
                {item.icon}
                <span>{item.title}</span>
              </div>
            );
          })}
        </div>
      </div>
      <div className={styles.map}>
        <div className={styles.mapMask} />
        <div className={styles.mapCon}>
          <img width="100%" height="100%" src={recordInfo?.image} />
        </div>
      </div>
      <div className={styles.infoTable}>
        <div className={styles.infoRow}>
          <div className={styles.infoCell}>
            <span>持续时长</span>
            <div>{recordInfo?.keepTime}</div>
          </div>
          <div className={styles.infoCell}>
            <span>入侵区域</span>
            <div>
              {["预警区", "警戒区", "识别处置区", "保护区"][
                recordInfo.regions || 2
              ] || "未知"}
            </div>
          </div>
        </div>
        <div className={styles.infoRow}>
          <div className={styles.infoCell}>
            <span>探测设备</span>
            <div>
              {recordInfo?.devices?.map((item) => {
                return <span>{item}</span>;
              })}
            </div>
          </div>
          <div className={styles.infoCell}>
            <span>飞行物数量</span>
            <div>{recordInfo?.count}</div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RecordItem;

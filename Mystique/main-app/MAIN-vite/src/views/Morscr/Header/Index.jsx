import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import { UserIcon, Exit2Icon } from "dc-icon";
import dayjs from "dayjs";

import { ReactComponent as AlarmIcon } from "./icons/alarm.svg";
import { ReactComponent as ControlIcon } from "./icons/control.svg";
import { ReactComponent as ListIcon } from "./icons/list.svg";
import { ReactComponent as NodeIcon } from "./icons/node.svg";
import { mainConf } from "../../../config";
import styles from "./style.module.less";

const Header = (props) => {
  const { onItemClick } = props;
  const [timeNow, setTimeNow] = useState("");
  const [userName, setuserName] = useState("李查查");

  const getWeek = () => {
    var datas = dayjs().day();
    var week = ["日", "一", "二", "三", "四", "五", "六"];
    return "星期" + week[datas];
  };

  const getNowTime = () => {
    const date = dayjs().format("YYYY-MM-DD");
    const time = dayjs().format("HH:mm:ss");
    return `${date} ${getWeek()} ${time}`;
  };

  useEffect(() => {
    const tmr = setInterval(() => {
      setTimeNow(getNowTime());
    }, 200);
    return () => {
      clearInterval(tmr);
    };
  }, []);

  return (
    <div className={styles.morscrHeader}>
      <div className={styles.headerIco}>
        <div className={styles.dcLogo} alt="decen" />
        <div>{mainConf.systemName}</div>
      </div>
      <div className={styles.tipItems}>
        <div className={`${styles.item} ${styles.warning}`}>
          <div className={`${styles.iconCon} ${styles.warnicon}`}>
            <AlarmIcon className={`${styles.icon} ${styles.svg}`} />
          </div>
          <div className={styles.des}>
            <div>入侵报警</div>
            <div className={styles.label}>20次</div>
          </div>
        </div>
        <div className={`${styles.item} ${styles.whitelist}`}>
          <div className={`${styles.iconCon} ${styles.whiteicon}`}>
            <ListIcon className={`${styles.icon} ${styles.svg}`} />
          </div>
          <div className={styles.des}>
            <div>白名单</div>
            <div className={styles.label}>10个</div>
          </div>
        </div>
        <div className={`${styles.item} ${styles.nodeCount}`}>
          <div className={`${styles.iconCon} ${styles.nodeicon}`}>
            <NodeIcon className={`${styles.icon} ${styles.svg}`} />
          </div>
          <div className={styles.des}>
            <div>设备数量</div>
            <div className={styles.label}>10个</div>
          </div>
        </div>
        <div className={`${styles.item} ${styles.dealTimes}`}>
          <div className={`${styles.iconCon} ${styles.dealicon}`}>
            <ControlIcon className={`${styles.icon} ${styles.svg}`} />
          </div>
          <div className={styles.des}>
            <div>反制无人机</div>
            <div className={styles.label}>19个</div>
          </div>
        </div>
      </div>
      <div className={styles.right}>
        <div className={styles.usrInfo}>
          <UserIcon iconSize={32} color="var(--theme-primary)" />
          <span>李查查</span>（执勤人员）
        </div>
        <div className={styles.timeLabel}>{timeNow}</div>
      </div>
    </div>
  );
};

Header.defaultProps = {
  onItemClick: () => {},
};

Header.propTypes = {
  onItemClick: PropTypes.func,
};

export default Header;

import React, { useState, useRef, useEffect } from "react";
import { OrderedListOutlined } from "@ant-design/icons";
import { useHistory } from "react-router-dom";
import { Radio, Calendar, message } from "dui";
import dayjs from "dayjs";

import AlarmList from "./components/AlarmList/Index.jsx";
import RadarList from "./components/RadarList/Index.jsx";
import SpecList from "./components/SpecList/Index.jsx";
import RecList from "./components/RecList/Index.jsx";
import Header from "../../components/Header/Index.jsx";

import styles from "./style.module.less";

// const { MenuType } = Header;
const History = (props) => {
  const history = useHistory();
  const [customTime, setCustomTime] = useState("最近三天");
  const [date3, setDate3] = useState([
    dayjs().subtract(3, "day").hour(0).minute(0).second(0),
    dayjs().hour(23).minute(59).second(59),
  ]);
  useEffect(() => {
    if (customTime === "最近三天") {
      setDate3([
        dayjs().subtract(3, "day").hour(0).minute(0).second(0),
        dayjs().hour(23).minute(59).second(59),
      ]);
    } else if (customTime === "最近一周") {
      setDate3([
        dayjs().subtract(7, "day").hour(0).minute(0).second(0),
        dayjs().hour(23).minute(59).second(59),
      ]);
    } else if (customTime === "最近一月") {
      setDate3([
        dayjs().subtract(1, "month").hour(0).minute(0).second(0),
        dayjs().hour(23).minute(59).second(59),
      ]);
    }
    // else if (customTime === "最近三月") {
    //   setDate3([
    //     dayjs().subtract(3, "month").hour(0).minute(0).second(0),
    //     dayjs().hour(23).minute(59).second(59),
    //   ]);
    // }
  }, [customTime]);

  const [tabItems] = useState([
    {
      name: "alarmList",
      title: "报警数据",
      icon: <OrderedListOutlined />,
      // component: <AlarmList timeRange={date3} />,
    },
    {
      name: "recList",
      title: "录像数据",
      icon: <OrderedListOutlined />,
      // component: <RecList />,
    },
    {
      name: "specList",
      title: "频谱数据",
      icon: <OrderedListOutlined />,
      // component: <SpecList />,
    },
    {
      name: "radarList",
      title: "雷达数据",
      icon: <OrderedListOutlined />,
      // component: <RadarList />,
    },
  ]);
  const [selItem, setSelItem] = useState(tabItems[0]);

  return (
    <div className={styles.historyRoot}>
      <Header
        title="历史记录"
        onBack={() => {
          history.goBack();
        }}
      />
      <div className={styles.rootContent}>
        <div className={styles.tableItems}>
          {tabItems.map((item) => {
            return (
              <div
                className={`${styles.tableItem} ${
                  selItem.name === item.name && styles.sel
                }`}
                onClick={() => setSelItem(item)}
              >
                {item.icon}
                <span>{item.title}</span>
              </div>
            );
          })}
        </div>
        <div className={styles.content}>
          <div className={styles.conditions}>
            {/* <span>选择时间范围</span> */}
            <Radio
              theme="highLight"
              options={[
                "最近三天",
                "最近一周",
                "最近一月",
                // "最近三月",
                "自定义时段",
              ]}
              value={customTime}
              onChange={(e) => setCustomTime(e)}
            />
            <Calendar.Range
              value={date3}
              disable={customTime !== "自定义时段"}
              onChange={(timeRange) => setDate3(timeRange)}
            />
          </div>
          {selItem.name === "alarmList" && <AlarmList timeRange={date3} />}
          {selItem.name === "recList" && <RecList timeRange={date3} />}
          {selItem.name === "specList" && <SpecList timeRange={date3} />}
          {selItem.name === "radarList" && <RadarList timeRange={date3} />}
        </div>
      </div>
    </div>
  );
};

export default History;

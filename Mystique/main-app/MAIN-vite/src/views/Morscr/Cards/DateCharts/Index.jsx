import React, { useState } from "react";

import {
  XAxis,
  YAxis,
  Tooltip,
  LineChart,
  Line,
  ResponsiveContainer,
} from "recharts";

import styles from "./style.module.less";

const DateCharts = (props) => {
  const [demoData] = useState(
    [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11].map((item) => {
      return {
        name: `${item + 1}月`,
        times: Math.round(Math.random() * 10),
      };
    })
  );

  return (
    <div className={styles.timeChartRoot}>
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={demoData}>
          <XAxis dataKey="name" stroke="#2995bd" />
          <YAxis width={25} stroke="#e0e0e0" />
          <Tooltip contentStyle={{ color: "black" }} />
          <Line
            type="monotone"
            dataKey="times"
            stroke="#22bee8"
            activeDot={{ r: 8 }}
            strokeWidth={2}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};

export default DateCharts;

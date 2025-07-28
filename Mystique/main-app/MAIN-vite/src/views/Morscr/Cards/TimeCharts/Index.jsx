import React, { useState } from "react";

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";

import styles from "./style.module.less";

const TimeCharts = (props) => {
  const [demoData] = useState(
    [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11].map((item) => {
      return {
        name: `${item * 2}-${(item + 1) * 2}`,
        times: Math.round(Math.random() * 10),
      };
    })
  );

  return (
    <div className={styles.timeChartRoot}>
      <ResponsiveContainer width="100%" height="100%">
        <BarChart data={demoData}>
          <defs>
            <linearGradient id="abc-bar-gradient" x1="0" x2="0" y1="0" y2="1">
              <stop offset="0%" stopColor="#22bee8" />
              <stop offset="100%" stopColor="#33a2f4" />
            </linearGradient>
          </defs>
          <XAxis dataKey="name" stroke="#2995bd" />
          <YAxis width={25} stroke="#e0e0e0" />
          <Tooltip contentStyle={{ color: "black" }} />
          <Bar dataKey="times" fill="url(#abc-bar-gradient)" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
};

export default TimeCharts;

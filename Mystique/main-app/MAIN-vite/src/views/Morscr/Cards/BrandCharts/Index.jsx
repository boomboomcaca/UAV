import React, { useState } from "react";

import { Cell, Legend, PieChart, Pie, ResponsiveContainer } from "recharts";

import styles from "./style.module.less";

const BrandCharts = (props) => {
  const [demoData] = useState([
    {
      name: "大疆",
      value: 60,
    },
    {
      name: "臻迪",
      value: 10,
    },
    {
      name: "司马",
      value: 15,
    },
    {
      name: "哈博森",
      value: 15,
    },
  ]);

  const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042"];

  const RADIAN = Math.PI / 180;
  const renderCustomizedLabel = ({
    cx,
    cy,
    midAngle,
    innerRadius,
    outerRadius,
    percent,
    index,
  }) => {
    const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    return (
      <text
        x={x}
        y={y}
        fill="white"
        textAnchor={x > cx ? "start" : "end"}
        dominantBaseline="central"
      >
        {`${(percent * 100).toFixed(0)}%`}
      </text>
    );
  };

  return (
    <div className={styles.timeChartRoot}>
      <ResponsiveContainer width="100%" height="100%">
        <PieChart width={400} height={400}>
          <Legend
            layout="vertical"
            width={72}
            align="right"
            verticalAlign="middle"
            margin={{ top: 0, left: 0, right: 0, bottom: 0 }}
          />
          <Pie
            data={demoData}
            // cx="50%"
            // cy="50%"
            labelLine={false}
            label={renderCustomizedLabel}
            outerRadius="80%"
            fill="#8884d8"
            dataKey="value"
          >
            {demoData.map((entry, index) => (
              <Cell
                key={`cell-${index}`}
                fill={COLORS[index % COLORS.length]}
              />
            ))}
          </Pie>
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
};

export default BrandCharts;

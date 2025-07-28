import React from "react";
import styles from "./style.module.less";
const getColumns = () => {
  // 标识
  // 型号
  // 飞行位置
  // 飞行高度
  // 飞手位置
  return [
    {
      key: "col1",
      name: "批号",
      // sort: true,
      // style: { width: "120px" },
    },
    {
      key: "col2",
      name: "当前位置",
      sort: false,
      style: { width: "55%" },
    },
  ];
};

const getData = () => {
  return [
    {
      kid: 1,
      col1: "100122",
      col2: "104.888888,29.888888",
    },
    {
      kid: 2,
      col1: "100122",
      col2: "104.888888,29.888888",
    },
    {
      kid: 3,
      col1: "100122",
      col2: "104.888888,29.888888",
    },
    {
      kid: 4,
      col1: "100122",
      col2: "104.888888,29.888888",
    },
    {
      kid: 5,
      col1: "100122",
      col2: "104.888888,29.888888",
    },
  ];
};

export { getColumns, getData };

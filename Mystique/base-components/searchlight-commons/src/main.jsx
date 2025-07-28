/*
 * @Author: wangXueDong
 * @Date: 2021-09-08 15:28:42
 * @LastEditors: XYQ
 * @LastEditTime: 2022-08-12 10:07:50
 */
import React from 'react';
import ReactDOM from 'react-dom'; // 入口文件
import Theme, { Mode } from '@dc/theme/dist/tools';
import '@dc/theme';
import App from './App.jsx';

window.theme = new Theme(Mode.dark);
ReactDOM.render(
  // <React.StrictMode>
  <App />,
  // </React.StrictMode>,
  document.getElementById('root'),
);

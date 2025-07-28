import React from "react";
import ReactDOM from "react-dom/client";
// import { ConfigProvider, theme } from 'antd';
import App from "./App";
import "./index.css";
import "./styles/antdTheme.module.less";
import "@dc/theme";

// const arr = [];
// const obj = {};
// for (let i = -100; i < 1000000; i += 1) {
//   arr[i] = Math.random();
//   obj[i] = Math.random();
// }

// setTimeout(() => {
//   const dt2 = new Date().getTime();
//   for (let i = 0; i < 1000000; i += 1) {
//     const indx = Math.round(Math.random() * 1000000 - 100);
//     const a = obj[indx];
//   }
//   console.log(new Date().getTime() - dt2);
// }, 3000);

// setTimeout(() => {
//   const dt1 = new Date().getTime();
//   for (let i = 0; i < 1000000; i += 1) {
//     const indx = Math.round(Math.random() * 1000000 - 100);
//     const a = arr[indx];
//   }
//   console.log(new Date().getTime() - dt1);

//   console.log("------------------");
// }, 5000);

// ConfigProvider.config({
//   theme: {
//     primaryColor: '#25b864',
//   },
// });
ReactDOM.createRoot(document.getElementById("root")).render(
  // <React.StrictMode>
  // <ConfigProvider theme={{}}>
  <App />
  // </ConfigProvider>
  // </React.StrictMode>
);

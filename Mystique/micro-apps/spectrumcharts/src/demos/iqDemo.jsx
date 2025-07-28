import React, { useEffect, useRef, useState } from "react";

import IQChart from "../components/IQChart/IQChart.jsx";
import demoData from "../components/mocker/DemoGenerator.js";
// import SpectrumHelper from "../components/spectrum/helper.js";
import BitsChart from "../components/BitsChart/BitsChart.jsx";
import { ChartTypes } from "../components/index.js";

const IQDemo = (props) => {
  /**
   * @type {{current:{setData:Function}}}
   */
  const setterRef = useRef();
  /**
   * @type {{current:{setData:Function}}}
   */
  const setterRef1 = useRef();

  useEffect(() => {
    let iData = null;
    let qData = null;
    const mocker = demoData({ frame: 2, type: "spectrum" }, (d) => {
      iData = d.data.map((dd) => dd / 10);
    });
    const mocker1 = demoData({ frame: 2, type: "spectrum" }, (d) => {
      qData = d.data.map((dd) => dd / 10);
      if (iData && qData) {
        if (setterRef.current) {
          setterRef.current.setData(iData, qData);
        }
        if (setterRef1.current) {
          setterRef1.current.setData(iData, qData);
        }
      }
    });
    return () => {
      mocker.dispose();
      mocker1.dispose();
    };
  }, []);

  return (
    <div style={{ width: "100%" }}>
      <div
        style={{
          width: "100%",
          height: "300px",
          display: "flex",
          flexDirection: "row",
          justifyContent: "space-between",
        }}
      >
        <div style={{ flex: 1 }}>
          <IQChart
            lightTheme
            chartLoad={(e) => {
              setterRef.current = e;
            }}
          />
        </div>
        <div style={{ flex: 1 }}>
          <IQChart
            lightTheme
            planisphere
            chartLoad={(e) => {
              setterRef1.current = e;
            }}
          />
        </div>
      </div>
      <div
        style={{
          width: "100%",
          height: "300px",
        }}
      >
        <BitsChart
          onLoad={(e) => {
            console.log("eeeee");
            setInterval(() => {
              const data = [];
              for (let i = 0; i < 1000; i += 1) {
                data[i] = Math.random() > 0.5 ? 1 : 0;
              }
              e.addRow(data);
              console.log("aaaaaa");
            }, 500);
          }}
        />
      </div>
    </div>
  );
};

export default IQDemo;

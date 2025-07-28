import React, { useEffect, useRef, useState } from "react";

import DDCChart from "../components/DDCChart/DDCChart.jsx";
import demoData from "../components/mocker/DemoGenerator.js";
// import SpectrumHelper from "../components/spectrum/helper.js";
import { ChartTypes } from "../components/index.js";

const DDCDemo = (props) => {
  /**
   * @type {{current:{setData:Function}}}
   */
  const setterRef = useRef();

  useEffect(() => {
    const mocker = demoData({ frame: 2, type: "spectrum" }, (d) => {
      if (setterRef.current) {
        setterRef.current.setData(d.data);
      }
    });

    return () => {
      mocker.dispose();
    };
  }, []);

  return (
    <div style={{ width: "100%" }}>
      <div
        style={{
          width: "100%",
          height: "400px",
        }}
      >
        <DDCChart
          onLoad={(e) => {
            setterRef.current = e;
          }}
        />
      </div>
    </div>
  );
};

export default DDCDemo;

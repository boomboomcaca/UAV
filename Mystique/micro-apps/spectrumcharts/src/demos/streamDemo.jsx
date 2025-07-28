import React, { useEffect, useRef, useState } from "react";
import { Button } from "dui";
// import Spectrum from "../components/spectrum/Spectrum.jsx";
// import Spectrum from "../components/allInOne/Spectrum.jsx";
import Stream from "../components/allInOne/Stream.jsx";
import demoData from "../components/mocker/DemoGenerator.js";
// import SpectrumHelper from "../components/spectrum/helper.js";
import { ChartTypes } from "../components/index.js";
import SpectrumChartHelper from "../components/utils/spectrumChartHelper.js";

const StreamDemo = (props) => {
  /**
   * @type {{current:SpectrumChartHelper}}
   */
  const setterRef = useRef();
  useEffect(() => {
    const tmr = setInterval(() => {
      setterRef.current.setData({
        type: "level",
        data: Math.random() * 55,
        timestamp: new Date().getTime() ,
      });
    }, 100);

    return () => {
      clearInterval(tmr);
    };
  }, []);

  const [units, setUnits] = useState([
    {
      key: "dBμV",
      label: "dBμV",
    },
    {
      key: "dBm",
      label: "dBm",
    },
    // {
    //   key: 'dBμVm',
    //   label: 'dBμV/m',
    // },
  ]);

  return (
    <div style={{ width: "100%" }}>
      <div style={{ display: "flex", flexDirection: "row" }}>
        <Button
          onClick={() => {
            setUnits([
              {
                key: "dBμV",
                label: "dBμV",
              },
              {
                key: "dBm",
                label: "dBm",
              },
              {
                key: "dBμVm",
                label: "dBμV/m",
              },
            ]);
          }}
        >
          set units
        </Button>
      </div>
      <div style={{ width: "100%", height: "500px" }}>
        <Stream
          axisY={{
            inside: true,
            autoRange: false,
            tickVisible: true,

          }}
          distributionBar
          streamTime={10}
          onLoad={(e) => {
            setterRef.current = e;
          }}
        />
      </div>
    </div>
  );
};

export default StreamDemo;

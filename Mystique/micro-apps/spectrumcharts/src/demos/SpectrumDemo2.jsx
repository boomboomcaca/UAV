import React, { useEffect, useRef, useState } from "react";
import { Button, Select } from "dui";
import Spectrum from "../components/allInOne/Spectrum.jsx";
// import demoData from "../components/mocker/DemoGenerator.js";
import demoData from "../components/mocker/mockWithWorker.js";
// import SpectrumHelper from "../components/spectrum/helper.js";
import SpectrumChartHelper from "../components/utils/spectrumChartHelper.js";
import { ChartTypes } from "../components/index.js";

const { Option } = Select;
const SpectrumDemo2 = (props) => {
  // const chartRef = useRef();
  /**
   * @type {{current:demoData}}
   */
  const mockerRef = useRef();
  const [frequency, setFrequency] = useState(98.5);
  const [bandwidth, setBandWidth] = useState(400);
  const paramsRef = useRef({
    frequency: 98,
    bandwidth: 40000,
  });
  const [visibleValue, setVisibleValue] = useState(0);

  const markerRef = useRef(0);
  /**
   * @type {{current:SpectrumChartHelper}}
   */
  const setterRef = useRef();
  useEffect(() => {
    console.log(setterRef.current);
    setTimeout(() => {
      setterRef.current.setSeriesVisible(["max", "avg", "min"]);
    }, 2900);
    setTimeout(() => {
      if (setterRef.current && !mockerRef.current) {
        console.log("instance:::");
        mockerRef.current = new demoData(
          { frame: 20, type: "spectrum", typedArray: true },
          (d) => {
            setterRef.current.setData({
              data: d.data,
              max: d.data.map((d) => d + 15),
              avg: d.data.map((d) => d - 5),
              frequency: paramsRef.current.frequency,
              bandwidth: paramsRef.current.bandwidth,
              timestamp: new Date().getTime(),
            });
          }
        );
      }

      // setFrequency(100);
    }, 1000);

    return () => {
      console.log("dispose mocker in spectrum chart");
      if (mockerRef.current) {
        mockerRef.current.dispose();
        mockerRef.current = undefined;
      }
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
      {/* <div
        style={{ position: "fixed", zIndex: 22, width: "100%", height: "100%" }}
      /> */}
      <div>
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
        <Button
          onClick={() => {
            markerRef.current = markerRef.current + 1;
            setterRef.current.selectMarker({ id: `M${markerRef.current}` });
          }}
        >
          SelectMarkerOnMobile
        </Button>
        <Button
          onClick={() => {
            setterRef.current.setMarkerFrequency({
              id: `M${markerRef.current}`,
              frequency: 100,
            });
          }}
        >
          setMarerFreq
        </Button>
        <Button
          onClick={() => {
            setterRef.current.setThreshold(40);
          }}
        >
          setThreshold
        </Button>
        <Select value={visibleValue} onChange={(val) => setVisibleValue(val)}>
          <Option value={0}>spec+rain</Option>
          <Option value={1}>spec</Option>
          <Option value={2}>rain</Option>
        </Select>
      </div>
      <div style={{ width: "100%", height: "500px" }}>
        <Spectrum
          // ref={chartRef}
          maxMarker={6}
          units={units}
          frequency={frequency}
          bandwidth={bandwidth}
          filterBandwidth={50}
          // axisY={{
          //   autoRange: true,
          //   tickVisible: true,
          // }}
          axisY={undefined}
          axisX=""
          showThreshold
          allowZoom
          useGPU={false}
          // viewOptions={{ axisX: true, toolbar: true, axisY: true }}
          visibleCharts={
            visibleValue === 0
              ? [ChartTypes.spectrum, ChartTypes.rain]
              : visibleValue === 1
              ? [ChartTypes.spectrum]
              : [ChartTypes.rain]
          }
          // mobileZoomMode="complex"
          onLoad={(e) => {
            setterRef.current = e;
            setterRef.current.watchMarker((e) => {
              // console.log("marker change:::", e);
            });
          }}
          onParameterChange={(e) => {
            console.log("Parameter changed from chart:::", e);
            if (e.frequency) {
              paramsRef.current.frequency = e.frequency;
              setFrequency(e.frequency);
            }
            if (e.bandwidth) {
              paramsRef.current.bandwidth = e.bandwidth;
              setBandWidth(e.bandwidth);
            }
          }}
          onThresholdChange={(e) => {
            console.log("onThresholdChange", e);
          }}
        />
      </div>
    </div>
  );
};

export default SpectrumDemo2;

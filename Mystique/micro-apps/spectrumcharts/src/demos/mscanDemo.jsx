import React, { useEffect, useRef, useState } from "react";
import { Button, Checkbox, message } from "dui";
// import Scan from "../components/combinescan/Index.jsx";
import { MScan } from "../components/index.js";
import { ChartTypes } from "../components/index.js";
import demoData from "../components/mocker/DemoGenerator.js";
import ScanChartHelper from "../components/utils/scanChartHelper.js";
// import SpectrumHelper from "../components/scan/helper";

const MScanDemo = (props) => {
  // const chartRef = useRef();
  const mockerRef = useRef();
  const [freqList, setFreqList] = useState();
  const [segments, setSegments] = useState();
  /**
   * @type {{current:ScanChartHelper}}
   */
  const setterRef = useRef();

  useEffect(() => {
    const freqList = [];
    for (let i = 87; i < 90; i += 0.5) {
      freqList.push(i);
    }
    console.log(freqList.length);
    setFreqList(freqList);
  }, []);

  useEffect(() => {
    if (freqList) {
      setSegments([
        {
          startFrequency: 1,
          stopFrequency: freqList.length,
          stepFrequency: 1000,
        },
      ]);
    }
  }, [freqList]);

  useEffect(() => {
    if (!segments) return;
    if (setterRef.current && !mockerRef.current) {
      console.log("instance:::");
      setTimeout(() => {
        mockerRef.current = demoData(
          {
            frame: 50,
            type: "scan",
            segments,
            performance: false,
          },
          (d) => {
            // TODO
            d.timestamp = new Date().getTime();
            setterRef.current.setData(d);
          }
        );
        setTimeout(() => {
          setterRef.current.setSeriesVisible(["max", "avg", "min"]);
        }, 2900);
      }, 1000);
    }

    return () => {
      mockerRef.current?.dispose();
      mockerRef.current = undefined;
    };
  }, [segments]);

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

  const [visibleCharts, setVisibleCharts] = useState([
    ChartTypes.spectrum,
    ChartTypes.rain,
  ]);

  const [chartSize, setChartSize] = useState({
    width: 600,
    height: 450,
  });

  const options = [
    {
      label: "频谱图",
      value: ChartTypes.spectrum,
    },
    {
      label: "瀑布图",
      value: ChartTypes.rain,
    },
    {
      label: "示向度图",
      value: ChartTypes.wbdf,
    },
  ];

  return (
    <div style={{ width: "100%" }}>
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
            setChartSize({ width: 640, height: 460 });
            setTimeout(() => {
              setterRef.current.resize();
            }, 800);
          }}
        >
          Change Size
        </Button>
        <Button
          onClick={() => {
            setterRef.current.zoomToSegment(0);
          }}
        >
          Zoom2Segment_0
        </Button>
        <Button
          onClick={() => {
            setterRef.current.resetZoom();
          }}
        >
          ResetZoom
        </Button>
        <Checkbox.Group
          options={options}
          value={visibleCharts}
          sort={false}
          // className={styles.cheboxes}
          onChange={(e) => {
            // 只能显示2个，先进先出
            if (e.length > 2) {
              const outItem = visibleCharts[0];
              const newItems = e.filter((it) => it !== outItem);
              setVisibleCharts(newItems);
            } else if (e.length === 0) {
              message.info("至少需要显示一个图表");
            } else {
              setVisibleCharts(e);
            }
          }}
        />
      </div>
      <div style={{ width: "100%", height: "540px" }}>
        <MScan
          // units={units}
          visibleCharts={visibleCharts}
          viewOptions={{ axisX: true, toolBar: true, axisY: { inside: true } }}
          frequencyList={freqList}
          mobileZoomMode="complex"
          useGPU={false}
          allowZoom={false}
          allowAddMarker={false}
          // mScanMode
          onLoad={(e) => {
            setterRef.current = e;
          }}
          threshold={20}
          onThresholdChange={(e) => {
            console.log("Threshold changed:::", e);
          }}
        />
      </div>
    </div>
  );
};

export default MScanDemo;

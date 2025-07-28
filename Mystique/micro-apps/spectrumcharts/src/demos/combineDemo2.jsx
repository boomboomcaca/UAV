import React, { useEffect, useRef, useState } from "react";
import { Button, Checkbox, message, Select } from "dui";
import CombineScan from "../components/allInOne/CombineScan.jsx";
import { ChartTypes } from "../components/index.js";
import demoData from "../components/mocker/DemoGenerator.js";
// import demoData from "../components/mocker/mockWithWorker.js";
import ScanChartHelper from "../components/utils/scanChartHelper.js";

const getSignalListData = () => {
  return {
    type: "signalsList",
    signals: [
      {
        guid: "10-190",
        segmentIdx: 0,
        freqIdxs: [10, 190],
      },
      {
        guid: "200-190",
        segmentIdx: 0,
        freqIdxs: [300, 490],
      },
      {
        guid: "300-190",
        segmentIdx: 0,
        freqIdxs: [600, 800],
      },
    ],
  };
};

const { Option } = Select;
const CombineDemo2 = (props) => {
  // const chartRef = useRef();
  const [chartMarkers, setChartMarkers] = useState();
  const markersRef = useRef();
  /**
   * @type {{current:demoData}}
   */
  const mockerRef = useRef();
  const dfScanMockerRef = useRef();
  const occMockerRef = useRef();
  const [segments, setSegments] = useState([
    {
      id: "123456",
      startFrequency: 88,
      stopFrequency: 2180,
      stepFrequency: 25,
    },
    {
      id: "123457",
      startFrequency: 1090,
      stopFrequency: 1600,
      stepFrequency: 25,
    },
    // {
    //   id: "123458",
    //   startFrequency: 200,
    //   stopFrequency: 500,
    //   stepFrequency: 50,
    // },
  ]);
  /**
   * @type {{current:ScanChartHelper}}
   */
  const setterRef = useRef();
  useEffect(() => {
    console.log(setterRef.current);
    setTimeout(() => {
      setterRef.current.setSeriesVisible(["max", "avg", "min"]);
    }, 2900);
    console.log("instance:::");
    let wbdfData = 0;
    let prevSignalTime = new Date().getTime();
    setTimeout(() => {
      // document.body.classList.add("lightTheme");
      if (setterRef.current && !mockerRef.current) {
        // mockerRef.current = new demoData(
        mockerRef.current = demoData(
          {
            frame: 3,
            type: "scan",
            segments,
            // performance: true,
            typedArray: true,
          },
          (d) => {
            // TODO
            d.timestamp = new Date().getTime();
            setterRef.current.setData(d);
          }
        );

        // dfScanMockerRef.current = new demoData(
        //   {
        //     frame: 20,
        //     type: "scan",
        //     segments,
        //     performance: false,
        //   },
        //   (d) => {
        //     // TODO
        //     // d.timestamp = new Date().getTime() * 1e5;
        //     // d.azimuths = d.data;
        //     // d.type = "dfscan";
        //     // setterRef.current.setData(d);
        //   }
        // );
        // occMockerRef.current = new demoData(
        //   {
        //     frame: 10,
        //     type: "scan",
        //     segments,
        //     performance: false,
        //     typedArray: true,
        //   },
        //   (d) => {
        //     d.type = "occupancy";
        //     setterRef.current.setData(d);
        //   }
        // );
      }
    }, 1000);

    return () => {
      mockerRef.current?.dispose();
      mockerRef.current = undefined;
    };
  }, [segments]);

  useEffect(() => {
    console.log(markerRef);
  }, [markersRef]);

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

  const markerRef = useRef(0);
  const [visibleValue, setVisibleValue] = useState(0);

  const [chartSize, setChartSize] = useState({
    width: 600,
    height: 450,
  });

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
          <Option value={3}>spec+scandf</Option>
          <Option value={1}>spec</Option>
          <Option value={2}>rain</Option>
          <Option value={4}>spec+rain+occ</Option>
          <Option value={5}>spec+rain+scandf</Option>
        </Select>
      </div>
      <div style={{ width: "100%", height: "500px" }}>
        <CombineScan
          // units={units}
          initCharts={[
            ChartTypes.spectrum,
            ChartTypes.rain,
            ChartTypes.occupancy,
          ]}
          visibleCharts={
            visibleValue === 0
              ? [ChartTypes.spectrum, ChartTypes.rain]
              : visibleValue === 1
              ? [ChartTypes.spectrum]
              : visibleValue === 3
              ? [ChartTypes.spectrum, ChartTypes.wbdf]
              : visibleValue === 4
              ? [ChartTypes.spectrum, ChartTypes.rain, ChartTypes.occupancy]
              : visibleValue === 5
              ? [ChartTypes.spectrum, ChartTypes.rain, ChartTypes.wbdf]
              : [ChartTypes.rain]
          }
          // viewOptions={{ axisX: true, toolBar: true, axisY: { inside: true } }}
          axisY={{ inside: false, autoRange: true, tickVisible: true }}
          axisX={{ inside: false }}
          segments={segments}
          showCursor
          // allowAddMarker={false}
          showThreshold
          // lightTheme
          showBand
          useGPU={false}
          // mobileZoomMode="complex"
          onLoad={(e) => {
            console.log("chart setter::::", e);
            setterRef.current = e;
            setterRef.current.on("CursorChange", (e) => {
              const freq = e.frequency;
            });
            // e.watchMarker((e) => {
            //   // console.log("marker change:::", e);
            //   setChartMarkers(new Date().getTime());
            // });
          }}
          threshold={25}
          onThresholdChange={(e) => {
            console.log("Threshold changed:::", e);
          }}
          onParameterChange={(e) => {
            console.log("Parameter changed from chart:::", e);
          }}
          onSignalSelect={(e) => {
            console.log("onSignalSelect:::", e);
          }}
        />
      </div>
    </div>
  );
};

export default CombineDemo2;

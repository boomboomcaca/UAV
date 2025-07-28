import React, { useEffect, useRef, useState } from "react";
import RainChartWorker from "../lib/RainChartGPUWorker";

import demoData from "../components/mocker/mockWithWorker";

const RainChartWithWorkerGPU = (props) => {
  const conRef = useRef();
  /**
   * @type {{current:RainChartWorker}}
   */
  const chartRef = useRef();
  /**
   * @type {{current:demoData}}
   */
  const mockerRef = useRef();

  const [running, setRunning] = useState(false);

  useEffect(() => {
    if (conRef.current && !chartRef.current) {
      chartRef.current = new RainChartWorker(conRef.current);
    }
  }, [conRef.current]);

  useEffect(() => {
    if (running) {
      mockerRef.current = new demoData(
        { frame: 30, type: "spectrum", typedArray: true },
        (e) => {
          chartRef.current.addRow(e.data);
        }
      );
    }
    return () => {
      if (mockerRef.current) {
        mockerRef.current.dispose();
        mockerRef.current = null;
      }
    };
  }, [running]);

  return (
    <div>
      <div>
        <button disabled={running} onClick={() => setRunning(true)}>
          开始
        </button>
        <button disabled={!running} onClick={() => setRunning(false)}>
          停止
        </button>
      </div>
      <div style={{ width: "955px", height: "242px" }} ref={conRef} />
      {/* <canvas id="mycanvas" width={960} height={400} /> */}
    </div>
  );
};
export default RainChartWithWorkerGPU;

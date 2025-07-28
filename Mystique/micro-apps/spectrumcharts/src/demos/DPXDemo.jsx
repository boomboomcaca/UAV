import React, { useEffect, useRef, useState, useLayoutEffect } from "react";

import DPXChart from "../components/DPXChart/DPXChart.jsx";
import demoData from "../components/mocker/DemoGenerator.js";
// import SpectrumHelper from "../components/spectrum/helper.js";
import { ChartTypes } from "../components/index.js";
import { DPX } from "../lib/index";

const DPXDemo = (props) => {
  /**
   * @type {{current:{setData:Function}}}
   */
  const setterRef = useRef();

  /**
   * @type {{current:HTMLElement}}
   */
  const canvasCon = useRef();
  /**
   * @type {{current:HTMLCanvasElement}}
   */
  const canvasRef = useRef();

  /**
   * @type {{current:DPX}}
   */
  const dpxRef = useRef();

  const [composites] = useState([
    "color",
    "color-burn",
    "color-dodge",
    "copy",
    "darken",
    "destination-atop",
    "destination-in",
    "destination-out",
    "destination-over",
    "difference",
    "exclusion",
    "hard-light",
    "hue",
    "lighten",
    "lighter",
    "luminosity",
    "multiply",
    "overlay",
    "saturation",
    "screen",
    "soft-light",
    "source-atop",
    "source-in",
    "source-out",
    "source-over",
    "xor",
  ]);
  const [composite, setComposite] = useState("source-over");

  useEffect(() => {
    const mocker = demoData(
      {
        frame: 100,
        type: "scan",
        segments: [
          { startFrequency: 87, stopFrequency: 200, stepFrequency: 25 },
        ],
      },
      (d) => {
        if (setterRef.current) {
          // console.log("get data ", d.data.length);
          d.type = "spectrum";
          d.timestamp = new Date().getTime();
          setterRef.current.setData(d);
        }
      }
    );

    return () => {
      mocker.dispose();
    };
  }, []);

  useEffect(() => {
    dpxRef.current = new DPX(canvasCon.current);
    dpxRef.current.initSeries({
      name: "real",
      color: "#00FF00",
      shadowColor: "#00FF00",
    });
    dpxRef.current.setAxisYRange(-20, 100);
    const mocker = demoData(
      {
        frame: 50,
        type: "scan",
        segments: [
          { startFrequency: 87, stopFrequency: 200, stepFrequency: 25 },
        ],
      },
      (d) => {
        if (setterRef.current) {
          const data = d.data.map((d) => d / 10);
          dpxRef.current.setDataToDPX(data);
          // dpxRef.current.setData([{ name: "real", data }]);
        }
      }
    );

    return () => {
      mocker.dispose();
    };
  }, []);

  useEffect(() => {
    const ctx = canvasRef.current.getContext("2d");
    ctx.clearRect(0, 0, 800, 200);
    ctx.globalAlpha = 0.1;
    ctx.fillStyle = "#000000";
    ctx.globalCompositeOperation = composite;
    for (let i = 0; i < 20; i += 1) {
      const offset1 = i * 2;
      const offset2 = offset1 * 2;
      const width = 100 - offset2;

      ctx.fillRect(100 + offset1, 50 + offset1, width, width);
      //  ctx.strokeStyle = "#ff0000";
      //  ctx.strokeRect(100 + offset1, 50 + offset1, width, width);
    }
    setTimeout(() => {
      const imgData = ctx.getImageData(100, 49, 100, 100);
      console.log(imgData);
    }, 1000);
    //     ctx.fillRect(100, 50, 100, 100);
  }, [composite]);

  return (
    <div style={{ width: "100%" }}>
      <div
        style={{
          width: "100%",
          height: "200px",
        }}
      >
        <DPXChart
          onLoad={(e) => {
            setterRef.current = e;
          }}
        />
      </div>
      <div
        ref={canvasCon}
        style={{
          width: "100%",
          height: "200px",
          // backgroundColor: "white",
        }}
      ></div>
      <div
        style={{
          width: "100%",
          height: "200px",
          backgroundColor: "white",
        }}
      >
        <select
          style={{ fontSize: "16px", padding: "4px" }}
          size={4}
          name="pets"
          id="pet-select"
          value={composite}
          onChange={(e) => {
            console.log(e.target.value);
            setComposite(e.target.value);
          }}
        >
          {composites.map((item) => (
            <option style={{ fontSize: "16px", padding: "2px" }} value={item}>
              {item}
            </option>
          ))}
        </select>

        <canvas ref={canvasRef} width="800px" height="200px" />
      </div>
    </div>
  );
};

export default DPXDemo;

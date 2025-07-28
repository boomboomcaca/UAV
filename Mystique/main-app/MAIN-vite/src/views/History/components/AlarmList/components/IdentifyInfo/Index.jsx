import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
import { PopUp, TabButton, message, Radio } from "dui";
import { Player } from "video-react";
import {
  CombineScan,
  ChartTypes,
  DemoGenerator,
} from "../../../../../../components/charts/components";
import { decode } from "@dc/msgpack";

import Modal from "../../../../../../components/Modal/Index.jsx";
import getConf from "../../../../../../config/index";
import { getEvidence, getSpectrumFrame } from "../../../../../../api/newServer";
import { mainConf } from "../../../../../../config/index";
import styles from "./style.module.less";

const IdentifyInfo = (props) => {
  const { record } = props;

  const config = getConf();
  const [option, setOption] = useState([]);
  const [state, setState] = useState();
  const [selEvidence, setSelEvidence] = useState();

  const [videoFile, setVideoFile] = useState();
  const [specFile, setSpecFile] = useState();

  /**
   * @type {{current:SpectrumHelper}}
   */
  const setterRef = useRef();
  const [visibleCharts] = useState([ChartTypes.spectrum, ChartTypes.rain]);
  const [segments, setSegments] = useState([
    { startFrequency: 87, stopFrequency: 108, stepFrequency: 25 },
  ]);

  useEffect(() => {
    // 查询取证信息
    if (record) {
      const { uavs } = record;
      if (uavs && uavs.length > 0) {
        setOption(
          uavs.map((item) => {
            return {
              label: item.model || "不明飞行物",
              value: item.id,
            };
          })
        );
        setState(uavs[0].id);
      }
    }
  }, [record]);

  useEffect(() => {
    if (state && !mainConf.mock) {
      const { uavs } = record;
      const selItem = uavs.find((u) => u.id === state);
      setSelEvidence(selItem);
      getEvidence(state)
        .then((res) => {
          res
            .json()
            .then((json) => {
              if (json) {
                // 获取第一条视频信息
                const video = json.find((e) => e.fileType === 2);
                if (video) {
                  console.log("video video path path:::", video);
                  setVideoFile(
                    `${config.apiBaseUrl1}${video.filePath}/${video.fileName}`
                  );
                }
                // 获取第一条频谱信息
                const spec = json.find((e) => e.fileType === 3);
                if (spec) {
                  setSpecFile(spec);
                }
              }
            })
            .catch((ee) => console.log("get getEvidence error0:::", ee));
        })
        .catch((er) => console.log("get getEvidence error1:::", er));
    }
  }, [state]);

  /**
   * 开始频谱数据回放
   */
  useEffect(() => {
    let tmr;
    if (specFile) {
      const { totalFrames, fileName, filePath, segments } = specFile;
      console.log("file change:::", segments);
      if (segments) setSegments(JSON.parse(segments));
      let frameIndex = 0;
      tmr = setInterval(() => {
        console.log(frameIndex);
        // 请求频谱数据
        getSpectrumFrame({ filePath, fileName, frameId: frameIndex })
          .then((res) => {
            res
              .arrayBuffer()
              .then((buffer) => {
                const fd = decode(
                  Array.prototype.slice.call(new Uint8Array(buffer))
                );
                console.log("get frame data::::", fd);
                if (fd.result === "ok") {
                  const specData = fd.data;
                  specData.forEach((it, index) => {
                    if (setterRef.current) {
                      setterRef.current.setData({
                        timestamp: new Date().getTime() * 1e5,
                        data: it.map((d) => d / 10),
                        segmentOffset: index,
                        offset: 0,
                        type: "scan",
                      });
                    }
                  });
                }
              })
              .catch((er) => console.log("get spectrum frame failed:::", er));
            // res
            //   .json((json) => {
            //     console.log("frame data::::", json);
            //     if (json.result === "OK") {
            //       const specData = json.result.data;
            //       specData.forEach((it, index) => {
            //         if (setterRef.current) {
            //           setterRef.current.setData({
            //             timestamp: new Date().getTime() * 1e5,
            //             data: it,
            //             segmentOffset: index,
            //             offset: 0,
            //           });
            //         }
            //       });
            //     }
            //   })
            //   .catch((ee) => console.log("get spectrum frame failed0:::", ee));
          })
          .catch((er) => console.log("get spectrum frame failed:::", er));
        frameIndex += 1;
        if (frameIndex >= totalFrames) {
          clearInterval(tmr);
        }
      }, 1000);
    }
    return () => {
      if (tmr) clearInterval(tmr);
    };
  }, [specFile]);

  const mockerRef = useRef();

  useEffect(() => {
    if (mainConf.mock) {
      setVideoFile("videoDemo.mp4");
      const segMents = [
        {
          startFrequency: 87,
          stopFrequency: 108,
          stepFrequency: 25,
        },
        {
          startFrequency: 137,
          stopFrequency: 167,
          stepFrequency: 25,
        },
      ];
      setSegments(segMents);

      setOption([
        {
          label: "飞行物1",
          value: "a",
        },
        {
          label: "飞行物2",
          value: "b",
        },
      ]);
    }
  }, []);

  useEffect(() => {
    if (mainConf.mock && segments && !mockerRef.current) {
      console.log("instance:::", segments);
      // setTimeout(() => {
      //   setterRef.current.resize();
      // }, 500);
      mockerRef.current = DemoGenerator(
        {
          frame: 30,
          type: "scan",
          segments,
        },
        (d) => {
          // TODO
          d.timestamp = new Date().getTime() * 1e5;
          if (setterRef.current) setterRef.current.setData(d);
        }
      );
    }
    return () => {
      mockerRef.current?.dispose();
      mockerRef.current = undefined;
    };
  }, [segments]);

  return (
    <div className={styles.identifyRoot}>
      {option.length > 0 && (
        // <TabButton
        //   state={state}
        //   onChange={(d) => setState(d)}
        //   option={option}
        // />

        <Radio
          theme="highLight"
          options={option}
          value={state}
          onChange={(e) => setState(e)}
        />
      )}
      <div className={styles.content}>
        <div className={styles.infoTable}>
          <div className={styles.infoRow}>
            <div className={styles.infoCell}>
              <span>类型</span>
              <div>低小慢无人机</div>
            </div>
            <div className={styles.infoCell}>
              <span>无线电频率</span>
              <div>{`${selEvidence?.radioFrequency}MHz`}</div>
            </div>
          </div>
          <div className={styles.infoRow}>
            <div className={styles.infoCell}>
              <span>电子指纹</span>
              <div>{selEvidence?.electronicFingerprint}</div>
            </div>
            <div className={styles.infoCell}>
              <span>最后飞行位置</span>
              <div>{`${Number(selEvidence?.lastFlightLongitude).toFixed(
                6
              )}，${Number(selEvidence?.lastFlightLatitude).toFixed(6)}`}</div>
            </div>
          </div>
          <div className={styles.infoRow}>
            <div className={styles.infoCell}>
              <span>飞手位置</span>
              <div>{`${Number(selEvidence?.pilotLongitude).toFixed(
                6
              )}，${Number(selEvidence?.pilotLatitude).toFixed(6)}`}</div>
            </div>
            <div className={styles.infoCell}>
              <span>返航位置</span>
              <div>{`${Number(selEvidence?.returnLongitude).toFixed(
                6
              )}，${Number(selEvidence?.returnLatitude).toFixed(6)}`}</div>
            </div>
          </div>
          <div className={styles.infoRow}>
            <div className={styles.infoCell}>
              <span>最高飞行速度</span>
              <div>{`垂直(${selEvidence?.lastFlightVerticalSpeed}m/s)，水平(${selEvidence?.lastFlightHorizontalSpeed}m/s)`}</div>
            </div>
            <div className={styles.infoCell}>
              <span>最低飞行高度</span>
              <div>{selEvidence?.lastFlightAltitude}m</div>
            </div>
          </div>
        </div>

        <div className={styles.recs}>
          <div className={styles.videoContainer}>
            {videoFile ? (
              // <video
              //   id="dewjlgjgwj"
              //   autoPlay="autoplay"
              //   loop="loop"
              //   controls
              //   width="100%"
              //   height="100%"
              //   muted="muted"
              // >
              //   <source src={videoFile} type="video/mp4" />
              // </video>
              <Player muted autoPlay playsInline>
                <source src={videoFile} />
              </Player>
            ) : (
              <div className={styles.vnodata} />
            )}
          </div>
          <div className={styles.specContainer}>
            {specFile || mainConf.mock ? (
              <CombineScan
                // units={units}
                visibleCharts={visibleCharts}
                viewOptions={{ axisX: false, toolBar: false, axisY: false }}
                segments={segments}
                onLoad={(e) => {
                  setterRef.current = e;
                }}
                //     threshold={20}
                //     onThresholdChange={(e) => {
                //       console.log("Threshold changed:::", e);
                //     }}
              />
            ) : (
              <div className={styles.vnodata} />
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default IdentifyInfo;

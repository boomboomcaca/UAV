import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import { Radio, InputNumber1, Loading } from "dui";
import { TaskStatus } from "ccesocket-webworker";
import parametersnamekey from "parametersnamekey";
// import DirectionButton from "./comopnents/directionButton/index.jsx";
import { ModuleCategory, ModuleState } from "../../../../utils/enums.js";
import config, { mainConf } from "../../../../config/index.js";
// import StreamedianPlayer from "./comopnents/streamedian/Index.jsx";
import JmuxerPlayer from "./comopnents/jmuxer/Index.jsx";
import MinusPlusButton from "./comopnents/minusPlusButton/Index.jsx";
import SvgComponent from "./comopnents/directionButton/SvgComponent.jsx";
import Switch from "./comopnents/switch/Index.jsx";
import styles from "./style.module.less";

// [
//   "停止",//   "焦距变大",//   "焦距变小",//   "焦点前调",//   "焦点后调",//   "光圈扩大",//   "光圈缩小",
//   "上",//   "下",//   "坐",//   "右",//   "右上",//   "右下",//   "左上",//   "左下",
// ];

// [0, 11, 12, 13, 14, 15, 16, 21, 22, 23, 24, 25, 26, 27, 28];

const directionCMD = {
  stop: 0,
  snapshot: 1,
  zoomPlus: 11,
  zoomMinus: 12,
  focusPlus: 13,
  focusMinus: 14,
  irisPlus: 15,
  irisMinus: 16,
  up: 21,
  down: 22,
  left: 23,
  right: 24,
};

/**
 *
 * @param {VideoMonitorProps} props
 * @returns
 */
const VideoMonitor = (props) => {
  const { videoServerUrl } = config();
  const { maxView, taskStatus, devices, onMaxView, onParamsChange } = props;
  // const { maxView, resizeChart, moduleState, taskStatus, devices } = props;
  const [maxMode, setMaxMode] = useState(false);
  const [speed, setSpeed] = useState(50);
  const options = [
    {
      label: "可见光",
      value: true,
    },
    {
      label: "热像",
      value: false,
    },
  ];

  const [trackType, setTrackType] = useState(true);
  const [laserOn, setLaserOn] = useState(false);
  const [isAutoTrack, setIsAutoTrack] = useState(true);
  const [identyRect, setIdentyRect] = useState(false);
  const [trackRect, setTrackRect] = useState(false);

  useEffect(() => {
    setMaxMode(maxView);
  }, [maxView]);

  useEffect(() => {
    console.log("video task status:::::::", taskStatus);
  }, [taskStatus]);

  /**
   * 监测设备列表
   * @type {Array<Array<Module>>}
   */
  const [deviceList, setDeviceList] = useState();

  useEffect(() => {
    if (devices && devices.length > 0) {
      const devs = devices.filter((d) =>
        d.moduleCategory.includes(ModuleCategory.recognizer)
      );
      setDeviceList(devs);
    }
  }, [devices]);

  // 设备故障信息
  const [errorTip, setErrorTip] = useState();
  useEffect(() => {
    if (deviceList && deviceList.length > 0) {
      console.log("rtst url:::", videoServerUrl, deviceList[0].rtspUrl);
      const okDevs = deviceList.filter((d) =>
        [ModuleState.fault, ModuleState.offline, ModuleState.disabled].includes(
          d.moduleState
        )
      );
      if (okDevs.length === deviceList.length) {
        setErrorTip("设备离线或故障");
      }
    }
  }, [deviceList]);

  return (
    <div className={styles.vmRoot}>
      <div className={`${styles.videoContainer} ${maxMode && styles.large}`}>
        {mainConf.mock ? (
          <video
            src="videoDemo.mp4"
            autoPlay="autoplay"
            loop="loop"
            width="100%"
            height="100%"
            muted="muted"
          />
        ) : (
          // <div>ddd</div>
          <JmuxerPlayer
            maxView={maxView}
            wsUrl={videoServerUrl}
            // rtspSrc={
            //   deviceList && deviceList.length > 0
            //     ? deviceList[0].rtspUrl
            //     : "rtsp://192.168.1.245:554/channel=0,stream=0"
            // }
            onTrack={(e) => {
              if (onParamsChange) {
                onParamsChange({
                  name: parametersnamekey.rect,
                  value: e,
                });
              }
            }}
            onItemClick={(e) => {
              // 截屏和录像
              if (e.action === "rec") {
                onParamsChange({
                  name: parametersnamekey.isRecording,
                  value: e.value,
                });
              } else {
                onParamsChange({
                  name: parametersnamekey.direction,
                  value: directionCMD.snapshot,
                });
              }
            }}
          />
        )}

        {/* 状态提示、loading等 */}
        {(errorTip || taskStatus !== TaskStatus.running) && !mainConf.mock && (
          <div className={styles.status} />
        )}
        {taskStatus === TaskStatus.starting && (
          <div className={styles.status}>
            <Loading type="colorful" />
          </div>
        )}
      </div>
      {maxMode && (
        <div className={styles.controls}>
          <div className={styles.directionCtl}>
            <SvgComponent
              width="100%"
              height="100%"
              fill="#FFFFFF"
              onChange={(e) => {
                console.log("onchange direction:::::", e);
                const { action } = e;
                switch (action) {
                  case "left":
                  case "right":
                  case "up":
                  case "down":
                    onParamsChange({
                      name: parametersnamekey.direction,
                      value: directionCMD[action],
                    });
                    break;
                  case "stop":
                    onParamsChange({
                      name: parametersnamekey.direction,
                      value: directionCMD[action],
                    });
                    break;
                  default:
                    onParamsChange({
                      name: parametersnamekey.stopTrack,
                      value: new Date().getTime(),
                    });
                    break;
                }
              }}
            />
          </div>
          <div className={styles.itemrows}>
            <div className={styles.row}>
              <span>焦距</span>
              <MinusPlusButton
                allowInput={false}
                onMinus={(e) => {
                  onParamsChange({
                    name: parametersnamekey.direction,
                    value: e ? directionCMD.stop : directionCMD.focusMinus,
                  });
                }}
                onPlus={(e) => {
                  onParamsChange({
                    name: parametersnamekey.direction,
                    value: e ? directionCMD.stop : directionCMD.focusPlus,
                  });
                }}
              />
            </div>
            <div className={styles.row}>
              <span>缩放</span>
              <MinusPlusButton
                allowInput={false}
                onMinus={(e) => {
                  onParamsChange({
                    name: parametersnamekey.direction,
                    value: e ? directionCMD.stop : directionCMD.zoomMinus,
                  });
                }}
                onPlus={(e) => {
                  onParamsChange({
                    name: parametersnamekey.direction,
                    value: e ? directionCMD.stop : directionCMD.zoomPlus,
                  });
                }}
              />
            </div>
            <div className={styles.row}>
              <span>光圈</span>
              <MinusPlusButton
                allowInput={false}
                onMinus={(e) => {
                  onParamsChange({
                    name: parametersnamekey.direction,
                    value: e ? directionCMD.stop : directionCMD.irisMinus,
                  });
                }}
                onPlus={(e) => {
                  onParamsChange({
                    name: parametersnamekey.direction,
                    value: e ? directionCMD.stop : directionCMD.irisPlus,
                  });
                }}
              />
            </div>
            <div className={styles.row}>
              <span>跟踪方式</span>
              <div>
                <Radio
                  theme="highLight"
                  options={options}
                  value={trackType}
                  onChange={(value) => {
                    setTrackType(value);
                    onParamsChange({
                      name: parametersnamekey.visibleLight,
                      value: value,
                    });
                  }}
                />
              </div>
            </div>
            <div className={styles.row}>
              <span>激光</span>
              <Switch
                labels={["关", "开"]}
                onChange={(e) => {
                  setLaserOn(e);
                  onParamsChange({
                    name: parametersnamekey.isLaser,
                    value: e,
                  });
                }}
                value={laserOn}
              />
            </div>
            <div className={styles.row}>
              <span>隐藏跟踪框</span>
              <div>
                <Switch
                  onChange={(e) => {
                    setTrackRect(e);
                    onParamsChange({
                      name: parametersnamekey.hideTrackRect,
                      value: e,
                    });
                  }}
                  value={trackRect}
                />
              </div>
            </div>
            <div className={styles.row}>
              <span>隐藏识别框</span>
              <div>
                <Switch
                  onChange={(e) => {
                    setIdentyRect(e);
                    onParamsChange({
                      name: parametersnamekey.hideIdentifyRect,
                      value: e,
                    });
                  }}
                  value={identyRect}
                />
              </div>
            </div>
            <div className={styles.row}>
              <span>转速</span>
              <MinusPlusButton
                minimum={0}
                maximum={255}
                value={speed}
                onChange={(e) => {
                  setSpeed(e);
                  onParamsChange({
                    name: parametersnamekey.speed,
                    value: e,
                  });
                }}
              />
            </div>
            <div className={styles.row}>
              <span>引导跟踪</span>
              <div>
                <Switch
                  labels={["关", "开"]}
                  onChange={(e) => {
                    setIsAutoTrack(e);
                    onParamsChange({
                      name: parametersnamekey.isAutoTrack,
                      value: e,
                    });
                  }}
                  value={isAutoTrack}
                />
              </div>
            </div>
            {/* <div className={styles.row}>
             
              <div className={styles.cell}>
                <span>AI模板</span>
                <div>
                  <Radio
                    options={options}
                    value={trackType}
                    onChange={(value) => {
                      setTrackType(value);
                      onParamsChange({
                        name: parametersnamekey.visibleLight,
                        value: value,
                      });
                    }}
                  />
                </div>
              </div>
            </div> */}
          </div>
        </div>
      )}

      {/* {deviceList &&
        ![ModuleState.fault, ModuleState.offline].includes(
          deviceList[0].moduleState
        ) &&
        !maxMode && (
          <div
            className={styles.mask}
            onClick={() => {
              if (onMaxView) {
                onMaxView("video");
              }
            }}
          />
        )} */}
      {/* 测试 */}
      {((!errorTip && taskStatus === TaskStatus.running) || mainConf.mock) &&
        !maxMode && (
          <div
            className={styles.mask}
            onClick={() => {
              if (onMaxView) {
                onMaxView("video");
              }
            }}
          />
        )}
    </div>
  );
};

VideoMonitor.defaultProps = {
  maxView: false,
  taskStatus: "",
  devices: undefined,
  onMaxView: () => {},
  onParamsChange: () => {},
};

VideoMonitor.prototype = {
  maxView: PropTypes.bool,
  taskStatus: PropTypes.string,
  devices: PropTypes.array,
  onMaxView: PropTypes.func,
  onParamsChange: PropTypes.func,
};

export default VideoMonitor;

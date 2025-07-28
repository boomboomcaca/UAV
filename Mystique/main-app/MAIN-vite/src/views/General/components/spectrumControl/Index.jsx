import React, { useState, useEffect } from "react";
import { Loading } from "dui";
import { TaskStatus } from "ccesocket-webworker";
import parametersnamekey from "parametersnamekey";
import { ModuleCategory, ModuleState } from "../../../../utils/enums.js";
import ReadyControl from "./ReadyControl/Index.jsx";
import { mainConf } from "../../../../config/index.js";
import styles from "./style.module.less";

/**
 *
 * @param {SpectrumControlProps} props
 * @returns
 */
const SpectrumControl = (props) => {
  // const { onDetail } = props;
  const { taskStatus, devices, onMaxView, maxView, onParamsChange } = props;
  const [showButton, setShowButton] = useState(false);
  const [running, setRunning] = useState(false);

  /**
   * 监测设备列表
   * @type {Array<Array<Module>>}
   */
  const [deviceList, setDeviceList] = useState();
  useEffect(() => {
    if (devices && devices.length > 0) {
      const devs = devices.filter((d) =>
        d.moduleCategory.includes(ModuleCategory.radioSuppressing)
      );
      setDeviceList(devs);
    }
  }, [devices]);

  // 设备故障信息
  const [errorTip, setErrorTip] = useState();
  useEffect(() => {
    if (deviceList && deviceList.length > 0) {
      console.log("压制设备列表：：：", deviceList);
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

  useEffect(() => {
    let mocker = () => {
      setTimeout(() => {
        setRunning(true);
        setTimeout(() => {
          setRunning(false);
          mocker();
        }, 6000);
      }, 5000);
    };
    if (mainConf.mock) mocker();
    return () => {
      mocker = undefined;
    };
  }, []);

  return (
    <div
      className={styles.controlRoot}
      onMouseEnter={() => {
        if (
          deviceList &&
          ![ModuleState.fault, ModuleState.offline].includes(
            deviceList[0].moduleState
          )
        )
          setShowButton(true);
      }}
      onMouseLeave={() => {
        setShowButton(false);
      }}
    >
      <div className={styles.content}>
        <svg>
          <defs>
            <clipPath id="in00dexBj01" clipPathUnits="objectBoundingBox">
              <path
                transform="scale(0.0001,0.0001)"
                d="M0 10.2703L8.18103 0L6.18293 7.43243L13 7.83784L2.01724 20L6.34146 10.5405L0 10.2703Z"
              />
            </clipPath>
          </defs>
        </svg>
        {running ? (
          <>
            <div className={styles.controling} />
            <div className={styles.controlingbg}>
              <div className={styles.lightning} />
            </div>
          </>
        ) : (
          <div className={styles.ctlbtn} />
        )}
        <div className={styles.tip}>{running ? "停止压制" : "开始压制"}</div>
        {/* 状态提示、loading等 */}
        {mainConf.mock ? (
          !maxView && (
            <div
              className={styles.mask}
              onClick={() => {
                setRunning(!running);
                // if (onMaxView) {
                //   onMaxView("monitoring");
                // }
              }}
            />
          )
        ) : (
          <>
            {(errorTip || taskStatus !== TaskStatus.running) && (
              <div className={styles.status}>{/* 当前设备离线或故障 */}</div>
            )}
            {taskStatus === TaskStatus.starting && (
              <div className={styles.status}>
                <Loading type="colorful" />
              </div>
            )}
            {!errorTip && taskStatus === TaskStatus.running && !maxView && (
              <div
                className={styles.mask}
                onClick={() => {
                  let run = !running;
                  setRunning(run);
                  if (onParamsChange) {
                    onParamsChange({
                      name: parametersnamekey.isOpenAll,
                      value: run,
                    });
                  }
                  // if (onMaxView) {
                  //   // onMaxView("monitoring");
                  // }
                }}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
};

export default SpectrumControl;

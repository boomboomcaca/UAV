import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
import { SegEditor, NewSegmentsEditor } from "searchlight-commons";
import { Checkbox, message, Table2, Loading, Button1 } from "dui";
// import {
//   CombineScan,
//   ChartTypes,
//   UnknownScan,
//   DemoGenerator,
// } from "spectrumcharts";

import { ListIcon } from "dc-icon";
import parametersnamekey from "parametersnamekey";
import { TaskStatus } from "ccesocket-webworker";

import { SpecIcon, RainIcon, PointIcon, TableIcon } from "./icons.jsx";
import RainPng from "./1.png";
import BearingPng from "./bearing.png";
import data from "./data.json";
import { getColumns, getData, demoSignals } from "./demo.jsx";
import { ModuleCategory, ModuleState } from "../../../../utils/enums.js";
import { ReactComponent as AutoRangeIcon } from "./autoRange.svg";
import {
  CombineScan,
  ChartTypes,
  DemoGenerator,
} from "../../../../components/charts/components/index";
import { mainConf } from "../../../../config/index.js";
import styles from "./style.module.less";

// /**
//  *
//  * @param {SpectrumMonitorProps} props
//  * @returns
//  */
const SpectrumMonitor = (props) => {
  const {
    maxView,
    taskStatus,
    devices,
    curFeature,
    onMaxView,
    segments,
    threshold,
    onParamsChange,
    onSignalSelect,
  } = props;

  const [maxMode, setMaxMode] = useState(false);
  const [segmentList, setSegmentList] = useState();

  useEffect(() => {
    console.log("seg ments :::::", segments);
    if (segments) setSegmentList(segments);
  }, [segments]);

  const setterRef = useRef();
  const mockerRef = useRef();

  const limit = {
    min: 20,
    max: 8000,
    stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  };

  const [visibleCharts, setVisibleCharts] = useState([
    ChartTypes.spectrum,
    ChartTypes.wbdf,
  ]);

  const [selSegment, setSelSegment] = useState();
  const selectHandle = (e) => {
    setSelSegment(e);
    if (e.flag) {
      setterRef.current.zoomToSegment(e.segmentIndex);
    } else {
      setterRef.current.resetZoom();
    }
  };

  // 设备故障信息
  const [errorTip, setErrorTip] = useState();

  /**
   * 监测设备列表
   * @type {Array<Array<Module>>}
   */
  const [deviceList, setDeviceList] = useState();
  useEffect(() => {
    if (devices && devices.length > 0) {
      const needTypes = [
        ModuleCategory.directionFinding,
        ModuleCategory.monitoring,
        ModuleCategory.decoder,
      ];
      const devs = devices.filter((d) => {
        let exist;
        d.moduleCategory.forEach((m) => {
          if (needTypes.includes(m)) {
            exist = m;
          }
        });
        return exist;
      });
      setDeviceList(devs);
    }
  }, [devices]);

  useEffect(() => {
    setMaxMode(maxView);
  }, [maxView]);

  const [allowEditSeg, setAllowEditSeg] = useState(true);

  useEffect(() => {
    if (mainConf.mock) {
      setTimeout(() => {
        if (setterRef.current && !mockerRef.current) {
          const segments1 = [
            {
              id: "123456",
              startFrequency: 88,
              stopFrequency: 108,
              stepFrequency: 25,
            },
            {
              id: "123457",
              startFrequency: 109,
              stopFrequency: 160,
              stepFrequency: 25,
            },
            {
              id: "123458",
              startFrequency: 200,
              stopFrequency: 300,
              stepFrequency: 50,
            },
          ];
          setSegmentList(segments1);
          setAllowEditSeg(true);
          setTimeout(() => {
            mockerRef.current = DemoGenerator(
              {
                frame: 12,
                type: "scan",
                segments: segments1,
              },
              (d) => {
                // 设置数据
                d.timestamp = new Date().getTime() * 1e5;
                setterRef.current.setData(d);
                const wbdf = { ...d };
                wbdf.azimuths = d.data.map(() => Math.random() * 360);
                wbdf.type = "dfscan";
                setterRef.current.setData(wbdf);
              }
            );
          }, 1000);
        }
      }, 800);
    }

    return () => {
      mockerRef.current?.dispose();
      mockerRef.current = undefined;
    };
  }, []);
  const [hasWbdf, setHasWbdf] = useState(false);

  useEffect(() => {
    console.log("device list change::::::--------", deviceList);
    if (deviceList && deviceList.length > 0) {
      const okDevs = deviceList.filter((d) =>
        [ModuleState.fault, ModuleState.offline, ModuleState.disabled].includes(
          d.moduleState
        )
      );
      if (okDevs.length === deviceList.length) {
        console.log("spectrum monirot errortip:::");
        setErrorTip("设备离线或故障");
      }
    }
  }, [deviceList]);

  useEffect(() => {
    if (deviceList && curFeature) {
      // 2023-7-26 默认监测（测向）站，陈帅说先找测向、再找监测、最后找运哨
      // 主设备使用使用运哨不变
      // 测向使用扫描测向，会使用宽带测向
      let defaultDevice = deviceList.find((d) =>
        d.moduleCategory.includes(ModuleCategory.directionFinding)
      );
      if (!defaultDevice) {
        defaultDevice = deviceList.find(
          (d) =>
            d.moduleCategory.includes(ModuleCategory.monitoring) &&
            !d.moduleCategory.includes(ModuleCategory.decoder)
        );
      }
      if (!defaultDevice) {
        defaultDevice = deviceList.find((d) =>
          d.moduleCategory.includes(ModuleCategory.decoder)
        );
      }

      if (defaultDevice.supportedFeatures.includes("scandf")) {
        setHasWbdf(defaultDevice.supportedFeatures.includes("scandf"));
      }
      console.log("defaultDevice：：：：：", defaultDevice);
      onParamsChange({
        name: "defaultDevice",
        value: { id: defaultDevice.id, location: defaultDevice.location },
      });
      if (defaultDevice.model === "RxDrone" && !mainConf.mock) {
        // 此设备不能修改频段
        setAllowEditSeg(false);
        setTimeout(() => {
          onParamsChange({
            name: parametersnamekey.scanSegments,
            value: [
              {
                name: "无人机频段（2.4G）",
                startFrequency: 0.1,
                stopFrequency: 4608,
                stepFrequency: 100,
              },
              {
                name: "无人机频段（5.8G）",
                startFrequency: 0.1,
                stopFrequency: 2304,
                stepFrequency: 100,
              },
            ],
          });
        }, 600);
      }
    }
  }, [deviceList, curFeature, onParamsChange]);

  // useEffect(() => {
  //   if (setterRef.current) {
  //     setTimeout(() => {
  //       setterRef.current.resize();
  //     }, 300);
  //   }
  // }, [resizeChart, visibleCharts, maxView]);

  const getOptions = (hasWbdf, hasRain) => {
    const options = [
      {
        label: "频谱图",
        value: ChartTypes.spectrum,
        icon: <SpecIcon />,
      },
    ];
    if (hasRain) {
      options.push({
        label: "瀑布图",
        value: ChartTypes.rain,
        icon: (
          <div>
            <img src={RainPng} width={24} height={20} />
          </div>
        ),
      });
    }
    if (hasWbdf) {
      options.push({
        label: "示向度图",
        value: ChartTypes.wbdf,
        icon: (
          <div>
            <img src={BearingPng} width={24} height={20} />
          </div>
        ),
      });
    }
    options.push({
      label: "信号表",
      value: "list",
      icon: (
        <div
          style={{
            display: "flex",
            flexDirection: "row",
            alignItems: "center",
          }}
        >
          <ListIcon iconSize={24} color="var(--theme-primary)" />
        </div>
      ),
    });
    return options;
  };

  // const [signalList, setSingalList] = useState(demoSignals);
  const [signalTable, setSignalTable] = useState();
  useEffect(() => {
    if (segmentList && segmentList.length > 0 && maxMode) {
      /**
       *
       * @param {Array<{segmentIdx:Number,freqIdxs:[Number,Number],azimuth:Number}>} e
       */
      window.viewSignals = (e) => {
        const tableList = [];
        e.forEach((item, index) => {
          const seg = segmentList[item.segmentIdx];
          if (seg) {
            const bandOffset = item.freqIdxs[1] - item.freqIdxs[0] + 1;
            const freqOffset = Math.round(item.freqIdxs[0] + bandOffset / 2);
            const bandwidth = seg.stepFrequency * bandOffset;
            const frequency =
              seg.startFrequency + (seg.stepFrequency * freqOffset) / 1000;
            tableList.push({
              id: item.guid,
              no: index + 1,
              bandwidth,
              frequency,
              azimuth: item.azimuth,
            });
          }
        });
        setSignalTable(tableList);
      };
    } else {
      window.viewSignals = () => {};
    }
  }, [segmentList, maxMode]);

  return (
    <div className={styles.specmRoot}>
      <div className={styles.content}>
        <div className={styles.chartContainer}>
          {maxMode && (
            <div className={styles.segTitle}>
              <Button1
                style={{
                  margin: 0,
                  padding: 0,
                  height: "40px",
                  width: "36px",
                  display: "flex",
                  justifyContent: "center",
                  alignItems: "center",
                }}
              >
                <AutoRangeIcon />
              </Button1>
              <div className={styles.b}>
                <NewSegmentsEditor
                  selectSegment={selSegment}
                  selectedChange={selectHandle}
                  // deleteSegmentFunc={deleteHandle}
                  segmentList={segmentList}
                  editable={false}
                  onlyName={!allowEditSeg}
                />
              </div>
              {allowEditSeg && (
                <SegEditor
                  // disabled={isSelect.flag}
                  segmentList={segmentList}
                  limit={limit}
                  getSegmentList={(e) => {
                    console.log("seg edit:::", e);
                    if (onParamsChange) {
                      onParamsChange({
                        name: parametersnamekey.scanSegments,
                        value: e,
                      });
                    }
                  }}
                  data={data}
                />
              )}
            </div>
          )}
          <div className={styles.container}>
            {(visibleCharts.includes(ChartTypes.spectrum) ||
              visibleCharts.includes(ChartTypes.rain) ||
              visibleCharts.includes(ChartTypes.wbdf)) && (
              <div style={{ flex: 1 }}>
                {/* {allowEditSeg ? ( */}
                <CombineScan
                  visibleCharts={visibleCharts}
                  axisX={maxMode ? {} : ""}
                  axisY={maxMode ? { inside: false } : ""}
                  chartGap={maxMode ? 4 : 1}
                  padding={maxMode ? "" : "0"}
                  // showBand={maxMode}
                  showCursor={maxMode}
                  showThreshold={maxMode}
                  segments={segmentList}
                  threshold={threshold}
                  useGPU={false}
                  onLoad={(e) => {
                    console.log("scan chart loaded:::");
                    setterRef.current = e;
                    setTimeout(() => {
                      setterRef.current.setSeriesVisible(["max", "thr"]);
                    }, 1000);

                    window.specSetter = e;
                  }}
                  onBandChange={(e) => {
                    console.log("on band change:::", e);
                    //  更新参数
                    onParamsChange({
                      name: parametersnamekey.markScanDf,
                      value: `${e.id}|${e.segmentIndex}|${e.segmentOffset}|${e.segmentOffset1}`,
                    });
                  }}
                  onThresholdChange={(e) => {
                    console.log("thrChange:::", e);
                    onParamsChange({
                      name: parametersnamekey.levelThreshold,
                      value: e,
                    });
                  }}
                  onSignalSelect={(e) => onSignalSelect(e)}
                />
                {/* ) : (
                  <UnknownScan
                    segments={segmentList}
                    onLoad={(e) => {
                      setterRef.current = e;
                      window.specSetter = e;
                    }}
                    viewOptions={
                      maxMode
                        ? {
                            axisX: true,
                            axisY: { inside: true },
                            toolbar: true,
                          }
                        : {}
                    }
                  />
                )} */}
              </div>
            )}
            {visibleCharts.includes("list") && (
              <div className={styles.signalList}>
                <Table2
                  // rowKey="kid"
                  columns={getColumns()}
                  showSelection={false}
                  // selectRowIndex={1}
                  options={{ canRowSelect: true, bordered: false }}
                  data={signalTable}
                />
              </div>
            )}
          </div>
          {maxMode && allowEditSeg && (
            <div className={styles.footer}>
              <Checkbox.Group
                options={getOptions(hasWbdf, allowEditSeg)}
                value={visibleCharts}
                sort={false}
                className={styles.cheboxes}
                optionCls={styles.cheboxItem}
                onChange={(e) => {
                  // 只能显示2个，先进先出
                  if (e.length > 2) {
                    const outItem = visibleCharts[0];
                    const newItems = e.filter((it) => it !== outItem);
                    setVisibleCharts(newItems);
                  } else if (e.length === 0) {
                    message.info("至少保留一个图表");
                  } else {
                    setVisibleCharts(e);
                  }
                }}
              />
            </div>
          )}
          {/* 状态提示、loading等 */}
          {mainConf.mock ? (
            !maxMode && (
              <div
                className={styles.mask}
                onClick={() => {
                  if (onMaxView) {
                    onMaxView("monitoring");
                  }
                }}
              />
            )
          ) : (
            <>
              {(errorTip || taskStatus !== TaskStatus.running) &&
                !mainConf.mock && (
                  <div className={styles.status}>
                    {/* 当前设备离线或故障 */}
                  </div>
                )}
              {taskStatus === TaskStatus.starting && (
                <div className={styles.status}>
                  <Loading type="colorful" />
                </div>
              )}
              {!errorTip && taskStatus === TaskStatus.running && !maxMode && (
                <div
                  className={styles.mask}
                  onClick={() => {
                    if (onMaxView) {
                      onMaxView("monitoring");
                    }
                  }}
                />
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
};

SpectrumMonitor.defaultProps = {
  maxView: false,
  taskStatus: "",
  devices: [],
  onParamsChange: () => {},
  threshold: 30,
  onSignalSelect: () => {},
  // curFeature: undefined,
};

SpectrumMonitor.propTypes = {
  maxView: PropTypes.bool,
  taskStatus: PropTypes.string,
  devices: PropTypes.array,
  onParamsChange: PropTypes.func,
  threshold: PropTypes.number,
  onSignalSelect: PropTypes.func,
  // curFeature: PropTypes.any,
};

export default SpectrumMonitor;

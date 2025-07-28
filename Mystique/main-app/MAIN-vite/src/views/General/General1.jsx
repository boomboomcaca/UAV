import React, { useState, useRef, useEffect, useCallback } from "react";
import PropTypes from "prop-types";
import { message } from "dui";
import parametersnamekey from "parametersnamekey";
import Settings from "searchlight-settings";
import { Route, Switch, Link, useLocation, useHistory } from "react-router-dom";
import SpectrumMonitor from "./components/spectrumMonitor/Index.jsx";
import VideoMonitor from "./components/videoMonitor/Index.jsx";
import SpectrumControl from "./components/spectrumControl/Index.jsx";
import RadarMonitor from "./components/radarMonitor/Index.jsx";
import Alarms from "./components/alarms/Index.jsx";
import useNrmState from "@/hooks/useNrmState.jsx";
import useSegment from "@/hooks/useSegment.jsx";
import PlaneDetails from "./components/planeDetails/Index.jsx";
import MapControl from "../../components/mapControl/Index.jsx";
import { getAllDeviceInfo } from "../../api/business.js";
import { getData, realMock } from "./components/alarms/demo.jsx";
import Header from "../../components/Header/Index.jsx";
import { parseSearchParams } from "../../utils/publicFunc";
import { mainConf } from "../../config/index.js";
import SwitchMode from "./components/switchMode/Index.jsx";
import myTurf from "../../utils/myTurf";
import styles from "./style1.module.less";

const General = () => {
  // 接收route传值
  const location = useLocation();
  const history = useHistory();
  const [large, setLarge] = useState();
  const [fit, setFit] = useState(false);
  // const location = useLocation();
  const [map, setMap] = useState();
  const mapRef = useRef();

  const [itemContainers, setItemContainers] = useState([]);
  // const [headIcons, setHeadIcons] = useState([MenuType.HOME]);
  const [paramsVisible, setParamsVisible] = useState(false);
  const [showPlaneDetail, setShowPlaneDetail] = useState(false);
  const defaultSpecDevice = useRef();
  const selSignalRef = useRef();
  const prevSignalsRef = useRef();

  useEffect(() => {
    const params = parseSearchParams(
      window.location.search || window.location.hash
    );
    const { fit, large } = params;
    setLarge(large);
    setFit(fit);
  }, [location]);

  useEffect(() => {
    if (!fit) {
      setShowPlaneDetail(false);
      setParamsVisible(false);
    }
    if (map) {
      setTimeout(() => {
        map.resize();
      }, 500);
    }
  }, [map, fit]);

  const [devices, setDevices] = useState([]);
  const [devTypes, setDevTypes] = useState(
    mainConf.mock
      ? ["monitoring", "recognizer", "radar", "radioSuppressing"]
      : []
  );

  useEffect(() => {
    getAllDeviceInfo((e) => {
      setDevices(e.deviceModules);
      // 监测和测向放到一起
      const tps = e.moduleCategory.map((mc) =>
        mc === "directionFinding" ? "monitoring" : mc
      );
      setDevTypes([...new Set(tps)]);
    });
    let mocker;
    if (mainConf.mock) {
      setTimeout(() => {
        mocker = realMock((alarm) => {
          // 处理报警数据
          window.onAlarm(alarm);
          window.alarmToHome(alarm);

          if (alarm.alarmStatus === "allClear") {
            isNewAlarmRef.current = true;
            curAlarmRef.current.forEach((it) => alarmsRef.current.push(it));
            curAlarmRef.current = [];
          }

          // 缓存轨迹
          if (alarm.details) {
            curAlarmRef.current = alarm.details;
            cacheGPS(alarm.details);
            alarmsRef.current = alarm.details || [];
            // 更新地图位置
            const tracks = [];
            for (let key in uavTrace.current) {
              tracks.push(uavTrace.current[key]);
            }
            mapRef.current.drawPlanes(tracks);
            if (alarm.mockRestart) {
              uavTrace.current = {};
            }
          }
        });
      }, 4000);
    }

    return () => {
      if (mocker) clearInterval(mocker);
    };
  }, []);

  useEffect(() => {
    // if (devTypes && devTypes.length > 0) {
    const deviceTypes = [
      "1",
      "radar",
      "monitoring",
      "cracker",
      "uavDecoder",
      "decoder", // ads-b
      "radioSuppressing",
    ];
    const validDev = devTypes.filter(
      (dev) =>
        deviceTypes.includes(dev) &&
        dev !== "recognizer" &&
        dev !== "uavDecoder"
    );

    const count = deviceTypes.length;
    let startIndex = 0;
    if (validDev.length < count) {
      startIndex = Math.floor((count - validDev.length) / 2);
    }

    const containers = [];
    const gapOffset = 8;
    const itmHeight = 95;
    const itemOffset = validDev.length % 2 === 1 ? itmHeight / 3 : 0;
    console.log("validDev.length", validDev, validDev.length, itemOffset);
    const totalHeight =
      itmHeight * count + gapOffset * (count - 1) - itemOffset;
    for (let i = 0; i < count; i += 1) {
      containers.push({
        top: `calc((95% - ${totalHeight}px) / 2 + ${
          (itmHeight + gapOffset) * i
        }px)`,
      });
    }
    deviceTypes.forEach((tp) => {
      if (validDev.includes(tp)) {
        containers[startIndex].moduleCategory = tp;
        startIndex += 1;
      }
    });
    // validDev.forEach((item) => {
    //   containers[startIndex].moduleCategory = item;
    //   startIndex += 1;
    // });
    // ------- test ------
    // ["1", "2", "monitoring", "radar", "radioSuppressing", "0", "0"].forEach(
    //   (item, index) => {
    //     if (validDev.includes(item)) containers[index].moduleCategory = item;
    //     // startIndex += 1;
    //   }
    // );
    // ------- test ------
    console.log("---------------------", validDev);
    setItemContainers(containers);
    // }
  }, [devTypes]);

  const initialize = (params) => {
    if (params) {
    }
  };

  const notiFilterFn = (noti) => {
    if (noti.type === "compass") {
      // PubSub.publish(CompassToken, noti);
    }
  };
  const uavTrace = useRef({});

  const cacheGPS = (uavs) => {
    uavs.forEach((item) => {
      const curLocation = [item.droneLongitude, item.droneLatitude];
      if (!uavTrace.current[item.droneSerialNum]) {
        const cacheData = { ...item };
        cacheData.id = item.droneSerialNum;
        cacheData.type = "uav";
        cacheData.coordinates = [curLocation];
        // cacheData.description = ""
        uavTrace.current[item.droneSerialNum] = cacheData;
      } else {
        uavTrace.current[item.droneSerialNum].coordinates.push(curLocation);
        // uavTrace.current[item.DroneSerialNum].description=""
      }
      uavTrace.current[item.droneSerialNum].pilot = [
        item.pilotLongitude,
        item.pilotLatitude,
      ];
    });
  };
  const mockBearingRef = useRef(false);
  const startMockRef = useRef(false);
  const preMockAngle = useRef(-1);

  const drawBearing_old = (angle) => {
    if (angle >= 0) {
      if (angle > 180) {
        angle = angle - 360;
      }
      const bearing2 = myTurf.destination(
        defaultSpecDevice.current.location,
        10,
        angle
      );
      mapRef.current.drawBearing([
        {
          coordinates: [
            defaultSpecDevice.current.location,
            bearing2.geometry.coordinates,
          ],
          color: "#00FF00",
          lineWidth: mockBearingRef.current ? 2 : 4,
        },
      ]);
    } else {
      mapRef.current.drawBearing([]);
    }
  };

  /**
   *
   * @param {Array<{azimuth:Number,segmentIdx:Number,freqIndxs:{startFreqIdx:Number,stopFreqIdx:number}}>} bearings
   */
  const drawBearing = (bearings) => {
    const drawDatas = [];
    if (bearings) {
      bearings.forEach((item) => {
        const id =
          item.guid ||
          item.Guid ||
          `${item.segmentIdx}-${item.freqIdxs[0]}-${item.freqIdxs[1]}`;
        let angle = item.azimuth;
        if (angle >= 0) {
          if (angle > 180) {
            angle = angle - 360;
          }
          const bearing2 = myTurf.destination(
            defaultSpecDevice.current.location,
            10,
            angle
          );
          const selId =
            selSignalRef.current?.guid || selSignalRef.current?.Guid;
          if (id !== selId) {
            drawDatas.push({
              coordinates: [
                defaultSpecDevice.current.location,
                bearing2.geometry.coordinates,
              ],
              color: "#00FF00",
              lineWidth: 2,
            });
          }
        }
      });
    }
    if (selSignalRef.current) {
      // 处理选中
      if (prevSelDFind.current > -1) {
        const bearing21 = myTurf.destination(
          defaultSpecDevice.current.location,
          10,
          prevSelDFind.current > 180
            ? prevSelDFind.current - 360
            : prevSelDFind.current
        );
        drawDatas.push({
          coordinates: [
            defaultSpecDevice.current.location,
            bearing21.geometry.coordinates,
          ],
          color: "#ff4c2a",
          lineWidth: 4,
        });
      }
    }
    mapRef.current.drawBearing(drawDatas);
  };

  // 是否为新的报警
  const isNewAlarmRef = useRef(true);
  const curAlarmRef = useRef([]);
  const alarmsRef = useRef([]);
  // 选中信号的示向度
  const prevSelDFind = useRef(-1);

  const handleCCEData = (CCEdata) => {
    if (CCEdata) {
      CCEdata.dataCollection.forEach((item) => {
        if (item.type === "scan" || item.type === "dfscan") {
          if (item.type === "dfscan") {
            // const overZero = item.azimuths.find((a) => a != -1);
            // if (overZero >= 0) console.log(item);
          }
          // 处理频谱图
          if (window.specSetter) {
            window.specSetter.setData(item);
          }
        } else if (item.type === "securityAlarm") {
          console.log("rec alarms:::", item);
          // 处理报警数据
          window.onAlarm(item);
          window.alarmToHome(item);
          if (item.alarmStatus === "allClear") {
            isNewAlarmRef.current = true;
            curAlarmRef.current.forEach((it) => alarmsRef.current.push(it));
            curAlarmRef.current = [];
          }

          // 缓存轨迹
          if (item.details) {
            curAlarmRef.current = item.details;
            if (item.details.length > 0) cacheGPS(item.details);
            else uavTrace.current = {};
            alarmsRef.current = item.details || [];
            // 20230729 使用运哨mock  bearing
            if (alarmsRef.current.length > 0) {
              const angle = alarmsRef.current[0].yawAngle;
              preMockAngle.current = angle;
              if (mockBearingRef.current) {
                drawBearing_old(angle);
              }
            }
            // 更新地图位置
            const tracks = [];
            for (let key in uavTrace.current) {
              tracks.push(uavTrace.current[key]);
            }
            mapRef.current.drawPlanes(tracks);
          }
        } else if (item.type === "dfind") {
          // 绘制示向线
          let angle = item.azimuth;
          if (prevSelDFind.current !== angle) prevSelDFind.current = angle;
          if (selSignalRef.current) {
            drawBearing();
          }
        } else if (item.type === "signalsList") {
          if (!mockBearingRef.current) {
            if (
              prevSignalsRef.current &&
              prevSignalsRef.current.length === 0 &&
              item.signals.length == 0
            )
              return;
            window.specSetter.setData(item);
            window.viewSignals(item.signals);
            drawBearing(item.signals);
            prevSignalsRef.current = item.signals;
          }
        }
      });
    }
  };

  const {
    taskInfoRef,
    nodeSelectorRef,
    rootDomId,
    taskRef,
    taskStatus,
    moreParam,
    setMoreParam,
    taskInfoShow,
    setTaskInfoShow,
    taskInfo,
    features,
    selFeature,
    parameterItems,
    setParameterItems,
    stationSeling,
    setStationSeling,
    dataListShow,
    setDataListShow,
    notify,
    setNotify,
    setIsCanReport,
    startCCE,
  } = useNrmState(initialize, handleCCEData, notiFilterFn);

  useEffect(() => {
    if (notify && notify.type === "error") {
      message.error(notify.msg);
    }
  }, [notify]);

  const {
    segsRef,
    segparametersRef,
    compIndexRef,
    segments,
    segIndex,
    setSegIndex,
    compIndex,
    setCompIndex,
    segpage,
    setsegpage,
  } = useSegment(parameterItems);
  const [defaultThr, setDefaultThr] = useState(10);
  useEffect(() => {
    console.log("parameter changed:::", parameterItems);
    if (parameterItems) {
      const thrItem = parameterItems.find(
        (p) => p.name === parametersnamekey.levelThreshold
      );
      if (thrItem) {
        setDefaultThr(thrItem.value);
      }
    }
  }, [parameterItems]);

  useEffect(() => {
    console.log("selected feature changed:::", selFeature);
    if (selFeature && !mainConf.mock) {
      if (selFeature === "none") {
        message.warning("请先在【设备管理】中配置管控站点和设备");
      } else {
        // setCurFeature(JSON.parse(JSON.stringify(selFeature)));
        setTimeout(() => {
          startCCE();
        }, 1500);
      }
    }
  }, [selFeature]);

  // 修改segments的唯一汇聚点
  const saveSegments = (newSegs) => {
    if (
      JSON.stringify(newSegs) === JSON.stringify(segments) ||
      !nodeSelectorRef.current
    ) {
      return;
    }
    // segment变化 生成报表置灰
    setIsCanReport(false);
    // 修复选中
    if (segIndex > newSegs.length - 1) {
      setSegIndex(0);
    }
    // 修复聚焦
    if (compIndex > newSegs.length - 1) {
      setCompIndex(newSegs.length === 1 ? -1 : 0);
    }
    // 修复分页
    if (newSegs.length < 5 && segpage === 2) {
      setsegpage(1);
    }
    // 处理频段门限值
    // const nneThr = new Array(newSegs.length).fill(6);
    // setthrVal(nneThr);
    // UPDATE SCANSEGMENTS
    // taskRef.current?.setParameters?.([
    //   { name: "scanSegments", parameters: newSegs },
    // ]);
    // 恶心的转格式
    // const lowParam = lowFormatIn(newSegs, segparametersRef.current);
    // nodeSelectorRef.current?.updateParameter("scanSegments", newSegs);
    console.log("update scan segmetns:::", newSegs);
    nodeSelectorRef.current.updateParameter(
      parametersnamekey.scanSegments,
      newSegs
    );
  };

  const [selPalne, setSelPlane] = useState({});

  const onParamsChange = useCallback((e) => {
    if (e.name === parametersnamekey.scanSegments) {
      saveSegments(e.value);
    } else if (e.name === "defaultDevice") {
      defaultSpecDevice.current = e.value;
    } else {
      if (e.name === parametersnamekey.markScanDf) {
        startMockRef.current = true;
      }
      nodeSelectorRef.current.updateParameter(e.name, e.value);
    }
  }, []);

  return (
    <div
      className={styles.generalRoot}
      style={{
        position: fit ? "fixed" : "absolute",
        zIndex: fit ? 99 : undefined,
      }}
    >
      {fit && (
        <Header
          title="综合管控"
          moreVisible={large === "monitoring"}
          onBack={() => {
            history.goBack();
          }}
          onMore={() => setParamsVisible(true)}
        />
      )}
      <div className={`${styles.container} ${fit && styles.containerFit}`}>
        <div className={`${styles.content} ${fit && styles.fit}`}>
          <div
            className={`${styles.mapCon} ${fit && styles.fit} ${
              large && styles.large
            }`}
          >
            <MapControl
              regionBar={fit}
              devices={devices}
              zoom2Fit={!fit}
              navigator={fit}
              legend={fit}
              resize={`resize_${large}_${fit}`}
              onLoaded={(map) => {
                mapRef.current = map;
                setMap(map);
              }}
              onSelectFeature={(e) => {
                console.log("sel :::", e);
                if (e.e && e.type === "plane") {
                  let sel = alarmsRef.current.find(
                    (a) => a.droneSerialNum === e.e
                  );
                  if (!sel) {
                    sel = curAlarmRef.current.find(
                      (a) => a.droneSerialNum === e.e
                    );
                  }
                  setSelPlane(sel);
                  // 查找详情数据，显示详情面板
                  setShowPlaneDetail(true);
                }
              }}
            />
          </div>
          {selFeature !== "none" && devTypes.includes("recognizer") && (
            <div
              className={`${styles.videoCon} ${fit && styles.fit} ${
                large === "video" && styles.large1
              } ${large && large !== "video" && styles.large0}`}
            >
              <VideoMonitor
                maxView={large === "video"}
                taskStatus={taskStatus}
                devices={devices}
                onParamsChange={(e) => {
                  console.log("update parameters:::", e);
                  nodeSelectorRef.current.updateParameter(e.name, e.value);
                }}
                onMaxView={(e) => {
                  if (large) history.replace(`/index?fit=true&large=${e}`);
                  else history.push(`/index?fit=true&large=${e}`);
                }}
              />
            </div>
          )}
          {itemContainers.map((item, index) => {
            if (item.moduleCategory) {
              return (
                <div
                  className={`${styles.itemcon} ${fit && styles.fit} ${
                    large === item.moduleCategory && styles.large
                  }`}
                  style={
                    large === item.moduleCategory
                      ? null
                      : {
                          top: item.top,
                          right: fit ? "32px" : "-8px",
                          width: fit ? "117px" : "145px",
                          height: "95px",
                          // marginTop: index > 0 ? "8px" : undefined,
                          // monitoring ,radar, radioSuppressing
                        }
                  }
                >
                  <div className={styles.itemcontent}>
                    {item.moduleCategory === "monitoring" && (
                      <SpectrumMonitor
                        resizeChart={`${fit}_${large}`}
                        taskStatus={taskStatus}
                        curFeature={selFeature}
                        devices={devices}
                        maxView={large === item.moduleCategory}
                        threshold={defaultThr}
                        onMaxView={(e) => {
                          if (large)
                            history.replace(`/index?fit=true&large=${e}`);
                          else history.push(`/index?fit=true&large=${e}`);
                        }}
                        segments={mainConf.mock ? undefined : segments}
                        onParamsChange={onParamsChange}
                        onSignalSelect={(e) => {
                          console.log("signal select:::", e);
                          if (e) {
                            const signal = prevSignalsRef.current.find(
                              (s) => s.guid === e || s.Guid === e
                            );
                            if (signal) {
                              selSignalRef.current = signal;
                              console.log(signal);
                              // item.freqIdxs[1] - item.freqIdxs[0] + 1
                              // TODO 下发参数
                              nodeSelectorRef.current.updateParameter(
                                parametersnamekey.markScanDf,
                                `${signal.guid}|${signal.segmentIdx}|${signal.freqIdxs[0]}|${signal.freqIdxs[1]}`
                              );
                            } else selSignalRef.current = null;
                          } else selSignalRef.current = null;
                          drawBearing(prevSignalsRef.current);
                          mapRef.current.setCenter(
                            defaultSpecDevice.current.location
                          );
                        }}
                      />
                    )}
                    {item.moduleCategory === "radar" && (
                      <RadarMonitor
                        maxView={large === item.moduleCategory}
                        // showList={fit && (!large || large === "radar")}
                        taskStatus={taskStatus}
                        devices={devices}
                        onMaxView={(e) => {
                          if (large)
                            history.replace(`/index?fit=true&large=${e}`);
                          else history.push(`/index?fit=true&large=${e}`);
                        }}
                      />
                    )}
                    {item.moduleCategory === "radioSuppressing" && (
                      <SpectrumControl
                        taskStatus={taskStatus}
                        devices={devices}
                        onParamsChange={(e) => {
                          console.log("update parameters:::", e);
                          nodeSelectorRef.current.updateParameter(
                            e.name,
                            e.value
                          );
                        }}
                      />
                    )}
                  </div>
                </div>
              );
            }
            return null;
          })}

          <div className={`${styles.alarmCon} ${large && styles.large}`}>
            {/* <span /> */}
            <Alarms
              fit={fit}
              onSelectChange={(id) => {
                if (map) {
                  map.setPlaneSelect(id);
                }
              }}
            />
            {/* <span /> */}
          </div>
          <SwitchMode />
        </div>
      </div>

      <Settings
        visible={paramsVisible}
        exceptParamKeys={[parametersnamekey.scanSegments]}
        // 设备参数
        parameters={parameterItems}
        // 参数变更
        onValueChanged={(e) => {
          console.log("onValueChanged", e);
          nodeSelectorRef.current.updateParameter(e.name, e.value);
        }}
        onHide={() => {
          setParamsVisible(false);
        }}
      />
      <PlaneDetails
        show={fit && showPlaneDetail}
        detailInfo={selPalne}
        className={`${styles.detailinfo} ${large && styles.large}`}
        onClose={() => setShowPlaneDetail(false)}
      />
    </div>
  );
};

General.defaultProps = {};

General.propTypes = {};

export default General;

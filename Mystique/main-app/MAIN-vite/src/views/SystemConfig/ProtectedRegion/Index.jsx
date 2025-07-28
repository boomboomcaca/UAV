import React, { useState, useRef, useEffect } from "react";
import PropTypes from "prop-types";
import MapControl, {
  RegionType,
} from "../../../components/mapControl/Index.jsx";
import { Modal, message, Radio, InputNumber } from "dui";
import turf from "../../../utils/myTurf";
import StepWizard from "./StepWizard/Index.jsx";
import { getAllDeviceInfo } from "../../../api/business.js";
import styles from "./index.module.less";

const ProtectedRegion = (props) => {
  const mapInstanceRef = useRef();

  const [showModal, setShowModal] = useState(false);
  const [regionConfig, setRegionConfig] = useState();
  // 当前用户绘制的区域
  const usrRegionRef = useRef();
  const [stepValue, setStepValue] = useState({ index: 0 });
  const [editType, setEditTYpe] = useState("地图绘制");

  const regionNames = ["region0", "region2", "region3", "region5"];
  const cacheRegions = (regionConfig, stepValue) => {
    const newRegions = regionConfig ? { ...regionConfig } : {};
    // 当前编辑的
    let usrRegion = usrRegionRef.current[0];
    const isPoint = usrRegion.geometry.type === "Point";
    if (stepValue.index === 0) {
      if (isPoint) {
        newRegions.center = usrRegion.geometry.coordinates;
        // 转换为面
        usrRegion = turf.circle(newRegions.center, 0.025);
      } else {
        const polygon = turf.polygon(usrRegion.geometry.coordinates);
        const centroid = turf.centroid(polygon);
        newRegions.center = centroid.geometry.coordinates;
      }
    }

    newRegions[regionNames[stepValue.index]] = { feature: usrRegion };
    const regConfigString = JSON.stringify(newRegions);
    const newObj = JSON.parse(regConfigString);
    setRegionConfig(newObj);
  };

  const asyncByRegions0 = () => {
    const newRegions = regionConfig ? { ...regionConfig } : {};
    const region0 = newRegions.region0;
    let feature = region0.feature;
    const isPoint = feature.geometry.type === "Point";
    if (isPoint) {
      let center = feature.geometry.coordinates;
      feature = turf.circle(center, 0.025);
      region0.feature = feature;
      newRegions[RegionType.region2] = {
        feature: turf.circle(center, 2),
        outRadius: 2000,
        alti: 1000,
        type: "refer", // 'refter' 保护区外延 'custom' 自定义
      };
      newRegions[RegionType.region3] = {
        feature: turf.circle(center, 3),
        outRadius: 3000,
        alti: 1000,
        type: "refer", // 'refter' 保护区外延 'custom' 自定义
      };
      newRegions[RegionType.region5] = {
        feature: turf.circle(center, 5),
        outRadius: 5000,
        alti: 1000,
        type: "refer", // 'refter' 保护区外延 'custom' 自定义
      };
    } else {
      const polygon = turf.polygon(feature.geometry.coordinates);
      const reg2 = turf.buffer(polygon, 2, { steps: 4 });
      reg2.id = `${RegionType.region2}${String(Math.random()).slice(2, 6)}`;
      const reg3 = turf.buffer(polygon, 3, { steps: 8 });
      reg3.id = `${RegionType.region3}${String(Math.random()).slice(2, 6)}`;
      const reg5 = turf.buffer(polygon, 5, { steps: 16 });
      reg5.id = `${RegionType.region5}${String(Math.random()).slice(2, 6)}`;
      newRegions[RegionType.region2] = { feature: reg2 };
      newRegions[RegionType.region3] = { feature: reg3 };
      newRegions[RegionType.region5] = { feature: reg5 };
    }
    const regConfigString = JSON.stringify(newRegions);
    const newObj = JSON.parse(regConfigString);
    setRegionConfig(newObj);
  };

  const saveRegions = () => {
    const regConfigString = JSON.stringify(regionConfig);
    localStorage.setItem("regionconfig", regConfigString);
    message.success("保存成功");
  };

  const loadConfigFromStorage = () => {
    const regConfig = localStorage.getItem("regionconfig");
    if (regConfig) {
      const regionConfig = JSON.parse(regConfig);
      setRegionConfig(regionConfig);
    } else {
      setRegionConfig(undefined);
    }
  };

  useEffect(() => {
    loadConfigFromStorage();
  }, []);

  const initToolBar = (show) => {
    console.log("init toolar:::::", show);
    if (mapInstanceRef.current)
      mapInstanceRef.current.showToolbar(
        show
          ? {
              polygon: "绘面",
              point: "标点",
            }
          : undefined,
        true
      );
  };

  useEffect(() => {
    initToolBar(editType === "地图绘制");
  }, [editType]);

  const [devices, setDevices] = useState([]);

  useEffect(() => {
    getAllDeviceInfo((e) => {
      setDevices(e.deviceModules);
    });
  }, []);

  return (
    <div className={styles.protectRoot}>
      <div className={styles.mapContainer}>
        <MapControl
          devices={devices}
          legend={false}
          navigator={false}
          regionBar
          region0
          region2
          region3
          region5
          // region5={stepValue.index === 3}
          tempRegionConfig={regionConfig}
          onLoaded={(map) => {
            mapInstanceRef.current = map;
            initToolBar(editType === "地图绘制");
          }}
          onDrawFeature={(e) => {
            usrRegionRef.current = e;
            console.log("set ddggf:::", e);
          }}
        />
        {/* <RegionEditor
          regionConfig={regionConfig}
          onChange={(e) => {
            // 1. 读取最新的区域边界
            // 2. 绘制当前选择的区域边界
            setSelRegion(e);
          }}
          onDraw={(e) => {
            console.log("on draw:::", e);
            if (mapInstanceRef.current)
              mapInstanceRef.current.showToolbar(
                e
                  ? {
                      polygon: "绘面",
                      point: "标点",
                    }
                  : undefined,
                true
              );
          }}
          onOk={(e) => {
            if (e) {
              if (!usrRegionRef.current) {
                message.warn("请在地图上绘制保护区（点）");
                return false;
              }
              // 同步更新其它
              if (selRegionRef.current === RegionType.region0) {
                setShowModal(true);
              } else {
                // 保存
                saveRegsions();
              }
            } else {
              // 取消
              usrRegionRef.current = undefined;
            }
            return true;
          }}
        /> */}
        <div className={styles.stepEditor}>
          <StepWizard
            stepValue={stepValue}
            onPrev={(e) => {
              setStepValue(e);
            }}
            onNext={(e) => {
              if (
                (!regionConfig ||
                  (regionConfig &&
                    !regionConfig[regionNames[stepValue.index]])) &&
                !usrRegionRef.current
              ) {
                message.info("请设置当前防护区");
                return;
              }
              if (usrRegionRef.current) {
                cacheRegions(regionConfig, stepValue);
                usrRegionRef.current = undefined;
                if (stepValue.index === 0) {
                  setShowModal(true);
                }
              }
              setStepValue(e);
            }}
            onCancel={() => {
              loadConfigFromStorage();
              setStepValue({ index: 0 });
            }}
            onOk={() => saveRegions()}
          >
            <div className={styles.stcontent}>
              {regionConfig && regionConfig[regionNames[stepValue.index]] ? (
                <span>当前防护区已设置</span>
              ) : (
                <span style={{ color: "#ac4b1ee0" }}>请设置当前防护区 </span>
              )}
              <div className={styles.editor}>
                <Radio
                  theme="highLight"
                  options={
                    stepValue.index === 0
                      ? ["地图绘制", "导入GPS"]
                      : ["区域参数", "地图绘制", "导入GPS"]
                  }
                  value={editType}
                  onChange={(e) => setEditTYpe(e)}
                />
                {stepValue.index > 0 && editType === "区域参数" && (
                  <>
                    <div className={styles.setItem}>
                      <span>保护区外延（m）:</span>
                      <InputNumber
                        value={
                          stepValue.index === 1
                            ? 2000
                            : stepValue.index === 2
                            ? 3000
                            : 5000
                        }
                        min={500}
                        max={8000}
                        step={100}
                        disabled={editType !== "区域参数"}
                      />
                    </div>
                    <div className={styles.setItem}>
                      <span>区域高度（m）:</span>
                      <InputNumber
                        value={1000}
                        min={100}
                        max={2000}
                        step={100}
                        disabled={editType !== "区域参数"}
                      />
                    </div>
                  </>
                )}
              </div>
            </div>
          </StepWizard>
        </div>
      </div>
      <Modal
        visible={showModal}
        title="任务信息"
        onCancel={() => {
          setShowModal(false);
        }}
        closable={false}
        onOk={() => {
          setShowModal(false);
          asyncByRegions0();
        }}
      >
        <div>是否同步更新识别处置区、警戒区和预警区？</div>
      </Modal>
    </div>
  );
};

ProtectedRegion.defaultProps = {};

ProtectedRegion.propTypes = {};

export default ProtectedRegion;

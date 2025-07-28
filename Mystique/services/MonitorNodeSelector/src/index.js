import { query, setDataItem } from "@dc/previousparameters";
import parametersnamekey from "parametersnamekey";
import ParametersVerifier from "parametersverifier";
import bindAnt from "./bindAntFreq.js";

/**
 * 初始化站点
 * 1. 查询当前站点和功能
 * 2. 获取历史站点和功能
 * 3. 如果历史站点和功能可用则使用历史站点和功能
 * 4. 获取功能参数
 * 5. 获取历史参数
 * 6. 根据历史参数加载功能参数
 * 7. 处理带入参数
 * 8. 外部参数绑定处理...
 */
class MonitorNodeSelector {
  // 缓存外部传入参数
  #state;

  // 缓存当前可用的站点
  #moduleStations;

  // 上一次使用站点和功能信息本地存储的key
  #preModuleStorageKey = "previousFeatures";

  /**
   * 当前选中的功能
   * @type {{parameters:Array}}
   */
  #selectedModule;

  #frequencyAbility = {};

  #verifier;

  /**
   * 当前功能可用站点加载回调
   * @callback onLoadModules
   * @param {Arrary} e
   */
  /**
   * 进入功能选择默认模块加载完成回调
   * @callback onModuleInitialized
   * @param {{edgeId: String,featureId: String, edgeName: String,featureName: String,deviceName: String,address: String,moduleState: String,isActive:boolean,type:String,longitude: Number,latitude: Number,category:Number}} e
   */
  /**
   * 参数变更回调
   * @callback onParameterChanged
   * @param {{parameters:Arrary,newModule:boolean}} e
   */
  /**
   * http网络请求错误回调
   * @callback onError
   * @param {{code:Number,message:String}} e
   */
  /**
   * 构造函数
   * @param {{features:Arrary<String>,edgeId:String, moduleId:String, appKey:String, axios:Object, triggerParams:Object,onLoadModules:onLoadModules,onModuleInitialized:onModuleInitialized,onParameterChanged:onParameterChanged,onError:onError,onlyIdle:Boolean}} options
   */
  constructor(options) {
    this.#state = options;
    // 2022-9-25 默认只能选择空闲的
    if (this.#state.onlyIdle === undefined || this.#state.onlyIdle === null) {
      this.#state.onlyIdle = true;
    }
    this.#filterCloudNodes();
    this.#verifier = new ParametersVerifier();
    // console.log("MonitorNodeSelector instance");
  }

  /**
   * 从服务器更新站点列表，在onLoadModules中接收
   */
  updateCloudNodes = () => {
    this.#filterCloudNodes(true);
  };

  #filterCloudNodes = (update = false) => {
    // 1. 查询当前站点和功能
    const { features, onModuleInitialized, onError } = this.#state;
    // 筛选可用站点
    const supportedStations = [];
    this.#getMonitorNodes()
      .then((res) => {
        if (res.error && onError) {
          onError(res.error);
        }
        const moduleData = [];
        if (res.result instanceof Array) {
          res.result.forEach((node) => {
            const supportedDirevers = [];
            node.modules.forEach((md) => {
              if (md.moduleType === "driver") {
                let flag = false;
                features.forEach((f) => {
                  if (md.supportedFeatures.includes(f)) {
                    flag = true;
                  }
                });
                if (flag) {
                  supportedDirevers.push(md);
                  moduleData.push({
                    edgeId: node.id,
                    featureId: md.id,
                    mfid: md.mfid,
                    deviceId: md.deviceId,
                    edgeName: node.name,
                    deviceName: md.deviceName,
                    featureName: md.displayName,
                    address: node.address,
                    moduleState:
                      node.state === "disabled" ? node.state : md.moduleState,
                    isActive: node.isActive,
                    type: node.type,
                    longitude: node.longitude,
                    latitude: node.latitude,
                    category: node.category,
                  });
                }
              }
            });
            if (supportedDirevers.length > 0) {
              const tmpStation = { ...node };
              tmpStation.modules = supportedDirevers;
              supportedStations.push(tmpStation);
            }
          });
          this.#moduleStations = moduleData;

          // warning low 了点，先加延时解决吧，需要向外吐出能力信息，先选择默认站，然后initModuleAbility完成后（用timeout）再通知
          if (!update) {
            this.#initDefaultNode();
            setTimeout(() => {
              onModuleInitialized(this.#selectedModule);
              if (this.#selectedModule)
                this.#initModuleParameters(this.#selectedModule);
            }, 300);
          }
          this.#initModuleAbility(supportedStations);
        }
      })
      .catch(() => {
        if (onError) {
          onError({
            code: -1,
            message: "获取站点列表失败",
          });
        }
      });
  };

  /**
   *
   * @param {Arrary} supportedStations
   */
  #initModuleAbility = (supportedStations) => {
    const { features, onLoadModules } = this.#state;
    supportedStations.forEach((node, index) => {
      const { id, modules } = node;
      this.#getFeatureAbility({
        edgeId: id,
        supportedFeatures: features.join(),
      })
        .then((res) => {
          if (res.result) {
            for (let i = 0; i < modules.length; i += 1) {
              const moduleId = modules[i].id;
              const mAbility = res.result.find((a) => a.id === moduleId);
              modules[i].frequency = {
                minimum: 20,
                maximum: 3600,
              };
              modules[i].ifBandwidth = 40000;
              if (mAbility) {
                if (mAbility.frequency) {
                  modules[i].frequency = mAbility.frequency;
                  this.#frequencyAbility[moduleId] = mAbility.frequency;
                }
                if (mAbility.ifBandwidth) {
                  modules[i].ifBandwidth = mAbility.ifBandwidth;
                }
              }
              // 2022-12-5 liujian 向外暴露能力信息
              if (
                this.#selectedModule &&
                this.#selectedModule.featureId === moduleId
              ) {
                this.#selectedModule.frequency = modules[i].frequency;
                this.#selectedModule.ifBandwidth = modules[i].ifBandwidth;
              }
            }
          }
        })
        .catch((er) => {
          console.log("暂时不知道怎么处理这个异常好", er);
        })
        .finally(() => {
          if (index === supportedStations.length - 1) {
            onLoadModules(supportedStations);
          }
        });
    });
  };

  #initDefaultNode = () => {
    const { appKey, edgeId, moduleId, triggerParams, onlyIdle } = this.#state;
    const { edgeId: edgeId1, moduleId: moduleId1 } = triggerParams || {};
    const eid = edgeId || edgeId1;
    const mid = moduleId || moduleId1;
    let enforce = triggerParams ? triggerParams.enforceEdge : undefined;
    // 2. 获取历史站点和功能 {edgeId,featureId}
    let prevFeatures = this.#loadPrevFeatures();
    const prevFeature = prevFeatures ? prevFeatures[appKey] : undefined;
    // 3. 如果历史站点和功能可用则使用历史站点和功能
    let module; // undefined 表示无可用站点
    if (mid) {
      if (enforce || !onlyIdle) {
        // 强制选择，不管是否被占用
        module = this.#moduleStations.find((m) => m.featureId === mid);
      } else {
        module = this.#moduleStations.find(
          (m) => m.featureId === mid && m.moduleState === "idle"
        );
      }
    }
    if (eid && !module) {
      module = this.#moduleStations.find(
        (m) => m.edgeId === eid && m.moduleState === "idle"
      );
      if (!module && (enforce || !onlyIdle)) {
        // 强制选择，不管是否被占用
        module = this.#moduleStations.find((m) => m.edgeId === eid);
      }
    }
    // const states = ["idle", "busy"];
    if (prevFeature && !module) {
      const { edgeId: edgeId2, featureId } = prevFeature;
      if (onlyIdle) {
        module = this.#moduleStations.find(
          (m) =>
            m.edgeId === edgeId2 &&
            m.featureId === featureId &&
            m.moduleState === "idle"
        );
      } else {
        module = this.#moduleStations.find(
          (m) => m.edgeId === edgeId2 && m.featureId === featureId
        );
      }
    }
    if (!module) {
      module = this.#moduleStations.find((m) => {
        if (onlyIdle) {
          return m.moduleState === "idle";
        }
        return true;
      });
    }
    if (module) {
      this.#parseConstraintScript(module.featureId, module.deviceId);
    }
    this.#selectedModule = module;
  };

  #initModuleParameters = (selectedModule) => {
    const { triggerParams, onParameterChanged, onError } = this.#state;
    // 4. 获取功能参数
    const { edgeId, featureId } = selectedModule;
    this.#getModuleParameters({
      edgeId,
      id: featureId,
    })
      .then((res3) => {
        if (res3.error && onError) {
          onError(res3.error);
        }
        if (res3.result) {
          const { parameters } = res3.result;
          // 2022-7-19 边缘端改了，不用反序了
          // parameters.forEach((t) => {
          //   if (t.values && t.values.length > 3) {
          //     t.values = t.values.reverse();
          //     t.displayValues = t.displayValues.reverse();
          //   }
          // });
          // * 5. 获取历史参数
          // * 6. 根据历史参数加载功能参数
          query("moduleId", featureId)
            .then((res) => {
              if (
                res &&
                res[0] &&
                res[0].args instanceof Array &&
                res[0].args.length > 0
              ) {
                const historyParams = res[0].args;
                parameters.forEach((t) => {
                  const history = historyParams.find((h) => h.name === t.name);
                  if (history) {
                    t.parameters = history.parameters;
                    t.value = history.value;
                  }
                });
              }
            })
            .finally(() => {
              // 2022-7-11 liujian 外部应用自行处理
              // this.#initScanSegments(parameters);
              // * 7. 处理带入参数
              if (triggerParams) {
                // 需要单独处理的参数
                const fixedKeys = [
                  parametersnamekey.frequency,
                  parametersnamekey.dfBandwidth,
                  parametersnamekey.ifBandwidth,
                  parametersnamekey.mscanPoints,
                ];
                // 批量处理
                for (let key in triggerParams) {
                  if (!fixedKeys.includes(String(key))) {
                    const pItem = parameters.find((p) => p.name === key);
                    if (pItem) {
                      const x = triggerParams[key];
                      // 2022-9-8 liujian 增加模板参数处理
                      if (pItem.template && pItem.template.length > 0) {
                        pItem.parameters = x;
                      } else {
                        pItem.value = x;
                      }
                    }
                  }
                }

                // 单独处理参数
                if (triggerParams.frequency) {
                  const freq = parameters.find(
                    (p) => p.name === parametersnamekey.frequency
                  );
                  if (freq) {
                    // freq.value = triggerParams.frequency;
                    const x = Number(triggerParams.frequency);
                    if (!Number.isNaN(x) && freq.minimum && freq.maximum) {
                      if (x >= freq.minimum && x <= freq.maximum) {
                        freq.value = x;
                      }
                    }
                  }
                }
                if (triggerParams.dfBandwidth) {
                  const bw = parameters.find(
                    (p) => p.name === parametersnamekey.dfBandwidth
                  );
                  if (bw) {
                    const x = Number(triggerParams.dfBandwidth);
                    if (!Number.isNaN(x) && bw.values) {
                      if (bw.values.includes(x)) {
                        bw.value = x;
                      }
                    }
                  }
                }
                if (triggerParams.bandwidth) {
                  const bws = [
                    parametersnamekey.ifBandwidth,
                    "span",
                    parametersnamekey.filterBandwidth,
                  ];
                  for (let i = 0; i < bws.length; i += 1) {
                    const bw = parameters.find((p) => p.name === bws[i]);
                    if (bw) {
                      // bw.value = triggerParams.bandwidth;
                      const x = Number(triggerParams.bandwidth);
                      if (!Number.isNaN(x) && bw.values) {
                        if (bw.values.includes(x)) {
                          bw.value = x;
                        }
                      }
                      break;
                    }
                  }
                }
                if (triggerParams.mscanPoints) {
                  const pItem = parameters.find(
                    (p) => p.name === parametersnamekey.mscanPoints
                  );
                  if (pItem) {
                    const pointTemplate = {};
                    let bandwidthItems = [];
                    let defaultBw = 0;
                    pItem.template.forEach((pp) => {
                      pointTemplate[pp.name] = pp.value;
                      if (pp.name === parametersnamekey.filterBandwidth) {
                        bandwidthItems = pp.values;
                        defaultBw = pp.default;
                      }
                    });
                    pItem.parameters = triggerParams.mscanPoints.map(
                      (point) => {
                        const newPoint = { ...pointTemplate };
                        newPoint[parametersnamekey.frequency] = point.frequency;
                        newPoint[parametersnamekey.ifBandwidth] =
                          bandwidthItems.includes(point.bandwidth)
                            ? point.bandwidth
                            : defaultBw;

                        return newPoint;
                      }
                    );
                  }
                }
              }
              selectedModule.parameters = parameters;
              setTimeout(() => {
                const verifyInfo = this.#verifier.globalVerify({
                  axios: this.#state.axios,
                  feature: this.#state.features[0],
                  parameters,
                  moduleId: featureId,
                  verifyScript: this.#state.constraintScript,
                });
                const freqAbility = this.#frequencyAbility[featureId];
                bindAnt(freqAbility, verifyInfo.parameters);
                onParameterChanged({
                  parameters: verifyInfo.parameters,
                  changedItems: verifyInfo.parameters.filter((p) =>
                    verifyInfo.changedItems.includes(p.name)
                  ),
                  newModule: true,
                });
              }, 50);
            });
        }
      })
      .catch(() => {
        if (onError) {
          onError({
            code: -1,
            message: "获取功能参数失败",
          });
        }
      });
  };

  /**
   * 更新选站信息，更新后触发 onModuleInitialized，onParameterChanged
   * @param {Object} feature 功能信息
   * @param {Object} station 站点信息
   */
  updateModuleSelect = (feature, station) => {
    // edgeId: node.id,
    // featureId: md.id,
    // edgeName: node.name,
    // deviceName: md.deviceName,
    // featureName: md.displayName,
    // address: node.address,
    // moduleState: md.moduleState,
    // isActive: node.isActive,
    // type: node.type,
    // longitude: node.longitude,
    // latitude: node.latitude,
    // category: node.category,
    const { onModuleInitialized } = this.#state;
    if (
      feature &&
      station &&
      (!this.#selectedModule || feature.id !== this.#selectedModule.featureId)
    ) {
      this.#parseConstraintScript(feature.id, feature.deviceId);
      this.#selectedModule = {
        edgeId: station.id,
        featureId: feature.id,
        mfid: station.mfid,
        deviceId: feature.deviceId,
        edgeName: station.name,
        deviceName: feature.deviceName,
        featureName: feature.displayName,
        address: station.address,
        moduleState: feature.moduleState,
        isActive: station.isActive,
        type: station.type,
        longitude: station.longitude,
        latitude: station.latitude,
        category: station.category,
        frequency: feature.frequency,
        ifBandwidth: feature.ifBandwidth,
      };
      // 更新参数
      this.#initModuleParameters(this.#selectedModule);
      // 触发选站变更
      onModuleInitialized(this.#selectedModule);
    }
  };

  /**
   * 更新参数，更新后触发onParameterChanged
   * 争对单独处理参数
   * @param {String} name 参数名称
   * @param {object} value 参数值
   */
  updateParameter = (name, value) => {
    const { parameters } = this.#selectedModule;
    const paramItem = parameters.find((p) => p.name === name);
    if (paramItem) {
      if (paramItem.template && paramItem.template.length > 0) {
        paramItem.parameters = value;
      } else {
        paramItem.value = value;
      }
      const { onParameterChanged } = this.#state;
      const verifyInfo = this.#verifier.verifyOnChange({
        parameters,
        changedItem: name,
      });
      if (name === parametersnamekey.antennaID) {
        // 查询设备能力
        const freqAbility =
          this.#frequencyAbility[this.#selectedModule.featureId];
        bindAnt(freqAbility, verifyInfo.parameters);
      }

      onParameterChanged({
        parameters: verifyInfo.parameters,
        changedItems: verifyInfo.parameters.filter((p) =>
          verifyInfo.changedItems.includes(p.name)
        ),
        newModule: false,
      });
    }
  };

  /**
   * 更新参数，更新后触发onParameterChanged
   * 争对参数批量绑定变更
   * @param {Arrary} params 参数数组 ，简单结构Array<{name,value}>；  复杂结构，边缘端定义结构
   * @param {boolean} isSimple 是否为简单结构 Array<{name,value}>
   */
  updateParameters = (params, isSimple) => {
    const { parameters, featureId } = this.#selectedModule;
    const nameKeys = [];
    if (isSimple) {
      for (let i = 0; i < params.length; i += 1) {
        nameKeys.push(params[i].name);
        const paramItem = parameters.find((p) => p.name === params[i].name);
        if (paramItem) {
          paramItem.value = params[i].value;
        }
      }
    } else {
      for (let i = 0; i < params.length; i += 1) {
        nameKeys.push(params[i].name);
        const indx = parameters.findIndex((p) => p.name === params[i].name);
        if (indx > -1) {
          parameters[indx] = params[i];
        }
      }
    }
    const { onParameterChanged } = this.#state;
    let changedItems = [];
    let newParams = parameters;
    let verifyInfo;
    nameKeys.forEach((item) => {
      verifyInfo = this.#verifier.verifyOnChange({
        parameters,
        changedItem: item,
      });
      changedItems = changedItems.concat(verifyInfo.changedItems);
      newParams = verifyInfo.parameters;
    });
    if (changedItems.includes(parametersnamekey.antennaID)) {
      // 查询设备能力
      const freqAbility = this.#frequencyAbility[featureId];
      bindAnt(freqAbility, verifyInfo.parameters);
    }
    onParameterChanged({
      parameters: newParams,
      changedItems: newParams.filter((p) => changedItems.includes(p.name)),
      newModule: false,
    });
  };

  /**
   * 保存历史信息
   * 一般在任务启动和停止的时候调用
   * @param {Array<String>} excepts
   */
  saveCache = (excepts) => {
    const { appKey } = this.#state;
    const { parameters } = this.#selectedModule;
    let filter = parameters;
    if (excepts && excepts.length > 0) {
      filter = parameters.filter((p) => !excepts.includes(p.name));
    }
    // 保存选站信息
    let prevFeatures = this.#loadPrevFeatures();
    if (!prevFeatures) {
      prevFeatures = {};
    }
    const { edgeId, featureId } = this.#selectedModule;
    prevFeatures[appKey] = { edgeId, featureId };
    window.localStorage.setItem(
      this.#preModuleStorageKey,
      JSON.stringify(prevFeatures)
    );
    // 保存参数信息
    const historyParams = filter.map((p) => {
      return { name: p.name, parameters: p.parameters, value: p.value };
    });

    setDataItem({
      feature: appKey,
      user: window.sessionStorage.getItem("usrName") || "unknown",
      moduleId: featureId,
      args: historyParams,
    }).then((res) => {
      console.log(res);
    });
  };

  #parseConstraintScript = (featureId, deviceId) => {
    this.#getConstraintScript(featureId)
      .then((res) => {
        const data = res.result;
        if (data && data.constraintScript) {
          const arrs = JSON.parse(data.constraintScript);
          Object.keys(arrs).forEach((item) => {
            if (item === deviceId) {
              this.#state.constraintScript = arrs[item];
            }
          });
        }
      })
      .catch((er) => {
        console.log("get constraint script error:::", er);
      });
  };

  #loadPrevFeatures = () => {
    try {
      const prevString = window.localStorage.getItem(this.#preModuleStorageKey);
      const prevFeatures = JSON.parse(prevString);
      return prevFeatures;
    } catch {
      window.localStorage.removeItem(this.#preModuleStorageKey);
    }
    return undefined;
  };

  /**
   * 初始化扫描频段，因为频段扫描默认没有给频段
   * @param {Arrary} parameters 功能参数
   */
  #initScanSegments = (parameters) => {
    const segment = parameters.find((p) => p.name === "scanSegments");
    if (segment) {
      if (!segment.parameters || segment.parameters.length === 0) {
        segment.parameters = [
          {
            startFrequency: 87,
            stopFrequency: 108,
            stepFrequency: 25,
            id: "initial",
          },
        ];
      }
    }
  };

  #getMonitorNodes = () => {
    const { axios, features } = this.#state;
    return axios({
      url: "rmbt/edge/getEdgeList",
      params: {
        supportedFeatures: features,
        isParam: false,
      },
      method: "get",
    });
  };

  #getModuleParameters = (params) => {
    const { axios } = this.#state;
    return axios({
      url: "/rmbt/edge/getFuncParams",
      method: "get",
      params,
    });
  };

  #getFeatureAbility = (params) => {
    const { axios } = this.#state;
    return axios({
      url: "/rmbt/device/getDeviceAbility",
      method: "get",
      params,
    });
    // return httpRequest(`/rmbt/device/getEdgeAbility?edgeId=${edgeId}`, 'GET');
  };

  #getConstraintScript = (featureId) => {
    // MD，deviceId? 还是moduleId????
    const { axios } = this.#state;
    return axios(`/rmbt/device/getConstraintScript?id=${featureId}`);
    //     .then((res) => {
    //       const data = res.result;
    //       if (data && data.constraintScript) {
    //         const arrs = JSON.parse(data.constraintScript);
    //         Object.keys(arrs).forEach((item) => {
    //           if (item === featureId) {
    //             callback && callback(arrs[item]);
    //             return;
    //           }
    //         });
    //       }
    //       callback && callback(null);
    //     })
    //     .catch(() => {
    //       callback && callback(null);
    //     });
  };
}

export default MonitorNodeSelector;

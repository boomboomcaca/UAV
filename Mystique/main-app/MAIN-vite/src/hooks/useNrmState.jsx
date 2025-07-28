/* eslint-disable no-param-reassign */
import {
  useState,
  useEffect,
  useContext,
  useRef,
  useLayoutEffect,
} from "react";
import NotiFilter from "notifilter";
import MonitorNodeSelector from "monitornodeselector";
import langT from "dc-intl";
import parametersnamekey from "parametersnamekey";
import useCCE from "./useCCE";
import axios from "@/utils/axios";
// import AppContext from "@/store/context";
import getConfig from "@/config";
import { createGUID, getOneSetting } from "@/Tools/tools";
import { getTimeSpanHMS, stampDate } from "@/Tools/timeHelper";
// import { name } from "~/package";
const name = "uavdef";

// 根元素ID，主要用于获取dom截图，这里之所以要随机生成是因为考虑子应用多开
const rootDomId = createGUID();
const pluginName = "频段扫描";

// handleCCEData: ccesocket接收数据
// notiFilterFn: notiFilter扩展函数 提供给外部处理gps compass sync
function useEdge(initialize, handleCCEData, notiFilterFn) {
  // const {
  //   state: {
  //     actions: { replayMode, options },
  //   },
  // } = useContext(AppContext);
  const { wsNotiUrl } = getConfig();
  const parameterItemsRef = useRef([]);
  const firstComeRef = useRef(true);
  const nodeSelectorRef = useRef();

  // 更多参数
  const [moreParam, setMoreParam] = useState(false);
  // 任务信息是否显示
  const [taskInfoShow, setTaskInfoShow] = useState(false);
  // 任务信息
  const [taskInfo, setTaskInfo] = useState({});
  // 具备当前能力的功能
  const [features, setFeatures] = useState([]);
  // 当前选中的功能
  const [selFeature, setSelFeature] = useState();
  // 当前功能参数
  const [parameterItems, setParameterItems] = useState([]);
  // 选择监测站弹层
  const [stationSeling, setStationSeling] = useState(false);
  // 数据列表弹层
  const [dataListShow, setDataListShow] = useState(false);
  // 通知信息
  const [notify, setNotify] = useState();

  // 添加提示消息到底部状态栏
  const pushMessageToNoti = (msg, type = "info") => setNotify({ type, msg });

  const {
    creatTimeRef,
    taskInfoRef,
    taskRef,
    taskStatus,
    isCanReport,
    setIsCanReport,
    startTask,
  } = useCCE(handleCCEData, pushMessageToNoti);

  const showTaskInfo = () => {
    const runTime = creatTimeRef.current
      ? getTimeSpanHMS(new Date(), creatTimeRef.current, 100)
      : "";
    setTaskInfo({
      ...selFeature,
      creatTime: creatTimeRef.current ? stampDate(creatTimeRef.current) : "",
      runTime,
      moduleState: taskStatus === "running" ? "busy" : selFeature.moduleState,
    });
    setTaskInfoShow(true);
  };

  const startCCE = () => {
    firstComeRef.current = false;
    startTask(
      {
        edgeId: selFeature.edgeId,
        moduleId: selFeature.featureId,
        feature: name,
        pluginName,
      },
      parameterItemsRef.current
    );
  };

  useEffect(() => {
    let unregister;
    if (selFeature && selFeature !== "none") {
      unregister = NotiFilter.register({
        url: wsNotiUrl,
        onmessage: (ret) => {
          const { result } = ret;
          if (result.dataCollection) {
            for (let i = 0; i < result.dataCollection.length; i += 1) {
              const noti = result.dataCollection[i];
              notiFilterFn && notiFilterFn(noti);
              // 站点离线
              if (noti.type === "edgeStateChange" && noti.state === "offline") {
                pushMessageToNoti(
                  langT("commons", "stationOffline"),
                  "warning"
                );
              }
              // 站点上线
              if (noti.type === "edgeStateChange" && noti.state === "online") {
                pushMessageToNoti("站点已上线");
              }
              // 被占用
              if (noti.type === "taskChangeInfo") {
                pushMessageToNoti(langT("commons", "deviceSeize"), "warning");
              }
              // 设备故障，等待恢复
              // 设备故障，等待恢复超时
              if (
                noti.type === "moduleStateChange" &&
                noti.id === selFeature.featureId
              ) {
                if (noti.state === "fault") {
                  pushMessageToNoti(langT("commons", "deviceFault"), "warning");
                }
                if (noti.state === "offline") {
                  pushMessageToNoti(
                    langT("commons", "deviceOffline"),
                    "warning"
                  );
                }
              }
            }
          }
        },
        edgeId: [selFeature.edgeId],
      });
    }
    return () => {
      if (unregister) {
        unregister();
      }
    };
  }, [selFeature]);

  useLayoutEffect(() => {
    parameterItemsRef.current = parameterItems;
  }, [parameterItems]);

  useLayoutEffect(() => {
    if (firstComeRef.current) {
      const startTaskOnLoad = getOneSetting("startTaskOnLoad").value;
      if (startTaskOnLoad) {
        const scanSegments = parameterItems?.find(
          (item) => item.name === "scanSegments" && item.parameters.length > 0
        );
        // 当存在频段数据 且taskStatus初始化变为open 后 认为是第一次 此时处理自启 且only一次
        // if (scanSegments && taskStatus === "open") {
        //   startCCE();
        //   firstComeRef.current = false;
        // }
      }
    }
  }, [parameterItems, taskStatus]);

  // useLayoutEffect(() => {
  //   if (options?.edgeInfo && replayMode) {
  //     setSelFeature(options?.edgeInfo);
  //   }
  // }, [options]);

  useEffect(() => {
    // if (!replayMode) {
    nodeSelectorRef.current = new MonitorNodeSelector({
      features: [name],
      appKey: name,
      // triggerParams: options,
      axios,
      onlyIdle: false,
      onLoadModules: (ace) => {
        setFeatures(ace);
      },
      onModuleInitialized: (ace) => {
        console.log("onModuleInitialized", ace);
        // 20230308 liujian 设置none用于UI渲染
        setSelFeature(ace || "none");
      },
      onParameterChanged: (ace) => {
        console.log('onParameterChanged"""""""');
        if (ace.newModule) {
          initialize(ace.parameters);
          // 20230306 liujian 增加默认频段
          const pItem = ace.parameters.find(
            (p) => p.name === parametersnamekey.scanSegments
          );
          if (pItem) {
            pItem.parameters = [
              // { startFrequency: 2400, stopFrequency: 2480, stepFrequency: 400 },
              // { startFrequency: 5150, stopFrequency: 5350, stepFrequency: 400 },
              { startFrequency: 5725, stopFrequency: 5850, stepFrequency: 100 },
            ];
          }
          // 设置光电默认转速
          const speedItem = ace.parameters.find(
            (p) => p.name === parametersnamekey.speed
          );
          if (speedItem) {
            speedItem.value = 45;
          }
        }
        console.log("monitor node slector onParameterChanged:::", ace);
        if (ace.changedItems) {
          taskRef.current?.setParameters?.(ace.changedItems);
        }
        setParameterItems([...ace.parameters]);
      },
    });
    // }
  }, []);

  return {
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
    isCanReport,
    setIsCanReport,
    showTaskInfo,
    startCCE,
  };
}

export default useEdge;

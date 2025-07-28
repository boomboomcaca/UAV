import { useState, useEffect, useContext, useRef } from "react";
import langT from "dc-intl";
import Socket, { CCEEvents } from "ccesocket-webworker";
// import { lowFormatOut } from '@/Tools/tools';
// import AppContext from '@/store/context';
import getConfig from "@/config";

function useCCE(
  handleCCEData,
  pushMessageToNoti = () => {},
  msgforerror = () => {}
) {
  // const {
  //   state: {
  //     actions: { replayMode },
  //   },
  // } = useContext(AppContext);
  const { appid, wsTaskUrl } = getConfig();

  const creatTimeRef = useRef();
  const taskInfoRef = useRef({});
  const taskRef = useRef({});

  const [taskStatus, setTaskStatus] = useState("unavailable");
  // 是否可以导出报表
  const [isCanReport, setIsCanReport] = useState(false);

  const initTask = () => {
    let ts = taskStatus;
    const t = new Socket(
      wsTaskUrl,
      () => {
        t.on(CCEEvents.onStatusChange, (readyState) => {
          if (readyState) {
            if (readyState === "open") {
              // ts === 'unavailable' 初始化连接
              let msg = langT("commons", "serverConnected");
              let type = "info";

              switch (ts) {
                // 网络故障恢复
                case "connecting":
                  msg = langT("commons", "netRecover");
                  break;
                // 启动任务
                case "starting":
                  msg = langT("commons", "taskStartFailure");
                  type = "error";
                  break;
                // 异常中断
                case "running":
                  msg = langT("commons", "serverDisconnected");
                  type = "error";
                  break;
                // 正常停止
                case "stopping":
                  creatTimeRef.current = null;
                  msg = langT("commons", "taskOver");
                  break;
                default:
                  break;
              }
              pushMessageToNoti(msg, type);
            }

            if (readyState === "connecting" && ts !== "unavailable") {
              pushMessageToNoti(langT("commons", "netInterrupt"), "error");
            }
            if (readyState === "starting") {
              pushMessageToNoti(langT("commons", "taskStarting"), "info");
            }
            if (ts === "starting" && readyState === "running") {
              pushMessageToNoti(langT("commons", "taskReady"), "info");
            }
            if (readyState === "unavailable") {
              pushMessageToNoti(
                langT("commons", "cannotConnetServer"),
                "error"
              );
            }
          }
          if (readyState && ts !== readyState) {
            if (ts === "running" && readyState === "stopping") {
              setIsCanReport(true);
            }
            if (readyState === "starting") {
              setIsCanReport(false);
            }
            ts = readyState;
            setTaskStatus(readyState);
          }
        });
        t.on(CCEEvents.onStartTask, (res) => {
          if (res.result) {
            // 记录任务启动时间
            creatTimeRef.current = new Date();
            taskInfoRef.current = {
              ...taskInfoRef.current,
              taskId: res.result.taskId,
            };
          }
          if (res.error) {
            window.console.log("cceError=====;", res.error);
            if (res.error instanceof Object) {
              msgforerror(res.error.message);
              // pushMessageToNoti(res.error.message, "error");
            } else {
              msgforerror(res.error);
              // pushMessageToNoti(res.error, "error");
            }
          }
        });
        t.on(CCEEvents.onData, (res) => {
          const { dataCollection, edgeId } = res;
          if (ts === "running" && dataCollection && edgeId) {
            // 接收处理数据
            handleCCEData(res);
          }
        });
      },
      { dataCheck: false }
    );
    taskRef.current = t;
  };

  const startTask = (params, parameterItems) => {
    console.log("start task::::", params, parameterItems);
    const parameters = parameterItems.map((item) => {
      // if (item.name === 'scanSegments') {
      //   return { name: item.name, value: item.value, parameters: lowFormatOut(item.parameters) };
      // }
      return {
        name: item.name,
        value: item.value,
        parameters: item.parameters,
      };
    });
    taskRef.current?.start({ ...params, pluginId: appid }, parameters, false);
  };

  const beforeunload = (e) => {
    // e.returnValue = "自定义文本";
    if (taskRef.current) {
      taskRef.current.stop();
      taskRef.current.close();
    }
  };

  useEffect(() => {
    initTask();
    window.addEventListener("beforeunload", beforeunload);

    return () => {
      if (taskRef.current) {
        taskRef.current.stop();
        taskRef.current.close();
      }
      window.removeEventListener("beforeunload", beforeunload);
    };
  }, []);

  return {
    creatTimeRef,
    taskInfoRef,
    taskRef,
    taskStatus,
    isCanReport,
    setIsCanReport,

    startTask,
  };
}

export default useCCE;

import axios from "../utils/axios";

// /rmbt/edge/getEdgeList?supportedFeatures=uavd&isParam=false
export const getDevices = () => {
  return axios({
    url: "/rmbt/edge/getEdgeList",
    method: "get",
    params: {
      supportedFeatures: "uavdef",
      isParam: false,
    },
  });
};

export const getDeviceInfo = (deviceId) => {
  // return axios(`/rmbt/device/getOne?id=${deviceId}`, "GET");
  return axios({
    url: "/rmbt/device/getOne",
    method: "get",
    params: {
      id: deviceId,
    },
  });
};

/**
 *
 * @param {getAllDeviceInfoCallback} callback
 */
export const getAllDeviceInfo = (callback) => {
  getDevices().then((res) => {
    const { result } = res;
    if (result && result.length > 0) {
      const deviceModules = result[0].modules.filter(
        (m) => m.moduleType === "device"
      );
      const moduleCategory = [];
      // 已经执行了多少了
      let count = 0;
      deviceModules.forEach((m) => {
        m.moduleCategory.forEach((item) => moduleCategory.push(item));
        const devid = m.id;
        getDeviceInfo(devid)
          .then((res) => {
            if (res.result) {
              const parameters = res.result.parameters;
              const pItem = parameters.find((p) => p.name === "ipAddress");
              const gpsItem = parameters.find((p) => p.name === "address");
              const rtspItem = parameters.find((p) => p.name === "rtspUrl");
              console.log(gpsItem);
              if (pItem && gpsItem) {
                const coordinate = String(gpsItem.value || " , ").split(",");
                m.ipAddress = pItem.value;
                m.location = [
                  Number.parseFloat(coordinate[0].trim() || "104.067999"),
                  Number.parseFloat(coordinate[1].trim() || "30.553209"),
                ];
                // warning 用第一个分类来代表设备类型不严谨
                m.type = m.moduleCategory[0];
                m.state = m.moduleState;
                console.log("ModuleCategory.recognizer", m);
                if (m.moduleCategory.includes("recognizer")) {
                  m.rtspUrl =
                    rtspItem?.value ||
                    `rtsp://${pItem.value}:554/channel=0,stream=0`;
                }
              }
            }
          })
          .finally(() => {
            count += 1;
            if (count === deviceModules.length) {
              callback({
                deviceModules,
                moduleCategory,
              });
            }
          });
      });
    }
  });
};

/**
 * This callback is displayed as a global member.
 * @callback getAllDeviceInfoCallback
 * @param {{deviceModules:Array<Object>,moduleCategory:Array<Object>}}} e
 */

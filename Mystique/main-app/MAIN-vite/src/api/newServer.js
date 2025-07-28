import getConf from "../config";

let baseUrl;
const initUrl = () => {
  const config = getConf();
  const { apiBaseUrl1 } = config;
  console.log("apiBaseUrl1", apiBaseUrl1);
  baseUrl = apiBaseUrl1;
};

/**
 *
 * @param {String} api
 * @param {*} params
 * @returns
 */
const http_get = (api, params) => {
  if (!baseUrl) {
    initUrl();
  }
  let apiUrl = api;
  if (params) {
    apiUrl += "?";
    const keys = Object.keys(params);
    keys.forEach((item) => {
      const value = params[item];
      if (value instanceof Array) {
        value.forEach((val) => {
          apiUrl = `${apiUrl}${item}=${val}&`;
        });
      } else {
        apiUrl = `${apiUrl}${item}=${params[item]}&`;
      }
    });
  }
  //   apiUrl = encodeURIComponent()
  return fetch(`${baseUrl}${apiUrl.slice(0, apiUrl.length - 1)}`);
};

export const getAlarmRecords = (startTime, stopTime) => {
  return http_get("/UavDef/Records", {
    startTime,
    stopTime,
  });
};

export const getEvidence = (evid) => {
  return http_get("/UavDef/PlaybackFiles", {
    evidenceIds: evid,
  });
};

export const getVideos = (recordId) => {
  return http_get("/UavDef/Evidence", {
    recordIds: recordId,
  });
};

export const getTracks = (recordId) => {
  return http_get("/UavDef/UavPaths", {
    recordIds: recordId,
  });
};

export const getSpectrumFrame = ({ filePath, fileName, frameId }) => {
  return http_get("/UavDef/GetFrame", { filePath, fileName, frameId });
};

/**
 *
 * @returns {Promise<Array<{id:Number,droneSerialNum:String}>>}
 */
// export const getWhiteList = () => {
//   return axios({
//     url: "/UavDef/GetWhiteLists",
//     method: "get",
//     params: {},
//   });
// };

/**
 *
 * @param {String} sn
 * @returns {Promise}
 */
export const addWhiteList = (sn) => {
  if (!baseUrl) {
    initUrl();
  }
  // 请求配置
  const requestOptions = {
    method: "PUT",
    headers: {
      "Content-Type": "application/json", // 指定请求体的数据类型为 JSON
      // 如果有其他请求头，可以在这里添加
    },
    // body: JSON.stringify({ droneSerialNum: [sn] }), // 将数据转换为 JSON 字符串并设置为请求体
  };
  // const apiUrl = `${baseUrl}/UavDef/InsertWhiteLists`;
  // // 发送 PUT 请求
  // return fetch(apiUrl, requestOptions);

  let apiUrl = "/UavDef/InsertWhiteLists";

  apiUrl += "?";

  [sn].forEach((val) => {
    apiUrl = `${apiUrl}droneSerialNum=${val}&`;
  });

  //   apiUrl = encodeURIComponent()
  return fetch(`${baseUrl}${apiUrl.slice(0, apiUrl.length - 1)}`, requestOptions);
};

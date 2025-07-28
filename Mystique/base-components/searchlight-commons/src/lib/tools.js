const key = 'Commons_DEMO_KEY';

export function getDefaultDemoKey() {
  let ret = null;
  try {
    ret = JSON.parse(sessionStorage.getItem(key));
  } catch (error) {
    window.console.log(error);
  }
  return ret;
}

export function saveDefaultDemoKey(token) {
  return sessionStorage.setItem(key, JSON.stringify(token));
}

export function removeToken() {
  return sessionStorage.removeItem(key);
}

/**
 * 数据平滑
 * @param {Array} data 待平滑数据
 * @param {boolean} type avg,max,min,round
 * @param {boolean} times 平滑次数 2
 * @param {boolean} points 平滑点基数 5
 */
export const solidData = (array, type = 'avg', times = 1, points = 5) => {
  // 多次平滑
  let ret = array;
  for (let i = 0; i < times; i += 1) {
    ret = executeSolid([...ret], type, points);
  }
  return ret;
};

export const executeSolid = (data, type = 'avg', points = 5) => {
  const solid = [];
  let resolution = data.length > 1170 ? Math.round(data.length / 180) : points; // 平滑点基数，左右?个
  resolution = Math.round(resolution);
  for (let i = 0; i < data.length; i += 1) {
    let sum = 0;
    let start = Math.round(i - resolution / 2);
    start = start < 0 ? 0 : start;
    let end = Math.round(i + resolution / 2);
    end = end > data.length - 1 ? data.length - 1 : end;
    for (let m = start; m < end; m += 1) {
      sum += data[m];
    }
    const avg = sum / (end - start);
    if (type === 'round') {
      solid[i] = Math.round(avg);
    } else if (type === 'max') {
      solid[i] = Math.max(data[i], avg);
    } else if (type === 'min') {
      solid[i] = Math.min(data[i], avg);
    } else {
      solid[i] = avg;
    }
  }
  return solid;
};

// export const getSysDate = async (dcAxios) => {
//   // 系统时间Date
//   let timeDifference = 0;
//   if (dcAxios) {
//     // 获取系统时间
//     const sysTime = await new Promise((resolve, reject) => {
//       // 获取系统时间
//       if (dcAxios) {
//         try {
//           dcAxios({
//             url: '/manager/runtime/getDate',
//             method: 'get',
//           }).then((res) => {
//             const { result } = res;
//             if (result) {
//               const sysDate = new Date(result);
//               const nowDate = new Date();
//               timeDifference = sysDate.getTime() - nowDate.getTime();
//               window.sessionStorage.setItem('sysTimeDifference', timeDifference);
//               // 系统时间戳差值
//               resolve(result);
//             }
//             resolve(null);
//           });
//         } catch (err) {
//           resolve(null);
//         }
//       } else {
//         resolve(null);
//       }
//     });
//   }
//   timeDifference = window.sessionStorage.getItem('sysTimeDifference')
//     ? Number(window.sessionStorage.getItem('sysTimeDifference'))
//     : 0;
//   let nowDateTime = new Date().getTime();
//   if (timeDifference) {
//     nowDateTime += timeDifference;
//     return {
//       timeDifference,
//       date: new Date(nowDateTime),
//     };
//   }
//   return {
//     timeDifference,
//     date: new Date(),
//   };
// };

export const getSysDate = () => {
  // 系统时间Date
  let timeDifference = 0;
  timeDifference = window.sessionStorage.getItem('sysTimeDifference')
    ? Number(window.sessionStorage.getItem('sysTimeDifference'))
    : 0;
  let nowDateTime = new Date().getTime();
  if (timeDifference) {
    nowDateTime += timeDifference;
  }
  return {
    timeDifference,
    date: new Date(nowDateTime),
  };
};

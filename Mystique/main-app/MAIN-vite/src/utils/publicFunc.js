/*
 * @Author: dengys
 * @Date: 2021-06-07 20:04:14
 * @LastEditors: dengys
 * @LastEditTime: 2021-06-21 11:12:47
 */

/**
 * 获取地址栏 ?参数，返回键值对对象
 */
export const getQuery = () => {
  const { href } = window.location;
  const query = href.split("?");
  if (!query[1]) return {};

  const queryArr = decodeURI(query[1]).split("&");
  const queryObj = queryArr.reduce((prev, next) => {
    const item = next.split("=");
    return { ...prev, [item[0]]: item[1] };
  }, {});
  return queryObj;
};

/**
 * Android  ios 平板适配
 * 设计稿 1180*820
 * 只适配短边，即高度
 * @param {*} doc
 */
export const setViewport = (doc) => {
  // 设计稿 1180*820
  // const devScale = window.devicePixelRatio;
  // 实际物理像素
  // const sWidth = window.screen.width * devScale;
  // const sHeight = window.screen.height * devScale;
  window.console.log(
    `devicePixelRatio: ${window.devicePixelRatio}  width:${window.screen.width}  height:${window.screen.height}`
  );
  // Android dp，IOS pt
  const sWidth = window.screen.width;
  const sHeight = window.screen.height;
  let metaEl = doc.querySelector('meta[name="viewport"]');
  let scale = 0.8;
  // IOS 上高度和宽带相反了？？？
  if (sHeight < sWidth) {
    const hhheight = (sHeight / 620).toFixed(5);
    const wwwidth = (sWidth / 1200).toFixed(5);
    scale = hhheight > wwwidth ? wwwidth : hhheight;
  } else {
    const hhheight = (sWidth / 620).toFixed(5);
    const wwwidth = (sHeight / 1200).toFixed(5);
    scale = hhheight > wwwidth ? wwwidth : hhheight;
  }
  // 判断客户端，动态改写meta:viewport标签
  // alert(scale);
  if (!metaEl) {
    metaEl = doc.createElement("meta");
    metaEl.setAttribute("name", "viewport");
    metaEl.setAttribute(
      "content",
      // eslint-disable-next-line
      `width=device-width, viewport-fit=cover, initial-scale=${scale}, maximum-scale=${scale}, minimum-scale=${scale}, user-scalable=no`
    );
    doc.documentElement.firstElementChild.appendChild(metaEl);
  } else {
    metaEl.setAttribute(
      "content",
      // eslint-disable-next-line
      `width=device-width, viewport-fit=cover, initial-scale=${scale}, maximum-scale=${scale}, minimum-scale=${scale}, user-scalable=no`
    );
  }
};

export const zoomPage = () => {
  return localStorage.getItem("pageZoom") === "A";
};

export const initZoom = () => {
  const isBig = zoomPage();
  let zoomLevel = 100;
  if (isBig) {
    // 大号字体
    zoomLevel = 125;
  }
  const pixelRatio = window.devicePixelRatio;
  document.body.style.zoom = `${zoomLevel / pixelRatio}%`;
};

export const downloadUseEleA = (content, fileName) => {
  // a标签下载
  const a = document.createElement("a");
  a.href = content;
  a.download = fileName;
  a.style.display = "none";
  document.body.appendChild(a);
  a.click();
  setTimeout(() => {
    document.body.removeChild(a);
    // 2022-1-24 liujian 释放资源
    window.URL.revokeObjectURL(content);
  }, 1000);
};

export const parseSearchParams = (search) => {
  if (search) {
    const deCodeStr = decodeURIComponent(search);
    const quoteIndex = deCodeStr.indexOf("?");
    const params = deCodeStr.slice(quoteIndex + 1).split("&");
    if (params.length > 0) {
      const searchParams = {};
      params.forEach((item) => {
        const pItemArr = item.split("=");
        if (pItemArr.length == 2) searchParams[pItemArr[0]] = pItemArr[1];
      });
      return searchParams;
    }
  }
  return {};
};

export function disposeCCE() {
  setTimeout(() => {
    if (window.cces) {
      try {
        window.cces[window.cces.length - 1]?.close();
        for (let c = 0; c < window.cces.length; c += 1) {
          try {
            const cce = window.cces[c];
            if (cce) {
              cce.close();
              window.cces[c] = undefined;
            }
          } catch {}
        }
      } catch {}
    }
  }, 100);
}

// 将 Blob 对象转换为 ArrayBuffer
/**
 *
 * @param {*} blob
 * @returns {Promise<ArrayBuffer>}
 */
export function blobToArrayBuffer(blob) {
  return new Promise((resolve, reject) => {
    let reader = new FileReader();
    reader.onload = () => {
      resolve(reader.result);
    };
    reader.onerror = reject;
    reader.readAsArrayBuffer(blob);
  });
}

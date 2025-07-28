class chartUtils {
  // 是否允许缩放，
  // true:内部根据像素宽度/高度处理数据抽取，且可调用zoom方法设置缩放位置；
  // false则内部不进行数据抽取，外部需要订阅onSizeChange回调根据像素宽度/高度先进行数据抽取，如果数据不满足要求，则不进行绘制
  zoomInside = true;
  // true bar绘制在底层 false bar绘制在顶层
  barFront = false;
  // 瀑布图渲染模式，fill 完全填充，pixel横向填充、纵向（行）按像素填充
  rainRenderMode = "fill";
  // chart像素尺寸变更回调
  onSizeChange;
  minimumY = -20;
  maximumY = 80;
  // 内部使用，用于通知尺寸变更
  #onResize;
  // X轴方向缩放起始值 %
  zoomPercent = { start: 0, end: 1 };
  zoomPercentY = { start: 0, end: 1 };
  // zoomInfo
  // X轴方向缩放起始值 索引
  zoomStart;
  // X轴方向缩放结束值 索引
  zoomEnd;
  // Y轴方向缩放起始值 索引
  zoomStartY;
  // Y轴方向缩放结束值 索引
  zoomEndY;

  #resizing = false;

  #resizeTimeout = null;

  /**
   *
   * @param {HTMLElement} container
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean, barFront:boolean,renderMode:String }} option
   * @param {Function} onResize
   * @param {boolean} useGPU
   */
  constructor(container, option, onResize, useGPU) {
    this.container = container;
    if (option) {
      this.zoomInside =
        option.zoomInside === undefined ? true : option.zoomInside;
      this.onSizeChange = option.onSizeChange;
      this.barFront = option.barFront;
      this.rainRenderMode = option.renderMode || "fill";
    }
    this.#onResize = onResize;

    const cvs = document.createElement("canvas");
    cvs.style.aspectRatio = "unset";
    container.appendChild(cvs);
    if (!useGPU) {
      this.ctx = cvs.getContext("2d");
      this.ctx.imageSmoothingEnabled = false;
    }
    this.cvs = cvs;
    this.#onResize1(container);
  }

  // 2021-12-30  liujian MD 会死循环
  /**
   * 容器尺寸大小变更处理
   */
  #onResize1 = () => {
    this.resizeObserver = new ResizeObserver((entries) => {
      if (entries.length > 0 && !this.#resizing) {
        if (this.#resizeTimeout) {
          clearTimeout(this.#resizeTimeout);
          this.#resizeTimeout = null;
        }
        const rect = entries[0].contentRect;
        this.rect = {
          x: Math.round(rect.x),
          y: Math.round(rect.y),
          width: Math.round(rect.width),
          height: Math.round(rect.height),
        };
        this.#resizeTimeout = setTimeout(() => {
          this.#resizing = true;
          this.#initCanvasSize();
          this.fireSizeChange(this.rect.width, this.rect.height);
          this.#resizing = false;
        }, 255);
      }
    });
    this.resizeObserver.observe(this.container);
  };

  fireSizeChange(width, height) {
    if (this.#onResize) {
      this.#onResize(width, height);
    }
    if (this.onSizeChange) {
      this.onSizeChange(width, height);
    }
  }

  /**
   * 资源释放
   * 释放尺寸变更监听
   */
  dispose = () => {
    // const child = this.container.childElementCount;
    // for (let i = child - 1; i > -1; i -= 1) {
    //   this.container.removeChild(this.container.children[i]);
    // }
    this.resizeObserver.unobserve(this.container);
    this.resizeObserver.disconnect();
    // try {
    //   clearInterval(this.#tmrResizeListener);
    // } catch {
    //   /* empty */
    // }
  };
  /**
   * 初始化canvas 尺寸
   */
  #initCanvasSize = () => {
    const cvs = this.container.querySelector("canvas");
    // this.resizeObserver.disconnect();
    cvs.width = this.rect.width;
    cvs.height = this.rect.height;
    // this.resizeObserver.observe(this.container);
  };
}

/**
 * 抽取
 * @param {Array} spectrum
 * @param {Number} outMax
 * @param {Array} referData 参考数据和outMax一样长，这样可用避免重复new Array而节省性能
 * @returns
 */
const combineData = (spectrum, outMax, referData) => {
  const spectrumLength = spectrum.length; // 缓存数组长度
  if (spectrumLength > outMax) {
    const res = referData ? referData.slice(0) : new Float32Array(outMax); // 预先定义结果数组的长度
    let freqIndex = 0;
    const perPointGap = spectrumLength / (outMax - 1);
    let freqN = freqIndex + perPointGap / 2;
    for (let j = 0; j < outMax; j += 1) {
      let maxValue = spectrum[freqIndex];
      const k = Math.round(freqN);
      for (; freqIndex < k; freqIndex += 1) {
        const vl = spectrum[freqIndex];
        if (maxValue < vl) {
          maxValue = vl;
        }
      }
      res[j] = maxValue;
      freqN += perPointGap;
      if (freqN > spectrumLength) freqN = spectrumLength;
    }
    return res;
  }
  return spectrum.slice(0);
};

/**
 * 生成渐变色
 * @param {Array} colors
 * @param {Number} outCount
 * @returns
 */
const gradientColors = (colors, outCount) => {
  const gap = outCount / (colors.length - 1);
  const outColors = [];
  for (let i = 1; i < colors.length; i += 1) {
    const from = Math.round(gap * (i - 1));
    const to = Math.round(gap * i);
    const r1 = parseInt(String(colors[i - 1]).slice(1, 3), 16);
    const g1 = parseInt(String(colors[i - 1]).slice(3, 5), 16);
    const b1 = parseInt(String(colors[i - 1]).slice(5, 7), 16);
    const r2 = parseInt(String(colors[i]).slice(1, 3), 16);
    const g2 = parseInt(String(colors[i]).slice(3, 5), 16);
    const b2 = parseInt(String(colors[i]).slice(5, 7), 16);
    const rsetp = (r2 - r1) / gap;
    const gstep = (g2 - g1) / gap;
    const bstep = (b2 - b1) / gap;
    for (let l = from; l < to; l += 1) {
      const r = Math.round(r1 + rsetp * (l - from));
      const g = Math.round(g1 + gstep * (l - from));
      const b = Math.round(b1 + bstep * (l - from));
      // window.console.log(r, g, b);
      // const rs = r.toString(16).padStart(2, "0");
      // const gs = g.toString(16).padStart(2, "0");
      // const bs = b.toString(16).padStart(2, "0");
      // const hex = `#${rs}${gs}${bs}`;
      outColors.push({
        R: r,
        G: g,
        B: b,
      });
    }
  }
  return outColors;
};

/**
 * 生成渐变色
 * @param {Array} colors
 * @param {Number} outCount
 * @returns
 */
const gradientColorsGPU = (colors, outCount) => {
  const gap = outCount / (colors.length - 1);
  const outColors = [];
  for (let i = 1; i < colors.length; i += 1) {
    const from = Math.round(gap * (i - 1));
    const to = Math.round(gap * i);
    const r1 = parseInt(String(colors[i - 1]).slice(1, 3), 16);
    const g1 = parseInt(String(colors[i - 1]).slice(3, 5), 16);
    const b1 = parseInt(String(colors[i - 1]).slice(5, 7), 16);
    const r2 = parseInt(String(colors[i]).slice(1, 3), 16);
    const g2 = parseInt(String(colors[i]).slice(3, 5), 16);
    const b2 = parseInt(String(colors[i]).slice(5, 7), 16);
    const rsetp = (r2 - r1) / gap;
    const gstep = (g2 - g1) / gap;
    const bstep = (b2 - b1) / gap;
    for (let l = from; l < to; l += 1) {
      const r = Math.round(r1 + rsetp * (l - from));
      const g = Math.round(g1 + gstep * (l - from));
      const b = Math.round(b1 + bstep * (l - from));
      outColors.push(r);
      outColors.push(g);
      outColors.push(b);
    }
  }
  return outColors;
};

function hex2RGBA(hex) {
  const R = parseInt(`0x${hex.substring(1, 3)}`);
  const G = parseInt(`0x${hex.substring(3, 5)}`);
  const B = parseInt(`0x${hex.substring(5, 7)}`);
  let A = 255;
  if (hex.length > 8) {
    A = parseInt(`0x${hex.substring(7, 9)}`);
  }
  return { R, G, B, A };
}

export default chartUtils;
export { combineData, gradientColors, gradientColorsGPU, hex2RGBA };

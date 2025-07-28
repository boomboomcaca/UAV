import { solidData } from '../../lib/tools';

class Editor {
  #edits = [];

  #baseData = [];

  #options = {
    lineColor: '#FF4C2B',
    thikness: 2,
    pen: '#00FF00',
  };

  #allowEdit = false;

  #container;

  #canvas;

  #ctx;

  #rect;

  // 监听容器尺寸变更
  #tmrResizeListener;

  // 判断容器是否已被卸载，超过10次取到的rect值都为0，则判断为已被卸载，自动释放计时器
  #detachTimes = 0;

  minimumY = -20;

  maximumY = 80;

  #penning = false;

  #allowDrag = false;

  // 是否可以拖动了，鼠标位置接近线条
  #canDrag = false;

  #dragging = false;

  // 拖动点
  #dragPoint = -1;

  #startPos = { x: 0, y: 0 };

  #zoomStart = 0;

  #zoomEnd = 0;
  // #stopPos = { x: 0, y: 0 };

  /**
   *
   * @param {HTMLElement} container
   * @param {{onSizeChange:Function<Number,Number>, lineColor:String,thikness:Number,pen:String}} option
   */
  constructor(container, option) {
    this.#container = container;
    if (option) {
      this.#options = { ...this.#options, ...option };
    }

    const cvs = document.createElement('canvas');
    this.#canvas = cvs;
    cvs.onmousedown = (ev) => {
      // console.log(ev);
      if (this.#allowEdit) {
        this.#penning = true;
        this.#startPos = { x: ev.offsetX, y: ev.offsetY };
      }
      if (this.#canDrag) {
        this.#dragging = true;
      }
    };
    cvs.onmouseleave = (ev) => {
      this.#penning = false;
    };
    cvs.onmousemove = (ev) => {
      if (this.#penning) {
        // 绘制图
        this.#drawData();
        // 绘制修改
        this.#ctx.beginPath();
        this.#ctx.moveTo(this.#startPos.x, this.#startPos.y);
        this.#ctx.lineTo(ev.offsetX, this.#startPos.y);

        this.#ctx.strokeStyle = this.#options.pen;
        this.#ctx.lineWidth = this.#options.thikness * 2;
        this.#ctx.stroke();
      }
      if (this.#allowDrag && !this.#dragging) {
        // 计算是否允许拖动
        const { width, height } = this.#rect;
        const dataLen = this.#zoomEnd - this.#zoomStart + 1;
        let pointIndex = (ev.offsetX * dataLen) / width;
        pointIndex = Math.floor(pointIndex + this.#zoomStart);
        this.#dragPoint = pointIndex;
        const yRange = this.maximumY - this.minimumY;
        const scale = (this.#edits[0][pointIndex] - this.minimumY) / yRange;
        const pixelY = height - scale * height;
        if (ev.offsetY > pixelY - 4 && ev.offsetY < pixelY + 4) {
          this.#canvas.style.cursor = 'row-resize';
          this.#canDrag = true;
        } else {
          this.#canvas.style.cursor = 'default';
          this.#canDrag = false;
        }
      }
      if (this.#dragging) {
        const newVal =
          ((this.#rect.height - ev.offsetY) * (this.maximumY - this.minimumY)) / this.#rect.height + this.minimumY;
        const gap = newVal - this.#edits[0][this.#dragPoint];
        for (let i = 0; i < this.#edits[0].length; i += 1) {
          this.#edits[0][i] = Math.round(this.#edits[0][i] + gap);
        }
        this.#drawData();
      }
    };
    cvs.onmouseup = (ev) => {
      this.#dragging = false;
      if (this.#penning) {
        this.#penning = false;
        // 生效
        // this.#stopPos = { x: ev.offsetX, y: ev.offsetY };
        let newVal =
          ((this.#rect.height - this.#startPos.y) * (this.maximumY - this.minimumY)) / this.#rect.height +
          this.minimumY;
        newVal = Math.round(newVal);
        const dataLen = this.#zoomEnd - this.#zoomStart + 1; //  this.#baseData.length;
        let startIndex = (this.#startPos.x * dataLen) / this.#rect.width;
        // console.log(this.#startPos.x, this.#rect.width, dataLen, this.#canvas);
        // -1 是计算又偏差？？暂时没时间取研究
        startIndex = Math.floor(startIndex + this.#zoomStart);

        let stopIndex = (ev.offsetX * dataLen) / this.#rect.width;
        stopIndex = Math.ceil(stopIndex + this.#zoomStart - 1);
        if (startIndex > stopIndex) {
          const tmp = stopIndex;
          stopIndex = startIndex;
          stopIndex = tmp;
        }
        for (let i = startIndex; i <= stopIndex; i += 1) {
          this.#edits[0][i] = Math.round(newVal);
        }
        this.#drawData();
      }
    };
    cvs.onkeyup = (ev) => {
      // console.log(ev.key);
    };
    cvs.style.aspectRatio = 'unset';
    container.appendChild(cvs);
    this.#ctx = cvs.getContext('2d');
    this.#ctx.imageSmoothingEnabled = false;
    this.resize();
    // this.#tmrResizeListener = setInterval(() => {
    //   this.resize();
    // }, 1000);
    // setTimeout(() => {
    //   this.resize();
    // }, 1000);
  }

  /**
   * 设置参考数据
   * @param {Array} data
   * @param {Array} thresholdData
   */
  setBaseData = (data, thresholdData) => {
    let dataLen = 0;
    if (data) {
      dataLen = data.length;
    }
    if (dataLen === 0 && thresholdData) {
      dataLen = thresholdData.length;
    }
    this.#baseData = data;
    if (thresholdData) {
      this.#edits = [[...thresholdData]];
    } else if (data) {
      const sData = solidData(data, 'round', 1);
      this.#edits = [sData];
    }

    this.#zoomStart = 0;
    this.#zoomEnd = dataLen - 1;
    // 绘制图
    this.#drawData();
  };

  startEdit = () => {
    this.#allowEdit = true;
    this.#canvas.style.cursor = 'crosshair';
    this.#allowDrag = false;
  };

  stopEdit = () => {
    this.#allowEdit = false;
    this.#canvas.style.cursor = 'default';
  };

  startDrag = () => {
    this.#allowDrag = true;
    this.#canvas.style.cursor = 'default';
    this.#allowEdit = false;
  };

  stopDrag = () => {
    this.#allowDrag = false;
    this.#canvas.style.cursor = 'default';
  };

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange = (minimum, maximum) => {
    this.minimumY = minimum;
    this.maximumY = maximum;
    // 重绘
    this.#drawData();
  };

  getThreshold = () => {
    return this.#edits[0];
  };

  /**
   * canvas自适应容器尺寸
   */
  resize = () => {
    const rect = this.#container.getBoundingClientRect();
    const x = Math.round(rect.x);
    const y = Math.round(rect.y);
    const width = Math.round(rect.width);
    const height = Math.round(rect.height);
    if (x === 0 && y === 0 && width === 0 && height === 0) {
      this.#detachTimes += 1;
      if (this.#detachTimes > 10) {
        try {
          clearInterval(this.#tmrResizeListener);
        } catch {
          console.log('Dispose tmr');
        }
      }
      return;
    }
    if (
      !this.#rect ||
      this.#rect.x !== x ||
      this.#rect.y !== y ||
      this.#rect.width !== width ||
      this.#rect.height !== height
    ) {
      this.#rect = {
        x,
        y,
        width,
        height,
        left: Math.round(rect.left),
        right: Math.round(rect.right),
        top: Math.round(rect.top),
        bottom: Math.round(rect.bottom),
      };
      const cvs = this.#container.querySelector('canvas');
      cvs.width = this.#rect.width;
      cvs.height = this.#rect.height;
      if (this.#options.onSizeChange) {
        this.#options.onSizeChange(width, height);
      }
      this.#drawData();
      // this.fireSizeChange(this.#rect.width, this.#rect.height);
    }
  };

  #drawData = () => {
    const { width, height } = this.#rect;
    if (!width || width === 0 || !height || height === 0) return;
    this.#ctx.clearRect(0, 0, width, height);
    let drawData = this.#edits[this.#edits.length - 1];
    if (drawData) {
      const { lineColor, thikness } = this.#options;
      let baseData = this.#baseData;
      if (this.#zoomStart > 0 || this.#zoomEnd < drawData.length - 1) {
        drawData = this.#edits[this.#edits.length - 1].slice(this.#zoomStart, this.#zoomEnd);
        if (baseData) baseData = this.#baseData.slice(this.#zoomStart, this.#zoomEnd);
      }
      if (baseData) {
        // 绘制基础线
        this.#drawStepLine(baseData, 'rgba(255, 230, 93, 0.4)', 1);
      }
      // 绘制编辑线
      this.#drawStepLine(drawData, lineColor, thikness);
    }
  };

  /**
   * 绘制方法
   * @param {*} data
   */
  #drawLine = (data, color, lineWidth) => {
    if (data) {
      const { width, height } = this.#rect;
      // if (!width || width === 0 || !height || height === 0) return;
      // this.ctx.clearRect(0, 0, width, height);
      const yRange = this.maximumY - this.minimumY;
      const perWidth = width / (data.length - 1);
      // const half = perWidth / 2;
      this.#ctx.beginPath();
      for (let i = 0; i < data.length; i += 1) {
        const x = perWidth * i;
        const scale = (data[i] - this.minimumY) / yRange;
        const y = Math.floor(height - scale * height);
        if (i === 0) {
          this.#ctx.moveTo(x, y);
        } else {
          this.#ctx.lineTo(x, y);
        }
      }
      this.#ctx.strokeStyle = color;
      this.#ctx.lineWidth = lineWidth;
      this.#ctx.stroke();
    }
  };

  /**
   * 绘制方法
   * @param {*} data
   */
  #drawStepLine = (data, color, lineWidth) => {
    const { width, height } = this.#rect;
    let perWidth = width / data.length;
    this.#ctx.beginPath();
    if (perWidth >= 2) {
      // 梯形线
      for (let i = 0; i < data.length; i += 1) {
        const x = perWidth * i;
        const x2 = perWidth * (i + 1);
        const scale = (data[i] - this.minimumY) / (this.maximumY - this.minimumY);
        const y = Math.floor(height - scale * height);
        if (i === 0) {
          this.#ctx.moveTo(x, y);
        } else {
          this.#ctx.lineTo(x, y);
        }
        this.#ctx.lineTo(x2, y);
      }
    } else {
      // 折线
      // perWidth = width / data.length;
      for (let i = 0; i < data.length; i += 1) {
        const x = perWidth * i;
        const scale = (data[i] - this.minimumY) / (this.maximumY - this.minimumY);
        const y = Math.floor(height - scale * height);
        if (i === 0) {
          this.#ctx.moveTo(x, y);
        } else {
          this.#ctx.lineTo(x, y);
        }
      }
    }
    this.#ctx.strokeStyle = color;
    this.#ctx.lineWidth = lineWidth;
    this.#ctx.stroke();
  };

  /**
   * 缩放
   * @param {Number} start
   * @param {Number} end
   */
  zoom(start, end) {
    this.#zoomStart = start;
    this.#zoomEnd = end;
    this.#drawData();
  }
}

export default Editor;

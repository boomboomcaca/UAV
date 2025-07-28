import chartUtils, { combineData, gradientColors } from "./chartUtils";

/**
 * 雨点图/瀑布图
 */
class RainChart {
  // 瀑布图色带颜色，渐变后
  #colors = [];
  // 瀑布图数据缓存
  #rainImageData = [];
  // 辅助
  #utils;
  // 上一次行数
  #prevRowCount = 1;
  // 是否所有数据都已经收起
  #preIsOver = true;

  workerBlob;

  /**
   * @type {Worker}
   */
  worker;

  /**
   * 构造函数
   * @param {HTMLElement} container
   * @param {Array<string>} pallete 色带颜色，从大到小
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean}} optionMore
   */
  constructor(container, pallete, optionMore) {
    // super(container, optionMore);

    const options = optionMore || {};
    this.#utils = new chartUtils(container, options);
    this.#utils.colorBlends = pallete || ["#0000FF", "#00FF00", "#FF0000"];
    this.#colors = gradientColors(this.#utils.colorBlends, 100);
    //     this.#initColors();
    //     this.worker.postMessage({ colors: this.#colors });
  }

  /**
   * @param {Uint8ClampedArray} imageData
   */
  setImageData(imageData) {
    let rowCount = this.#utils.rect.height;
    let pixelWidth = this.#utils.rect.width;
    if (imageData.length === pixelWidth * rowCount * 4) {
      0;
      try {
        const newImageData = new ImageData(imageData, pixelWidth, rowCount);
        this.#utils.ctx.putImageData(newImageData, 0, 0);
      } catch (ex) {
        console.log(pixelWidth, rowCount, ex);
      }
    }
  }

  setFixedMatrix(matrix) {
    const { width, height } = this.#utils.rect;
    const rowCount = matrix.length;
    const colCount = matrix[0].length;
    // 初始化buffer
    this.#utils.ctx.clearRect(0, 0, width, height);
    const rainImgData = this.#utils.ctx.createImageData(width, height);
    this.#rainImageData = rainImgData.data;
    let drawMatrix = matrix;
    // 处理横向抽取
    if (colCount > width) {
      const data = [];
      matrix.forEach((item) => {
        let d = item;
        // 抽取数据
        if (d.length > width) {
          d = combineData(d, width);
        }
        data.push(d);
      });
      drawMatrix = data;
    }
    // TODO 处理纵向抽取
    if (rowCount > height) {
      //
    }
    this.#drawMatrix(drawMatrix);
  }

  /**
   *
   * @param {Array<Array>} matrix 倒序二维矩阵
   */
  #drawMatrix = (matrix) => {
    const rowCount = matrix.length;
    const validRow = matrix[0];
    const colCount = validRow.length;
    const { width, height } = this.#utils.rect;

    const { minimumY, maximumY } = this.#utils;
    const perWidth = width / colCount;
    const perHeight = height / rowCount;
    let startY = 0;
    let row = 0;
    for (let y = rowCount - 1; y >= 0; y -= 1) {
      const endY = Math.round(perHeight * row + perHeight);
      const oneRow = matrix[y];
      let startX = 0;
      for (let x = 0; x < colCount; x += 1) {
        const val = oneRow[x];
        const endX = Math.round(perWidth * x + perWidth);

        if (val < 2) {
          // 无填充，透明
          for (let px = startX; px < endX; px += 1) {
            for (let py = startY; py < endY; py += 1) {
              const cIndex = (px + width * py) * 4;
              this.#rainImageData[cIndex + 3] = 0;
            }
          }
        } else {
          // 根据颜色填充
          let colorIndex = Math.floor(val);
          if (colorIndex < minimumY) {
            colorIndex = minimumY;
          }
          if (colorIndex > maximumY) {
            colorIndex = maximumY;
          }
          const c = this.#colors[colorIndex];

          for (let px = startX; px < endX; px += 1) {
            for (let py = startY; py < endY; py += 1) {
              const cIndex = (px + width * py) * 4;
              this.#rainImageData[cIndex] = c.R;
              this.#rainImageData[cIndex + 1] = c.G;
              this.#rainImageData[cIndex + 2] = c.B;
              this.#rainImageData[cIndex + 3] = 255;
            }
          }
        }
        startX = endX;
      }
      startY = endY;
      row += 1;
    }
    const newImageData = new ImageData(this.#rainImageData, width);
    this.#utils.ctx.putImageData(newImageData, 0, 0);
  };

  /**
   * Y轴范围变更
   * @param {Numer} minimum
   * @param {Numer} maximum
   */
  setAxisYRange(minimum, maximum) {
    if (minimum >= maximum) return;
    this.#utils.minimumY = minimum;
    this.#utils.maximumY = maximum;
    this.#colors = gradientColors(
      this.#utils.colorBlends,
      maximum - minimum + 1
    );

    // const { width, height } = this.#utils.rect;
    // this.#utils.ctx.clearRect(0, 0, width, height);
  }

  clear() {
    // 清除绘制
    this.#utils.ctx.clearRect(
      0,
      0,
      this.#utils.rect.width,
      this.#utils.rect.height
    );
  }

  /**
   *
   * @returns {HTMLCanvasElement}
   */
  getCanvas() {
    return this.#utils.cvs;
  }

  dispose() {
    this.#utils.dispose();
  }
}

export default RainChart;

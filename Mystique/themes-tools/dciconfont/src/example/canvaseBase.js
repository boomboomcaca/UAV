export default class CanvasBase {
  constructor(node, options) {
    this.ctx = null;
    this.canvas = null;
    this.mountNode = node;
    this.contextHeight = 0;
    this.contextWidth = 0;
    if (options) {
      this.options = options;
    } else {
      this.options = {};
    }

    if (!this.mountNode) {
      return false;
    }
    const width = this.mountNode.offsetWidth;
    const height = this.mountNode.offsetHeight;
    this.canvas = document.createElement("canvas");
    if (this.contextHeight === 0) {
      this.contextHeight = this.canvas.height = height;
    } else {
      this.canvas.height = this.contextHeight;
    }

    if (this.contextWidth === 0) {
      this.contextWidth = this.canvas.width = width;
    } else {
      this.canvas.width = this.contextWidth;
    }

    this.ctx = this.canvas.getContext("2d");

    this.appendContext();
  }

  close() {
    this.stopRender();
    this.canvas = null;
    this.ctx = null;
    this.mountNode = null;
  }

  reset(options) {
    if (options) {
      this.options = options;
    }
    this.stopRender();

    this.init();
  }

  init() {
    this.drawAnimation();
  }

  renderAnimation() {
    this.ctx.clearRect(0, 0, this.contextWidth, this.contextHeight);
  }

  drawAnimation() {
    this.renderAnimation();
    this.animaReq = window.requestAnimationFrame(this.drawAnimation.bind(this));
  }

  stopRender() {
    window.cancelAnimationFrame(this.animaReq);
  }

  appendContext() {
    this.mountNode.appendChild(this.canvas);
  }

  drawImage(img) {
    const TrueImg = new Image();
    TrueImg.src = img;
    TrueImg.onload = () => {
      console.log(TrueImg);
      const offsetX = (this.contextWidth - TrueImg.width) / 2;
      const offsetY = (this.contextHeight - TrueImg.height) / 2;
      // this.ctx.drawImage(TrueImg, offsetX, offsetY);

      this.ctx.fillStyle = "#00a1d6";
      this.ctx.fillRect(
        20,
        20,
        this.contextWidth - 20 * 2,
        this.contextHeight - 20 * 2
      );

      // this.drawExpandBorder();
    };
  }

  drawExpandBorder() {
    const imageData = this.ctx.getImageData(
      0,
      0,
      this.contextWidth,
      this.contextHeight
    );

    const { data } = imageData;

    // 边界像素坐标
    const pathCoordinate = [];

    // x轴
    for (let r = 0; r < this.contextHeight - 1; r++) {
      // y轴

      for (let c = 0; c < this.contextWidth - 1; c++) {
        const rIndexCurrent = 4 * (c + r * this.contextWidth);
        const gIndexCurrent = 4 * (c + r * this.contextWidth) + 1;
        const bIndexCurrent = 4 * (c + r * this.contextWidth) + 2;
        const aIndexCurrent = 4 * (c + r * this.contextWidth) + 3;

        const rIndexRight = 4 * (c + 1 + r * this.contextWidth);
        const gIndexRight = 4 * (c + 1 + r * this.contextWidth) + 1;
        const bIndexRight = 4 * (c + 1 + r * this.contextWidth) + 2;
        const aIndexRight = 4 * (c + 1 + r * this.contextWidth) + 3;

        const rIndexDown = 4 * (c + (r + 1) * this.contextWidth);
        const gIndexDown = 4 * (c + (r + 1) * this.contextWidth) + 1;
        const bIndexDown = 4 * (c + (r + 1) * this.contextWidth) + 2;
        const aIndexDown = 4 * (c + (r + 1) * this.contextWidth) + 3;

        const disX = Math.abs(data[aIndexCurrent] - data[aIndexRight]);
        const disY = Math.abs(data[aIndexCurrent] - data[aIndexDown]);

        if (disX >= 200 || disY >= 200) {
          pathCoordinate.push([c, r]);
        }
      }
    }
    console.log(pathCoordinate);

    const len = pathCoordinate.length;
    const centerX = this.contextWidth / 2;
    const centerY = this.contextHeight / 2;

    function getAfterXy(position) {
      const [x, y] = position;
      const xx = Math.abs(x - centerX);
      const yy = Math.abs(y - centerY);

      const dis = Math.round(Math.sqrt(Math.pow(xx, 2) + Math.pow(yy, 2)));

      // 反正弦值
      const asinVal = Math.asin((x - centerX) / dis);

      // 反余弦
      const acosVal = Math.acos((y - centerY) / dis);

      const newDis = dis + 4;

      const newY = Math.round(newDis * Math.sin(asinVal) + centerY);
      const newX = Math.round(newDis * Math.cos(acosVal) + centerX);

      return [newX, newY];
    }

    const transedImageData = new Uint8ClampedArray(
      this.contextWidth * this.contextHeight * 4
    );

    const newImagdata = new ImageData(
      transedImageData,
      this.contextWidth,
      this.contextHeight
    );

    if (len > 0) {
      for (let i = 0; i < len; i++) {
        // const [x, y] = pathCoordinate[i];
        const [nx, ny] = getAfterXy(pathCoordinate[i]);
        // console.log(nx, ny);
        imageData.data[(nx + ny * this.contextWidth) * 4] = 255;
        imageData.data[(nx + ny * this.contextWidth) * 4 + 1] = 0;
        imageData.data[(nx + ny * this.contextWidth) * +2] = 0;
        imageData.data[(nx + ny * this.contextWidth) * +3] = 255;
        // newImagdata.data[nx * ny * 4] = 165;
        // newImagdata.data[nx * ny * 4 + 1] = 0;
        // newImagdata.data[nx * ny * 4 + 2] = 0;
      }
    }

    // imageData.data = transedImageData;
    console.log(newImagdata);
    this.ctx.putImageData(imageData, 0, 0);
  }

  calcBorder() {
    const imageData = this.ctx.getImageData(
      0,
      0,
      this.contextWidth,
      this.contextHeight
    );

    const { data } = imageData;

    this.getContourData(imageData);
  }

  getContourData(contourData) {
    var w = this.contextWidth;
    var h = this.contextHeight;
    const _draft = this.ctx.createImageData(w, h);
    var pixelsData = contourData;
    var pixels = pixelsData.data;

    //遍历像素并标记
    for (var r = 0; r < h - 1; r++) {
      for (var c = 0; c < w - 1; c++) {
        var rIndexCurrent = 4 * (c + r * w);
        var gIndexCurrent = 4 * (c + r * w) + 1;
        var bIndexCurrent = 4 * (c + r * w) + 2;
        var aIndexCurrent = 4 * (c + r * w) + 3;

        var rIndexRight = 4 * (c + 1 + r * w);
        var gIndexRight = 4 * (c + 1 + r * w) + 1;
        var bIndexRight = 4 * (c + 1 + r * w) + 2;
        var aIndexRight = 4 * (c + 1 + r * w) + 3;

        var rIndexDown = 4 * (c + (r + 1) * w);
        var gIndexDown = 4 * (c + (r + 1) * w) + 1;
        var bIndexDown = 4 * (c + (r + 1) * w) + 2;
        var aIndexDown = 4 * (c + (r + 1) * w) + 3;

        var currentPixel = new pixel(
          pixels[rIndexCurrent],
          pixels[gIndexCurrent],
          pixels[bIndexCurrent],
          pixels[aIndexCurrent]
        );
        var rightPixel = new pixel(
          pixels[rIndexRight],
          pixels[gIndexRight],
          pixels[bIndexRight],
          pixels[aIndexRight]
        );
        var downPixel = new pixel(
          pixels[rIndexDown],
          pixels[gIndexDown],
          pixels[bIndexDown],
          pixels[aIndexDown]
        );

        var horizonDis = distance(currentPixel, rightPixel);
        var verticalDis = distance(currentPixel, downPixel);

        if (horizonDis >= 200 || verticalDis >= 200) {
          pixelsData.data[rIndexCurrent] = 255;
          pixelsData.data[gIndexCurrent] = 0;
          pixelsData.data[bIndexCurrent] = 0;
          pixelsData.data[aIndexCurrent] = 255;
        }
        // else {
        //   pixelsData.data[rIndexCurrent] = 0;
        //   pixelsData.data[gIndexCurrent] = 0;
        //   pixelsData.data[bIndexCurrent] = 0;
        //   pixelsData.data[aIndexCurrent] = 0;
        // }
      }
    }

    console.log(_draft);

    // this.ctx.clearRect(0, 0, this.contextHeight, this.contextWidth);
    this.ctx.putImageData(pixelsData, 0, 0);

    //计算距离
    function distance(pa, pb) {
      var sum = 0;
      sum += Math.abs(Number(pa.r) - Number(pb.r));
      sum += Math.abs(Number(pa.g) - Number(pb.g));
      sum += Math.abs(Number(pa.b) - Number(pb.b));
      var sum = Math.abs(Number(pa.a) - Number(pb.a));
      return sum;
    }

    //自定义像素对象
    function pixel(red, green, blue, aval) {
      this.r = red;
      this.g = green;
      this.b = blue;
      this.a = aval;
    }
  }

  resize() {
    this.contextHeight = this.mountNode.offsetHeight;
    this.contextWidth = this.mountNode.offsetWidth;
    this.canvas.width = this.contextWidth;
    this.canvas.height = this.contextHeight;
  }
}

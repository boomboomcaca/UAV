import chartBg from './assets/dfchart.png';
import centerPanel from './assets/centerPanel.png';

function debounce(fn, wait = 500) {
  let timer = null;

  return function anymous(...args) {
    clearTimeout(timer);
    timer = setTimeout(() => {
      timer = null;
      return fn.call(this, ...args);
    }, wait);
  };
}

const DESIGN_SIZE = 400;
const ARROW_SIDE_LENGTH = 15;

const bgImgs = [
  { img: chartBg, width: 308, height: 308, centerOffsetX: 0, centerOffsetY: 0 },
  { img: centerPanel, width: 80, height: 80, centerOffsetX: 0, centerOffsetY: 0 },
];

export default class CanvasDfChart {
  constructor(options) {
    if (options) {
      this.options = options;
    } else {
      this.options = {};
    }
    this.multiPointerColors = options.multiPointerColors || [];
    this.bearingList = options.bearings || [];
    this.hasMounted = false;
    this.backgroundLoaded = false;
    this.ctx = null;
    this.canvas = null;
    this.mountNode = null;
    this.staticLayers = [];
    this.compassHeading = 0;
    this.activedBearingId = options.defaultSelectedId || null;
  }

  init() {
    this.calcSize();
    this.loadBackground();
    this.drawOneTick();
  }

  calcSize() {
    this.chartSize = Math.min(this.contextHeight, this.contextWidth);
    this.transRatio = this.chartSize / DESIGN_SIZE;
    this.centerX = this.contextWidth / 2;
    this.centerY = this.contextHeight / 2;
  }

  getChartSize() {
    return this.chartSize;
  }

  mount(node) {
    this.mountNode = node;
    if (!this.mountNode) {
      return false;
    }
    this.contextHeight = 0;
    this.contextWidth = 0;
    const width = this.mountNode.offsetWidth || 400;
    const height = this.mountNode.offsetHeight || 400;
    this.canvas = document.createElement('canvas');
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
    this.ctx = this.canvas.getContext('2d');
    this.mountNode.appendChild(this.canvas);
    window.addEventListener('resize', this.resize.bind(this));
    this.hasMounted = true;
  }

  loadBackground() {
    const promises = [];

    if (!bgImgs || !bgImgs.length) {
      return false;
    }
    for (let i = 0; i < bgImgs.length; i++) {
      const bgImg = new Image();
      bgImg.src = bgImgs[i].img;
      const p = new Promise((resolve) => {
        bgImg.onload = () => {
          this.staticLayers[i] = bgImg;
          resolve();
        };
      });
      promises.push(p);
    }

    Promise.all(promises).then(() => {
      this.backgroundLoaded = true;
      this.drawOneTick();
    });
  }

  drawOneTick() {
    if (!this.ctx) {
      return;
    }
    this.ctx && this.ctx.clearRect(0, 0, this.contextWidth, this.contextHeight);

    if (this.backgroundLoaded === true) {
      for (let i = 0; i < this.staticLayers.length; i++) {
        const { width, height, centerOffsetX, centerOffsetY } = bgImgs[i];
        const startX = this.centerX - (width / 2 - centerOffsetX) * this.transRatio;
        const startY = this.centerY - (height / 2 - centerOffsetY) * this.transRatio;

        this.ctx.drawImage(this.staticLayers[i], startX, startY, width * this.transRatio, height * this.transRatio);
        if (i === 0) {
          this.drawCompassBack();

          this.drawMultiPointer();
        }
      }
      //   if (this.activedBearingId) {
      //     this.drawBearingText();
      //   }
    }
  }

  //   drawBearingText() {
  //     const bearing = this.bearingList.find((b) => b.id === this.activedBearingId)?.bearing;
  //     if (typeof bearing === 'number') {
  //       this.ctx.fillStyle = '#fff';
  //       this.ctx.font = '';
  //     }
  //   }

  // 画刻度
  drawCompassBack() {
    const SIZE = 167 * this.transRatio * 2;

    const gapCount = 180;
    const oneRidian = Math.PI / 90;

    for (let i = 1; i <= gapCount; i++) {
      let text = '';
      this.ctx.lineWidth = 3;
      if ((i - 1) % 15 === 0) {
        this.ctx.lineWidth = 5;
        this.ctx.strokeStyle = '#35E06580';
      } else {
        this.ctx.strokeStyle = '#114036';
      }

      if ((i - 1) % 15 === 0) {
        if (i === 1) {
          if (this.seatType === 'compass') {
            text = 'N';
          } else {
            text = '0°';
          }
        } else if ((i - 1) * 2 < 360) {
          text = `${(i - 1) * 2}°`;
        }
      }

      const x = this.contextWidth / 2 + (Math.sin((i - 1) * oneRidian) * SIZE) / 2;
      const y = this.contextHeight / 2 - (Math.cos((i - 1) * oneRidian) * SIZE) / 2;

      const x2 = this.contextWidth / 2 + Math.sin((i - 1) * oneRidian) * (SIZE / 2 - 10 * this.transRatio);

      const y2 = this.contextHeight / 2 - Math.cos((i - 1) * oneRidian) * (SIZE / 2 - 10 * this.transRatio);

      this.ctx.beginPath();
      this.ctx.moveTo(x, y);
      this.ctx.lineTo(x2, y2);
      this.ctx.stroke();
      this.ctx.closePath();

      this.ctx.font = `${Math.floor(11 * this.transRatio)}px Arial`;
      this.ctx.fillStyle = '#ffffff80';
      if (text) {
        const { width } = this.ctx.measureText(text);

        let textX, textY;

        const x3 = this.contextWidth / 2 + Math.sin((i - 1) * oneRidian) * (SIZE / 2 + 10 * this.transRatio);

        const y3 = this.contextHeight / 2 - Math.cos((i - 1) * oneRidian) * (SIZE / 2 + 10 * this.transRatio);

        textX = x3;
        textY = y3 + 5 * this.transRatio;

        if (Math.sin((i - 1) * oneRidian) > 0) {
          textX = textX - width / 2;
        } else {
          textX = textX - width * 0.65;
        }
        if (Math.cos((i - 1) * oneRidian) > 0) {
        }

        this.ctx.fillText(text, textX, textY);
      }
    }
  }

  updateBearings(bearings) {
    if (bearings instanceof Array) {
      this.bearingList = bearings;
      this.drawOneTick();
    }
  }

  setSelectdedIndex(id) {
    if (id) {
      this.activedBearingId = id;
      this.drawOneTick();
    }
  }

  drawMultiPointer() {
    const len = this.bearingList.length;

    const bottomLen = 5 * this.transRatio;
    const topLen = 3 * this.transRatio;
    const maxLen = 120 * this.transRatio;
    for (let i = 0; i < len; i++) {
      const value = this.bearingList[i]?.bearing;
      const height = maxLen; // (maxLen * value) / max;
      const x1 = 0 - bottomLen / 2;
      const y1 = 0;
      const x2 = 0 + bottomLen / 2;
      const y2 = 0;
      const x3 = 0 - topLen / 2;
      const y3 = 0 - height;
      const x4 = 0 + topLen / 2;
      const y4 = 0 - height;

      this.ctx.save();
      this.ctx.fillStyle = this.multiPointerColors[i] || '#00a1d5';

      this.ctx.translate(this.contextWidth / 2, this.contextHeight / 2);
      this.ctx.rotate(((value + this.compassHeading) * Math.PI) / 180);
      this.ctx.beginPath();
      this.ctx.moveTo(x1, y1);
      this.ctx.lineTo(x2, y2);
      this.ctx.lineTo(x4, y4);
      this.ctx.lineTo(x3, y3);
      this.ctx.closePath();
      this.ctx.shadowOffsetX = 0;
      this.ctx.shadowOffsetY = 0;
      this.ctx.shadowColor = '#00000066';
      this.ctx.shadowBlur = 10;
      this.ctx.fill();
      // 画个箭头
      if (this.activedBearingId === this.bearingList[i]?.id) {
        const triangleX1 = 0 - (ARROW_SIDE_LENGTH / 2) * this.transRatio;
        const triangleY1 = y3 + 11 * this.transRatio;
        const triangleX2 = 0 + (ARROW_SIDE_LENGTH / 2) * this.transRatio;
        const triangleY2 = y3 + 11 * this.transRatio;
        const triangleX3 = 0;
        const triangleY3 = y3 - 2 * this.transRatio;

        this.ctx.beginPath();
        this.ctx.moveTo(triangleX1, triangleY1);
        this.ctx.lineTo(triangleX2, triangleY2);
        this.ctx.lineTo(triangleX3, triangleY3);

        this.ctx.closePath();
        this.ctx.fill();
      }

      //

      this.ctx.restore();
    }
  }

  resize() {
    const after = debounce(() => {
      try {
        if (this.canvas) {
          this.canvas.width = 0;
          this.canvas.height = 0;
          this.contextHeight = this.mountNode.offsetHeight;
          this.contextWidth = this.mountNode.offsetWidth;
          this.canvas.width = this.contextWidth;
          this.canvas.height = this.contextHeight;

          this.calcSize();
          this.drawOneTick();
        }
      } catch (err) {
        console.error(err);
      }
    }, 200);
    after();
  }
}

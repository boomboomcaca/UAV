import {
  Application,
  Sprite,
  Texture,
  BufferResource,
  BaseTexture,
  Rectangle,
  FORMATS,
  ALPHA_MODES,
  settings,
} from "pixi.js";

class RainChartGPU {
  rect = { width: 100, height: 100 };
  rainRGBData = new Uint8Array(this.rect.width * this.rect.height * 3);
  /**
   * @type {Application}
   */
  pixi;

  /**
   * @type {BufferResource}
   */
  pixiBuffer;

  /**
   * @type {Texture}
   */
  texture;

  /**
   * @type {Sprite}
   */
  sprite;

  /**
   * 构造函数
   * @param {HTMLElement} container
   * onSizeChange canvas像素尺寸变更回调
   * zoomInside 是否支持内部缩放，默认 true，false则调用zoom()不生效
   * renderMode 瀑布图渲染模式，fill 完全填充，pixel横向填充、纵向（行）按像素填充
   * @param {{onSizeChange:Function<Number,Number>, zoomInside:boolean, renderMode:String}} optionMore
   */
  constructor(container, optionMore) {
    this.options = optionMore || {};

    this.container = container;
    this.#onResize();
    this.pixi = new Application({
      width: 100,
      height: 100,
      backgroundColor: 0x000000,
      resizeTo: container,
      antialias: false,
      clearBeforeRender: false,
      powerPreference: "high-performance",

      // backgroundAlpha: false,
    });
    container.appendChild(this.pixi.view);
    // const { width, height } = this.rect;
    this.pixiBuffer = new BufferResource(this.rainRGBData, {
      width: this.rect.width,
      height: this.rect.height,
      unpackAlignment: 1,
    });
    // 创建一个 Texture 对象，并指定 bufferResource 作为像素数据来源
    this.texture = new Texture(
      new BaseTexture(this.pixiBuffer, {
        format: FORMATS.RGB,
        alphaMode: ALPHA_MODES.NPM,
      }),
      new Rectangle(0, 0, this.rect.width, this.rect.height)
    );
    this.sprite = new Sprite(this.texture);

    this.sprite.interactive = false;
    this.sprite.eventMode = "none";
    // this.sprite.blendMode= BLEND_MODES.
    this.sprite.position.set(0, 0);
    this.pixi.ticker.maxFPS = 15;
    this.pixi.ticker.deltaMS = 100;
    this.pixi.stage.addChild(this.sprite);
  }

  #resizePixi() {
    console.log("resizePixi--------");
    this.pixiBuffer.dispose();
    this.pixiBuffer.destroy();
    this.pixiBuffer = new BufferResource(this.rainRGBData, {
      width: this.rect.width,
      height: this.rect.height,
      // 20230829 liujian这个很重要，不然要求buffer的长度必须是4的倍数
      unpackAlignment: 1,
    });
    // 创建一个新的 BaseTexture 对象，指定新的宽度和高度
    const newBaseTexture = new BaseTexture(this.pixiBuffer, {
      format: FORMATS.RGB,
      alphaMode: ALPHA_MODES.NO_PREMULTIPLIED_ALPHA,
      // width: this.rect.width,
      // height: this.rect.height,
    });
    // 更新纹理对象的 BaseTexture
    this.texture.baseTexture = newBaseTexture;
    // 更新纹理对象的 UV 坐标
    this.texture.frame = new Rectangle(0, 0, this.rect.width, this.rect.height);
    this.texture.updateUvs();
  }

  /**
   * 容器尺寸大小变更处理
   */
  #onResize = () => {
    this.resizeObserver = new ResizeObserver((entries) => {
      if (entries.length > 0) {
        const rect = entries[0].contentRect;
        this.rect = {
          width: Math.round(rect.width),
          height: Math.round(rect.height),
        };
        this.pixi.resize();
        const { onSizeChange } = this.options;
        if (onSizeChange) onSizeChange(this.rect.width, this.rect.height);
      }
    });
    this.resizeObserver.observe(this.container);
  };

  /**
   * @param {Uint8Array} data
   */
  setImageData(data) {
    const isChange = data.length !== this.rainRGBData.length;
    this.rainRGBData = data;
    if (isChange) {
      this.#resizePixi();
    } else {
      this.pixiBuffer.data = data;
      this.texture.update();
    }
  }

  setFixedMatrix(matrix) {
    // TODO
  }

  dispose() {
    if (this.pixiBuffer) {
      this.pixiBuffer.dispose();
      this.pixiBuffer.destroy();
    }
    this.texture.destroy();
    this.sprite.destroy();
    this.pixi.destroy();
    this.resizeObserver.unobserve(this.container);
    this.resizeObserver.disconnect();
  }
}

export default RainChartGPU;

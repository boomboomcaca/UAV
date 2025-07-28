class PCMPlayer {
  // option;

  // samples;

  constructor(option) {
    window.AudioContext =
      window.AudioContext || window.webkitAudioContext || window.mozAudioContext || window.msAudioContext;

    this.init(option);
  }

  init(option) {
    const defaults = {
      encoding: '16bitInt',
      channels: 1,
      sampleRate: 8000,
      flushingTime: 1000,
    };
    this.option = { ...defaults, ...option };
    this.samples = new Float32Array();
    this.flush = this.flush.bind(this);
    // 实时码流， 非文件流，则 flushingTime为0
    if (this.option.flushingTime > 0) {
      this.interval = setInterval(this.flush, this.option.flushingTime);
    }
    this.maxValue = this.getMaxValue();
    this.checkAudioDevice(() => {
      this.createAudioContext();
    });
  }

  getMaxValue() {
    const encodings = {
      '8bitInt': 128,
      '16bitInt': 32768,
      '32bitInt': 2147483648,
      '32bitFloat': 1,
    };
    // window.console.log(this.option);
    return encodings[this.option.encoding] ? encodings[this.option.encoding] : encodings['16bitInt'];
  }

  getTypedArray(data) {
    const { encoding } = this.option;
    return new {
      '8bitInt': Int8Array,
      '16bitInt': Int16Array,
      '32bitInt': Int32Array,
      '32bitFloat': Float32Array,
    }[encoding](data);
  }

  createAudioContext() {
    this.audioCtx = new window.AudioContext();
    this.gainNode = this.audioCtx.createGain();
    this.gainNode.gain.value = 1;
    this.gainNode.connect(this.audioCtx.destination);
    this.startTime = this.audioCtx.currentTime;
  }

  // eslint-disable-next-line class-methods-use-this
  checkAudioDevice(callback) {
    // 判断操作系统类型，决定是否检查设备状态
    // warning 此处不确定这样判断是不是存在问题
    let checkAudioDevice = true;
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent)) {
      checkAudioDevice = false;
    }
    if (!checkAudioDevice) {
      callback();
      return;
    }
    if (!navigator.mediaDevices || !navigator.mediaDevices.enumerateDevices) {
      console.log('enumerateDevices() not supported.');
      return;
    }
    // List cameras and microphones.
    navigator.mediaDevices
      .enumerateDevices()
      .then((devices) => {
        const audioDev = devices.find((d) => d.kind === 'audiooutput');
        if (audioDev && callback) {
          callback();
        }
      })
      .catch((err) => {
        console.log(JSON.stringify(err));
      });
  }

  // eslint-disable-next-line class-methods-use-this
  isTypedArray(data) {
    return data.byteLength && data.buffer && data.buffer.constructor === ArrayBuffer;
  }

  /**
   * 播放音频流数据
   * 播放原始pcm裸数据
   * @param {*} data
   */
  feed(data) {
    if (!this.audioCtx) {
      // 判断是否具有播放设备，可惜接口需要https才支持
      // this.checkAudioDevice(() => {
      this.createAudioContext();
      // });
    }
    if (!this.isTypedArray(data) || !this.audioCtx) return;
    const d = this.getFormatedValue(data);
    const tmp = new Float32Array(this.samples.length + d.length);
    tmp.set(this.samples, 0);
    tmp.set(d, this.samples.length);
    this.samples = tmp;
    // 实时码流，立即播放
    if (!this.interval) {
      this.flush();
    }
  }

  getFormatedValue(data) {
    // console.log('test');
    const d = this.getTypedArray(data.buffer);
    const float32 = new Float32Array(d.length);

    for (let i = 0; i < d.length; i += 1) {
      float32[i] = d[i] / this.maxValue;
    }
    return float32;
  }

  setVolume(volume) {
    this.gainNode.gain.value = volume;
  }

  suspend() {
    if (this.audioCtx) {
      this.audioCtx.suspend();
    }
  }

  resume() {
    if (this.audioCtx) {
      this.audioCtx.resume();
    }
  }

  destroy() {
    if (this.interval) {
      clearInterval(this.interval);
    }
    this.samples = null;
    if (this.audioCtx) {
      this.audioCtx.close();
    }
    this.audioCtx = null;
  }

  flush() {
    // const flush = () => {
    if (!this.samples.length || !this.audioCtx) return;
    const bufferSource = this.audioCtx.createBufferSource();
    const length = this.samples.length / this.option.channels;
    const audioBuffer = this.audioCtx.createBuffer(this.option.channels, length, this.option.sampleRate);
    let audioData;
    let offset;
    // let decrement;

    for (let channel = 0; channel < this.option.channels; channel += 1) {
      audioData = audioBuffer.getChannelData(channel);
      offset = channel;
      // decrement = 50;
      for (let i = 0; i < length; i += 1) {
        audioData[i] = this.samples[offset];
        // if (i < 50) {
        //   audioData[i] = (audioData[i] * i) / 50;
        // }
        // if (i >= length - 51) {
        //   audioData[i] = (audioData[i] * (decrement -= 1)) / 50;
        // }
        offset += this.option.channels;
      }
    }

    if (this.startTime < this.audioCtx.currentTime) {
      this.startTime = this.audioCtx.currentTime;
    }
    // window.console.log(
    //   `start vs current ${this.startTime} vs ${this.audioCtx.currentTime} duration: ${audioBuffer.duration}`
    // );
    bufferSource.buffer = audioBuffer;
    bufferSource.connect(this.gainNode);
    bufferSource.start(this.startTime);
    this.startTime += audioBuffer.duration;
    this.samples = new Float32Array();
  }
}

export default PCMPlayer;

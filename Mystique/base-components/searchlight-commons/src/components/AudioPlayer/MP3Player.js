class Mp3Player {
  // option;

  // samples = new Uint8Array();

  // tmrFulsh;

  constructor(option) {
    window.AudioContext =
      window.AudioContext || window.webkitAudioContext || window.mozAudioContext || window.msAudioContext;
    const defaults = {
      flushingTime: 1000,
    };
    this.option = { ...defaults, ...option };
    window.console.log(this.option);
    this.flush = this.flush.bind(this);
    // 实时码流， 非文件流，则 flushingTime为0
    if (this.option.flushingTime > 0) {
      this.tmrFulsh = setInterval(this.flush, this.option.flushingTime);
    }
    // this.maxValue = this.getMaxValue();
    this.samples = new Uint8Array();
    this.checkAudioDevice(() => {
      this.createAudioContext();
    });
  }

  createAudioContext() {
    try {
      this.audioCtx = new window.AudioContext();
      // console.log(this.audioCtx);
      this.gainNode = this.audioCtx.createGain();
      this.gainNode.gain.value = 1;
      this.gainNode.connect(this.audioCtx.destination);
      this.startTime = this.audioCtx.currentTime;
    } catch (ex) {
      window.console.log(ex);
      // audiooutput
    }
  }

  // eslint-disable-next-line class-methods-use-this
  checkAudioDevice(callback) {
    // 判断操作系统类型，决定是否检查设备状态
    // warning 此处不确定这样判断是不是存在问题
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry/i.test(navigator.userAgent)) {
      callback();
      return;
    }
    if (navigator.mediaDevices && navigator.mediaDevices.enumerateDevices) {
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
          window.console.log(JSON.stringify(err));
        });
    }
  }

  feed(data) {
    const tmp = new Uint8Array(this.samples.length + data.length);
    tmp.set(this.samples, 0);
    tmp.set(data, this.samples.length);
    // console.log(tmp, data);
    this.samples = tmp;
    // 实时码流，立即播放
    if (!this.tmrFulsh) {
      this.flush();
    }
  }

  flush() {
    if (!this.audioCtx) {
      // 判断是否具有播放设备，可惜接口需要https才支持
      // this.checkAudioDevice(() => {
      this.createAudioContext();
      // });
    }
    if (!this.audioCtx || !this.samples.length) return;
    const temp = this.samples.slice(0);
    this.samples = new Uint8Array();
    this.audioCtx.decodeAudioData(
      temp.buffer,
      (buffer) => {
        try {
          // 解码成功时的回调函数
          this.audioBuffer = buffer;
          this.source = this.audioCtx.createBufferSource();
          this.source.buffer = this.audioBuffer;
          if (this.startTime < this.audioCtx.currentTime) {
            this.startTime = this.audioCtx.currentTime;
          }
          this.source.connect(this.gainNode);
          this.source.start(this.startTime); // 立即播放
        } catch (ex) {
          window.console.log(ex);
        }
        this.startTime += this.audioBuffer.duration;
      },
      (e) => {
        // 解码出错时的回调函数
        window.console.log('Error decoding', e);
      },
    );
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
    if (this.tmrFulsh) {
      clearInterval(this.tmrFulsh);
    }
    if (this.audioCtx) {
      this.audioCtx.close();
    }
    this.audioCtx = null;
  }
}

export default Mp3Player;

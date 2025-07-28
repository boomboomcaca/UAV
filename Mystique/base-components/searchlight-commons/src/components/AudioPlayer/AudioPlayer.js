import MP3Player from './MP3Player';
import PCMPlayer from './PCMPlayer';

/**
 * 构造函数
 * streamType: string ['demodstream', 'filestream']  default 'demodstream'
 * 设定是否为解码流，解码流需要实时播放，不能有延迟
 * @param {object} option  {streamType:'demodstream'}   {streamType:'filestream'}
 */
function AudioPlayer(option) {
  const defaults = {
    streamType: 'demodstream',
  };
  this.option = { ...defaults, ...option };
}

AudioPlayer.prototype.feed = function initSound(audioData) {
  const { format, samplingRate, channels, bitsPerSample, data } = audioData;
  this.initPlayer(format, samplingRate, channels, bitsPerSample);
  if (data && this.player) {
    this.player.feed(new Uint8Array(data));
  }
};

AudioPlayer.prototype.initPlayer = function initPlayer(format, samplingRate, channelNumber, bitsPerSample) {
  if (!this.audioFormat || this.audioFormat !== format || this.samplingRate !== samplingRate) {
    this.audioFormat = format;
    this.samplingRate = samplingRate;
    if (this.player) {
      this.destroy();
    }
    let code = '16bitInt';
    if (bitsPerSample === 8) {
      code = '8bitInt';
    }
    if (bitsPerSample === 32) {
      code = '32bitInt';
    }
    // 解码流之所以为0，是为了实时播放，避免延迟；文件流则以1s为时间间隔播放缓存
    const flushingTime = this.option.streamType === 'demodstream' ? 0 : 1000;
    if (format === 'pcm') {
      this.player = new PCMPlayer({
        encoding: code,
        channels: channelNumber || 1,
        sampleRate: samplingRate || 22050,
        flushingTime,
      });
    } else {
      this.player = new MP3Player({
        flushingTime,
      });
    }
  }
};

AudioPlayer.prototype.suspend = function suspendPlay() {
  if (this.player) {
    this.player.suspend();
  }
};

AudioPlayer.prototype.resume = function resumePlay() {
  if (this.player) {
    this.player.resume();
  }
};

AudioPlayer.prototype.destroy = function destroy() {
  if (this.player) {
    this.player.destroy();
  }
  this.player = null;
};

export default AudioPlayer;

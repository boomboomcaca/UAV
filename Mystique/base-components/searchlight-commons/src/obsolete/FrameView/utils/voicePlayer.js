import AudioPlayer from '../../AudioPlayer';

// ########################################
// #########    1.创建实例        ##########
// ########################################
let audioPlayer = null;
let canFeed = false;

const initPlayer = () => {
  audioPlayer = new AudioPlayer();
  canFeed = true;
};

// ########################################
// #######    2.传入播放数据        ########
// ########################################
const playData = (buffer) => {
  if (audioPlayer && canFeed) {
    if (buffer && buffer.data) {
      audioPlayer.feed(buffer);
    }
  }
};

// ########################################
// #######    3.播放与暂停          ########
// ########################################
const play = (isPlay) => {
  if (audioPlayer) {
    isPlay ? audioPlayer.resume() : audioPlayer.suspend();
  }
};

// ########################################
// ########    4.销毁实例        ###########
// ########################################
const killPlayer = (tag = null) => {
  canFeed = false;
  if (audioPlayer) {
    audioPlayer.destroy();
    audioPlayer = null;
  }
};

export default { initPlayer, playData, play, killPlayer };

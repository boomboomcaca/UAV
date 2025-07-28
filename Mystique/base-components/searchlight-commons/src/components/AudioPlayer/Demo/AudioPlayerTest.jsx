import React, { useEffect, useState } from 'react';
import { Button, Switch } from 'dui';
import AudioPlayer from '../AudioPlayer';

const homepage = './';

const AudioPlayerTest = () => {
  // 是否为mp3数据
  const [isMP3, setIsMP3] = useState(false);
  // 播放器实例
  const [player, setPlayer] = useState(undefined);
  // 是否正在播放着
  const [playing, setPlaying] = useState(false);
  // 是否已暂停
  const [suspend, setSuspend] = useState(false);
  // 音频数据设置计数器
  const [timer, setTimer] = useState(undefined);

  const playAudio = () => {
    window.console.log('playAudio');
    const url = process.env.NODE_ENV === 'production' ? './public/audioTest.dat' : `.${homepage}/audioTest.dat`;
    window.console.log(homepage, url);
    fetch(url, {
      method: 'get',
      responseType: 'arraybuffer', // 'blob',
    })
      .then((res) => {
        return res.arrayBuffer();
      })
      .then((u8Buffer) => {
        // const u8Buffer = new Uint8Array(buffer);
        // 起始时间
        const startTime = new Date().getTime();
        let count = 0;
        const tmr = setInterval(() => {
          if (player && playing && !suspend) {
            // 误差调整
            if (count > 0 && count % 20 === 0) {
              const curTime = new Date().getTime();
              const gap = curTime - startTime;
              if (count * 1000 - gap >= 1000) {
                return;
              }
            }
            // 音频采样率为22050 ， 故每次给入1s 2s ?数据
            const start = count * 44100;
            let len = 44100;
            if (start + len > u8Buffer.length) {
              len = u8Buffer.length - start;
              count = -1;
            }
            const once = u8Buffer.slice(start, start + len);
            // ########################################
            // #######    2.eg:传入播放数据    ########
            // ########################################
            const data = {
              format: 'pcm',
              samplingRate: '22050',
              channelNumber: 1,
              bitsPerSample: 16,
              data: once,
            };
            player.feed(data);
            count += 1;
          }
        }, 500);
        setTimer(tmr);
      });
  };

  const playMp3Audio = () => {
    window.console.log('playAudio');
    const url = process.env.NODE_ENV === 'production' ? './mp3test.dat' : `.${homepage}/mp3test.dat`;
    window.console.log(homepage, url);
    fetch(url, {
      method: 'get',
      responseType: 'arraybuffer', // 'blob',
    })
      .then((res) => {
        return res.arrayBuffer();
      })
      .then((u8Buffer) => {
        // const u8Buffer = new Uint8Array(buffer);
        // Object.prototype.toString.call(buffer);
        // 起始时间
        const startTime = new Date().getTime();
        let count = 1;
        const tmr = setInterval(() => {
          if (player && playing && !suspend) {
            // 误差调整
            if (count > 0 && count % 20 === 0) {
              const curTime = new Date().getTime();
              const gap = curTime - startTime;
              if (count * 1000 - gap >= 1000) {
                return;
              }
            }
            // 音频采样率为22050 ， 故每次给入1s 2s ?数据
            const start = count * 44100;
            let len = 44100;
            if (start + len > u8Buffer.length) {
              len = u8Buffer.length - start;
              count = -1;
            }
            const once = u8Buffer.slice(start, start + len);
            // ########################################
            // #######    2.eg:传入播放数据    ########
            // ########################################
            const data = {
              data: once,
              format: 'mp3',
            };
            player.feed(data);
            count += 1;
          }
        }, 500);
        setTimer(tmr);
      });
  };

  const initialize = () => {
    // ########################################
    // #########    1.eg:创建实例    ##########
    // ########################################
    // 注意：对于实时监测任务播放音频 应传入 不传  或 'demodstream'
    const audioPlayer = new AudioPlayer({ streamType: 'filestream' });
    // const audioPlayer = new AudioPlayer();
    setPlayer(audioPlayer);
  };

  // 播放
  const play = () => {
    setPlaying(true);
  };

  // 暂停
  const suspendPlay = () => {
    setSuspend(true);
    player.suspend();
  };

  // 继续
  const resumePlay = () => {
    setSuspend(false);
    player.resume();
  };

  // 销毁播放器
  const destroy = () => {
    if (player) {
      window.console.log('destroy');
      setPlaying(false);
      // clearInterval(timer);
      // player.destroy();
      // setPlayer(undefined);
    }
  };

  useEffect(() => {
    window.console.log('useEffect', playing, isMP3);
    if (playing) {
      if (isMP3) {
        playMp3Audio();
      } else {
        playAudio();
      }
    }
    if (!playing && player) {
      clearInterval(timer);
      // ########################################
      // ########    4.eg:销毁实例    ###########
      // ########################################
      player.destroy();
      setPlayer(undefined);
    }
  }, [playing]);

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
      <Switch
        checkedChildren="MP3"
        unCheckedChildren="PCM"
        checked={isMP3}
        disabled={playing}
        onChange={(e) => setIsMP3(e)}
      />
      <Button disabled={player !== undefined} onClick={() => initialize()}>
        实例化
      </Button>
      <Button disabled={playing || player === undefined} onClick={() => play(true)}>
        播放
      </Button>
      {/* <Button
        disabled={!playing || player === undefined}
        onClick={() => flush()}
      >
        flush
      </Button> */}
      <Button disabled={player === undefined} onClick={() => destroy()}>
        销毁
      </Button>
      <div>
        <Button disabled={!playing || suspend} onClick={() => suspendPlay()}>
          暂停
        </Button>
        <Button disabled={!suspend} onClick={() => resumePlay()}>
          继续
        </Button>
      </div>
    </div>
  );
};

export default AudioPlayerTest;

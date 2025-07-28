/* eslint-disable */
import React, { useEffect, useState, useRef } from 'react';
import { Button } from 'dui';
import PCMPlayer from '../PCMPlayer';
import MP3Player from '../MP3Player';

const homepage = './';

const PCMPlayerTest = () => {
  // 播放器实例
  const [player, setPlayer] = useState(undefined);
  // 是否正在播放着
  const [playing, setPlaying] = useState(false);
  // 是否已暂停
  const [suspend, setSuspend] = useState(false);
  // 音频数据设置计数器
  const [timer, setTimer] = useState(undefined);

  const dom = useRef();
  useEffect(() => {
    if (dom.current) {
      // const dd = document.createElement('audio');
      // dd.src = 'gsm610.wav';
      // dom.current.appendChild(dd);
    }
  }, [dom.current]);

  useEffect(() => {
    window.console.log('useEffect', playing);
    if (playing) {
      playAudio();
    }
    if (!playing && player) {
      clearInterval(timer);
      // ########################################
      // ########    3.eg:销毁实例    ###########
      // ########################################
      player.destroy();
      setPlayer(undefined);
    }
  }, [playing]);

  // const isDoubleChannel = true;
  // $$$$$ gsm610 浏览器本身不支持
  const format = 'gsm610'; // "pcm2Chanl" "gsm610"

  const playAudio = () => {
    window.console.log('playAudio');
    let url = process.env.NODE_ENV === 'production' ? './public/audioTest.dat' : `.${homepage}/audioTest.dat`;
    if (format === 'pcm2Chanl') {
      url = process.env.NODE_ENV === 'production' ? './public/Uncover.wav' : `.${homepage}/Uncover.wav`;
    }
    if (format === 'gsm610') {
      url = process.env.NODE_ENV === 'production' ? './public/gsm610.wav' : `.${homepage}/gsm610.wav`;
    }
    window.console.log(homepage, url);
    fetch(url, {
      method: 'get',
      responseType: 'arraybuffer', // 'blob',
    })
      .then((res) => {
        return res.arrayBuffer();
      })
      .then((buffer) => {
        const u8Buffer = new Uint8Array(buffer);
        // 起始时间
        const startTime = new Date().getTime();
        let count = 0;
        const tmr = setInterval(() => {
          if (player && playing && !suspend) {
            // 误差调整
            // if (count > 0 && count % 20 === 0) {
            //   const curTime = new Date().getTime();
            //   const gap = curTime - startTime;
            //   if (count * 1000 - gap >= 1000) {
            //     return;
            //   }
            // }
            // 音频采样率为22050 ， 故每次给入1s 2s ?数据
            const dataLen = format === 'gsm610' ? 16000 : 44100;
            const start = count * dataLen;
            let len = dataLen;
            if (start + len > u8Buffer.length) {
              len = u8Buffer.length - start;
              count = -1;
            }
            const once = u8Buffer.slice(start, start + len);
            // ########################################
            // #######    2.eg:传入播放数据    ########
            // ########################################
            player.feed(once);
            count += 1;
          }
        }, 450);
        setTimer(tmr);
      });
  };

  const initialize = () => {
    // ########################################
    // #########    1.eg:创建实例    ##########
    // ########################################
    const audioPlayer = new PCMPlayer({
      encoding: format === 'gsm610' ? '16bitInt' : '16bitInt',
      channels: format === 'pcm2Chanl' ? 2 : 1,
      sampleRate: format === 'gsm610' ? 8000 : 22050,
      flushingTime: 2000,
    });
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
    }
  };

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }} ref={dom}>
      <span>PCM</span>
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
      {/* <video width="320" height="240" controls>
        <source src="gsm610.wav" type="video/mp4" />
      </video> */}
      <audio controls>
        <source src="gsm610.wav" type="audio/ogg" />
      </audio>
    </div>
  );
};

export default PCMPlayerTest;

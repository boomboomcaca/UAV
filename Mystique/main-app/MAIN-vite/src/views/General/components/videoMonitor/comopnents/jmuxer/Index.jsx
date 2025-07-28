import React, { useEffect, useState, useRef } from "react";
import PropTypes from "prop-types";
import JMuxer from "jmuxer";

import VideoBar from "../videoBar/Index.jsx";
import { blobToArrayBuffer } from "../../../../../../utils/publicFunc.js";
import styles from "./style.module.less";

const JmuxerPlayer = (props) => {
  const { wsUrl, maxView, onTrack, onItemClick } = props;
  const [videoId] = useState(String(Math.random()).slice(10));
  const [mdPos, setMDPOS] = useState({ x: 0, y: 0 });
  const [curMPOS, setCurMPOS] = useState({ x: 0, y: 0 });
  /**
   * @type {{current:JMuxer}}
   */
  const playerRef = useRef();
  /**
   * @type {{current:WebSocket}}
   */
  const socketRef = useRef();
  // 当前页面在后台显示？
  const pageInBackground = useRef(false);

  const initPlayer = () => {
    playerRef.current = new JMuxer({
      node: document.getElementById(videoId),
      debug: false,
      mode: "video",
      // flushingTime: 2000,
      flushingTime: 1,
      onError: (e) => {
        console.log("error:::", e);
      },
    });
  };

  const initSocket = (wsUrl) => {
    const socket = new WebSocket(wsUrl);
    // 处理 WebSocket 事件
    socket.onopen = () => {
      console.log("WebSocket connected");
    };

    socket.onmessage = (event) => {
      if (!pageInBackground.current) {
        blobToArrayBuffer(event.data).then((arrayBuffer) => {
          if (arrayBuffer.byteLength === 4) {
            const view = new DataView(arrayBuffer);
            duration = view.getInt32(0, true);
          } else {
            playerRef.current.feed({
              video: new Uint8Array(arrayBuffer),
              // duration,
            });
          }
        });
      }
    };

    socket.onclose = () => {
      console.log("WebSocket disconnected");
    };
    socketRef.current = socket;
  };

  useEffect(() => {
    if (wsUrl && !playerRef.current) {
      let visibilityChangeHandler = () => {
        if (document.hidden) {
          // 浏览器在后台
          pageInBackground.current = true;
        } else {
          // 浏览器在前台
          pageInBackground.current = false;
        }
      };
      document.addEventListener("visibilitychange", visibilityChangeHandler);
      initPlayer();
      document.getElementById(videoId).playbackRate = 1.25;
      initSocket(wsUrl);

      return () => {
        window.removeEventListener("visibilitychange", visibilityChangeHandler);
        visibilityChangeHandler = undefined;
        socketRef.current.close();
        playerRef.current.destroy();
        playerRef.current = null;
      };
    }
    return null;
  }, [wsUrl]);

  return (
    <div className={styles.playerCon}>
      <video className={styles.videoTag} id={videoId} muted autoPlay />
      <div
        className={styles.opmask}
        onMouseDown={(e) => {
          const ele = document.getElementById(videoId);
          const rect = ele.getBoundingClientRect();
          const pos = { x: e.clientX - rect.left, y: e.clientY - rect.top };
          setMDPOS(pos);
          setCurMPOS(pos);
        }}
        onMouseMove={(e) => {
          if (curMPOS.y > 0) {
            const ele = document.getElementById(videoId);
            const rect = ele.getBoundingClientRect();
            console.log(e.clientX, e.clientY, rect);
            const pos = { x: e.clientX - rect.left, y: e.clientY - rect.top };
            setCurMPOS(pos);
          }
        }}
        onMouseUp={(e) => {
          const ele = document.getElementById(videoId);
          const rect = ele.getBoundingClientRect();
          const { width, height } = rect;
          const wScale = 704 / width;
          const hScale = 576 / height;
          onTrack([
            Math.floor(mdPos.x * wScale),
            Math.floor(curMPOS.x * wScale),
            Math.floor(mdPos.y * hScale),
            Math.floor(curMPOS.y * hScale),
          ]);
          setMDPOS({ x: 0, y: 0 });
          setCurMPOS({ x: 0, y: 0 });
        }}
      />
      {mdPos.y > 0 && (
        <div
          className={styles.rect}
          style={{
            left: mdPos.x,
            top: mdPos.y,
            width: curMPOS.x - mdPos.x,
            height: curMPOS.y - mdPos.y,
          }}
        />
      )}
      {maxView && (
        <div className={styles.bar}>
          <VideoBar onItemClick={onItemClick} />
        </div>
      )}
    </div>
  );
};

JmuxerPlayer.defaultProps = {
  wsUrl: "",
  maxView: false,
  onItemClick: () => {},
  onTrack: () => {},
};

JmuxerPlayer.propTypes = {
  wsUrl: PropTypes.string,
  maxView: PropTypes.bool,
  onItemClick: PropTypes.func,
  onTrack: PropTypes.func,
};

export default JmuxerPlayer;

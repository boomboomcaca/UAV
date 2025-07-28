import React, { useState, useEffect, useRef } from "react";
// import PropeTypes from "prop-types";

import { mainConf } from "../../config";
import minIcon from "./icons/minimize.png";
import maxIcon from "./icons/maximize.png";
import closeIcon from "./icons/close.png";
import normalIcon from "./icons/normal-size.png";
import styles from "./style.module.less";

const ElectronHeader = (props) => {
  const [formState, setFormState] = useState("maximize");
  const [visible, setVisible] = useState(false);
  const mouseDownRef = useRef();
  const hideTimeout = useRef();
  const headerIdRef = useRef([
    `electron_${new Date().getTime()}`,
    `electron1_${new Date().getTime()}`,
    `devicon_${new Date().getTime()}`,
  ]);
  const dragging = useRef(false);

  const sendMessage2Main = ({ name, value }) => {
    window.ipcRenderer?.send("client", {
      name,
      value,
      hash: window.location.hash,
    });
  };

  useEffect(() => {
    let bodyMouseMove = (e) => {
      if (e.clientY < 10) {
        setVisible(true);
        if (hideTimeout.current) {
          clearTimeout(hideTimeout.current);
          hideTimeout.current = undefined;
        }
      }
      if (dragging.current) {
        sendMessage2Main({
          name: "dragging",
          value: { x: e.screenX, y: e.screenY },
        });
      }
    };
    let bodyMouseUp = () => {
      if (dragging.current) sendMessage2Main({ name: "stopdrag" });
      dragging.current = false;
    };
    document.body.addEventListener("mousemove", bodyMouseMove);
    document.body.addEventListener("mouseup", bodyMouseUp);
    return () => {
      document.body.removeEventListener("mousemove", bodyMouseMove);
      document.body.removeEventListener("mouseup", bodyMouseUp);
      bodyMouseMove = undefined;
      bodyMouseUp = undefined;
    };
  }, []);

  return (
    <div
      id={headerIdRef.current[0]}
      className={`${styles.eleHeader} ${visible && styles.show}`}
      onMouseLeave={() => {
        if (!dragging.current) {
          hideTimeout.current = setTimeout(() => {
            setVisible(false);
          }, 1000);
        }
      }}
      onMouseDown={(e) => {
        if (headerIdRef.current.includes(e.target.id)) {
          dragging.current = true;
          sendMessage2Main({
            name: "startdrag",
            value: { x: e.screenX, y: e.screenY },
          });
        }
      }}
      onMouseMove={(e) => {
        // if (headerIdRef.current.includes(e.target.id)) {
        //   window.ipcRenderer?.send("client", {
        //     name: "dragging",
        //     value: { x: e.screenX, y: e.screenY },
        //   });
        // }
      }}
      onMouseUp={(e) => {
        dragging.current = false;
        sendMessage2Main({ name: "stopdrag" });
      }}
    >
      <div className={styles.title} id={headerIdRef.current[1]}>
        <div
          id={headerIdRef.current[2]}
          className={styles.logo}
          onMouseDown={(e) => {
            if (
              !mouseDownRef.current &&
              !dragging.current &&
              e.target.id === headerIdRef.current[2]
            ) {
              mouseDownRef.current = true;
              setTimeout(() => {
                if (mouseDownRef.current) {
                  mouseDownRef.current = false;
                  console.log("opendevtools");
                  sendMessage2Main({ name: "opendevtools" });
                }
              }, 1200);
            }
          }}
          onMouseUp={(e) => {
            mouseDownRef.current = false;
            console.log("logo onMouseUp::", e.target, mouseDownRef.current);            
          }}
        />
        <div>{mainConf.systemName}</div>
      </div>
      <div className={styles.buttons}>
        {[
          { name: "minimize", icon: minIcon },
          { name: "maximize", icon: maxIcon },
          { name: "normal", icon: normalIcon },
          { name: "close", icon: closeIcon },
        ].map((item) => {
          if (item.name !== formState) {
            return (
              <div
                className={styles.button}
                onClick={() => {
                  console.log(item.name);
                  // window.ipcRenderer?.send("client", { name: item.name });
                  sendMessage2Main({ name: item.name });
                  if (["maximize", "normal"].includes(item.name)) {
                    setFormState(item.name);
                  }
                }}
              >
                <img src={item.icon} />
              </div>
            );
          }
          return null;
        })}
      </div>
    </div>
  );
};

ElectronHeader.defaultProps = {
  //   visible: false,
  //   onItemClicked: () => {},
};

ElectronHeader.prototype = {
  //   visible: PropeTypes.bool,
  //   onItemClicked: PropeTypes.func,
};

export default ElectronHeader;

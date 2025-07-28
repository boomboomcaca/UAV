import React, { useState, useEffect, useRef } from "react";
import { useLocation, useHistory, Link } from "react-router-dom";
import { Tooltip } from "react-tooltip";

import { ReactComponent as BorderBold } from "./icons/home_border_bold.svg";
import { ReactComponent as Border } from "./icons/home_border.svg";

import Entry from "./components/Entry/Index";
import General from "../General/General1.jsx";
import Header from "../../components/Header/Index";
import { removeToken } from "../../utils/auth";
import { parseSearchParams, disposeCCE } from "../../utils/publicFunc";
import { mainConf } from "../../config";

import styles from "./style.module.less";

const Home = (props) => {
  // 接收route传值
  const location = useLocation();
  const history = useHistory();
  const [hasRegion, setHasRegion] = useState(true);
  /**
   * @type {{current:HTMLDivElement}}
   */
  const maskRef = useRef();
  const [fitGeneral, setFitGeneral] = useState(false);
  const [mouseInMask, setMouseInMask] = useState(false);
  const [hasAlarm, setHasAlarm] = useState(false);
  // 鼠标是否在中心区域，在则可穿透map事件
  const [mouseInCenter, setMouseInCenter] = useState(false);

  useEffect(() => {
    window.alarmToHome = (e) => {
      if (e) setHasAlarm(e.alarmStatus !== "allClear");
    };
    const rect = maskRef.current.getBoundingClientRect();
    const x = rect.left + window.screenLeft * window.devicePixelRatio;
    const y = rect.top + window.screenTop * window.devicePixelRatio;
    const mouseMoveBody = (e) => {
      if (maskRef.current) {
        const offsetX = e.screenX - x;
        const offsetX1 = rect.width - offsetX;
        const offsetY = e.screenY - y;
        // 判断当前鼠标位置，在指定区域则可事件穿透
        if (
          ((offsetX > 390 / window.devicePixelRatio &&
            offsetY > 170 / window.devicePixelRatio) ||
            (offsetY > 310 / window.devicePixelRatio &&
              offsetX > 65 / window.devicePixelRatio)) &&
          offsetX1 > 180 / window.devicePixelRatio
        ) {
          maskRef.current.style.pointerEvents = "none";
        } else {
          maskRef.current.style.pointerEvents = "all";
        }
      }
    };
    // document.body.addEventListener("mousemove", mouseMoveBody);

    return () => {
      // document.body.removeEventListener("mousemove", mouseMoveBody);
    };
  }, []);

  useEffect(() => {
    console.log("location effect===", location);
    // const { indexEntry } = state;
    // if (!indexEntry || indexEntry.length === 0) return;
    const params = parseSearchParams(
      window.location.search || window.location.hash
    );
    const { fit } = params;

    setFitGeneral(fit || false);
  }, [location]);

  useEffect(() => {
    setHasRegion(localStorage.getItem("regionconfig"));
  }, []);

  return (
    <div className={styles.homeroot}>
      {/* <div> */}
      {!fitGeneral && (
        <Header
          title={mainConf.systemName}
          backVisible={false}
          homePage
          onLoginout={() => {
            removeToken();
            history.replace("/");
          }}
        />
      )}
      {/* </div> */}
      <div className={styles.contentContainer}>
        <div className={styles.content}>
          <svg>
            <defs>
              <clipPath id="indexBj01" clipPathUnits="objectBoundingBox">
                {/* <path
                  transform="scale(0.0010622,0.0020800)"
                  d="M 47.5,-0.5 C 329.5,-0.5 611.5,-0.5 893.5,-0.5C 915.467,52.0299 929.633,106.697 936,163.5C 937.936,182.288 939.436,200.955 940.5,219.5C 940.5,231.167 940.5,242.833 940.5,254.5C 937.918,330.238 922.585,403.404 894.5,474C 611.833,474.667 329.167,474.667 46.5,474C 15.2085,398.373 -0.124802,319.54 0.5,237.5C 0.156139,157.053 14.6561,79.3865 44,4.5C 44.6972,2.4156 45.8639,0.748938 47.5,-0.5 Z M 48.5,2.5 C 329.834,2.33333 611.167,2.5 892.5,3C 944.957,137.152 951.79,273.652 913,412.5C 907.2,432.907 900.033,452.74 891.5,472C 610.5,472.667 329.5,472.667 48.5,472C 3.40499,359.297 -8.42834,243.13 13,123.5C 20.5117,81.8082 32.345,41.4749 48.5,2.5 Z"
                /> */}
                <path
                  transform="scale(0.0005676,0.0010888)"
                  d="M87.8324 907.843C-64.0577 544.469 14.6567 184.098 87.8581 12.2831C90.9498 5.02643 98.0783 0.5 105.966 0.5H1657.9C1665.86 0.5 1673.09 5.24855 1676.17 12.581C1822.69 360.962 1747.83 734.727 1676.08 908.058C1673.03 915.405 1665.86 920 1657.91 920H106.144C98.1657 920 90.9094 915.204 87.8324 907.843Z"
                />
              </clipPath>
              <clipPath id="indexBj010" clipPathUnits="objectBoundingBox">
                <path
                  transform="scale(0.0010655,0.0018500) ,translate(0, 52)"
                  d="M 47.5,-0.5 C 329.5,-0.5 611.5,-0.5 893.5,-0.5C 915.467,52.0299 929.633,106.697 936,163.5C 937.936,182.288 939.436,200.955 940.5,219.5C 940.5,231.167 940.5,242.833 940.5,254.5C 937.918,330.238 922.585,403.404 894.5,474C 611.833,474.667 329.167,474.667 46.5,474C 15.2085,398.373 -0.124802,319.54 0.5,237.5C 0.156139,157.053 14.6561,79.3865 44,4.5C 44.6972,2.4156 45.8639,0.748938 47.5,-0.5 Z M 48.5,2.5 C 329.834,2.33333 611.167,2.5 892.5,3C 944.957,137.152 951.79,273.652 913,412.5C 907.2,432.907 900.033,452.74 891.5,472C 610.5,472.667 329.5,472.667 48.5,472C 3.40499,359.297 -8.42834,243.13 13,123.5C 20.5117,81.8082 32.345,41.4749 48.5,2.5 Z"
                />
              </clipPath>
            </defs>
          </svg>
          {!fitGeneral && (
            <>
              <Border className={styles.borderSvg} />
              <div className={styles.borderTick}>
                {hasAlarm ? (
                  <>
                    {/* <div className={`${styles.redLeft}`} />
                    <div className={`${styles.redRight}`} /> */}
                    <div className={`${styles.red}`} />
                  </>
                ) : (
                  <>
                    {/* <div className={`${styles.greenLeft}`} />
                    <div className={`${styles.greenRight}`} /> */}
                    <div className={`${styles.green}`} />
                  </>
                )}
              </div>

              <BorderBold className={styles.borderbold} />
              {/* 高亮box-shadow层 */}
              <div
                className={`${styles.maskBorder} ${
                  mouseInMask && styles.mouein
                }`}
              >
                <div />
              </div>
            </>
          )}
          <div className={`${styles.mapcon} ${!fitGeneral && styles.nofit}`}>
            <General />
          </div>
          {!fitGeneral && (
            <>
              <div
                ref={maskRef}
                className={styles.generalMask}
                onMouseOver={(e) => {
                  setMouseInMask(true);
                }}
                onMouseOut={() => {
                  setMouseInMask(false);
                }}
                onClick={() => {
                  setMouseInMask(false);
                  history.push("/index?fit=true");
                }}
              >
                <div
                  className={styles.testBorder1}
                  style={{
                    backgroundColor: mouseInCenter
                      ? "rgba(90, 0, 60, 0.1)"
                      : "unset",
                  }}
                  onMouseEnter={() => {
                    setMouseInCenter(false);
                  }}
                />
                <div
                  className={`${styles.testBorder2} ${
                    mouseInCenter && styles.showShadow
                  }`}
                />
                <div
                  className={styles.testBorder}
                  style={{ pointerEvents: mouseInCenter ? "none" : "all" }}
                  onMouseEnter={() => {
                    setMouseInCenter(true);
                  }}
                />
              </div>

              <Entry />
            </>
          )}
        </div>
      </div>
      {!hasRegion && (
        <>
          <Tooltip
            id="my-tooltaierep981"
            style={{ color: "#f26f55", zIndex: 9999 }}
          />
          <div
            className={styles.warning}
            data-tooltip-id="my-tooltaierep981"
            data-tooltip-content="当前未设置防护区"
            data-tooltip-place="bottom"
          />
        </>
      )}
    </div>
  );
};

export default Home;

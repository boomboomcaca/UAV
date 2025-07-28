import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import dayjs from "dayjs";

import { ReactComponent as BackIcon } from "../../assets/icons/header_back.svg";
import { ReactComponent as MoreIcon } from "../../assets/icons/header_more.svg";
import styles from "./style.module.less";

const LoginOutIcon = (props) => {
  const { className } = props;
  return (
    <svg
      width="32"
      height="32"
      viewBox="0 0 40 40"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <g>
        <path
          className={className}
          d="M27.8359 28.9543C26.069 30.5526 23.9136 31.5721 21.6252 31.8919C19.3368 32.2117 17.0112 31.8184 14.9239 30.7587C12.8366 29.699 11.0751 28.0172 9.84807 25.9126C8.62103 23.808 7.97985 21.3687 8.00048 18.8837C8.02112 16.3988 8.70271 13.9722 9.96451 11.8916C11.2263 9.81094 13.0155 8.16336 15.12 7.14401C17.2245 6.12466 19.5563 5.77626 21.839 6.14008C24.1218 6.50391 26.2599 7.56471 28 9.19683"
          stroke="white"
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          className={className}
          d="M18 19H32"
          stroke="white"
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          className={className}
          d="M27 24L32 19L27 14"
          stroke="white"
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </g>
    </svg>
  );
};

const Header = (props) => {
  const {
    title,
    backVisible,
    moreVisible,
    onBack,
    onMore,
    homePage,
    onLoginout,
  } = props;
  const [timeStr, setTimeStr] = useState();

  const [usrName] = useState(sessionStorage.getItem("userName"));

  useEffect(() => {
    let prevTime = new Date().getTime();
    const updateTime = () => {
      if (new Date().getTime() - prevTime > 900) {
        setTimeStr(dayjs().format("YYYY-MM-DD HH:mm:ss"));
        prevTime = new Date().getTime();
      }
      requestAnimationFrame(updateTime);
    };
    updateTime();
    return () => {
      // TODO
    };
  }, []);

  return (
    <div className={styles.headerRoot}>
      <div className={styles.titleLayer}>
        <span />
        <div>
          <div className={styles.title}>{title}</div>
        </div>
        <span />
      </div>
      <div className={styles.logoLayer}>
        <div className={styles.left}>
          <div className={styles.logo} />
          <div className={styles.buttonsleft}>
            {/* <div className={styles.back} /> */}
            {backVisible && (
              <BackIcon
                className={styles.back}
                onClick={() => {
                  onBack();
                }}
              />
            )}
          </div>
        </div>
        <span />
        <div className={styles.right}>
          <div className={styles.buttonsright}>
            {moreVisible && (
              <MoreIcon
                className={styles.more}
                onClick={() => {
                  onMore();
                }}
              />
            )}
          </div>
          <div className={styles.rightright}>
            <div style={{ fontSize: "14px" }}>{timeStr}</div>
            {homePage && (
              <>
                <div>{usrName}</div>
                <div
                  className={styles.loginoutIcon}
                  onClick={() => {
                    onLoginout();
                  }}
                >
                  <LoginOutIcon className={styles.icon} />
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

Header.defaultProps = {
  title: "",
  backVisible: true,
  moreVisible: false,
  homePage: false,
  onBack: () => {},
  onMore: () => {},
  onLoginout: () => {},
};

Header.prototype = {
  title: PropTypes.string,
  backVisible: PropTypes.bool,
  moreVisible: PropTypes.bool,
  onBack: PropTypes.func,
  onMore: PropTypes.func,
  homePage: PropTypes.bool,
  onLoginout: PropTypes.func,
};

export default Header;

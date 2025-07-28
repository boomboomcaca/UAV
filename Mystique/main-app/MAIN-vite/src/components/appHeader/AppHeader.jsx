import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import { UserIcon, Exit2Icon } from "dc-icon";
import { Link } from "react-router-dom";
import { mainConf } from "../../config";
import styles from "./appHeader.module.less";

const AppHeader = (props) => {
  const { onItemClick } = props;

  const [userName, setuserName] = useState("李查查");

  useEffect(() => {
    const uName = window.sessionStorage.getItem("userName");
    // setuserName(uName || "");
  }, []);

  return (
    <div
      className={styles.loginHeader}
      style={{
        width: "100%",
        height: window.location.hash === "#/login" ? "120px" : "72px",
        backgroundColor: window.location.hash === "#/login" ? "" : "#232949",
        boxShadow:
          window.location.hash === "#/login"
            ? ""
            : "0 4px 2px -2px rgb(255 255 255 / 15%)",
      }}
    >
      <div className={styles.headerIco}>
        {window.location.hash === "#/login" ? (
          <div className={styles.loginDcLogo} alt="decenLogo" />
        ) : (
          <div className={styles.dcLogo} alt="decen" />
        )}
        {mainConf.systemName}
      </div>
      <div style={{ flex: 1 }} />
      {window.location.hash.startsWith("#/index") ? (
        <div className={styles.right}>
          <Link
            to={
              window.location.hash.includes("/entry")
                ? "/index/entry/permission"
                : "/index/permission"
            }
            className={styles.usrIcon}
            key="userIcon"
            style={{
              width: userName && userName.length > 0 ? "auto" : "45px",
              color: "var(--theme-primary)",
            }}
          >
            <UserIcon iconSize={28} color="var(--theme-primary)" /> {userName}
          </Link>
          <div
            className={styles.iconButon}
            onClick={() => onItemClick({ type: "loginout" })}
          >
            <Exit2Icon iconSize={24} color="var(--theme-icon-primary)" />
            退出登录
          </div>
        </div>
      ) : null}
    </div>
  );
};

AppHeader.defaultProps = {
  onItemClick: () => {},
};

AppHeader.propTypes = {
  onItemClick: PropTypes.func,
};

export default AppHeader;

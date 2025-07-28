import React, { useState, useEffect } from "react";
import { message, Modal, Loading } from "dui";
import Logger from "@dc/logger";
import jwtDecode from "jwt-decode";
import { useHistory } from "react-router-dom";
import langT, { setLocale } from "dc-intl";
import { setToken } from "../../utils/auth";
import { login, getToken } from "../../api/account";
import { initConfig } from "../../utils/configManager";
import ConfigComponent from "./components/config.jsx";
import AppHeader from "../../components/appHeader/AppHeader.jsx";
import { ReactComponent as UserIcon } from "../../assets/icons/login_user.svg";
import { ReactComponent as PwdIcon } from "../../assets/icons/login_pwd.svg";
import { mainConf } from "../../config";
import style from "./login.module.less";

const Login = () => {
  const [loading, setLoading] = useState(false);
  const [showConfigModal, setShowConfigModa] = useState(false);
  const [configJson, setConfigJson] = useState();
  const [configData, setConfigData] = useState();
  const [language, setLanguage] = useState("zh");
  const [usrName, setUsrName] = useState();
  const [usrPwd, setUsrPwd] = useState();
  const [usrInutTip, setUsrInputTip] = useState();
  const [pwdInputTip, setPwdInputTip] = useState();
  const [loginTip, setLoginTip] = useState();
  const [verStr, setVerStr] = useState("");
  // 从一体化过来
  const [ticket, saveTicket] = useState();
  const [rememberFlag, setRememberFlag] = useState(() => {
    if (localStorage.getItem("autoLogin")) {
      return true;
    }
    return false;
  });

  const history = useHistory();

  useEffect(() => {
    if (rememberFlag) {
      localStorage.setItem("autoLogin", "yes");
    } else {
      localStorage.removeItem("autoLogin");
    }
  }, [rememberFlag]);

  useEffect(() => {
    fetch("ver.txt").then((res) => {
      res.text().then((r) => {
        console.log(r);
        setVerStr(`V1.${r}`);
      });
    });
  }, []);

  useEffect(() => {
    const lag = localStorage.getItem("language");
    if (lag) {
      setLocale(lag);
      setLanguage(lag);
    } else {
      setLocale(language);
    }
    // 重置截图插件
    const usable = localStorage.getItem("browser-screenshot-plugin");
    if (usable && usable === "2") {
      localStorage.setItem("browser-screenshot-plugin", "0");
    }
  }, []);

  const loginComplete = (complete, useTicket) => {
    complete
      .then((res) => {
        const { result, error } = res;
        if (result) {
          setToken(result, rememberFlag);
          window.sessionStorage.setItem("SET_MIDDLE_VIEW", "banner");
          window.sessionStorage.setItem("userName", jwtDecode(result).userName);
          window.sessionStorage.setItem("usrName", jwtDecode(result).account);
          history.replace("/index");
        }
        if (error) {
          message.error(error.message);
          saveTicket(undefined);
        }
      })
      .finally(() => {
        setLoading(false);
      });
  };

  const handleSubmitFinish = (res) => {
    setLoading(true);
    if (mainConf.mock) {
      setTimeout(() => {
        setLoading(false);
        setToken("mock");
        window.sessionStorage.setItem("userName", "李查查");
        window.sessionStorage.setItem("usrName", "李查查");
        history.replace("/index");
      }, 500);
    } else {
      loginComplete(
        login({
          account: res.userName,
          password: res.password,
        })
      );
    }
  };

  useEffect(() => {
    if (window.baseConf) {
      const oldVal = JSON.stringify(window.baseConf);
      setConfigJson(oldVal);
    }
  }, []);

  const keydownHandle = (e) => {
    const { key, ctrlKey, shiftKey, altKey } = e;
    if (ctrlKey && shiftKey && key === "I" && altKey) {
      setShowConfigModa(true);
    }
    if (ctrlKey && shiftKey && key === "C" && altKey) {
      localStorage.clear();
      try {
        // 清除参数缓存
        indexedDB.deleteDatabase("dcDataTable");
      } catch {
        (er) => window.console.log(er);
      }
      message.success("已清除缓存");
      // 刷新拉取新配置
      window.location.reload();
    }
    if (ctrlKey && shiftKey && key === "L" && altKey) {
      Logger.downloadData();
    }
  };

  useEffect(() => {
    document.body.addEventListener("keydown", keydownHandle);
    return () => {
      document.body.removeEventListener("keydown", keydownHandle);
    };
  }, []);

  const saveConfig = () => {
    try {
      // TODO 校验IP地址和端口号
      setShowConfigModa(false);
      // 存储到本地
      try {
        setConfigJson(configData);
        window.localStorage.setItem("baseConf", configData);
        // 加载本地配置
        initConfig();
      } catch (ex) {
        message.info("配置失败");
        // 清楚
        localStorage.clear();
        // 刷新拉取新配置
        window.location.reload();
      }
    } catch (ex) {
      message.info(`配置错误 ${JSON.stringify(ex)}`);
    }
  };

  useEffect(() => {
    window.onresize = () => {
      // window.location.reload();
    };
    return () => {
      window.onresize = null;
    };
  }, []);

  // useEffect(() => {
  //   // 请求配置
  //   const requestOptions = {
  //     method: "PUT",
  //     headers: {
  //       "Content-Type": "application/json", // 指定请求体的数据类型为 JSON
  //       // 如果有其他请求头，可以在这里添加
  //     },
  //     // body: JSON.stringify({ droneSerialNum: [sn] }), // 将数据转换为 JSON 字符串并设置为请求体
  //   };
  //   // const apiUrl = `${baseUrl}/UavDef/InsertWhiteLists`;
  //   // // 发送 PUT 请求
  //   // return fetch(apiUrl, requestOptions);

  //   let apiUrl = "/UavDef/InsertWhiteLists";

  //   apiUrl += "?";

  //   ["dg", "dgttr"].forEach((val) => {
  //     apiUrl = `${apiUrl}droneSerialNum=${val}&`;
  //   });

  //   //   apiUrl = encodeURIComponent()
  //   fetch(`http://127.0.0.1:8190${apiUrl.slice(0, apiUrl.length - 1)}`, requestOptions).then();
  // }, []);

  return (
    <>
      <div className={style.container}>
        <div
          className={style.leftLogin}
          onClick={(e) => {
            e.preventDefault();
            return null;
          }}
          onDoubleClick={() => setShowConfigModa(true)}
        >
          <AppHeader />
        </div>

        <div className={style.loginfrm}>
          <div className={style.top}>
            <div className={style.l} />
            <span />
            <div className={style.r} />
          </div>
          <div
            className={style.content}
            onKeyDown={(e) => {
              if (e.code && e.code.toLowerCase() === "enter") {
                if (usrName && usrPwd && !loading) {
                  handleSubmitFinish({
                    userName: usrName,
                    password: usrPwd,
                  });
                }
              }
            }}
          >
            <div className={style.title}>
              系统登录
              <div
                style={{
                  textAlign: "center",
                  width: "100%",
                  fontSize: "14px",
                  opacity: 0.6,
                }}
              >
                {verStr}
              </div>
            </div>
            <div className={style.input}>
              <UserIcon className={style.icon} />
              <input
                placeholder="请输入用户名"
                minLength={5}
                maxLength={20}
                value={usrName}
                onChange={(e) => {
                  setUsrName(e.target.value);
                  setUsrPwd("");
                  setPwdInputTip("");
                  setLoginTip("");
                }}
              />
            </div>
            <div className={style.logintip}>{usrInutTip}</div>
            <div className={style.input}>
              <PwdIcon className={style.icon} />
              <input
                placeholder="请输入密码"
                type="password"
                minLength={5}
                maxLength={20}
                value={usrPwd}
                onChange={(e) => {
                  setUsrPwd(e.target.value);
                }}
              />
            </div>
            <div className={style.logintip}>{pwdInputTip}</div>
            <div
              className={`${style.loginbtn} ${loading && style.disable}`}
              onClick={() => {
                console.log(usrName, usrPwd);
                if (usrName && usrPwd && !loading) {
                  handleSubmitFinish({
                    userName: usrName,
                    password: usrPwd,
                  });
                }
              }}
            >
              {loading && (
                <div className={style.loading}>
                  <Loading loadingSize={25} type="single" className={style.loadingStyle} />
                </div>
              )}
              <div className={style.label}>登录</div>
            </div>
            <div className={style.logintip}>{loginTip}</div>
          </div>
          <div className={style.bottom}>
            <div className={style.l} />
            <span />
            <div className={style.r} />
          </div>
        </div>
      </div>
      <Modal
        visible={showConfigModal}
        title="应用配置"
        onOk={saveConfig}
        okText="确定"
        mask
        destroyOnClose
        maskClosable={false}
        className={style.configModal}
        width={550}
        closable={false}
        onCancel={() => {
          setShowConfigModa(false);
        }}
        cancelText="取消"
      >
        <ConfigComponent configJson={configJson} onConfigChange={(value) => setConfigData(value)} />
      </Modal>
    </>
  );
};

export default Login;

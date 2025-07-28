import React, { useEffect, useState, useContext } from "react";
import {
  HashRouter as Router,
  Route,
  Switch,
  Redirect,
  withRouter,
  useLocation,
  useHistory,
} from "react-router-dom";
import jwtDecode from "jwt-decode";
import Logger from "@dc/logger";
import { App } from "@capacitor/app";
import { Toast } from "@capacitor/toast";
import { refreshLogin } from "../../api/account";
import { setToken } from "../../utils/auth";
import routes from "../../routes";
import fetchConfig from "../../utils/configManager";
import styles from "./index.module.less";
import MainContext from "../../context/context.jsx";

const Index = () => {
  const history = useHistory();
  const location = useLocation();
  const { pathname } = location;
  const { auth } = JSON.parse(localStorage.getItem("user")) || "";

  // 全局缓存
  const ctx = useContext(MainContext);
  const [state = "", dispatch = null] = ctx;
  // const [state, dispatch] = useReducer(reducer, initState);
  useEffect(() => {
    // 终极大招，解决配置更新后不能立马更新子应用地址的问题
    // window.projConfig.main.indexEntry = undefined;
    fetchConfig((res) => {
      dispatch({
        type: "setMicroConfig",
        value: res,
      });
    });
  }, []);

  useEffect(() => {
    const token = sessionStorage.getItem("User-Token");
    const autoLogin = localStorage.getItem("autoLogin");
    const alToken = localStorage.getItem("User-Token");
    // 获取session，有则直已登录，没有则需要重新登录||自动登录
    if (token) {
      // 已登陆
      // window.sessionStorage.setItem("SET_MIDDLE_VIEW", "banner");
    } else if (autoLogin && alToken) {
      // 自动登录
      refreshLogin(alToken)
        .then((res) => {
          const decode = jwtDecode(res.result);
          setToken(res.result, !!window.localStorage.getItem("User-Token"));
          window.sessionStorage.setItem("userName", decode.userName);
          window.sessionStorage.setItem("usrName", decode.account);
          // window.sessionStorage.setItem("SET_MIDDLE_VIEW", "banner");
        })
        .catch((ex) => {
          Logger.addError("login", ex);
          // message.info('未授权，请重新登陆');
          history.replace("/login");
        });
    } else {
      history.replace("/login");
    }
  }, []);

  // 移动端退出app处理
  useEffect(() => {
    let timeExit = 0;
    App.addListener("backButton", () => {
      if (pathname === "/index") {
        if (timeExit === 0) {
          Toast.show({
            text: "再按一次退出",
          });
          timeExit = new Date().getTime();
          setTimeout(() => {
            timeExit = 0;
          }, 2000);
        } else {
          const now = new Date().getTime();
          if (now - timeExit <= 2000) {
            App.exitApp();
          }
        }
      } else {
        history.goBack();
      }
    });
    return () => {
      App.removeAllListeners();
    };
  }, []);

  return (
    <div className={styles.content}>
      {routes.length > 0 && (
        <Switch>
          {routes.map((item) => {
            return (
              <Route
                key={item.path}
                path={item.path}
                // exact={item.exact}
                render={(prop) => {
                  console.log("render index", routes, prop);
                  // if (!routes.includes(prop.pathname)) {
                  //   return <Redirect to="/404" {...prop} />;
                  // }
                  if (!prop.match.isExact) {
                    return <Redirect to="/404" {...prop} />;
                  }
                  if (!auth) {
                    return <item.component {...prop} />;
                  }
                  if (item.auth && item.auth.includes(auth)) {
                    return <item.component {...prop} />;
                  }
                  return <Redirect to="/404" {...prop} />;
                }}
              />
            );
          })}
          <Redirect to="/404" />
        </Switch>
      )}
    </div>
  );
};

export default withRouter(Index);

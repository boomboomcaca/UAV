import React, { useReducer, useState, useEffect } from "react";
import { HashRouter as Router, Route, Switch, Redirect, useHistory } from "react-router-dom";
import AsyncLoadable from "./utils/AsyncLoadable.jsx";

// 全局样式，尽量不要在此处修改，会造成主应用/子应用相互影响
import "./styles/App.less";
import "react-tooltip/dist/react-tooltip.css";
// 公共模块 不用异步加载，是为了子应用挂载时dom节点已存在
import Index from "./views/Index/Index.jsx";
import MainContext, { reducer, initState } from "./context/context.jsx";
// import ElectronHeader from "./components/ElectronHeader/Index.jsx";
import { getSystemInfo, initPlatform } from "./utils/capacitorUtils";

// 基础页面
const View404 = AsyncLoadable(() => import(/* webpackChunkName: '404' */ "./views/Others/404"));

const Login = AsyncLoadable(() => import(/* webpackChunkName: 'login' */ "./views/Login"));

// 修改 antd 的css前缀prefixCls 功能，需要配合less 变量一起使用，参见 https://ant.design/components/config-provider-cn/
const App = () => {
  // const [state, dispatch] = useReducer(reducer, initState)
  const [state, dispatch] = useReducer(reducer, initState);
  // const [isElectron, setIsElectron] = useState(false);

  useEffect(() => {
    getSystemInfo(() => {
      // setIsElectron(window.App.platform === "electron");
    });
  }, []);

  const keydownHandle = (e) => {
    const { key, ctrlKey, shiftKey, altKey } = e;
    if (ctrlKey && shiftKey && key === "D" && altKey) {
      window.ipcRenderer?.send("client", {
        name: "opendevtools",
        hash: window.location.hash,
      });
    }
  };

  useEffect(() => {
    document.body.addEventListener("keydown", keydownHandle);
    return () => {
      document.body.removeEventListener("keydown", keydownHandle);
    };
  }, []);

  return (
    <MainContext.Provider value={[state, dispatch]}>
      {/* {isElectron && <ElectronHeader />} */}
      <Router>
        <Switch>
          <Route
            path="/"
            exact
            render={(props) => {
              return <Redirect to="/index" {...props} />;
            }}
          />
          {/* 专门拿来跳转子应用 */}
          {/* <Route
            path="/loadmicro"
            exact
            render={(props) => {
              // history.
              const { location } = props;
              const { search } = location;
              return <Redirect to={`/index${search}`} {...props} />;
            }}
          /> */}
          <Route path="/404" component={View404} />
          <Route path="/login" component={Login} />
          <Route component={Index} />
        </Switch>
      </Router>
    </MainContext.Provider>
  );
};

export default App;

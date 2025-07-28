import React, { useReducer } from 'react';

const initState = {
  // 子应用配置
  microApps: [0],
  // 6 大入口
  indexEntry: [1],
  // 功能权限
  permission: { permission: [] },
  // 功能权限 代码
  permissionCode: [],
  // 快捷跳转参数
  switchParams: undefined,
};

const getRole = (roles) => {
  const arr = [];
  roles.forEach((item) => {
    if (item.children.length === 0) {
      arr.push(item.code.toLowerCase());
    } else {
      item.children.forEach((it) => {
        arr.push(it.code.toLowerCase());
      });
    }
  });
  return arr;
};

const MainContext = React.createContext(initState);

const reducer = (state, action) => {
  switch (action.type) {
    case 'setMicroConfig':
      const newSate = {
        ...state,
        ...action.value,
      };
      return newSate;
    case 'setPermission':
      state.permission = action.value;
      state.permissionCode = getRole(action.value.permission);
      const permission = {
        permission: action.value,
        permissionCode: getRole(action.value.permission),
      };
      return { ...state, ...permission };
    case 'setSwtichParams': {
      const newSate = {
        ...state,
      };
      newSate.switchParams = action.value;
      return newSate;
    }
    default:
      return state;
  }
};

// const [state, dispatch] = useReducer(reducer, initState);

export default MainContext;
export { reducer, initState };

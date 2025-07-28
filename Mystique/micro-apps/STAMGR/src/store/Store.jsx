import React, { useReducer } from 'react';
import PropTypes from 'prop-types';

// 全局状态管理
import { defaultState, reducer } from './reducer';
import AppContext from './context';

const Store = (props) => {
  const { children, actions } = props;
  const [state, dispatch] = useReducer(reducer, { ...defaultState, actions });

  return <AppContext.Provider value={{ state, dispatch }}>{children}</AppContext.Provider>;
};

Store.propTypes = {
  actions: PropTypes.any.isRequired,
  children: PropTypes.any.isRequired,
};

export default Store;

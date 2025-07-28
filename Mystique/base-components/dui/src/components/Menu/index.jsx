import React from 'react';
import PropTypes from 'prop-types';

import styles from './index.module.less';

import AppContext from './context';

const Menu = (props) => {
  const { children, onClick, value } = props;
  return (
    <AppContext.Provider value={{ onClick, selectValue: value }}>
      <div className={styles.menu}>{children}</div>
    </AppContext.Provider>
  );
};

Menu.defaultProps = {
  children: '',
  onClick: () => {},
  value: null,
};

Menu.propTypes = {
  children: PropTypes.any,
  onClick: PropTypes.func,
  value: PropTypes.any,
};

export default Menu;

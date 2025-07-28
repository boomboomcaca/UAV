import React from 'react';
import PropTypes from 'prop-types';
// import styles from './index.module.less';

const Main = (props) => {
  const { children } = props;
  return <>{children}</>;
};

Main.defaultProps = {
  children: null,
};

Main.propTypes = {
  children: PropTypes.any,
};

Main.typeTag = 'Main';

export default Main;

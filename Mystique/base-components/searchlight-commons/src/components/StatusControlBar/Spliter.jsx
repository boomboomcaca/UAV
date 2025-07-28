import React from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';

const Spliter = (props) => {
  const { width } = props;
  return <div className={styles.split} style={{ width }} />;
};

Spliter.defaultProps = {
  width: 8,
};

Spliter.propTypes = {
  width: PropTypes.number,
};

export default Spliter;

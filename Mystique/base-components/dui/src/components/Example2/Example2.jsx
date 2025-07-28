import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Example2 = (props) => {
  const { className } = props;
  return <div className={classnames(styles.root, className)}>Hello Example2</div>;
};

Example2.defaultProps = {
  className: null,
};

Example2.propTypes = {
  className: PropTypes.any,
};

export default Example2;

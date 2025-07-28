import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import EdgeInfo from '..';
import styles from './index.module.less';

const Demo = (props) => {
  const { className } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <EdgeInfo />
    </div>
  );
};

Demo.defaultProps = {
  className: null,
};

Demo.propTypes = {
  className: PropTypes.any,
};

export default Demo;

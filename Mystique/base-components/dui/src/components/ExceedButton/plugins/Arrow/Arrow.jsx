import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
// import ArrowPng from './asset/Arrow.png';
// import ArrowOpenedPng from './asset/ArrowOpened.png';
import Icons from '../../asset';
import styles from './index.module.less';

const Arrow = (props) => {
  const { className, disable, opened } = props;

  return (
    <div className={classnames(styles.root, className)} style={disable ? { opacity: 0.2 } : null}>
      <img alt="" src={opened ? Icons.ArrowOpened : Icons.Arrow} />
    </div>
  );
};

Arrow.defaultProps = {
  className: null,
  opened: false,
  disable: false,
};

Arrow.propTypes = {
  className: PropTypes.any,
  opened: PropTypes.bool,
  disable: PropTypes.bool,
};

export default Arrow;

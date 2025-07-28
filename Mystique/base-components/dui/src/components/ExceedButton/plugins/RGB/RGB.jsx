import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const RGB = (props) => {
  const { className, style, type } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={classnames(styles.rgb, className, styles[type])} style={style} />
    </div>
  );
};

RGB.defaultProps = {
  className: null,
  style: null,
  type: null, // r g b rgb y
};

RGB.propTypes = {
  className: PropTypes.any,
  style: PropTypes.any,
  type: PropTypes.any,
};

export default RGB;

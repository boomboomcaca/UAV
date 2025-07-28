import React from 'react';
import { createPortal } from 'react-dom';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Mask = (props) => {
  const { usePortal, className, children, style, ...retProps } = props;

  const content = (
    <div
      className={classnames(styles.mask, className)}
      style={{ ...style, position: usePortal === true ? 'fixed' : 'absolute' }}
      {...retProps}
    />
  );
  if (usePortal) {
    if (typeof usePortal === 'string') {
      const nodeDiv = document.querySelector(usePortal);
      if (nodeDiv) {
        return createPortal(content, nodeDiv);
      }
    }
    return createPortal(content, document.body);
  }
  return content;
};

Mask.defaultProps = {
  children: null,
  onClick: () => {},
  usePortal: false,
  className: '',
};

Mask.propTypes = {
  children: PropTypes.any,
  onClick: PropTypes.func,
  usePortal: PropTypes.oneOfType([PropTypes.bool, PropTypes.string]),
  className: PropTypes.string,
};

export default Mask;

import React from 'react';
import PropTypes from 'prop-types';
import { createPortal } from 'react-dom';
import { CSSTransition } from 'react-transition-group';

import './animate.less';
import Mask from './Mask.jsx';
import styles from './index.module.less';

const Popup = (props) => {
  const { children, maskclosable, usePortal, visible, onCancel, PopupTransition, destroyOnClose, mask } = props;

  const onMaskClick = (e) => {
    e.stopPropagation();
    if (maskclosable && onCancel && e.target === e.currentTarget) {
      onCancel();
    }
  };

  const content = (
    <>
      {mask && (
        <CSSTransition in={visible} timeout={300} classNames="rtg-fade" unmountOnExit appear>
          <Mask onClick={onMaskClick} usePortal={usePortal} />
        </CSSTransition>
      )}

      <CSSTransition in={visible} timeout={300} classNames={PopupTransition} unmountOnExit appear>
        <div
          className={styles.popup}
          style={{ position: usePortal === true ? 'fixed' : 'absolute' }}
          onClick={onMaskClick}
        >
          {children}
        </div>
      </CSSTransition>
    </>
  );

  if (destroyOnClose && visible === false) {
    return null;
  }

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

Popup.defaultProps = {
  visible: false,
  PopupTransition: 'rtg-slide-up',
  maskclosable: true,
  destroyOnClose: false,
  mask: true,
  onCancel: null,
  usePortal: true,
  children: null,
};

Popup.propTypes = {
  visible: PropTypes.bool,
  PopupTransition: PropTypes.string,
  maskclosable: PropTypes.bool,
  mask: PropTypes.bool,
  destroyOnClose: PropTypes.bool,
  onCancel: PropTypes.func,
  usePortal: PropTypes.oneOfType([PropTypes.bool, PropTypes.string]),
  children: PropTypes.any,
};

export default Popup;

import React, { useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import { createPortal } from 'react-dom';
import { CSSTransition } from 'react-transition-group';
import './animate.less';

import Mask from '../Mask';
import styles from './index.module.less';

const PopUp = (props) => {
  const {
    children,
    maskclosable,
    usePortal,
    visible,
    onCancel,
    popupTransition,
    destroyOnClose,
    mask,
    popStyle,
    clickStop,
  } = props;

  // 判断是直接点击还是在子元素按下鼠标后 在父层移出鼠标触发的点击
  const currentTargetIn = useRef(false);

  const onMaskClick = (e) => {
    if (maskclosable && onCancel && e.target === e.currentTarget && currentTargetIn.current) {
      onCancel();
      currentTargetIn.current = false;
    }
  };

  const onMouseDown = (e) => {
    if (e.target === e.currentTarget) {
      currentTargetIn.current = true;
    }
  };

  useEffect(() => {
    window.banblankstart = visible;
  }, [visible]);

  const content = (
    <>
      {mask && (
        <CSSTransition in={visible} timeout={300} classNames="rtg-fade" unmountOnExit appear>
          <Mask usePortal={usePortal} style={popStyle} />
        </CSSTransition>
      )}

      <CSSTransition in={visible} timeout={300} classNames={popupTransition} unmountOnExit appear>
        <div
          className={styles.popup}
          style={{ ...popStyle, position: usePortal === true ? 'fixed' : 'absolute' }}
          onClick={(e) => {
            if (clickStop) {
              e.stopPropagation();
            }
            onMaskClick(e);
          }}
          onMouseDown={onMouseDown}
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

PopUp.defaultProps = {
  visible: false,
  popupTransition: 'rtg-slide-right',
  maskclosable: true,
  destroyOnClose: false,
  mask: true,
  onCancel: null,
  usePortal: true,
  children: null,
  popStyle: {},
  clickStop: false,
};

PopUp.propTypes = {
  visible: PropTypes.bool,
  popupTransition: PropTypes.string,
  maskclosable: PropTypes.bool,
  mask: PropTypes.bool,
  destroyOnClose: PropTypes.bool,
  onCancel: PropTypes.func,
  usePortal: PropTypes.oneOfType([PropTypes.bool, PropTypes.string]),
  children: PropTypes.any,
  popStyle: PropTypes.object,
  clickStop: PropTypes.bool,
};

export default PopUp;

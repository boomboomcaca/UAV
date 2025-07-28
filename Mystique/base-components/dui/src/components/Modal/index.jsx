import React, { useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import Icon from '@ant-design/icons';

import PopUp from '../PopUp';
import styles from './index.module.less';

const CloseSvg = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <circle opacity="0.5" r="12" transform="matrix(1 0 0 -1 12 12)" fill="black" />
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M16.7812 8.28033C17.074 7.98744 17.074 7.51256 16.7812 7.21967C16.4883 6.92678 16.0134 6.92678 15.7205 7.21967L12.0004 10.9398L8.28033 7.21967C7.98744 6.92678 7.51256 6.92678 7.21967 7.21967C6.92678 7.51256 6.92678 7.98744 7.21967 8.28033L10.9398 12.0004L7.21967 15.7205C6.92678 16.0134 6.92678 16.4883 7.21967 16.7812C7.51256 17.074 7.98744 17.074 8.28033 16.7812L12.0004 13.0611L15.7205 16.7812C16.0134 17.074 16.4883 17.074 16.7812 16.7812C17.074 16.4883 17.074 16.0134 16.7812 15.7205L13.0611 12.0004L16.7812 8.28033Z"
      fill="white"
    />
  </svg>
);

const Modal = (props) => {
  const {
    usePortal,
    maskclosable,
    children,
    headerNode,
    visible,
    closable,
    onCancel,
    onOk,
    style,
    title,
    footer,
    bodyStyle,
    footerStyle,
    destroyOnClose,
    reHeightKey,
    heterotypic,
    heterotypicChild,
  } = props;
  const domRef = useRef();

  useEffect(() => {
    if (visible && domRef.current) {
      const modalHeight = domRef.current.offsetHeight;
      const screenHieght = document.body.clientHeight;
      if (modalHeight > screenHieght - 120) {
        domRef.current.style.transform = 'translateY(0)';
        domRef.current.style.top = '100px';
      } else {
        domRef.current.style.top = `${(screenHieght - modalHeight) / 2}px`;
        domRef.current.style.transform = 'translateY(0)';
      }
    }
  }, [visible, reHeightKey]);

  const defaultFooter = (
    <div className={styles.footerBtn}>
      <div className={styles.btn} onClick={onCancel}>
        取 消
      </div>
      <div className={styles.btn} onClick={onOk}>
        确 认
      </div>
    </div>
  );

  return (
    <PopUp
      visible={visible}
      popupTransition="rtg-zoom"
      maskclosable={maskclosable}
      usePortal={usePortal}
      popStyle={{ zIndex: 2000 }}
      destroyOnClose={destroyOnClose}
    >
      <div className={styles.modalnew444} style={style} ref={domRef}>
        <div className={styles.ct}>
          <div className={styles.header}>
            <div className={styles.title}>{title}</div>
            {closable && <Icon component={CloseSvg} className={styles.close} onClick={onCancel} />}
            {headerNode}
          </div>
          <div className={styles.content} style={bodyStyle}>
            {children}
          </div>
          {heterotypic ? (
            <div className={styles.heterotypicFooter}>
              <div className={styles.leftLineBg}>
                <div className={styles.onlyline} />
              </div>
              <div className={styles.hudu} />
              <div className={styles.btnarea}>
                {heterotypicChild}
                <div className={styles.btn} onClick={onCancel}>
                  取消
                </div>
                <div className={styles.btn} onClick={onOk}>
                  确定
                </div>
              </div>
            </div>
          ) : footer !== null ? (
            <div className={styles.footer} style={footerStyle}>
              {footer || defaultFooter}
            </div>
          ) : null}
        </div>
      </div>
    </PopUp>
  );
};

Modal.defaultProps = {
  maskclosable: false,
  destroyOnClose: false,
  usePortal: true,
  children: null,
  headerNode: null,
  visible: false,
  closable: true,
  onCancel: null,
  onOk: null,
  style: {},
  bodyStyle: {},
  footerStyle: {},
  title: '',
  footer: false,
  reHeightKey: '',
  heterotypic: false,
  heterotypicChild: false,
};

Modal.propTypes = {
  maskclosable: PropTypes.bool,
  destroyOnClose: PropTypes.bool,
  usePortal: PropTypes.bool,
  children: PropTypes.any,
  headerNode: PropTypes.any,
  visible: PropTypes.bool,
  closable: PropTypes.bool,
  onCancel: PropTypes.func,
  onOk: PropTypes.func,
  style: PropTypes.object,
  bodyStyle: PropTypes.object,
  footerStyle: PropTypes.object,
  title: PropTypes.string,
  footer: PropTypes.any,
  reHeightKey: PropTypes.bool,
  heterotypic: PropTypes.bool,
  heterotypicChild: PropTypes.any,
};

export default Modal;

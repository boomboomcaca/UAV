import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Popup from './Popup.jsx';
import styles from './drawer.module.less';

const PopupDrawer = (props) => {
  const { children, contentStyle, contentClassName, title, titleAttach, onTitleAttachClick, showClose } = props;

  const { visible, popStyle, popClassName, closeOnMask, onClose, getContainer, destoryOnClose } = props;

  const pp = {
    visible,
    popStyle,
    popClassName,
    closeOnMask,
    onClose,
    getContainer,
    destoryOnClose,
  };

  const [show, setShow] = useState(pp.visible);
  useEffect(() => {
    if (pp.visible) {
      setShow(true);
    }
  }, [pp.visible]);

  return (
    <Popup {...pp} popClassName={classnames(pp.visible ? styles.popupShow : styles.popupHide, pp.popClassName)}>
      <div className={styles.clip}>
        <div className={styles.header}>
          <div className={styles.title}>{title}</div>
          <div
            className={classnames(styles.titleAttach, showClose ? styles.hasClose : null)}
            style={titleAttach === '' ? { display: 'none' } : null}
            onClick={onTitleAttachClick}
          >
            {titleAttach}
          </div>
          {showClose ? <div className={styles.close} onClick={onClose} /> : null}
          <div className={styles.more} />
        </div>
        <div className={classnames(styles.content, contentClassName)} style={contentStyle}>
          {show ? children : null}
        </div>
      </div>
      {/* <div className={styles.close} onClick={onClose}>
        <div />
      </div> */}
    </Popup>
  );
};

PopupDrawer.defaultProps = {
  ...Popup.defaultProps,
  title: '',
  children: null,
  contentStyle: null,
  contentClassName: null,
  titleAttach: null,
  onTitleAttachClick: () => {},
  showClose: false,
};

PopupDrawer.propTypes = {
  ...Popup.propTypes,
  title: PropTypes.string,
  children: PropTypes.any,
  contentStyle: PropTypes.any,
  contentClassName: PropTypes.any,
  titleAttach: PropTypes.any,
  onTitleAttachClick: PropTypes.func,
  showClose: PropTypes.bool,
};

export default PopupDrawer;

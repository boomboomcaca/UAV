import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Popup from './Popup.jsx';
import styles from './modal.module.less';

const PopupModal = (props) => {
  const { children, title } = props;

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
    <Popup {...pp} popClassName={classnames(pp.popClassName, pp.visible ? styles.popupShow : styles.popupHide)}>
      <div className={styles.header}>
        <div className={styles.title}>{title}</div>
        <div className={styles.close} onClick={onClose} />
      </div>
      <div className={styles.content}>{show ? children : null}</div>
    </Popup>
  );
};

PopupModal.defaultProps = {
  ...Popup.defaultProps,
  title: '',
  children: null,
};

PopupModal.propTypes = {
  ...Popup.propTypes,
  title: PropTypes.string,
  children: PropTypes.any,
};

export default PopupModal;

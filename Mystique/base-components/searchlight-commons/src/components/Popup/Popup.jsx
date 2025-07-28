import React, { useEffect, useRef, useState } from 'react';
import { createPortal } from 'react-dom';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { CSSTransition } from 'react-transition-group';
import { createTimeID } from '../../lib/random';
import styles from './index.module.less';
import './animate.less';

const Popup = (props) => {
  const { visible, children, popStyle, popClassName, closeOnMask, ghost, onClose, getContainer, destoryOnClose } =
    props;

  const [firstVisible, setFirstVisible] = useState(false);

  const id = useRef(createTimeID()).current;
  const popup = useRef(document.createElement('div')).current;

  useEffect(() => {
    const getById = typeof getContainer === 'string';
    if (visible && !firstVisible) {
      if (getContainer === false || typeof getContainer === 'string') {
        popup.id = id;
        popup.style.position = 'absolute';
        popup.style.width = '100%';
        popup.style.height = '100%';
        popup.style.zIndex = '1000';
        popup.style.top = 0;
        popup.style.left = 0;
        popup.style.transition = 'opacity 0.3s';
        popup.style.opacity = 0;
        popup.style.pointerEvents = 'none';
        let elem = null;
        if (getById) {
          elem = document.getElementById(getContainer);
        }
        if (elem) {
          elem.appendChild(popup);
        } else document.body.appendChild(popup);
      }
      setFirstVisible(visible);
    }
    if (popup && (getContainer === false || typeof getContainer === 'string')) {
      popup.style.opacity = visible ? 1 : 0;
      popup.style.pointerEvents = visible ? (ghost ? 'none' : 'all') : 'none';
    }
    if (!visible) {
      if (destoryOnClose) {
        const child = document.getElementById(id);
        if (child) {
          let elem = null;
          if (getById) {
            elem = document.getElementById(getContainer);
          }
          if (elem) {
            elem.removeChild(child);
          } else document.body.removeChild(child);
          setFirstVisible(false);
        }
      }
    }
  }, [visible]);

  useEffect(() => {
    return () => {
      const getById = typeof getContainer === 'string';
      const child = document.getElementById(id);
      if (child) {
        let elem = null;
        if (getById) {
          elem = document.getElementById(getContainer);
        }
        if (elem) {
          elem.removeChild(child);
        } else {
          document.body.removeChild(child);
        }
      }
    };
  }, []);

  const content = (
    <CSSTransition in={visible} timeout={300} classNames="rtg-fade" appear>
      <div className={classnames(styles.root, visible ? null : styles.hide, ghost ? styles.ghost : null)}>
        <div className={classnames(styles.mask, ghost ? styles.ghost : null)} onClick={closeOnMask ? onClose : null} />
        <div
          className={classnames(styles.popup, popClassName)}
          style={visible ? { ...popStyle, pointerEvents: 'all' } : popStyle}
        >
          <div className={styles.content}>{children}</div>
        </div>
      </div>
    </CSSTransition>
  );

  return firstVisible
    ? getContainer === false || typeof getContainer === 'string'
      ? createPortal(destoryOnClose && !visible ? null : content, popup)
      : destoryOnClose && !visible
      ? null
      : content
    : null;
};

Popup.defaultProps = {
  visible: false,
  children: null,
  popStyle: null,
  popClassName: null,
  closeOnMask: true,
  ghost: false,
  onClose: () => {},
  getContainer: true,
  destoryOnClose: false,
};

Popup.propTypes = {
  visible: PropTypes.bool,
  children: PropTypes.any,
  popStyle: PropTypes.any,
  popClassName: PropTypes.any,
  closeOnMask: PropTypes.bool,
  ghost: PropTypes.bool,
  onClose: PropTypes.func,
  getContainer: PropTypes.any,
  destoryOnClose: PropTypes.bool,
};

export default Popup;

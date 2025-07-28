/* eslint-disable react/no-render-return-value */
/* eslint-disable no-underscore-dangle */
import React, { useState, useEffect, useRef } from 'react';
import ReactDOM from 'react-dom';
import PropTypes from 'prop-types';
import Item from './Item.jsx';
import styles from './index.module.less';
import './animate.less';

const __MSG_KEY__ = `cbe4ba59-ad9d-4c56-86a8-2e3e90510ee6`;

const ShowType = {
  SUCCESS: 'success',
  INFO: 'info',
  WARNING: 'warning',
  ERROR: 'error',
  LOADING: 'loading',
  TOAST: 'Toast',
};

const Message = (props) => {
  const { kid, type, msg, duration, icon } = props;
  const cachesRef = useRef([]);
  const [caches, setCaches] = useState([]);

  const hasMessage = () => {
    setTimeout(() => {
      const hass = cachesRef.current.filter((ccf) => {
        return !ccf.active;
      });
      if (hass && hass.length > 0) {
        for (let i = 0; i < hass.length; i += 1) {
          const e = hass[i];
          const idx = cachesRef.current.indexOf(e);
          cachesRef.current.splice(idx, 1);
        }
      }
      if (cachesRef.current.length === 0) {
        let MsgContainer = window[__MSG_KEY__];
        if (MsgContainer) {
          document.body.removeChild(MsgContainer);
          MsgContainer = null;
          delete window[__MSG_KEY__];
        }
      }
    }, 500);
  };

  const timeout = (item) => {
    const idx = cachesRef.current.indexOf(item);
    if (idx > -1) {
      cachesRef.current[idx].active = false;
      setCaches([...cachesRef.current]);
      hasMessage();
    }
  };

  useEffect(() => {
    const has = cachesRef.current.find((cc) => {
      return cc.kid === kid;
    });
    if (kid && has) {
      has.type = type;
      has.msg = msg;
      has.active = true;
      clearTimeout(has.timer);
      has.timer = setTimeout(() => {
        timeout(has);
      }, (duration || 2) * 1000);
    } else {
      const item = {
        kid,
        type,
        icon,
        msg,
        active: true,
        timestamp: new Date().getTime(),
      };
      item.timer = setTimeout(() => {
        timeout(item);
      }, (duration || 2) * 1000);
      cachesRef.current.unshift(item);
    }
    if (cachesRef.current.length > 3) {
      const cache = cachesRef.current[cachesRef.current.length - 1];
      cache.active = false;
      hasMessage();
    }
    setCaches([...cachesRef.current]);
  }, [props]);

  return (
    <div className={styles.root}>
      {caches.map((cache) => {
        return <Item key={cache.timestamp} item={cache} />;
      })}
    </div>
  );
};

Message.show = (type, msg) => {
  let MsgContainer = window[__MSG_KEY__];
  if (!MsgContainer) {
    MsgContainer = document.createElement('div');
    MsgContainer.style.position = 'absolute';
    MsgContainer.style.backgroundColor = 'transparent';
    MsgContainer.style.pointerEvents = 'none';
    MsgContainer.style.top = type === 'Toast' ? '50%' : '50%';
    MsgContainer.style.left = '50%';
    MsgContainer.style.zIndex = '9999';
    MsgContainer.style.overflow = 'hide';
    MsgContainer.style.transform = 'translate(-50%,-50%)';
    document.body.appendChild(MsgContainer);
    window[__MSG_KEY__] = MsgContainer;
  }

  const param = { kid: null, duration: null };
  if (typeof msg === 'string') {
    param.msg = msg;
  }
  if (typeof msg === 'object') {
    const { key, content, duration, icon } = msg;
    param.msg = content || null;
    param.kid = key || null;
    param.duration = duration || null;
    param.icon = icon || null;
  }
  return ReactDOM.render(param.msg ? <Message type={type} {...param} /> : null, MsgContainer);
};

Message.defaultProps = {
  kid: null,
  type: ShowType.INFO,
  icon: null,
  msg: '暂无消息提示。',
  duration: 2,
};

Message.propTypes = {
  kid: PropTypes.any,
  type: PropTypes.any,
  icon: PropTypes.any,
  msg: PropTypes.any,
  duration: PropTypes.number,
};

export default Message;
export { ShowType };

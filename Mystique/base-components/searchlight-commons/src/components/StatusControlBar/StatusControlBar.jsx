import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import Spliter from './Spliter.jsx';
import Main from './Main.jsx';
import Message from './Message.jsx';
import Action from './Action.jsx';

import styles from './index.module.less';

const StatusControlBar = (props) => {
  const { className, children } = props;

  const [hasMain, setHasMain] = useState(false);
  const [hasMessage, setHasMessage] = useState(false);
  const [hasAction, setHasAction] = useState(false);

  useEffect(() => {
    setHasMain(hasChild('Main'));
    setHasMessage(hasChild('Message'));
    setHasAction(hasChild('Action'));
  }, [children]);

  const hasChild = (name) => {
    const element = (toString.call(children) === '[object Array]' ? children : [children]).find((child) => {
      return child.type !== undefined ? child.type.typeTag === name : false;
    });
    return !!element;
  };

  const getChildByName = (name) => {
    const element = (toString.call(children) === '[object Array]' ? children : [children]).find((child) => {
      return child.type !== undefined ? child.type.typeTag === name : false;
    });
    return element;
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.main}>{getChildByName('Main')}</div>
      {hasMain ? <Spliter width={hasMessage ? 32 : 0} /> : null}
      <div className={styles.message}>{getChildByName('Message')}</div>
      {hasMessage ? <Spliter width={hasAction ? 32 : 0} /> : null}
      <div className={styles.action}>{getChildByName('Action')}</div>
    </div>
  );
};

StatusControlBar.defaultProps = {
  className: null,
  children: null,
};

StatusControlBar.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
};

StatusControlBar.Main = Main;
StatusControlBar.Message = Message;
StatusControlBar.Action = Action;

export default StatusControlBar;

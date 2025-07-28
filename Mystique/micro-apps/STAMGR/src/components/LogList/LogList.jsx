import React, { useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

let isMouseOver = false;

const onMouseEnter = () => {
  isMouseOver = true;
};

const onMouseLeave = () => {
  isMouseOver = false;
};

const LogList = (props) => {
  const { className, children } = props;

  const refDiv = useRef();

  useEffect(() => {
    refDiv.current.onmouseenter = onMouseEnter;
    refDiv.current.onmouseleave = onMouseLeave;
  }, [refDiv]);

  const scrollToEnd = () => {
    if (!isMouseOver) {
      refDiv.current.scrollTop = refDiv.current.scrollHeight;
    }
  };

  useEffect(() => {
    scrollToEnd();
  }, [children]);

  return (
    <div ref={refDiv} className={classnames(styles.root, className)}>
      {children}
    </div>
  );
};

LogList.defaultProps = {
  className: null,
  children: null,
};

LogList.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
};

export default LogList;

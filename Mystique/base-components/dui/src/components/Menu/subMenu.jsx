import React, { useEffect, useRef, useState, useContext, useMemo } from 'react';
import classnames from 'classnames';
import { isFragment } from 'react-is';
import PropTypes from 'prop-types';
import Icon from '@ant-design/icons';
import AppContext from './context';

import styles from './index.module.less';

function toArray(children, option) {
  let ret = [];

  React.Children.forEach(children, (child) => {
    if ((child === undefined || child === null) && !option.keepEmpty) {
      return;
    }

    if (Array.isArray(child)) {
      ret = ret.concat(toArray(child));
    } else if (isFragment(child) && child.props) {
      ret = ret.concat(toArray(child.props.children, option));
    } else {
      ret.push(child);
    }
  });

  return ret;
}

function parseChildren(children) {
  return toArray(children).map((child, index) => {
    if (React.isValidElement(child)) {
      const { key } = child;
      let eventKey = child.props?.eventKey ?? key;
      const transit = child.props?.transit;
      const emptyKey = eventKey === null || eventKey === undefined;
      if (emptyKey) {
        eventKey = `tmp_key-${index}`;
      }

      const cloneProps = {
        key: eventKey,
        eventKey,
        transit,
      };

      if (process.env.NODE_ENV !== 'production' && emptyKey) {
        cloneProps.warnKey = true;
      }

      return React.cloneElement(child, cloneProps);
    }

    return child;
  });
}

const SubMenu = (props) => {
  const { title, children, defaultOpen } = props;
  const [open, setOpen] = useState(false);
  const isUseDefault = useRef(true);
  const { selectValue } = useContext(AppContext);

  const changeOpen = () => {
    setOpen(!open);
    isUseDefault.current = false;
  };

  const isselect = useMemo(() => {
    let isOpen = false;
    toArray(children).forEach((child) => {
      const { key } = child;
      const eventKey = child.props?.eventKey ?? key;
      if (eventKey === selectValue) {
        isOpen = true;
      }
    });
    return isOpen;
  }, [children, selectValue]);

  useEffect(() => {
    if (defaultOpen && isUseDefault.current) {
      setOpen(true);
    }
  }, [defaultOpen]);

  return (
    <div
      className={classnames(styles.submenu, {
        // [styles.opensubmenu]: open,
        [styles.selectsubmenu]: isselect,
      })}
    >
      <div className={styles.cont} onClick={changeOpen}>
        <div className={styles.title}>{title}</div>
        <Icon component={arrowSvg} className={open ? styles.open : styles.close} />
      </div>
      <div className={open ? styles.show : styles.hide}>{parseChildren(children)}</div>
    </div>
  );
};

SubMenu.defaultProps = {
  title: '',
  children: '',
  defaultOpen: false,
};

SubMenu.propTypes = {
  title: PropTypes.any,
  children: PropTypes.any,
  defaultOpen: PropTypes.bool,
};

const arrowSvg = () => (
  <svg width="10" height="8" viewBox="0 0 10 8" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M4.17801 6.81355C4.57569 7.38756 5.42431 7.38756 5.82199 6.81355L9.10877 2.06949C9.56825 1.40629 9.0936 0.5 8.28678 0.5L1.71322 0.5C0.906401 0.5 0.431746 1.40629 0.891226 2.06949L4.17801 6.81355Z"
      fill="var(--theme-font-30)"
    />
  </svg>
);

export default SubMenu;

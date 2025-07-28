import React, { useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useThrottleFn } from 'ahooks';
import Empty from '../Empty';
import useWindowSize from './useWindowSize';
import styles from './flow.module.less';

// TODO warning!!! 未作虚拟化
const Flow = (props) => {
  const { className, loadMore, children } = props;

  const rootRef = useRef(null);
  const listRef = useRef(null);
  const triggerRef = useRef(null);

  useWindowSize((/* size */) => {
    setTimeout(onScrollListener, 200);
  });

  const onScrollListener = () => {
    const { current } = rootRef;
    const { clientHeight, scrollTop, scrollHeight } = current;

    const atBottom = scrollTop + clientHeight >= scrollHeight - 20;
    if (atBottom) {
      if (!triggerRef.current) {
        let toload = 0;
        if (atBottom) {
          toload += 1;
        }
        if (toload > 0) {
          triggerRef.current = true;
          window.console.log(toload);
          loadMore(toload);
        }
      }
    }
  };

  useEffect(() => {
    triggerRef.current = false;
  }, [children?.length]);

  const { run: runScroll } = useThrottleFn(onScrollListener, { wait: 250 });

  return (
    <div ref={rootRef} className={classnames(styles.root, className)} onScroll={runScroll}>
      <div ref={listRef} className={classnames(styles.list)}>
        {children && children.length > 0 ? children : <Empty />}
      </div>
    </div>
  );
};

Flow.defaultProps = {
  className: null,
  children: null,
  loadMore: () => {},
};

Flow.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
  loadMore: PropTypes.func,
};

export default Flow;

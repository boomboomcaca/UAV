/* eslint-disable consistent-return */
/* eslint-disable no-underscore-dangle */

import React, { useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useThrottleFn } from 'ahooks';
import Empty from '../Empty';
import useWindowSize from './useWindowSize';
import Item from './Item.jsx';
import styles from './index.module.less';

const _BASE_WIDTH_ = 200;

const Virtualized = (props) => {
  const { className, children, baseSize, loadMore, dataSource, itemTemplate } = props;

  const rootRef = useRef(null);
  const listRef = useRef(null);
  const triggerRef = useRef(null);

  const offset = useRef({}).current;
  const [refresh, setRefresh] = useState(0);

  useWindowSize((/* size */) => {
    setTimeout(onScrollListener, 200);
  });

  const getK = (x) => {
    const type = typeof x;
    if (type === 'string') {
      const idx = x.indexOf('%');
      if (idx > -1) {
        const k = parseInt(x.substring(0, idx), 10) / 100;
        return { tran: true, k };
      }
    }
    if (type === 'number') {
      return { tran: false, k: x };
    }
  };

  const getWidth = (width, actualWidth) => {
    const { tran, k } = getK(width);
    if (tran) {
      return Math.floor(k * actualWidth);
    }
    return k;
  };

  const fixSize = () => {
    if (listRef.current && (children || dataSource) && (children?.length > 0 || dataSource?.length > 0)) {
      const { offsetWidth } = listRef.current;
      const actualWidth = offsetWidth - (baseSize.scrollWidth || 3);
      const bw = baseSize && baseSize.width ? getWidth(baseSize.width, actualWidth) : _BASE_WIDTH_;
      let count = Math.ceil(actualWidth / bw);
      let fixWidth = Math.floor(actualWidth / count);
      if (fixWidth < bw * 0.85) {
        count -= 1;
        fixWidth = Math.floor(actualWidth / count);
        if (fixWidth > bw * 1.15) fixWidth = bw * 1.15;
        offset.margin = Math.floor((actualWidth - fixWidth * count) / count / 2);
      } else {
        offset.margin = 0;
      }
      const fixHeight = baseSize && baseSize.height ? baseSize.height : fixWidth;

      offset.fixWidth = fixWidth;
      offset.fixHeight = fixHeight;
      offset.count = count;

      let ret = null;

      if (children) {
        ret = children.slice(offset.head, offset.tail).map((c) => {
          return React.cloneElement(c, {
            style: { width: fixWidth, height: fixHeight, margin: `0px ${offset.margin}px` },
          });
        });
      }
      if (dataSource && itemTemplate) {
        ret = dataSource.slice(offset.head, offset.tail || dataSource.length).map((c) => {
          return (
            <Item
              key={c.id || c}
              style={{ width: fixWidth, height: fixHeight, margin: `0px ${offset.margin}px` }}
              content={itemTemplate(c)}
            />
          );
        });
      }

      return ret;
    }
    return <Empty />;
  };

  const onScrollListener = () => {
    const { current } = rootRef;
    const { clientHeight, scrollTop, scrollHeight } = current;
    updateOffset(children?.length || dataSource?.length);

    const atBottom = scrollTop + clientHeight >= scrollHeight - 20;
    if (atBottom || offset.tail > (children?.length || dataSource?.length)) {
      if (!triggerRef.current) {
        let toload = 0;
        if (atBottom) {
          toload += 1;
        } else if (offset.tail > (children?.length || dataSource?.length)) {
          const ct = Math.ceil((offset.tail - children?.length || dataSource?.length) / (offset.tail - offset.head));
          for (let i = 0; i < ct; i += 1) {
            toload += 1;
          }
        }
        if (toload > 0) {
          triggerRef.current = true;
          loadMore(toload);
        }
      }
    }
    setRefresh((pre) => pre + 1);
  };

  useEffect(() => {
    triggerRef.current = false;
    onScrollListener();
  }, [children?.length, dataSource?.length]);

  const { run: runScroll } = useThrottleFn(onScrollListener, { wait: 250 });

  const updateOffset = (len) => {
    if (rootRef.current) {
      const { scrollTop, offsetHeight } = rootRef.current;
      const { count, fixHeight } = offset;
      if (scrollTop >= 0 && fixHeight && count) {
        const head = Math.floor(scrollTop / fixHeight) * count;
        const length = (Math.ceil(offsetHeight / fixHeight) + 1) * count;
        offset.head = head;
        offset.tail = head + length;
      } else {
        offset.head = 0;
        offset.tail = children?.length || dataSource?.length;
      }
      offset.scrollTop = scrollTop;
      if (len === 0) {
        offset.scrollTop = 0;
        offset.head = 0;
      }
    }
  };

  return (
    <div ref={rootRef} className={classnames(styles.root, className)} onScroll={runScroll}>
      <div
        ref={listRef}
        key={refresh}
        className={classnames(styles.list)}
        style={{
          marginTop: `${Math.round(offset.head / offset.count) * offset.fixHeight}px`,
          marginBottom: `${
            Math.round((children?.length || dataSource?.length - offset.tail) / offset.count) * offset.fixHeight
          }px`,
        }}
      >
        {fixSize()}
      </div>
    </div>
  );
};

Virtualized.Item = Item;

Virtualized.defaultProps = {
  className: null,
  children: null,
  baseSize: null,
  loadMore: () => {},
  dataSource: null,
  itemTemplate: null,
};

Virtualized.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
  baseSize: PropTypes.any,
  loadMore: PropTypes.func,
  dataSource: PropTypes.array,
  itemTemplate: PropTypes.func,
};

export default Virtualized;

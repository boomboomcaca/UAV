import React, { useRef, useMemo, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import Expander from './Expander.jsx';
import useScrollMore from './useScrollMore';
import Loading from '../Loading';
import Empty from '../Empty';
import styles from './index.module.less';

const defaultKEY = 'id';
const defaultLABEL = 'name';
const defaultChildren = 'children';
const defaultKM = { KEY: defaultKEY, LABEL: defaultLABEL, CHILDREN: defaultChildren };

const Expand = (props) => {
  const { dataSource, titleTemplate, itemTemplate, onLoadMore, onSelectChange, keyMap, defaultSelectMode } = props;

  const scrollRef = useRef(null);
  const canAutoScrollRef = useRef(true);
  let scrollID = useRef(null).current;

  const [expandIdx, setExpandIdx] = useState(-1);
  const [selectIdx, setSelectIdx] = useState({ dIndex: 0, iIndex: -1 });

  const [loading] = useScrollMore(scrollRef, dataSource?.length, onLoadMore);

  const getKeyMap = () => {
    let map = defaultKM;
    if (keyMap) {
      if (keyMap.KEY) {
        map = { ...map, KEY: keyMap.KEY };
      }
      if (keyMap.LABEL) {
        map = { ...map, LABEL: keyMap.LABEL };
      }
      if (keyMap.CHILDREN) {
        map = { ...map, CHILDREN: keyMap.CHILDREN };
      }
    }
    return map;
  };

  const almostDone = () => {
    if (canAutoScrollRef.current) {
      canAutoScrollRef.current = false;
      setTimeout(() => {
        const scroll = document.getElementById(scrollID);
        scroll?.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        });
        canAutoScrollRef.current = true;
      }, 0);
    }
  };

  const onItemClick = (temp, data, didx, item, iidx) => {
    setSelectIdx({ dIndex: didx, iIndex: iidx });
    onSelectChange({ temp, data, didx, item, iidx });
  };

  const getRender = useMemo(() => {
    const { KEY, LABEL, CHILDREN } = getKeyMap();

    return dataSource.map((t, i) => {
      const expand = (expandIdx === -1 && i === 0) || expandIdx === i;

      if (expand) scrollID = t.id;
      if (i === dataSource.length - 1) {
        almostDone();
      }

      return t[CHILDREN]?.length > 0 ? (
        <Expander
          id={t[KEY]}
          key={t[KEY]}
          // title={t[LABEL]}
          title={titleTemplate ? titleTemplate(t) : t[LABEL]}
          onExpandChanged={() => {
            canAutoScrollRef.current = true;
            setExpandIdx(i);
          }}
          onItemClick={(data, item, idx) => {
            onItemClick(t, data, i, item, idx);
          }}
          canSelect={defaultSelectMode}
          expand={expand}
          selectIndex={selectIdx && (expandIdx === -1 || expandIdx === selectIdx.dIndex) ? selectIdx.iIndex : -1}
          dataSource={t[CHILDREN]}
          itemTemplate={itemTemplate}
        />
      ) : null;
    });
  }, [dataSource, expandIdx, selectIdx, keyMap]);

  return (
    <div className={styles.root}>
      <div className={styles.list} ref={scrollRef}>
        <div className={styles.items}>{dataSource?.length > 0 ? getRender : <Empty className={styles.empty} />}</div>
      </div>
      <Loading loading={loading} loadingSize={20} className={styles.loading} loadingMsg="加载中..." />
    </div>
  );
};

Expand.defaultProps = {
  dataSource: null,
  titleTemplate: null,
  itemTemplate: null,
  onLoadMore: () => {},
  onSelectChange: () => {},
  defaultSelectMode: true,
  keyMap: defaultKM,
};

Expand.propTypes = {
  dataSource: PropTypes.any,
  titleTemplate: PropTypes.any,
  itemTemplate: PropTypes.any,
  onLoadMore: PropTypes.func,
  onSelectChange: PropTypes.func,
  defaultSelectMode: PropTypes.bool,
  keyMap: PropTypes.any,
};

export default Expand;

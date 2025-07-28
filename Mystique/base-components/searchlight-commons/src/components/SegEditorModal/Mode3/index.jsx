import React, { memo, useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Mode3 = (props) => {
  const { axios, limit, segments, onClickSeg } = props;

  // 预设数据
  const [treeData, setTreeData] = useState([]);

  const findHave = (seg) => {
    const findSame = segments.find(
      (item) =>
        item.startFrequency === seg.startFrequency &&
        item.stopFrequency === seg.stopFrequency &&
        item.stepFrequency === seg.stepFrequency,
    );
    return !!findSame;
  };

  useEffect(() => {
    axios?.({
      url: '/segment/scanSegment/getList',
      method: 'get',
    }).then((res) => {
      const groupList = [];
      const snap = {};
      const filterStep = res.result.filter((item) => limit.stepItems.includes(item.stepFrequency));
      filterStep.forEach((item) => {
        if (!groupList.includes(item.group)) {
          groupList.push(item.group);
        }

        if (snap[item.group]) {
          snap[item.group] = [...snap[item.group], item];
        } else {
          snap[item.group] = [item];
        }
      });
      const nneList = groupList.map((item) => ({ name: item, list: snap[item] }));
      setTreeData(nneList);
    });
  }, [limit]);

  return (
    <div className={styles.Mode3}>
      {treeData.map((groupList) => (
        <div className={styles.groupitem}>
          <div className={styles.groupName}>
            <div className={styles.line} />
            <span>{groupList.name}</span>
          </div>
          <div className={styles.groupList}>
            {groupList.list.map((item) => (
              <div
                className={classnames(styles.segitem, { [styles.active]: findHave(item) })}
                key={item.id}
                onClick={() => onClickSeg(item)}
              >
                <div className={styles.infoitem}>
                  <div className={styles.infohead}>
                    <span>{item.startFrequency}</span>
                    <span style={{ margin: '0 4px' }}>~</span>
                    <span>{item.stopFrequency}</span>
                    <span className={styles.unit}>MHz</span>
                  </div>
                  <div className={styles.unit}>
                    <span>@</span>
                    <span>{item.stepFrequency}</span>
                    <span>kHz</span>
                  </div>
                </div>
                <div className={styles.infoitem}>
                  {item.name && item.name !== '' && (
                    <div className={styles.segname} title={item.name}>
                      {item.name}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}
    </div>
  );
};

Mode3.propTypes = {
  axios: PropTypes.func.isRequired,
  limit: PropTypes.object.isRequired,
  segments: PropTypes.array.isRequired,
  onClickSeg: PropTypes.func.isRequired,
};

export default memo(Mode3);

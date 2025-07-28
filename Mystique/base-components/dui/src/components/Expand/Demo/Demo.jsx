import React, { useState } from 'react';
import classnames from 'classnames';
import Expand from '../index';
import templates from './testData';
import styles from './index.module.less';

export default function Demo() {
  const [moreData, setMoreData] = useState(templates);

  const onLoadMore = (callback) => {
    if (moreData.length < 50) {
      callback(true);
      const id = moreData.length + 1;
      setTimeout(() => {
        const newData = {
          id,
          name: `test${id}`,
          xs: [
            { id: id * 10 + 1, name: '?????' },
            { id: id * 10 + 2, name: '?????' },
            { id: id * 10 + 3, name: '?????' },
          ],
        };
        setMoreData([...moreData, newData]);
      }, 2000);
    }
  };

  return (
    <div className={styles.root}>
      <Expand
        defaultSelectMode={false}
        keyMap={{ CHILDREN: 'xs' }}
        dataSource={templates}
        titleTemplate={(d) => <div style={{ color: 'orangered' }}>{d.name}</div>}
        itemTemplate={(d, selected) => (
          <div className={classnames(styles.item, selected ? styles.itemSel : null)}>
            <div>{d.name}</div>
          </div>
        )}
        onSelectChange={(e) => {
          window.console.log(e);
        }}
      />
      <Expand
        keyMap={{ CHILDREN: 'xs' }}
        dataSource={moreData}
        itemTemplate={(d) => (
          <div className={styles.item}>
            <div>{d.name}</div>
          </div>
        )}
        onLoadMore={onLoadMore}
      />
    </div>
  );
}

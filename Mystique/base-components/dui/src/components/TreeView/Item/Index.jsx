import React, { useEffect, useState } from 'react';

import styles from './style.module.less';

const TreeItem = (props) => {
  const { name, code, cities, level, status, size, progress, onDownload } = props;

  const [nodeLevel, setNodeLevel] = useState(2);
  const [expand, setExpand] = useState(false);
  const [download, setDownload] = useState(0);

  useEffect(() => {
    if (level) {
      console.log(level);
      setNodeLevel(level + 1);
    }
  }, [level]);

  useEffect(() => {
    if (progress) {
      if (name === progress.progress) {
        setDownload(progress.value);
      }
    }
  }, [progress, name]);

  return (
    <div className={styles.itemRoot}>
      <div
        className={`${styles.itemTitle} ${nodeLevel !== 2 && styles.level1}`}
        style={{ paddingLeft: `${nodeLevel * 9}px` }}
        onClick={() => {
          if (cities) setExpand(!expand);
        }}
      >
        <div className={styles.itemLeft}>
          <span>{name}</span>
        </div>
        {!cities && (
          <div className={styles.itemRight}>
            <span>{size} M</span>
            {status === 2 ? (
              <div>已下载</div>
            ) : status === 1 ? (
              <div>下载中{download}%</div>
            ) : (
              <div
                className={styles.download}
                onClick={() => {
                  if (onDownload) {
                    onDownload(code);
                  }
                }}
              />
            )}
          </div>
        )}
      </div>
      {cities && expand && (
        <div className={styles.childnodes}>
          <div>
            {cities.map((it) => {
              console.log(it);
              return <TreeItem {...it} level={nodeLevel} progress={progress} onDownload={onDownload} />;
            })}
          </div>
        </div>
      )}
    </div>
  );
};

export default TreeItem;

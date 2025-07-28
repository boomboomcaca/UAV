import React, { useEffect, useState } from 'react';
import TreeItem from './Item/Index.jsx';
import styles from './style.module.less';

const TreeView = (props) => {
  const { rootName, nodes, onDownload } = props;
  const [expand, setExpand] = useState(false);
  return (
    <div className={styles.treeRoot}>
      <div className={styles.rootTitle} onClick={() => setExpand(!expand)}>
        <span>{rootName}</span>
      </div>
      {expand && nodes && (
        <div className={styles.nodes}>
          {nodes.map((item) => {
            return <TreeItem {...item} level={1} onDownload={onDownload} />;
          })}
        </div>
      )}
    </div>
  );
};

export default TreeView;

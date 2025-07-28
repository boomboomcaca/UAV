import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Table, Empty } from 'dui';
import WaveBubble from './Item/WaveBubble.jsx';
import MultiRing from './Item/MultiRing.jsx';
import columns, { data as test } from './columns.jsx';
import useServer from './useServer';
import styles from './index.module.less';

const Server = (props) => {
  const { className, data } = props;

  const { processes } = useServer();

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.title}>
        <span>IP地址:</span>
        <span>{data?.ip || '-.-.-.-'}</span>
      </div>
      <div className={styles.bubbles}>
        <WaveBubble progress={50} />
        <MultiRing title="内存占用率" value="3031" unit="MB" color="rgba(60, 229, 211, 0.2)" />
        <MultiRing title="磁盘读写速度" value="3695" unit="bps" color="rgba(60, 229, 117, 0.2)" />
        <MultiRing title="磁盘剩余存储空间" value="500" unit="G" color="rgba(45, 179, 255, 0.2)" />
        <div className={styles.shadow} />
      </div>
      <div className={styles.list}>
        <Table columns={columns} data={test} showSelection={false} />
        {/* {!prcesses || prcesses.length === 0 ? <Empty className={styles.empty} /> : null} */}
      </div>
    </div>
  );
};

Server.defaultProps = {
  className: null,
  data: null,
};

Server.propTypes = {
  className: PropTypes.any,
  data: PropTypes.any,
};

export default Server;

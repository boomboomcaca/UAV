import React, { useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Empty, ListView } from 'dui';
import Check from '../Check/Check.jsx';
import Item from '../Item/Item.jsx';
import styles from './index.module.less';

const Device = (props) => {
  const { className, data } = props;

  const [checking, seChecking] = useState(false);

  const onChecking = (bo) => {
    seChecking(bo);
  };

  return (
    <div className={classnames(styles.root, className)}>
      <Check dataSource={data} onLoading={onChecking} />
      {checking ? (
        <Empty className={styles.list} emptype={Empty.Device} message="自检中,请稍等..." />
      ) : (
        <ListView
          className={styles.list}
          baseSize={{ width: 300, height: 112 }}
          dataSource={data}
          itemTemplate={(item) => {
            return <Item className={styles.item} item={item} />;
          }}
        />
      )}
    </div>
  );
};

Device.defaultProps = {
  className: null,
  data: null,
};

Device.propTypes = {
  className: PropTypes.any,
  data: PropTypes.any,
};

export default Device;

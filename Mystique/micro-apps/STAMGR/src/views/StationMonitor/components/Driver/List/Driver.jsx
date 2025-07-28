import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ListView } from 'dui';
import Item from '../Item/Item.jsx';
import styles from './index.module.less';

const Device = (props) => {
  const { className, data, attach } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <ListView
        className={styles.list}
        baseSize={{ width: 340, height: 280 }}
        dataSource={data}
        itemTemplate={(item) => {
          return <Item className={styles.item} item={item} devices={attach} />;
        }}
      />
    </div>
  );
};

Device.defaultProps = {
  className: null,
  data: null,
  attach: null,
};

Device.propTypes = {
  className: PropTypes.any,
  data: PropTypes.any,
  attach: PropTypes.any,
};

export default Device;

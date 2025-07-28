import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ListView } from 'dui';
import Item from './Item.jsx';
import styles from './index.module.less';

const Camera = (props) => {
  const { className, data } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div>视频监控</div>
      <ListView
        className={styles.list}
        baseSize={{ width: 200, height: 160 }}
        dataSource={data}
        itemTemplate={(item) => {
          return <Item className={styles.item} item={item} />;
        }}
      />
    </div>
  );
};

Camera.defaultProps = {
  className: null,
  data: null,
};

Camera.propTypes = {
  className: PropTypes.any,
  data: PropTypes.any,
};

export default Camera;

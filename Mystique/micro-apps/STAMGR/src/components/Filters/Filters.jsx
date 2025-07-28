import React from 'react';
import PropTypes from 'prop-types';
import { Button, Drawer } from 'dui';
import styles from './index.module.less';

const Filters = (props) => {
  const { children, onClick, visible, title, onCancel } = props;
  return (
    <Drawer visible={visible} width="450px" title={title} onCancel={onCancel}>
      <div className={styles.root}>
        <div className={styles.child}>{children}</div>
        <div className={styles.btns}>
          <Button
            onClick={() => {
              onClick('reset');
            }}
            style={{ width: 160 }}
          >
            重置
          </Button>
          <Button
            onClick={() => {
              onClick('confirm');
            }}
            style={{ width: 160, color: '#3CE5D3' }}
          >
            确定
          </Button>
        </div>
      </div>
    </Drawer>
  );
};

Filters.defaultProps = {
  children: null,
  visible: false,
  title: '',
  onCancel: () => {},
  onClick: () => {},
};

Filters.propTypes = {
  children: PropTypes.any,
  visible: PropTypes.bool,
  title: PropTypes.string,
  onCancel: PropTypes.func,
  onClick: PropTypes.func,
};

export default Filters;

import React from 'react';
import PropTypes from 'prop-types';
import PopUp from '../PopUp';
import styles from './style.module.less';

const Drawer = (props) => {
  const { visible, onCancel, title, children, width, bodyStyle, maskclosable, headerIcon, usePortal, mask } = props;

  return (
    <PopUp
      popStyle={{ zIndex: 2000 }}
      visible={visible}
      maskclosable={maskclosable}
      onCancel={onCancel}
      usePortal={usePortal}
      mask={mask}
    >
      <div className={styles.drawer} style={{ width }}>
        <div className={styles.ct}>
          <div className={styles.hd}>
            <div className={styles.title}>{title}</div>
            <div className={styles.icon}>{headerIcon}</div>
          </div>
          <div className={styles.bd} style={bodyStyle}>
            {children}
          </div>
        </div>
      </div>
    </PopUp>
  );
};

Drawer.defaultProps = {
  visible: false,
  onCancel: () => {},
  title: '',
  children: null,
  width: '40%',
  bodyStyle: {},
  maskclosable: true,
  headerIcon: null,
  usePortal: true,
  mask: true,
};

Drawer.propTypes = {
  visible: PropTypes.bool,
  onCancel: PropTypes.func,
  title: PropTypes.string,
  children: PropTypes.any,
  width: PropTypes.string,
  bodyStyle: PropTypes.object,
  maskclosable: PropTypes.bool,
  headerIcon: PropTypes.any,
  usePortal: PropTypes.oneOfType([PropTypes.bool, PropTypes.string]),
  mask: PropTypes.bool,
};

export default Drawer;

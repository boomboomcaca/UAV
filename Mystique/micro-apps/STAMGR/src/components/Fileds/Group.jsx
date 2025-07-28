import React from 'react';
import PropTypes from 'prop-types';
import styles from './group.module.less';

const Group = (props) => {
  const { children, label, childStyle } = props;

  return (
    <div className={styles.root}>
      <div className={styles.label}>{label}</div>
      <div className={styles.child} style={childStyle}>
        {children}
      </div>
    </div>
  );
};

Group.defaultProps = {
  children: null,
  label: '',
  childStyle: null,
};

Group.propTypes = {
  children: PropTypes.any,
  label: PropTypes.string,
  childStyle: PropTypes.any,
};

Group.tag = 'group';

export default Group;

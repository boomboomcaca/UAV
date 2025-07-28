import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Ring = (props) => {
  const { className, children, borderColor } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.circle1}>
        <div className={styles.loader1} style={{ borderTopColor: borderColor }} />
        <div className={styles.loader2} style={{ borderTopColor: borderColor }} />
        <div className={styles.loader3} style={{ borderTopColor: borderColor }} />
      </div>
      <div className={styles.circle2}>
        <div className={styles.loader4} style={{ borderTopColor: borderColor }} />
        <div className={styles.loader5} style={{ borderTopColor: borderColor }} />
      </div>
      <div className={styles.children}>{children}</div>
    </div>
  );
};

Ring.defaultProps = {
  className: null,
  children: null,
  borderColor: 'rgba(60, 229, 211, 0.2)',
};

Ring.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
  borderColor: PropTypes.string,
};

export default Ring;

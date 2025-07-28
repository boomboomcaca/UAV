import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Uploading = (props) => {
  const { className, percentage } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.uploading}>{`${percentage}%`}</div>
      <div className={styles.progress}>
        <div
          style={{
            width: (64 * percentage) / 100,
          }}
        />
      </div>
    </div>
  );
};

Uploading.defaultProps = {
  className: null,
  percentage: 0,
};

Uploading.propTypes = {
  className: PropTypes.any,
  percentage: PropTypes.number,
};

export default Uploading;

import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Wave = (props) => {
  const { className, progress } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.wave}>
        <div className={styles.wave1} style={{ top: `${205 - progress}%` }} />
        <div className={styles.wave2} style={{ top: `${200 - progress}%` }} />
      </div>
    </div>
  );
};

Wave.defaultProps = {
  className: null,
  progress: 20,
};

Wave.propTypes = {
  className: PropTypes.any,
  progress: PropTypes.number,
};

export default Wave;

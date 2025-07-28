import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Bubble from '@/components/Bubble';
import Wave from '@/components/Wave';
import styles from './waveBubble.module.less';

const WaveBubble = (props) => {
  const { className, title, progress } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.title}>{title}</div>
      <div className={styles.circle}>
        <Wave className={styles.wave} progress={progress} />
        <Bubble>
          <div className={styles.progress}>
            <span>{progress}</span>
            <span>%</span>
          </div>
        </Bubble>
      </div>
    </div>
  );
};

WaveBubble.defaultProps = {
  className: null,
  title: 'CPU使用率',
  progress: 20,
};

WaveBubble.propTypes = {
  className: PropTypes.any,
  title: PropTypes.string,
  progress: PropTypes.number,
};

export default WaveBubble;

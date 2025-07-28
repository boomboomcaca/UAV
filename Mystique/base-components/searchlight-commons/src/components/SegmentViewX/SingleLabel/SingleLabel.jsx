import React from 'react';
import PropTypes from 'prop-types';
import styles from './singleLabel.module.less';

const SingleLabel = (props) => {
  const { startFrequency, stopFrequency, stepFrequency } = props;

  return (
    <div className={styles.ssroot}>
      <span>{`${startFrequency}MHz`}</span>
      <span className={styles.step}>{`${stepFrequency}kHz`}</span>
      <span>{`${stopFrequency}MHz`}</span>
    </div>
  );
};

SingleLabel.defaultProps = {
  startFrequency: 87,
  stopFrequency: 108,
  stepFrequency: 25,
};

SingleLabel.propTypes = {
  startFrequency: PropTypes.number,
  stopFrequency: PropTypes.number,
  stepFrequency: PropTypes.number,
};

export default SingleLabel;

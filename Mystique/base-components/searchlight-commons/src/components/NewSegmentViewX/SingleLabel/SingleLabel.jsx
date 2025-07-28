/*
 * @Author: dengys
 * @Date: 2022-04-21 10:58:32
 * @LastEditors: dengys
 * @LastEditTime: 2022-04-29 16:10:59
 */
import React from 'react';
import PropTypes from 'prop-types';
import styles from './singleLabel.module.less';

const SingleLabel = (props) => {
  const { width, startFrequency, stopFrequency, showStep, stepFrequency } = props;

  return (
    <div className={styles.SingleLabel2} style={{ width: `${width}%` }}>
      {width >= 15 && (
        <>
          <span>{`${startFrequency}MHz`}</span>
          {showStep && <span>{`${stepFrequency}kHz`}</span>}
          <span>{`${stopFrequency}MHz`}</span>
        </>
      )}
    </div>
  );
};

SingleLabel.defaultProps = {
  startFrequency: 87,
  stopFrequency: 108,
  width: 100,
  stepFrequency: 25,
  showStep: false,
};

SingleLabel.propTypes = {
  startFrequency: PropTypes.number,
  stopFrequency: PropTypes.number,
  width: PropTypes.number,
  stepFrequency: PropTypes.number,
  showStep: PropTypes.bool,
};

export default SingleLabel;

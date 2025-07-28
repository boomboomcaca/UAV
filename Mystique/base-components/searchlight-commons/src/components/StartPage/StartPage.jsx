import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import { PlayIcon } from 'dc-icon';
import decentestPng from './assets/decentest-icon.png';
import styles from './StartPage.module.less';

const StartPage = (props) => {
  const { onStart, visible } = props;
  const handleStep = () => {
    onStart();
  };
  return (
    <div className={styles.initTaskContainer} style={{ display: visible ? 'block' : 'none' }}>
      <div className={styles.initOne}>
        <div className={styles.initOne_aperature}>
          <div className={styles.initOne_circle} onClick={handleStep}>
            <PlayIcon iconSize={80} color="#3CE5D3" />
            <b>开始</b>
          </div>
          <div className={styles.initOne_aperature_b} />
          <div className={styles.initOne_aperature_t} />
          <div className={styles.initOne_circle_l} />
          <div className={styles.initOne_circle_r} />
        </div>
        <img className={styles.initOne_logo} alt="" src={decentestPng} />
      </div>
    </div>
  );
};
StartPage.defaultProps = {
  onStart: () => {},
  visible: true,
};

StartPage.propTypes = {
  onStart: PropTypes.func,
  visible: PropTypes.bool,
};

export default StartPage;

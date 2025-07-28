import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import { PlayIcon, StopIcon } from 'dc-icon';
import styles from './StartPage.module.less';

const StartPage = (props) => {
  const [iconSize, setIconSize] = useState(40);
  const [fontSize, setFontSize] = useState(24);
  const [start, setStart] = useState(false);
  const prevClickTimeRef = useRef(0);
  /**
   * @type {current:HTMLElement}
   */
  const rootNodeRef = useRef();
  const { onStart } = props;
  const handleStep = () => {
    const dt = new Date().getTime();
    if (dt - prevClickTimeRef.current > 200) {
      setStart(!start);
      onStart(!start);
    }
  };

  useEffect(() => {
    const tmr = setInterval(() => {
      if (rootNodeRef.current) {
        const rect = rootNodeRef.current.getClientRects()[0];
        const refer = Math.min(rect.width, rect.height);
        let is = Math.round(refer * 0.11);
        if (is < 16) is = 16;
        if (is > 72) {
          is = 72;
        }
        setIconSize(is);
        let fs = Math.round(refer * 0.066666);
        if (fs < 10) fs = 10;
        if (fs > 36) fs = 36;
        setFontSize(fs);
      }
    }, 1000);
    return () => {
      clearInterval(tmr);
    };
  }, []);

  return (
    <div className={styles.initTaskContainer} ref={rootNodeRef}>
      <div className={styles.circle_l} />
      <div className={styles.initOne_aperature}>
        <div className={styles.layer1}>
          <div className={styles.initOne_circle} onClick={handleStep}>
            <div style={{ marginTop: '99%' }}>
              {start ? (
                <StopIcon iconSize={iconSize} color="#FF0000" />
              ) : (
                <PlayIcon iconSize={iconSize} color="#3CE5D3" />
              )}
            </div>
            <b style={{ marginTop: '98%', fontSize }}>开始</b>
          </div>
          <div className={styles.initOne_aperature_b} />
          <div className={styles.initOne_aperature_t} />
        </div>
      </div>

      <div className={styles.circle_r} />
    </div>
  );
};
StartPage.defaultProps = {
  onStart: () => {},
};

StartPage.propTypes = {
  onStart: PropTypes.func,
};

export default StartPage;

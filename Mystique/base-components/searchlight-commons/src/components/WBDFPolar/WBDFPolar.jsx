import React, { useEffect, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import Dial from './Dial.jsx';
import TickLabel from './TickLabel.jsx';
import RenderLayer from './RenderLayer.jsx';
import styles from './styles.module.less';

export default function WBDFPolar(props) {
  const { showTick, showTickLabel, tickInside, onLoaded, resize } = props;
  const [scope, setScope] = useState(0);
  const wbdf = useRef();
  const prevSizeRefg = useRef({ w: 0, h: 0 });

  useEffect(() => {
    const tmr = setInterval(() => {
      const { offsetWidth, offsetHeight } = wbdf.current.parentElement;
      if (prevSizeRefg.current.w !== offsetWidth || prevSizeRefg.current !== offsetHeight) {
        prevSizeRefg.current = { w: offsetWidth, h: offsetHeight };
        setScope(offsetWidth >= offsetHeight ? offsetHeight : offsetWidth);
      }
    }, 1000);
    return () => {
      clearInterval(tmr);
    };
  }, []);

  return (
    <div className={styles.WBDFPolar} style={{ height: scope, width: scope }} ref={wbdf}>
      <div
        className={
          (!showTick || tickInside) && !showTickLabel
            ? styles.dialNoLabelAndTick
            : showTick && !tickInside
            ? styles.dial1
            : styles.dial
        }
      >
        <Dial {...props} />
      </div>
      {showTickLabel && <TickLabel {...props} />}
      <RenderLayer onReady={onLoaded} />
    </div>
  );
}

WBDFPolar.defaultProps = {
  showTick: true,
  showTickLabel: true,
  tickInside: false,
  onLoaded: undefined,
  resize: 0,
};

WBDFPolar.propTypes = {
  showTick: PropTypes.bool,
  showTickLabel: PropTypes.bool,
  tickInside: PropTypes.bool,
  onLoaded: PropTypes.func,
  resize: PropTypes.any,
};

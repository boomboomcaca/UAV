import React from 'react';
import PropTypes from 'prop-types';
import styles from './label.style.module.less';

const TickLabel = (props) => {
  // ['N', '30°', '60°', '90°', '120°', '150°', '180°', '210°', '240°', '270°', '300°', '330°']
  const { showTick, tickInside } = props;

  return (
    <div className={styles.labelRoot}>
      {[0, 30, 60, 90, 120, 150, 180, 210, 240, 270, 300, 330].map((item, index) => {
        return (
          <div
            className={tickInside || !showTick ? styles.tickInside : styles.tickInside}
            style={{ rotate: `${item - index * 0.1}deg` }}
          >
            <div style={{ rotate: `${0 - item}deg` }}>{`${item}°`}</div>
          </div>
        );
      })}
    </div>
  );
};

TickLabel.defaultProps = {
  showTick: true,
  tickInside: false,
};

TickLabel.propTypes = {
  showTick: PropTypes.bool,
  tickInside: PropTypes.bool,
};

export default TickLabel;

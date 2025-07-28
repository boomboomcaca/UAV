import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';

const AxisY = (props) => {
  const { minimumY, maximumY, unit } = props;
  const [ticks, setTicks] = useState([maximumY, minimumY]);
  useEffect(() => {
    const s = Math.round(maximumY - minimumY) / 10;
    const ts = [];
    for (let i = 0; i < 10; i += 1) {
      ts.push(maximumY - i * s);
    }
    ts.push(minimumY);
    setTicks(ts);
  }, [minimumY, maximumY]);

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'space-between',
        textAlign: 'right',
        paddingRight: '5px',
        width: '45px',
      }}
    >
      {ticks.map((v, index) => {
        return index === 0 ? (
          <div style={{ flex: 1 }}>
            <div style={{ color: 'var(--theme-font-30)', fontSize: '12px' }}>{unit}</div>
            <div style={{ fontSize: '12px', color: 'var(--theme-font-50)' }}>{v}</div>
          </div>
        ) : index === 10 ? (
          <div style={{ fontSize: '12px', color: 'var(--theme-font-50)' }}>{v}</div>
        ) : (
          <div style={{ flex: 1, fontSize: '12px', color: 'var(--theme-font-50)' }}>{v}</div>
        );
      })}
    </div>
  );
};

// AxisX.defaultProps = {
//   minimumY: -20,
//   maximumY: 80,
// };

AxisY.propTypes = {
  minimumY: PropTypes.number.isRequired,
  maximumY: PropTypes.number.isRequired,
  unit: PropTypes.string.isRequired,
};

export default AxisY;

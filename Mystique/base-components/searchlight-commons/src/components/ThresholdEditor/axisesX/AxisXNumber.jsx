import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';

const AxisXNumber = (props) => {
  const { zoomStart, zoomStop, unit } = props;
  const [ticks, setTicks] = useState([]);

  useEffect(() => {
    if (zoomStart !== null && zoomStop !== null) {
      // 连续值就显示11个
      const ticks1 = [];
      const gap = (zoomStop - zoomStart) / 10;
      for (let i = 0; i < 11; i += 1) {
        ticks1.push((zoomStart + i * gap).toFixed(4));
      }
      setTicks(ticks1);
    }
  }, [zoomStart, zoomStop]);

  return (
    <div
      style={{
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginLeft: '45px',
      }}
    >
      {ticks.map((v, index) => {
        return (
          <div
            style={{
              flex: index === 0 || index === 10 ? 1 : 2,
              fontSize: '12px',
              color: 'var(--theme-font-50)',
              textAlign: index === 0 ? 'left' : index === 10 ? 'right' : 'center',
            }}
          >
            {v}
          </div>
        );
      })}
    </div>
  );
};

AxisXNumber.defaultProps = {
  // 如果设置了labels，则这里为索引，否则为值
  zoomStart: null,
  // 如果设置了labels，则这里为索引，否则为值
  zoomStop: null,
  unit: 'MHz',
};

AxisXNumber.propTypes = {
  zoomStart: PropTypes.number,
  zoomStop: PropTypes.number,
  unit: PropTypes.string,
};

export default AxisXNumber;

import React, { useState, memo, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';

const AxisXLabels = (props) => {
  const { labels, zoomStart, zoomStop, unit } = props;
  const [ticks, setTicks] = useState([]);
  const [tickStyle, setTickStyle] = useState({
    marginTop: '0',
    rotate: 0,
  });

  useEffect(() => {
    if (labels) {
      setTicks(labels.slice(zoomStart, zoomStop + 1));
      if (labels.length > 20) {
        setTickStyle({
          marginTop: '8px',
          rotate: 45,
        });
        if (labels.length > 35) {
          setTickStyle({
            marginTop: '15px',
            rotate: 90,
          });
        }
      }
    }
  }, [labels, zoomStart, zoomStop]);

  return (
    <div
      style={{
        // display: 'flex',
        // flexDirection: 'row',
        // justifyContent: 'space-between',
        position: 'relative',
        marginLeft: '45px',
      }}
    >
      {ticks.map((v, index) => {
        const percGap = 100 / ticks.length;
        return (
          <div
            style={{
              position: 'absolute',
              left: `${percGap * index}%`,
              width: `${percGap}%`,
              fontSize: '12px',
              color: 'var(--theme-font-50)',
              transform: `rotate(-${tickStyle.rotate}deg)`,
              marginTop: tickStyle.marginTop,
              textAlign: 'center',
            }}
          >
            {v}
          </div>
        );
        // );
      })}
    </div>
  );
};

AxisXLabels.defaultProps = {
  labels: [],
  // 如果设置了labels，则这里为索引，否则为值
  zoomStart: 0,
  // 如果设置了labels，则这里为索引，否则为值
  zoomStop: 1,
  //   minimumY: -20,
  //   maximumY: 80,
  unit: 'MHz',
};

AxisXLabels.propTypes = {
  labels: PropTypes.array,
  zoomStart: PropTypes.number,
  zoomStop: PropTypes.number,
  unit: PropTypes.string,
};

export default AxisXLabels;

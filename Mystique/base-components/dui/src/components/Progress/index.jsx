import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import Circle from 'react-circle';
import styles from './styles.module.less';

export default function Progress(props) {
  const { value, closeClick } = props;
  const [nums, setNums] = useState(5);
  let timer = useRef();
  const [flag, setFlag] = useState(true);
  useEffect(() => {
    if (value === 100) {
      start();
    }
  }, [value]);
  useEffect(() => {
    if (nums === 0) {
      clean();
      setFlag(false);
    }
  }, [nums]);

  const start = () => {
    setNums(5);
    timer.current = setInterval(() => {
      //因为闭包原因，不是直接通过setNums(),修改里面的值，如果通过这种方式会一直为1
      setNums((n) => {
        return n - 1;
      });
    }, 1000);
  };
  const clean = () => {
    clearInterval(timer.current);
  };
  return (
    <div className={styles.box}>
      {flag && (
        <>
          <Circle
            animate={true} // Boolean: Animated/Static progress
            animationDuration="1s" // String: Length of animation
            responsive={false} // Boolean: Make SVG adapt to parent size
            size="150" // String: Defines the size of the circle.
            lineWidth="20" // String: Defines the thickness of the circle's stroke.
            progress={value} // String: Update to change the progress and percentage.
            progressColor={value === 100 ? 'green' : 'red'} // String: Color of "progress" portion of circle.
            bgColor="#6b778c" // String: Color of "empty" portion of circle.
            textColor="#fff" // String: Color of percentage text color.
            percentSpacing={10} // Number: Adjust spacing of "%" symbol and number.
            roundedStroke={true} // Boolean: Rounded/Flat line ends
            showPercentageSymbol={true} // Boolean: Show/hide only the "%" symbol.
          />
          {value === 100 ? (
            <div className={styles.text}>{`${nums}s后离开`}</div>
          ) : (
            <div
              onClick={() => {
                setFlag(false);
                closeClick('close');
              }}
            >
              x
            </div>
          )}
        </>
      )}
    </div>
  );
}
Progress.defaultProps = {
  value: 0,
  closeClick: () => {},
};

Progress.propTypes = {
  closeClick: PropTypes.func,
  value: PropTypes.number,
};

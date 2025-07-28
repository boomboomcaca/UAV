import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const radius = 40;
const strokeWidth = 10;
const circumference = Math.round(Math.PI * radius * 2);

const Circle = (props) => {
  const { className, progress, children } = props;

  const svgSize = '100%';
  const strokeLinecap = 'round';
  const strokeDashoffset = Math.round(((100 - Math.min(progress, 100)) / 100) * circumference);
  const transition = `stroke-dashoffset 1s ease-out`;

  const getCircle1 = (stroke, thickness) => {
    return <circle stroke={stroke} cx="50" cy="50" r={radius} strokeWidth={thickness} fill="none" />;
  };

  const getCircle2 = (stroke, thickness) => {
    return (
      <circle
        stroke={stroke}
        transform="rotate(-90 50 50)"
        cx="50"
        cy="50"
        r={radius}
        strokeDasharray={circumference}
        strokeWidth={thickness}
        strokeDashoffset={circumference}
        strokeLinecap={strokeLinecap}
        fill="none"
        style={{ strokeDashoffset, transition: progress !== 0 ? transition : null }}
      />
    );
  };

  return (
    <div className={classnames(styles.root, className)}>
      <svg width={svgSize} height={svgSize} viewBox="0 0 100 100">
        <defs>
          <linearGradient id="circle_gradient" gradientTransform="rotate(64)">
            <stop offset="0%" stopColor="#35E065" />
            <stop offset="50%" stopColor="#35E065" />
            <stop offset="100%" stopColor="#3CE5D3" />
          </linearGradient>
        </defs>
        {getCircle1('#353D5B', strokeWidth + 6)}
        {getCircle1('rgba(4, 5, 27, 0.3)', strokeWidth + 2)}
        {getCircle1('rgb(61, 99, 120)', strokeWidth + 2)}
        {getCircle1('rgb(46, 52, 80)', strokeWidth)}
        {getCircle2('url(#circle_gradient)', strokeWidth)}
        {getCircle2('#20202040', strokeWidth - 2)}
      </svg>
      <div className={styles.text}>{children}</div>
    </div>
  );
};

Circle.defaultProps = {
  className: null,
  progress: 0,
  children: null,
};

Circle.propTypes = {
  className: PropTypes.any,
  progress: PropTypes.number,
  children: PropTypes.any,
};

export default Circle;

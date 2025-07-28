import React from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';
import classNameMix from 'classnames';

const SvgComponent = (props) => {
  const { children, size, onClick } = props;
  const sizeHandle = () => {
    switch (size) {
      case 'small':
        return styles.small;
      case 'large':
        return styles.large;
      default:
        return '';
    }
  };
  return (
    <div className={classNameMix(styles.btn, sizeHandle())} onClick={onClick}>
      <svg
        width="80px"
        height="85px"
        viewBox="0 0 80 85"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <g filter="url(#filter0_d)">
          <path
            d="M41.0558 59.3031C41.8778 57.2035 43.8269 55.8042 46.0102 55.2413C56.3564 52.5734 64 43.1794 64 32C64 18.7452 53.2548 8 40 8C26.7452 8 16 18.7452 16 32C16 43.1184 23.5605 52.4709 33.8207 55.197C35.8022 55.7235 37.5851 56.9408 38.4682 58.791L39.0069 59.9194C39.3838 60.7091 40.5215 60.668 40.8405 59.8532L41.0558 59.3031Z"
            fill="#353D5B"
          />
        </g>
        <defs>
          <filter
            id="filter0_d"
            x="0"
            y="0"
            width="80"
            height="84.4886"
            filterUnits="userSpaceOnUse"
            colorInterpolationFilters="sRGB"
          >
            <feFlood floodOpacity="0" result="BackgroundImageFix" />
            <feColorMatrix
              in="SourceAlpha"
              type="matrix"
              values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            />
            <feOffset dy="8" />
            <feGaussianBlur stdDeviation="8" />
            <feColorMatrix
              type="matrix"
              values="0 0 0 0 0.0109375 0 0 0 0 0.0420673 0 0 0 0 0.145833 0 0 0 0.2 0"
            />
            <feBlend
              mode="normal"
              in2="BackgroundImageFix"
              result="effect1_dropShadow"
            />
            <feBlend
              mode="normal"
              in="SourceGraphic"
              in2="effect1_dropShadow"
              result="shape"
            />
          </filter>
        </defs>
      </svg>
      <div className={styles.btn_type}>{children}</div>
    </div>
  );
};
SvgComponent.defaultProps = {
  children: null,
  size: '',
  onClick: null,
};

SvgComponent.propTypes = {
  children: PropTypes.element,
  size: PropTypes.string,
  onClick: PropTypes.func,
};
export default SvgComponent;

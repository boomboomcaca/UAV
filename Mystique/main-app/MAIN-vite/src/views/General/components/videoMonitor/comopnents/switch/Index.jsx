import React from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";

const SvgIcon = () => {
  return (
    <svg
      width="36"
      height="20"
      viewBox="0 0 36 20"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      preserveAspectRatio="none meet"
    >
      <rect
        x="0.5"
        y="0.5"
        width="35"
        height="19"
        rx="9.5"
        fill="#082E59"
        stroke="url(#paint0_linear_177_26166)"
      />
      <g filter="url(#filter0_ii_177_26166)">
        <circle cx="10" cy="10" r="10" fill="#204C80" />
      </g>
      <defs>
        <filter
          id="filter0_ii_177_26166"
          x="0"
          y="-1"
          width="20"
          height="22"
          filterUnits="userSpaceOnUse"
          color-interpolation-filters="sRGB"
        >
          <feFlood flood-opacity="0" result="BackgroundImageFix" />
          <feBlend
            mode="normal"
            in="SourceGraphic"
            in2="BackgroundImageFix"
            result="shape"
          />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix
            type="matrix"
            values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.2 0"
          />
          <feBlend
            mode="normal"
            in2="shape"
            result="effect1_innerShadow_177_26166"
          />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="-1" />
          <feGaussianBlur stdDeviation="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.3 0"
          />
          <feBlend
            mode="normal"
            in2="effect1_innerShadow_177_26166"
            result="effect2_innerShadow_177_26166"
          />
        </filter>
        <linearGradient
          id="paint0_linear_177_26166"
          x1="18"
          y1="0"
          x2="18"
          y2="20"
          gradientUnits="userSpaceOnUse"
        >
          <stop stop-color="#091537" />
          <stop offset="1" stop-color="#214A79" />
        </linearGradient>
      </defs>
    </svg>
  );
};

const SvgIcon1 = () => {
  return (
    <svg
      width="36"
      height="20"
      viewBox="0 0 36 20"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <rect
        x="0.5"
        y="0.5"
        width="35"
        height="19"
        rx="9.5"
        fill="#40A9FF"
        stroke="url(#paint0_linear_177_26300)"
      />
      <g filter="url(#filter0_ii_177_26300)">
        <circle cx="26" cy="10" r="10" fill="#204C80" />
      </g>
      <defs>
        <filter
          id="filter0_ii_177_26300"
          x="16"
          y="-1"
          width="20"
          height="22"
          filterUnits="userSpaceOnUse"
          color-interpolation-filters="sRGB"
        >
          <feFlood flood-opacity="0" result="BackgroundImageFix" />
          <feBlend
            mode="normal"
            in="SourceGraphic"
            in2="BackgroundImageFix"
            result="shape"
          />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix
            type="matrix"
            values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.2 0"
          />
          <feBlend
            mode="normal"
            in2="shape"
            result="effect1_innerShadow_177_26300"
          />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="-1" />
          <feGaussianBlur stdDeviation="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.3 0"
          />
          <feBlend
            mode="normal"
            in2="effect1_innerShadow_177_26300"
            result="effect2_innerShadow_177_26300"
          />
        </filter>
        <linearGradient
          id="paint0_linear_177_26300"
          x1="18"
          y1="0"
          x2="18"
          y2="20"
          gradientUnits="userSpaceOnUse"
        >
          <stop stop-color="#091537" />
          <stop offset="1" stop-color="#214A79" />
        </linearGradient>
      </defs>
    </svg>
  );
};

const Switch = (props) => {
  const { value, onChange, labels } = props;
  return (
    <div
      className={`${styles.wroot} ${value && styles.true}`}
      onClick={() => {
        onChange(!value);
      }}
    >
      <span>{labels[value ? 1 : 0]}</span>
      <div className={styles.icon}>{value ? <SvgIcon1 /> : <SvgIcon />}</div>
    </div>
  );
};

Switch.defaultProps = {
  value: false,
  onChange: () => {},
  labels: ["否", "是"],
};

Switch.propTypes = {
  value: PropTypes.bool,
  onChange: PropTypes.func,
  labels: PropTypes.array,
};

export default Switch;

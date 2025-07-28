import React, { useState } from "react";
import PropTypes from "prop-types";

import styles from "./style.module.less";

const SvgComponent = (props) => {
  const { width, height, fill, onChange, tracking } = props;
  const [isTracking, setTracking] = useState(true); // useState(tracking);

  return (
    <svg
      width={width}
      height={height}
      viewBox="0 0 292 292"
      fill="none"
      shapeRendering="geometricPrecision"
      className={styles.svgcomRoot}
    >
      <g filter="url(#a1)">
        {/* right */}
        <path
          className={styles.right}
          onMouseDown={() => {
            onChange({ action: "right" });
          }}
          onMouseUp={() => {
            console.log("stop::::::::::::");
            onChange({ action: "stop" });
          }}
          d="M218.664 218.664a103.448 103.448 0 0 0 22.392-33.48 103.254 103.254 0 0 0 7.895-39.471 102.992 102.992 0 0 0-7.804-39.453 102.807 102.807 0 0 0-22.315-33.428l-73 73 72.832 72.832Z"
          fill="#113662"
        />
      </g>
      <g filter="url(#b1)">
        {/* top */}
        <path
          className={styles.right}
          onMouseDown={() => {
            onChange({ action: "up" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
          d="M73.336 73.336a103.443 103.443 0 0 1 33.48-22.392 103.254 103.254 0 0 1 39.471-7.895 102.984 102.984 0 0 1 39.453 7.804 102.788 102.788 0 0 1 33.428 22.315l-73 73-72.832-72.832Z"
          fill="#072B54"
        />
      </g>
      <g filter="url(#c1)">
        {/* bottom */}
        <path
          className={styles.right}
          onMouseDown={() => {
            onChange({ action: "down" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
          d="M218.664 218.664a103.448 103.448 0 0 1-33.48 22.392 103.254 103.254 0 0 1-39.471 7.895 102.992 102.992 0 0 1-39.453-7.804 102.807 102.807 0 0 1-33.428-22.315l73-73 72.832 72.832Z"
          fill="#072B54"
        />
      </g>
      <g filter="url(#d1)">
        {/* left */}
        <path
          className={styles.right}
          onMouseDown={() => {
            onChange({ action: "left" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
          d="M73.336 73.336a103.443 103.443 0 0 0-22.392 33.48 103.254 103.254 0 0 0-7.895 39.471 102.984 102.984 0 0 0 7.804 39.453 102.788 102.788 0 0 0 22.315 33.428l73-73-72.832-72.832Z"
          fill="#113662"
        />
      </g>

      <g
        filter="url(#e1)"
        className={styles.stopTrack}
        onClick={() => {
          if (isTracking) {
            onChange({
              action: "stoptrack",
            });
          }
        }}
      >
        <circle cx="146" cy="146" r="32" fill="#148BED" />
        {tracking && (
          /* <circle cx="146" cy="146" r="8" fill={fill}></circle> */
          <>
            <path
              d="M146 128v5m0 27v5m18.5-18.5h-5m-27 0h-5"
              stroke={fill}
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            />
            <circle cx="146" cy="146" r="13" stroke={fill} strokeWidth="2" />
            <rect
              x="139"
              y="139"
              width="14"
              height="14"
              rx="2"
              fill="#FC511B"
            />
          </>
        )}
      </g>
      <g filter="url(#f1)">
        <path
          fillRule="evenodd"
          clipRule="evenodd"
          d="M146 262c64.065 0 116-51.935 116-116S210.065 30 146 30 30 81.935 30 146s51.935 116 116 116Zm.2-6.6c60.31 0 109.2-48.891 109.2-109.2C255.4 85.89 206.51 37 146.2 37S37 85.89 37 146.2c0 60.309 48.89 109.2 109.2 109.2Z"
          fill="url(#g1)"
        />
      </g>
      <path
        d="m136 80 10-10 10 10m-20 132 10 10 10-10m56-76 10 10-10 10M80 136l-10 10 10 10"
        stroke="#30B4FF"
        strokeWidth="3"
        strokeLinecap="round"
        strokeLinejoin="round"
      />

      <defs>
        <filter
          id="a1"
          x="145.832"
          y="71.832"
          width="103.119"
          height="147.832"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />

          <feBlend in="SourceGraphic" in2="BackgroundImageFix" result="shape" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="1" />

          <feGaussianBlur stdDeviation="1" />

          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />

          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.25 0" />

          <feBlend in2="shape" result="effect1_innerShadow_143_72351" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="-1" />

          <feGaussianBlur stdDeviation="2" />

          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />

          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.2 0" />

          <feBlend
            in2="effect1_innerShadow_143_72351"
            result="effect2_innerShadow_143_72351"
          />
        </filter>
        <filter
          id="b1"
          x="73.336"
          y="42.049"
          width="145.832"
          height="105.119"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />

          <feBlend in="SourceGraphic" in2="BackgroundImageFix" result="shape" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="1" />

          <feGaussianBlur stdDeviation="1" />

          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />

          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.25 0" />

          <feBlend in2="shape" result="effect1_innerShadow_143_72351" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="-1" />

          <feGaussianBlur stdDeviation="2" />

          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />

          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.2 0" />

          <feBlend
            in2="effect1_innerShadow_143_72351"
            result="effect2_innerShadow_143_72351"
          />
        </filter>
        <filter
          id="c1"
          x="72.832"
          y="144.832"
          width="145.832"
          height="105.119"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />

          <feBlend in="SourceGraphic" in2="BackgroundImageFix" result="shape" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="1" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.25 0" />
          <feBlend in2="shape" result="effect1_innerShadow_143_72351" />
          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="-1" />
          <feGaussianBlur stdDeviation="2" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.2 0" />
          <feBlend
            in2="effect1_innerShadow_143_72351"
            result="effect2_innerShadow_143_72351"
          />
        </filter>
        <filter
          id="d1"
          x="43.049"
          y="72.336"
          width="103.119"
          height="147.832"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />
          <feBlend in="SourceGraphic" in2="BackgroundImageFix" result="shape" />
          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="1" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.25 0" />
          <feBlend in2="shape" result="effect1_innerShadow_143_72351" />
          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="-1" />
          <feGaussianBlur stdDeviation="2" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.2 0" />
          <feBlend
            in2="effect1_innerShadow_143_72351"
            result="effect2_innerShadow_143_72351"
          />
        </filter>
        <filter
          id="e1"
          x="114"
          y="113"
          width="64"
          height="66"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />
          <feBlend in="SourceGraphic" in2="BackgroundImageFix" result="shape" />
          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="1" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.25 0" />
          <feBlend in2="shape" result="effect1_innerShadow_143_72351" />
          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="-1" />
          <feGaussianBlur stdDeviation="2" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.2 0" />
          <feBlend
            in2="effect1_innerShadow_143_72351"
            result="effect2_innerShadow_143_72351"
          />
        </filter>
        <filter
          id="f1"
          x="30"
          y="30"
          width="232.5"
          height="232"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />
          <feBlend in="SourceGraphic" in2="BackgroundImageFix" result="shape" />
          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.2 0" />
          <feBlend in2="shape" result="effect1_innerShadow_143_72351" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dx="0.5" />
          <feGaussianBlur stdDeviation="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.4 0" />
          <feBlend
            in2="effect1_innerShadow_143_72351"
            result="effect2_innerShadow_143_72351"
          />
        </filter>
        <linearGradient
          id="g1"
          x1="36.991"
          y1="175.931"
          x2="256.867"
          y2="116.14"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#205695" />

          <stop offset="1" stopColor="#18406E" />
        </linearGradient>
      </defs>
    </svg>
  );
};
SvgComponent.defaultProps = {
  onChange: () => {},
  tracking: true, // 让他随便点停止吧
};

SvgComponent.propTypes = {
  onChange: PropTypes.func,
  tracking: PropTypes.any,
};

export default SvgComponent;

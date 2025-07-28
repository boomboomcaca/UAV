import React from 'react';
import PropeTypes from 'prop-types';

const Dial = (props) => {
  const { tickInside, showTick } = props;
  return (
    <svg width="100%" height="100%" viewBox="0 0 320 298" fill="none" xmlns="http://www.w3.org/2000/svg">
      <circle cx="160.591" cy="150.309" r="117" fill="#04051B" />
      <g filter="url(#filter0_i_0:1)">
        <path
          d="M277.591 150.309C277.591 214.927 225.208 267.309 160.591 267.309C95.9735 267.309 43.5908 214.927 43.5908 150.309C43.5908 85.6919 95.9735 33.3093 160.591 33.3093C225.208 33.3093 277.591 85.6919 277.591 150.309Z"
          fill="#04051B"
        />
      </g>
      <g filter="url(#filter1_ii_0:1)">
        <circle cx="159" cy="151" r="76" fill="#04051B" />
      </g>
      <g opacity="0.1">
        <circle cx="161" cy="150" r="116.5" stroke="white" />
        <circle cx="161" cy="150" r="92.5" stroke="white" />
        <circle cx="161" cy="150" r="70.5" stroke="white" />
        <circle cx="161" cy="150" r="45.5" stroke="white" />
        <circle cx="161" cy="150" r="23.5" stroke="white" />
        <path d="M160.407 33.4963L160.407 267.496" stroke="white" />
        <path d="M164.467 67.7862L156.346 67.7862" stroke="white" />
        <path d="M164.467 46L156.346 46" stroke="white" />
        <path d="M164.467 91.9021L156.346 91.9021" stroke="white" />
        <path d="M164.467 115.264L156.346 115.264" stroke="white" />
        <path d="M164.467 138.627L156.346 138.627" stroke="white" />
        <path d="M164.467 161.989L156.346 161.989" stroke="white" />
        <path d="M164.467 185.351L156.346 185.351" stroke="white" />
        <path d="M164.467 208.714L156.346 208.714" stroke="white" />
        <path d="M164.467 232.076L156.346 232.076" stroke="white" />
        <path d="M164.467 254L156.346 254" stroke="white" />
        <path d="M277.406 150.496L43.4065 150.496" stroke="white" />
        <path d="M243.117 154.557L243.117 146.436" stroke="white" />
        <path d="M264.903 154.557L264.903 146.436" stroke="white" />
        <path d="M219.001 154.557L219.001 146.436" stroke="white" />
        <path d="M195.638 154.557L195.638 146.436" stroke="white" />
        <path d="M172.276 154.557L172.276 146.436" stroke="white" />
        <path d="M148.914 154.557L148.914 146.436" stroke="white" />
        <path d="M125.551 154.557L125.551 146.436" stroke="white" />
        <path d="M102.189 154.557L102.189 146.436" stroke="white" />
        <path d="M78.8268 154.557L78.8268 146.436" stroke="white" />
        <path d="M56.9028 154.557L56.9028 146.436" stroke="white" />
      </g>
      <rect x="35" y="25" width="252" height="252" fill="url(#pattern0)" />
      {showTick ? (
        <>
          {/* 0度 刻度 */}
          <rect x="159" y={tickInside ? '33' : '15'} width="3" height="8" fill="#35E065" fillOpacity="0.5" />
          {/* 180度 刻度 */}
          <rect x="159" y={tickInside ? '259' : '277'} width="3" height="8" fill="#35E065" fillOpacity="0.5" />
          {/* 90°刻度 */}
          <rect
            x={tickInside ? '277' : '297'}
            y="149"
            width="3"
            height="8"
            transform={`rotate(90 ${tickInside ? '277' : '297'} 149)`}
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 270°刻度 */}
          <rect
            x={tickInside ? '53' : '32'}
            y="149"
            width="3"
            height="8"
            transform={`rotate(90 ${tickInside ? '53' : '32'} 149)`}
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 30°刻度 */}
          <rect
            x={tickInside ? '216' : '229'}
            y={tickInside ? '48' : '33'}
            width="3"
            height="8"
            transform={`rotate(30 ${tickInside ? '216' : '229'} ${tickInside ? '48' : '33'})`}
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 210°刻度 */}
          <rect
            x={tickInside ? '107' : '96'}
            y={tickInside ? '244' : '256'}
            width="3"
            height="8"
            transform={`rotate(30 ${tickInside ? '107' : '96'} ${tickInside ? '244' : '256'})`}
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 120°刻度 */}
          <rect
            x={tickInside ? '265' : '278.4'}
            y={tickInside ? '203' : '219'}
            width="3"
            height="8"
            transform={`rotate(120 ${tickInside ? '265' : '278.4'} ${tickInside ? '203' : '219'})`}
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 300°刻度 */}
          <rect
            x={tickInside ? '65' : '49.4'}
            y={tickInside ? '98' : '86'}
            width="3"
            height="8"
            transform={`rotate(120 ${tickInside ? '65' : '49.4'} ${tickInside ? '98' : '86'})`}
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 60°刻度 */}
          <rect
            x={tickInside ? '261' : '277'}
            y={tickInside ? '91' : '79'}
            transform={`rotate(60 ${tickInside ? '261' : '277'} ${tickInside ? '91' : '79'})`}
            width="3"
            height="8"
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 240°刻度 */}
          <rect
            x={tickInside ? '64' : '48'}
            y={tickInside ? '201' : '212'}
            transform={`rotate(60 ${tickInside ? '64' : '48'} ${tickInside ? '201' : '212'})`}
            width="3"
            height="8"
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 150°刻度 */}
          <rect
            x={tickInside ? '222' : '228.6'}
            y={tickInside ? '249' : '267'}
            transform={`rotate(150 ${tickInside ? '222' : '228.6'} ${tickInside ? '249' : '267'})`}
            width="3"
            height="8"
            fill="#35E065"
            fillOpacity="0.5"
          />
          {/* 330° 刻度 */}
          <rect
            x={tickInside ? '103' : '98.6'}
            y={tickInside ? '58' : '39'}
            transform={`rotate(150 ${tickInside ? '103' : '98.6'} ${tickInside ? '58' : '39'})`}
            width="3"
            height="8"
            fill="#35E065"
            fillOpacity="0.5"
          />
        </>
      ) : null}
      <defs>
        <filter
          id="filter0_i_0:1"
          x="43.6"
          y="33.3"
          width="234"
          height="240"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />
          <feBlend mode="normal" in="SourceGraphic" in2="BackgroundImageFix" result="shape" />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="6" />
          <feGaussianBlur stdDeviation="7.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix type="matrix" values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.4 0" />
          <feBlend mode="normal" in2="shape" result="effect1_innerShadow_0:1" />
        </filter>
        <filter
          id="filter1_ii_0:1"
          x="83"
          y="74"
          width="152"
          height="154"
          filterUnits="userSpaceOnUse"
          colorInterpolationFilters="sRGB"
        >
          <feFlood floodOpacity="0" result="BackgroundImageFix" />
          <feBlend mode="normal" in="SourceGraphic" in2="BackgroundImageFix" result="shape" />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="1" />
          <feGaussianBlur stdDeviation="0.5" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix type="matrix" values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.3 0" />
          <feBlend mode="normal" in2="shape" result="effect1_innerShadow_0:1" />
          <feColorMatrix
            in="SourceAlpha"
            type="matrix"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />
          <feOffset dy="-1" />
          <feGaussianBlur stdDeviation="1" />
          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />
          <feColorMatrix type="matrix" values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.3 0" />
          <feBlend mode="normal" in2="effect1_innerShadow_0:1" result="effect2_innerShadow_0:1" />
        </filter>
        <pattern id="pattern0" patternContentUnits="objectBoundingBox" width="1" height="1">
          <use xlinkHref="#image0_0:1" transform="scale(0.00195695)" />
        </pattern>
        <linearGradient
          id="paint0_linear_0:1"
          x1="160.706"
          y1="35.8959"
          x2="160.706"
          y2="138.171"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="white" />
          <stop offset="1" stopColor="white" stopOpacity="0" />
        </linearGradient>
        <image
          id="image0_0:1"
          width="511"
          height="511"
          xlinkHref="data:image/png;base64,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"
        />
      </defs>
    </svg>
  );
};

Dial.defaultProps = {
  tickInside: false,
  showTick: true,
};

Dial.propTypes = {
  tickInside: PropeTypes.bool,
  showTick: PropeTypes.bool,
};

export default Dial;

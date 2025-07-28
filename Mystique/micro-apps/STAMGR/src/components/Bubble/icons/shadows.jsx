/* eslint-disable max-len */

import React from 'react';

const shadows = (color = '#2DB3FF') => (
  <svg /* width="120" height="120"  */ viewBox="0 0 120 120" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g>
      <circle cx="60" cy="59.9995" r="60" fill={color} fillOpacity="0.01" />
    </g>
    <circle cx="60" cy="59.9995" r="59.5" stroke={color} strokeOpacity="0.5" />
    <path
      d="M67.9 50.8499C46.153 72.1432 2 87.8001 2 60.244C2 27.7311 29.7342 2.00049 63.946 2.00049C100.85 2.00049 89.647 29.5566 67.9 50.8499Z"
      fill={`url(#${color}_paint0_linear)`}
      fillOpacity="0.15"
    />
    <path
      d="M72.5204 8.407C73.4019 12.9522 63.6524 11.3836 51.3993 13.76C39.1463 16.1365 30.8279 23.662 29.0747 19.1261C27.3216 14.5903 36.837 7.62132 49.0901 5.24483C61.3431 2.86834 71.5286 3.29361 72.5204 8.407Z"
      fill={`url(#${color}_paint1_linear)`}
    />
    <path
      d="M99.5679 22.4006C103.006 15.5253 86.9311 7.61033 80.1996 7.06748C70.8791 6.31584 72.0669 11.1251 83.0673 15.5253C89.3543 18.0401 97.9865 25.5633 99.5679 22.4006Z"
      fill={`url(#${color}_paint2_linear)`}
    />
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M55.0011 106.999C84.8245 106.999 109.001 82.822 109.001 52.9986C109.001 48.2285 108.383 43.6029 107.221 39.1975C110.288 45.8286 111.999 53.2148 111.999 61.0008C111.999 89.7196 88.7182 113.001 59.9994 113.001C43.7425 113.001 29.228 105.541 19.6926 93.857C29.1596 102.045 41.5019 106.999 55.0011 106.999Z"
      fill={`url(#${color}_paint3_linear)`}
    />
    <defs>
      <linearGradient
        id={`${color}_paint0_linear`}
        x1="45"
        y1="2.00049"
        x2="45"
        y2="75.0005"
        gradientUnits="userSpaceOnUse"
      >
        <stop stopColor={color} />
        <stop offset="1" stopColor={color} stopOpacity="0" />
      </linearGradient>
      <linearGradient
        id={`${color}_paint1_linear`}
        x1="70.9249"
        y1="4.66626"
        x2="24.1493"
        y2="17.4815"
        gradientUnits="userSpaceOnUse"
      >
        <stop stopColor="#E7FFFC" stopOpacity="0.21" />
        <stop offset="1" stopColor="white" stopOpacity="0.04" />
      </linearGradient>
      <linearGradient
        id={`${color}_paint2_linear`}
        x1="74.0274"
        y1="7.66729"
        x2="93.1092"
        y2="25.09"
        gradientUnits="userSpaceOnUse"
      >
        <stop stopColor="white" stopOpacity="0.16" />
        <stop offset="1" stopColor="white" stopOpacity="0.1" />
      </linearGradient>
      <linearGradient
        id={`${color}_paint3_linear`}
        x1="33.4973"
        y1="103.501"
        x2="109.498"
        y2="55.5016"
        gradientUnits="userSpaceOnUse"
      >
        <stop stopColor={color} stopOpacity="0" />
        <stop offset="0.489333" stopColor={color} stopOpacity="0.5" />
        <stop offset="1" stopColor={color} stopOpacity="0" />
      </linearGradient>
    </defs>
  </svg>
);

export default shadows;

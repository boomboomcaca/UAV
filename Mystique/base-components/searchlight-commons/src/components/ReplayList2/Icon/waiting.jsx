import React from 'react';

const waiting = (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g opacity="0.2">
      <path
        d="M4.0625 7.65806L4.81128 3.93267M4.0625 7.65806L7.45257 6.32048M4.0625 7.65806C5.13262 5.71512 6.88255 4.23579 8.97649 3.50396C11.0704 2.77212 13.3609 2.83933 15.4083 3.69268C17.4557 4.54603 19.1159 6.12542 20.0703 8.12776C20.7971 9.6528 21.0756 11.3414 20.8902 12.9998"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M19.8838 16.3419L19.135 20.0673M19.8838 16.3419L16.4937 17.6795M19.8838 16.3419C18.8137 18.2849 17.0637 19.7642 14.9698 20.496C12.8759 21.2279 10.5854 21.1607 8.53799 20.3073C6.49056 19.454 4.83038 17.8746 3.87603 15.8722C3.14913 14.3471 2.87071 12.6585 3.05612 11"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </g>
    <animateTransform
      attributeName="transform"
      begin="0s"
      dur="5s"
      type="rotate"
      from="0 0 0"
      to="-360 0 0"
      repeatCount="indefinite"
    />
  </svg>
);

export default waiting;

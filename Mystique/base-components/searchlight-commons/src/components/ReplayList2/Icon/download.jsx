import React from 'react';

const download = (opacity = 0.8) => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g opacity={opacity}>
      <path
        d="M7 19L17 19"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M9.85714 5H14.1429V10.8667H17L12 16L7 10.8667H9.85714V5Z"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </g>
  </svg>
);

export default download;

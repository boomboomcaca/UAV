import React from 'react';

const dropIcon = (deg = -90) => (
  <svg width="10" height="6" viewBox="0 0 10 6" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g transform={`rotate(${deg},5 3)`}>
      <path
        d="M0.757359 0.75736L5 5L9.24264 0.757359"
        stroke="#3CE5D3"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </g>
  </svg>
);

export default dropIcon;

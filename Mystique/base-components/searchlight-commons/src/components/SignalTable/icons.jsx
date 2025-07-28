import React from 'react';

export const sortState = (bo, sta) => (
  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="16" viewBox="0 0 5 16" fill="none">
    <path
      d="M 4 15 V 1 L 1 5"
      stroke={bo && sta === 'asc' ? '#3CE5D3' : 'var(--theme-font-30)'}
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      transform="translate(-3.5 0)"
    />
    <path
      d="M 1 1 V 15 L 4 11"
      stroke={bo && sta === 'desc' ? '#3CE5D3' : 'var(--theme-font-30)'}
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      transform="translate(3.5 0)"
    />
  </svg>
);

export const selectAll = (
  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="8" viewBox="0 0 10 8" fill="none">
    <path
      d="M9 1L3.74795 7L1 3.86779"
      stroke="#353D5B"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

export const selectSome = (
  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="2" viewBox="0 0 10 2" fill="none">
    <path d="M1 1L9 1" stroke="#353D5B" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

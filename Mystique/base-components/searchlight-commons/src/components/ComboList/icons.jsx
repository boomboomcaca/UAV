import React from 'react';

const warning = (
  <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
    <path
      d="M4.9943 18C4.62982 18 4.29749 17.8073 4.13668 17.5076C3.95444 17.208 3.95444 16.8119 4.13668 16.5122L11.1049 4.49235C11.2871 4.19266 11.6194 4 11.9625 4C12.327 4 12.6379 4.19266 12.8201 4.49235L19.8633 16.4801C20.0456 16.7798 20.0456 17.1758 19.8633 17.4755C19.6811 17.7752 19.3487 17.9679 19.0057 17.9679L4.9943 18Z"
      fill="#FFD118"
    />
    <path d="M12 9V13" stroke="black" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    <circle cx="12" cy="16" r="1" fill="#04051B" />
  </svg>
);

const error = (
  <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M12 20C16.4183 20 20 16.4183 20 12C20 7.58172 16.4183 4 12 4C7.58172 4 4 7.58172 4 12C4 16.4183 7.58172 20 12 20ZM15.7818 8.21967C16.0747 8.51256 16.0747 8.98744 15.7818 9.28033L13.0614 12.0007L15.7826 14.7219C16.0755 15.0148 16.0755 15.4897 15.7826 15.7826C15.4897 16.0755 15.0148 16.0755 14.7219 15.7826L12.0007 13.0614L9.28033 15.7818C8.98744 16.0747 8.51256 16.0747 8.21967 15.7818C7.92678 15.4889 7.92678 15.014 8.21967 14.7212L10.9401 12.0007L8.22043 9.28108C7.92753 8.98818 7.92753 8.51331 8.22043 8.22042C8.51332 7.92752 8.98819 7.92752 9.28109 8.22042L12.0007 10.9401L14.7212 8.21967C15.014 7.92678 15.4889 7.92678 15.7818 8.21967Z"
      fill="#FF4C2B"
    />
  </svg>
);

const info = (
  <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
    <g opacity="0.5">
      <path
        d="M7.11014 16.2156H4.70206C4.31593 16.2156 4 15.8994 4 15.5129V8.48635C4 8.09989 4.31593 7.78369 4.70206 7.78369H7.11014V16.2156Z"
        fill="var(--theme-font-100)"
      />
      <path
        d="M14.215 19.9256L6.80823 16.2155V7.78367L14.215 4.07365C14.6854 3.84177 15.233 4.18607 15.233 4.70604V19.2932C15.233 19.8202 14.6854 20.1574 14.215 19.9256Z"
        fill="var(--theme-font-100)"
      />
      <path
        d="M17.3391 6.65238L18.7432 5.24707"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeMiterlimit="10"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M18.7432 18.7592L17.3391 17.3539"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeMiterlimit="10"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
      <path
        d="M17.1918 12.5547H20"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeMiterlimit="10"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </g>
  </svg>
);

const drop = (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g opacity="0.3">
      <path
        d="M11.178 8.18645C11.5757 7.61244 12.4243 7.61244 12.822 8.18645L16.1088 12.9305C16.5683 13.5937 16.0936 14.5 15.2868 14.5L8.71322 14.5C7.9064 14.5 7.43175 13.5937 7.89123 12.9305L11.178 8.18645Z"
        fill="var(--theme-font-100)"
      />
    </g>
  </svg>
);

export { warning, error, info, drop };

const icons = { warning, error, info, drop };
const colors1 = { warning: '#FFD118CC', error: '#FF4C2BCC', info: 'var(--theme-font-80)' };
const colors2 = { warning: '#FFD118', error: '#FF4C2B', info: 'var(--theme-font-100)' };

export default icons;
export { colors1, colors2 };

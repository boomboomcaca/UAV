import React from 'react';

const pause = (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <rect x="5" y="4" width="4" height="16" rx="1" fill="white" />
    <rect x="15" y="4" width="4" height="16" rx="1" fill="white" />
  </svg>
);

const play = (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      fill="white"
      d="M17.6879 11.1583C18.3013 11.5518 18.3013 12.4482 17.6879 12.8417L7.03995 19.6724C6.37439 20.0993 5.5 19.6214 5.5 18.8307L5.5 5.16932C5.5 4.37859 6.37439 3.90067 7.03995 4.32762L17.6879 11.1583Z"
    />
  </svg>
);

const icons = { pause, play };

export default icons;

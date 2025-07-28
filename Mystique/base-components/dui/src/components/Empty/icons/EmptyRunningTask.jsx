import React from 'react';

const EmptyRunningTask = (
  <svg xmlns="http://www.w3.org/2000/svg" width="80" height="80" viewBox="0 0 80 80" fill="none">
    <rect x="0.5" y="8.5" width="79" height="63" rx="1.5" fill="#C4C4C4" />
    <rect x="0.5" y="8.5" width="79" height="63" rx="1.5" fill="#485275" />
    <rect x="0.5" y="8.5" width="79" height="63" rx="1.5" stroke="url(#paint0_linear)" />
    <g filter="url(#filter0_i)">
      <rect x="2" y="18" width="76" height="52" rx="2" fill="#2F3857" />
    </g>
    <rect x="2.5" y="18.5" width="75" height="51" rx="1.5" stroke="url(#paint1_linear)" />
    <rect x="10" y="26" width="60" height="5" rx="2.5" fill="#525D83" />
    <rect x="10" y="37" width="60" height="5" rx="2.5" fill="#525D83" />
    <rect x="10" y="50" width="32" height="5" rx="2.5" fill="#525D83" />
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M62.297 45.7083L66.0606 47.8812C65.1959 49.4992 65.7656 51.5232 67.3662 52.4473C67.6215 52.5947 67.8879 52.7054 68.1593 52.781L68.1593 56.3203C67.0013 56.3418 65.883 56.9519 65.2617 58.028C64.6966 59.0068 64.671 60.1489 65.0925 61.1102L61.8454 62.985C61.5655 62.5747 61.1913 62.2193 60.7324 61.9543C59.2517 61.0995 57.397 61.4792 56.3604 62.7742L53.3349 61.0274C53.9382 59.4822 53.3398 57.6859 51.859 56.831C51.4006 56.5663 50.9063 56.42 50.4115 56.3826L50.4115 52.762C51.2182 52.5212 51.9381 51.9836 52.3922 51.1969C52.9983 50.1472 52.9838 48.9096 52.4627 47.9087L56.8263 45.3893C57.1096 45.8207 57.4953 46.1942 57.9728 46.4699C59.431 47.3118 61.252 46.9562 62.297 45.7083ZM58.0038 56.4347C59.229 57.1421 60.7957 56.7223 61.5031 55.4971C62.2105 54.2719 61.7907 52.7052 60.5655 51.9978C59.3402 51.2904 57.7736 51.7102 57.0662 52.9354C56.3588 54.1607 56.7786 55.7274 58.0038 56.4347Z"
      fill="#6F7EA4"
    />
    <circle cx="74" cy="13" r="2" fill="#2F3857" />
    <circle cx="66" cy="13" r="2" fill="#2F3857" />
    <circle cx="58" cy="13" r="2" fill="#2F3857" />
    <defs>
      <filter
        id="filter0_i"
        x="2"
        y="18"
        width="76"
        height="53"
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
        <feColorMatrix type="matrix" values="0 0 0 0 0.0156863 0 0 0 0 0.0196078 0 0 0 0 0.105882 0 0 0 0.2 0" />
        <feBlend mode="normal" in2="shape" result="effect1_innerShadow" />
      </filter>
      <linearGradient id="paint0_linear" x1="40" y1="8" x2="40" y2="72" gradientUnits="userSpaceOnUse">
        <stop stopColor="#525D85" />
        <stop offset="1" stopColor="#39446A" />
      </linearGradient>
      <linearGradient id="paint1_linear" x1="40" y1="18" x2="40" y2="70" gradientUnits="userSpaceOnUse">
        <stop stopColor="#39446A" />
        <stop offset="1" stopColor="#525D85" />
      </linearGradient>
    </defs>
  </svg>
);

export default EmptyRunningTask;

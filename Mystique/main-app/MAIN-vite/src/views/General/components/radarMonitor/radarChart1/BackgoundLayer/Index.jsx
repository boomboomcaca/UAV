import React, { useState, useEffect, useRef } from "react";
import PropTypes from "prop-types";

const BackgroundLayer = (props) => {
  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 148 148"
      //     preserveAspectRatio="none meet"
      {...props}
    >
      <g filter="url(#a)">
        <path
          fillRule="evenodd"
          clipRule="evenodd"
          d="M74 135.492c33.961 0 61.492-27.531 61.492-61.492S107.961 12.508 74 12.508 12.508 40.038 12.508 74c0 33.961 27.53 61.492 61.492 61.492Zm0-5.124c31.131 0 56.368-25.237 56.368-56.368 0-31.131-25.237-56.368-56.368-56.368-31.13 0-56.368 25.238-56.368 56.368 0 31.131 25.237 56.368 56.368 56.368Z"
          fill="url(#b)"
        />
      </g>
      <g filter="url(#c)">
        <circle cx="74" cy="74" fill="url(#d)" r="54.172" />
      </g>
      <path
        fill="#40A9FF"
        d="M73.189 22.038h1.624v4.871h-1.624zm0 99.052h1.624v4.871h-1.624zm52.774-47.902v1.624h-4.871v-1.624zm-99.053 0v1.624h-4.871v-1.624zm83.258-36.504 1.148 1.148-3.444 3.445-1.148-1.149zm-70.039 70.04 1.148 1.148-3.444 3.445-1.148-1.149zm71.407 3.662-1.149 1.149-3.444-3.445 1.148-1.148zM41.277 40.128l-1.148 1.148-3.445-3.444 1.149-1.148z"
      />

      <g opacity="0.3" fill="#4190DA">
        <path d="m68.662 22.307 1.618-.142.424 4.853-1.617.141zm8.633 98.674 1.618-.142.424 4.853-1.617.141zm48.398-52.319.142 1.618-4.853.424-.141-1.617zm-98.675 8.633.142 1.618-4.853.424-.141-1.617zm79.759-43.622 1.244 1.044-3.131 3.731-1.244-1.044zM43.11 109.55l1.243 1.044-3.13 3.732-1.245-1.044zm71.216-2.772-1.044 1.244-3.731-3.131 1.044-1.244zM38.45 43.107l-1.043 1.244-3.732-3.13 1.044-1.245z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m64.178 22.969 1.6-.282.845 4.797-1.6.282zm17.199 97.547 1.6-.282.845 4.797-1.6.282zm43.654-56.339.282 1.6-4.797.845-.282-1.6zm-97.547 17.2.282 1.6-4.797.845-.282-1.6zm75.655-50.407 1.33.931-2.794 3.99-1.33-.93zm-56.815 81.138 1.33.931-2.794 3.99-1.33-.93zm70.705-8.969-.931 1.33-3.99-2.794.93-1.33zM35.89 46.326l-.932 1.33-3.99-2.794.931-1.33z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m59.766 24.02 1.569-.42 1.26 4.705-1.568.42zm25.636 95.676 1.569-.42 1.26 4.705-1.568.42zm38.578-59.928.42 1.569-4.705 1.26-.42-1.568zM28.303 85.403l.42 1.569-4.705 1.26-.42-1.568zm70.974-56.809 1.406.812-2.435 4.218-1.407-.812zM49.75 114.376l1.406.812-2.435 4.218-1.406-.812zm69.654-15.099-.812 1.406-4.218-2.435.812-1.407zM33.623 49.752l-.812 1.406-4.218-2.435.812-1.407z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m55.465 25.45 1.526-.555 1.666 4.577-1.526.555zm33.879 93.078 1.526-.555 1.666 4.577-1.526.555zm33.207-63.062.555 1.526-4.577 1.666-.555-1.526zM29.473 89.343l.555 1.526-4.577 1.666-.555-1.526zm65.751-62.779 1.472.687-2.058 4.414-1.472-.686zm-41.861 89.772 1.472.686-2.059 4.415-1.472-.686zm68.073-21.111-.686 1.472-4.415-2.059.686-1.472zm-89.77-41.862-.686 1.472-4.415-2.059.686-1.472z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m51.305 27.249 1.472-.686 2.058 4.414-1.471.687zm41.861 89.772 1.472-.686 2.058 4.414-1.471.687zm27.584-65.717.686 1.472-4.414 2.058-.687-1.471zM30.979 93.165l.686 1.472-4.414 2.058-.687-1.471zM91.01 24.894l1.526.555-1.666 4.578-1.526-.556zm-33.88 93.078 1.527.556-1.666 4.577-1.526-.555zm65.975-26.963-.555 1.526-4.578-1.666.556-1.526zM30.026 57.13l-.555 1.526-4.578-1.666.556-1.526z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m47.317 29.406 1.406-.812 2.435 4.218-1.406.812zm49.525 85.781 1.406-.812 2.436 4.218-1.406.812zm21.752-67.871.812 1.406-4.219 2.436-.812-1.407zM32.813 96.842l.812 1.406-4.218 2.436-.812-1.407zm53.851-73.243 1.569.42-1.261 4.705-1.569-.42zm-25.637 95.676 1.569.42-1.261 4.705-1.569-.42zM124.4 86.664l-.42 1.569-4.705-1.261.42-1.569zM28.725 61.027l-.42 1.569-4.705-1.261.42-1.569z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m43.53 31.902 1.33-.932 2.793 3.99-1.33.932zm56.814 81.137 1.33-.931 2.794 3.99-1.33.931zm15.754-69.508.931 1.33-3.99 2.794-.931-1.33zm-81.139 56.814.931 1.33-3.99 2.794-.931-1.33zm47.264-77.659 1.6.282-.847 4.797-1.599-.282zm-17.2 97.547 1.6.282-.847 4.797-1.599-.282zm60.289-38.009-.282 1.6-4.797-.847.282-1.599zM27.766 65.023l-.282 1.6-4.797-.847.282-1.599z" />
      </g>
      <g opacity="0.3" fill="#4190DA">
        <path d="m39.978 34.718 1.244-1.044 3.131 3.732-1.244 1.043zm63.668 75.878 1.244-1.044 3.131 3.732-1.244 1.043zm9.637-70.618 1.044 1.244-3.732 3.131-1.043-1.244zm-75.879 63.669 1.044 1.244-3.732 3.131-1.044-1.244zm40.317-81.481 1.618.142-.425 4.852-1.618-.141zm-8.633 98.675 1.618.142-.425 4.852-1.618-.142zm56.746-43.121-.142 1.618-4.852-.425.142-1.618zM27.16 69.087l-.142 1.618-4.852-.425.142-1.618z" />
      </g>
      <circle cx="74.733" cy="73.999" r="2.928" fill="#40a9ff" />

      <defs>
        <linearGradient
          id="b"
          x1="17.233"
          y1="89.206"
          x2="132.795"
          y2="57.781"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#205695" />

          <stop offset="1" stopColor="#18406E" />
        </linearGradient>
        <linearGradient
          id="d"
          x1="74"
          y1="19.828"
          x2="74"
          y2="128.172"
          gradientUnits="userSpaceOnUse"
        >
          <stop stopColor="#18406E" />

          <stop offset="1" stopColor="#0F1D41" />
        </linearGradient>
        <filter
          id="a"
          x="12.508"
          y="12.508"
          width="123.484"
          height="122.984"
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

          <feBlend in2="shape" result="effect1_innerShadow_174_75968" />

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
            in2="effect1_innerShadow_174_75968"
            result="effect2_innerShadow_174_75968"
          />
        </filter>
        <filter
          id="c"
          x="19.828"
          y="17.828"
          width="108.344"
          height="111.344"
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

          <feColorMatrix values="0 0 0 0 1 0 0 0 0 1 0 0 0 0 1 0 0 0 0.2 0" />

          <feBlend in2="shape" result="effect1_innerShadow_174_75968" />

          <feColorMatrix
            in="SourceAlpha"
            values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0"
            result="hardAlpha"
          />

          <feOffset dy="-2" />

          <feGaussianBlur stdDeviation="1" />

          <feComposite in2="hardAlpha" operator="arithmetic" k2="-1" k3="1" />

          <feColorMatrix values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.2 0" />

          <feBlend
            in2="effect1_innerShadow_174_75968"
            result="effect2_innerShadow_174_75968"
          />
        </filter>
      </defs>
    </svg>
  );
};

BackgroundLayer.defaultProps = {
  datas: [],
};

BackgroundLayer.propTypes = {
  datas: PropTypes.array,
};

export default BackgroundLayer;

import React, { useState, useEffect, useRef } from "react";
import PropTypes from "prop-types";

const RadarChart1 = (props) => {
  const { datas } = props;

  const [rotate, setRotate] = useState(0);
  const rotateRef = useRef(0);
  useEffect(() => {
    const tmr = setInterval(() => {
      const rt = rotateRef.current + 5;
      rotateRef.current = rt % 360;
      setRotate(rotateRef.current);
    }, 60);

    return () => {
      clearInterval(tmr);
    };
  }, []);

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
      <image
        //  href="Ellipse211111.png"
        //    height="108"
        //    width="108"
        height="54"
        width="54"
        //    transform="translate(74.6 74) rotate(00)"
        transform={`translate(74.6 74) rotate(${rotate})`}
        xlinkHref="data:image/png;base64,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"
      />

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

RadarChart1.defaultProps = {
  datas: [],
};

RadarChart1.propTypes = {
  datas: PropTypes.array,
};

export default RadarChart1;

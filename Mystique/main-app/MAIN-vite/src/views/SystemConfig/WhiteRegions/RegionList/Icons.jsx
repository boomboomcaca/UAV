export const AddIcon = (props) => {
  const { fill, size } = props;
  return (
    <svg
      width={size || "24"}
      height={size || "24"}
      viewBox="0 0 24 24"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <rect
        width="24"
        height="24"
        rx="2"
        fill={fill || "#148BED"}
        fill-opacity="0.2"
      />
      <path
        d="M6 12H18"
        stroke="#30B4FF"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
      <path
        d="M12 6L12 18"
        stroke="#30B4FF"
        stroke-width="2"
        stroke-linecap="round"
        stroke-linejoin="round"
      />
    </svg>
  );
};

export const DelIcon = (props) => {
  const { size, opacity } = props;
  return (
    <svg
      width={size || "10"}
      height={size || "10"}
      viewBox="0 0 10 10"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        fill-rule="evenodd"
        clip-rule="evenodd"
        d="M9.78115 1.28033C10.074 0.987437 10.074 0.512563 9.78115 0.21967C9.48826 -0.0732233 9.01338 -0.0732233 8.72049 0.21967L5.00041 3.93975L1.28033 0.21967C0.987437 -0.0732233 0.512563 -0.0732233 0.21967 0.21967C-0.0732233 0.512563 -0.0732233 0.987437 0.21967 1.28033L3.93975 5.00041L0.21967 8.72049C-0.0732233 9.01338 -0.0732233 9.48826 0.21967 9.78115C0.512563 10.074 0.987437 10.074 1.28033 9.78115L5.00041 6.06107L8.72049 9.78115C9.01338 10.074 9.48826 10.074 9.78115 9.78115C10.074 9.48826 10.074 9.01338 9.78115 8.72049L6.06107 5.00041L9.78115 1.28033Z"
        fill="white"
        fill-opacity={opacity || "0.5"}
      />
    </svg>
  );
};

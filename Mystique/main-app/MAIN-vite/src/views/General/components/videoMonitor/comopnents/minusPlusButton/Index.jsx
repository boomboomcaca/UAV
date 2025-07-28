import React from "react";
import PropTypes from "prop-types";
import classnames from "classnames";
// import { PlusOutlined, MinusOutlined } from "@ant-design/icons";
import styles from "./style.module.less";

const MinusOutlined = () => (
  <svg
    width="14"
    height="2"
    viewBox="0 0 14 2"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
  >
    <path
      d="M1 1H13"
      stroke="#30B4FF"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const PlusOutlined = () => (
  <svg
    width="24"
    height="24"
    viewBox="0 0 24 24"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
  >
    {/* <rect width="24" height="24" rx="2" fill="#148BED" fill-opacity="0.2" /> */}
    <path
      d="M6 12H18"
      stroke="#30B4FF"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
    <path
      d="M12 6L12 18"
      stroke="#30B4FF"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const MinusPlusButton = (props) => {
  const {
    onMinus,
    onPlus,
    inputClass,
    value,
    onChange,
    minimum,
    maximum,
    step,
    allowInput,
  } = props;

  const toSetValue = (type, min, max, step) => {
    let num = value;
    if (type === "add") {
      num += step;
    } else {
      num -= step;
    }
    num = min > num ? min : max < num ? max : num;
    onChange?.(num);
  };
  return (
    <div className={`${styles.setBox} ${!allowInput && styles.noInput}`}>
      <div
        className={styles.setButton}
        onClick={() => {
          if (allowInput) {
            toSetValue("cut", minimum, maximum, step);
          }
          // else {
          //   onMinus();
          // }
        }}
        onMouseDown={() => {
          onMinus();
        }}
        onMouseUp={() => {
          onMinus("stop");
        }}
      >
        <MinusOutlined
          style={{
            fontSize: 18,
            color: "var(--theme-primary)",
            fontWeight: 800,
          }}
        />
      </div>
      <div className={`${styles.setValue} ${allowInput && styles.showInput}`}>
        {allowInput && (
          <input
            autoComplete="off"
            className={classnames(inputClass, styles.input)}
            value={value}
            min={minimum}
            max={maximum}
            onChange={(e) => {
              const val = e.target.value;
              onChange(val);
            }}
            // onBlur={handalBlur}
            type="number"
          />
        )}
      </div>
      <div
        className={styles.setButton}
        onClick={() => {
          if (allowInput) {
            toSetValue("add", minimum, maximum, step);
          }
        }}
        onMouseDown={() => {
          onPlus();
        }}
        onMouseUp={() => {
          onPlus("stop");
        }}
      >
        <PlusOutlined style={{ fontSize: 18, color: "var(--theme-primary)" }} />
      </div>
    </div>
  );
};

MinusPlusButton.defaultProps = {
  onMinus: () => {},
  onPlus: () => {},
  minimum: 0,
  maximum: 255,
  step: 1,
  onChange: () => {},
  allowInput: true,
};

MinusPlusButton.propTypes = {
  onMinus: PropTypes.func,
  onPlus: PropTypes.func,
  minimum: PropTypes.number,
  maximum: PropTypes.number,
  onChange: PropTypes.func,
  step: PropTypes.number,
  allowInput: PropTypes.bool,
};

export default MinusPlusButton;

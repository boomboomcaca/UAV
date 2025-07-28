import React from "react";
import PropTypes from "prop-types";
import styles from "./style.module.less";

const Button = (props) => {
  const { disabled, icon, children, onClick } = props;

  const handleClick = () => {
    if (!onClick || disabled) return;
    onClick();
  };

  return (
    <div className={styles.btn} onClick={handleClick}>
      {icon ? <span className={`iconfont btn_icon ${icon}`} /> : null}
      {children}
    </div>
  );
};

Button.defaultProps = {
  children: "",
  disabled: false,
  icon: "",
  onClick: () => {},
};

Button.propTypes = {
  children: PropTypes.any,
  disabled: PropTypes.bool,
  icon: PropTypes.string,
  onClick: PropTypes.func,
};

export default Button;

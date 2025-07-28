import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './style.module.less';

const Button1 = (props) => {
  const { size, type, children, disabled, icon, style, onClick } = props;

  const [btnClassName, setbtnClassName] = useState({});

  useEffect(() => {
    const obj = classnames(
      styles.btn,
      styles[`btn_${type}`],
      styles[`btn_${size}`],
      styles[disabled ? 'btn_disabled' : ''],
    );
    setbtnClassName(obj);
  }, [type, size, disabled]);

  const handleClick = () => {
    if (!onClick || disabled) return;
    onClick();
  };

  return (
    <div className={btnClassName} onClick={handleClick} style={style}>
      {icon ? <span className={`iconfont btn_icon ${icon}`} /> : null}
      {children}
    </div>
  );
};

Button1.defaultProps = {
  style: {},
  children: '',
  type: 'default',
  size: 'middle',
  disabled: false,
  icon: '',
  onClick: () => {},
};

Button1.propTypes = {
  children: PropTypes.any,
  style: PropTypes.object,
  type: PropTypes.oneOf(['default', 'primary']),
  size: PropTypes.oneOf(['middle', 'small', 'large']),
  disabled: PropTypes.bool,
  icon: PropTypes.string,
  onClick: PropTypes.func,
};

export default Button1;

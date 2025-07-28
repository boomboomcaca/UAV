import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './style.module.less';

const ButtonRadio = (props) => {
  const { size, type, children, disabled, icon, style, onClick, status } = props;

  const [btnClassName, setbtnClassName] = useState({});

  useEffect(() => {
    const obj = classnames(
      styles.btn,
      styles[`btn_${type}`],
      styles[`btn_${size}`],
      styles[disabled ? 'btn_disabled' : ''],
      styles[status ? 'btn_status' : ''],
    );
    setbtnClassName(obj);
  }, [type, size, disabled, status]);

  const handleClick = () => {
    if (!onClick || disabled) return;
    onClick(!status);
  };

  return (
    <div className={btnClassName} onClick={handleClick} style={style}>
      {icon ? <span className={`iconfont btn_icon ${icon}`} /> : null}
      {children}
    </div>
  );
};

ButtonRadio.defaultProps = {
  style: {},
  children: '',
  type: 'default',
  size: 'middle',
  disabled: false,
  icon: '',
  status: false,
  onClick: () => {},
};

ButtonRadio.propTypes = {
  children: PropTypes.any,
  style: PropTypes.object,
  type: PropTypes.oneOf(['default', 'primary']),
  size: PropTypes.oneOf(['middle', 'small', 'large']),
  disabled: PropTypes.bool,
  icon: PropTypes.string,
  status: PropTypes.bool,
  onClick: PropTypes.func,
};

export default ButtonRadio;

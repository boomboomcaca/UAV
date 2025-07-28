import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Button = (props) => {
  const { children, className, onClick, primary } = props;
  return (
    <div className={classnames(styles.root, className)} onClick={onClick} style={primary ? { color: '#3ce5d3' } : null}>
      {children}
    </div>
  );
};

Button.defaultProps = {
  children: null,
  className: null,
  onClick: () => {},
  primary: true,
};

Button.propTypes = {
  children: PropTypes.any,
  className: PropTypes.any,
  onClick: PropTypes.func,
  primary: PropTypes.bool,
};

export default Button;

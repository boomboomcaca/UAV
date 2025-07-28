import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Label = (props) => {
  const { className, label, disable } = props;

  return (
    <div className={classnames(styles.root, className)} style={disable ? { opacity: 0.2, color: 'white' } : null}>
      {label}
    </div>
  );
};

Label.defaultProps = {
  className: null,
  label: null,
  disable: false,
};

Label.propTypes = {
  className: PropTypes.any,
  label: PropTypes.any,
  disable: PropTypes.bool,
};

export default Label;

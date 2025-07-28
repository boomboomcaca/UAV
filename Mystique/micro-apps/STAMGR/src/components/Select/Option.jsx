import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';

import styles from './index.module.less';

const Option = (props) => {
  const { value, children, className, onClick, title } = props;

  const handleClick = (val) => {
    onClick && onClick(val);
  };

  return (
    <div className={classnames(className, styles.option_item)} onClick={() => handleClick(value)}>
      <div className={styles.option_item_text} title={title}>
        {children}
      </div>
    </div>
  );
};

Option.defaultProps = {
  value: '',
  children: null,
  className: '',
  title: '',
  onClick: null,
};

Option.propTypes = {
  value: PropTypes.any,
  children: PropTypes.any,
  className: PropTypes.string,
  title: PropTypes.string,
  onClick: PropTypes.func,
};

export default Option;

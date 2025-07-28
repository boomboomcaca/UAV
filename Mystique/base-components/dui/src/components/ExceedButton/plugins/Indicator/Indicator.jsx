import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import RGB from '../RGB';
import styles from './index.module.less';

const Indicator = (props) => {
  const { className, indicator, disable, checked } = props;

  return (
    <div
      className={classnames(
        styles.root,
        className,
        disable ? styles.disable : null,
        typeof checked === 'boolean' ? styles.topLeft : null,
      )}
    >
      {indicator || <RGB type={checked ? 'y' : 'rgb'} />}
    </div>
  );
};

Indicator.defaultProps = {
  className: null,
  indicator: null,
  disable: false,
  checked: null,
};

Indicator.propTypes = {
  className: PropTypes.any,
  indicator: PropTypes.any,
  disable: PropTypes.bool,
  checked: PropTypes.bool,
};

export default Indicator;

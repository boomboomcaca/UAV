import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const ElectButton = (props) => {
  const { className, options, value, onChange } = props;

  return (
    <div className={classnames(styles.root, className)}>
      {options.map((o) => {
        return (
          <div
            key={o.key}
            className={classnames(styles.item, value === o.key ? styles.check : null)}
            onClick={() => {
              onChange(o);
            }}
          >
            {o.value}
          </div>
        );
      })}
    </div>
  );
};

ElectButton.defaultProps = {
  className: null,
  options: null,
  value: null,
  onChange: () => {},
};

ElectButton.propTypes = {
  className: PropTypes.any,
  options: PropTypes.any,
  value: PropTypes.any,
  onChange: PropTypes.func,
};

export default ElectButton;

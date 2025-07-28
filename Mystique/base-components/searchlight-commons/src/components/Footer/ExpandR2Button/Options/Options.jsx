import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Options = (props) => {
  const { className, value, options, onChange } = props;

  return (
    <div
      className={classnames(styles.root, className)}
      onClick={(e) => {
        e.stopPropagation();
      }}
    >
      <div className={styles.grid} style={{ backgroundSize: `50% ${100 / Math.ceil(options.length / 2)}%` }} />
      {options?.map((o) => {
        return (
          <div
            key={o.value}
            className={classnames(
              styles.item,
              value === o.value ? styles.selected : null,
              o.disable ? styles.disable : null,
            )}
            onClick={() => {
              onChange(o);
            }}
          >
            {o.label}
          </div>
        );
      })}
    </div>
  );
};

Options.defaultProps = {
  className: null,
  value: null,
  options: null,
  onChange: () => {},
};

Options.propTypes = {
  className: PropTypes.any,
  value: PropTypes.any,
  options: PropTypes.any,
  onChange: PropTypes.func,
};

export default Options;

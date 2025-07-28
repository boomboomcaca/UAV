import React, { useRef, forwardRef, useImperativeHandle } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Options = forwardRef((props, ref) => {
  const { className, visible, options, value, onItemClick } = props;

  const rootRef = useRef(null);
  const popupRef = useRef(null);

  useImperativeHandle(ref, () => ({
    div: rootRef.current,
  }));

  return (
    <div ref={rootRef} className={classnames(styles.root, className, visible ? styles.show : null)}>
      <div
        ref={popupRef}
        className={styles.list}
        style={
          options
            ? {
                gridTemplateColumns: `repeat(${
                  options.length > 5 ? Math.ceil(Math.sqrt(options.length)) : options.length
                }, minmax(100px, 1fr))`,
              }
            : null
        }
      >
        {options?.map((opt) => {
          return (
            <div
              className={classnames(styles.item, opt.value === value ? styles.select : null)}
              onClick={(e) => {
                e.stopPropagation();
                onItemClick(opt);
              }}
            >
              {opt.label}
            </div>
          );
        })}
      </div>
      <div className={styles.popArrow}>
        <div className={styles.triangle} />
      </div>
    </div>
  );
});

Options.defaultProps = {
  className: null,
  visible: false,
  options: null,
  value: null,
  onItemClick: () => {},
};

Options.propTypes = {
  className: PropTypes.any,
  visible: PropTypes.bool,
  options: PropTypes.any,
  value: PropTypes.any,
  onItemClick: PropTypes.func,
};

export default Options;

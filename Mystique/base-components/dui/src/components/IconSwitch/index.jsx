import React, { useState } from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';

const IconSwitch = (props) => {
  const { selected, onChange, icons, disabled } = props;

  const [clickState, setClickState] = useState(selected);
  return (
    <div className={styles.container} style={{ opacity: disabled ? '.5' : '' }}>
      <div className={styles.switch}>
        {clickState ? (
          <>
            <div className={styles.selected}>{icons[0]}</div>
            <div
              className={styles.unSelected}
              onClick={() => {
                if (disabled) {
                  return;
                }
                setClickState(!clickState);
                onChange(clickState);
              }}
            >
              {icons[1]}
            </div>
          </>
        ) : (
          <>
            <div
              className={styles.unSelected}
              onClick={() => {
                if (disabled) {
                  return;
                }
                setClickState(!clickState);
                onChange(clickState);
              }}
            >
              {icons[2]}
            </div>
            <div className={styles.selected}>{icons[3]}</div>
          </>
        )}
      </div>
    </div>
  );
};
IconSwitch.defaultProps = {
  icons: [],
  selected: true,
  disabled: false,
  onChange: null,
};

IconSwitch.propTypes = {
  icons: PropTypes.array,
  disabled: PropTypes.bool,
  selected: PropTypes.bool,
  onChange: PropTypes.func,
};
export default IconSwitch;

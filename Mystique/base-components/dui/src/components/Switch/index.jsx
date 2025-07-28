import React from 'react';
import classNameMix from 'classnames';
import PropTypes from 'prop-types';
import styles from './style.module.less';

const Switch = (props) => {
  const { disable, checkedChildren, unCheckedChildren, selected, onChange } = props;
  const checkNum = () => {
    if (disable) return;
    if (onChange) onChange(!selected);
  };
  return (
    <div className={styles.switchDui}>
      <div className={styles.label}>
        <input
          className={classNameMix(styles.muiSwitch, styles.muiSwitchAnimbg, disable ? styles.muiSwitchDisabled : '')}
          onChange={checkNum}
          type="checkbox"
          checked={selected}
        />
        <span style={{ color: disable ? 'var(--theme-font-50)' : 'var(--theme-font-100)' }}>
          {selected ? checkedChildren : unCheckedChildren}
        </span>
      </div>
    </div>
  );
};
Switch.defaultProps = {
  disable: false,
  checkedChildren: '',
  unCheckedChildren: '',
  selected: true,
  onChange: null,
};

Switch.propTypes = {
  disable: PropTypes.bool,
  checkedChildren: PropTypes.string,
  unCheckedChildren: PropTypes.string,
  selected: PropTypes.bool,
  onChange: PropTypes.func,
};
export default Switch;

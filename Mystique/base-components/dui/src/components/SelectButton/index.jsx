import React, { useState } from 'react';
import PropTypes from 'prop-types';
import styles from './style.module.less';
import classNameMix from 'classnames';

const SelectButton = (props) => {
  const { children, selected, onChange } = props;

  const [clickState, setClickState] = useState(selected);
  const clickHandle = () => {
    setClickState(() => {
      const value = !clickState;
      onChange(value);
      return value;
    });
  };
  return (
    <div
      className={classNameMix(styles.btn, clickState ? styles.btn_selected : '')}
      style={{ position: 'relative' }}
      onClick={clickHandle}
    >
      {clickState && (
        <div style={{ position: 'absolute', left: '-1px', top: '-3px' }}>
          <svg width="13px" height="13px" viewBox="0 0 13 13" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path
              d="M12.0459 0.239258H2.0459C0.941329 0.239258 0.0458984 1.13469 0.0458984 2.23926V12.2393L12.0459 0.239258Z"
              fill="#3DE7D5"
            />
            <svg width="6pt" height="5pt" viewBox="-1 0 6 5" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path
                d="M4.68945 1.43311L2.06343 4.43311L0.689453 2.867"
                stroke="var(--theme-font-100)"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </svg>
        </div>
      )}
      {children}
    </div>
  );
};
SelectButton.defaultProps = {
  children: '',
  selected: false,
  onChange: null,
};

SelectButton.propTypes = {
  children: PropTypes.string,
  selected: PropTypes.bool,
  onChange: PropTypes.func,
};
export default SelectButton;

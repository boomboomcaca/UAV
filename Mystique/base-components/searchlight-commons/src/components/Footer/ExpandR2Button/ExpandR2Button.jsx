import React, { useRef, useState, useCallback } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { pointOutArea, getArea } from './graph';
import Icons from './asset';
import Options from './Options';
import styles from './index.module.less';

const ExpandR2Button = (props) => {
  const { className, content, children, checked, disabled, options, value, onChange } = props;

  const rootRef = useRef(null);
  const popupRef = useRef(null);

  const [showPopup, setShowPopup] = useState(false);
  const showPopupRef = useRef(false);

  const onMouseUp = useCallback((e) => {
    const div1 = rootRef.current;
    const div2 = popupRef.current;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    if (pointOutArea(point, area1) && pointOutArea(point, area2)) {
      setShowPopup(false);
      showPopupRef.current = false;
      window.removeEventListener('mouseup', onMouseUp);
    }
  }, []);

  const onClick = () => {
    if (!disabled) {
      if (!showPopup) {
        if (!checked) {
          window.addEventListener('mouseup', onMouseUp);
          setShowPopup(true);
        } else {
          onChange({ checked: false });
        }
      }
    }
  };

  return (
    <div ref={rootRef} className={classnames(styles.root, className)} onClick={onClick}>
      <div className={classnames(styles.content, checked ? styles.checked : null, disabled ? styles.disabled : null)}>
        {content}
        <div className={styles.arrow}>
          <img alt="" src={showPopup ? Icons.ArrowOpened : Icons.Arrow} />
        </div>
      </div>
      <div
        className={classnames(styles.indicator, checked ? styles.indicator2 : styles.indicator1)}
        style={checked === undefined ? { display: 'none' } : disabled ? { opacity: 0.2 } : null}
      />
      <div
        className={classnames(styles.popup, showPopup ? styles.show : null)}
        onClick={(e) => {
          e.stopPropagation();
        }}
      >
        <div ref={popupRef} className={styles.list}>
          <Options
            value={value}
            options={options || []}
            onChange={(o) => {
              setShowPopup(false);
              onChange({ option: o, checked: true });
            }}
          />
        </div>
        <div className={styles.popArrow}>
          <div className={styles.triangle} />
        </div>
      </div>
    </div>
  );
};

ExpandR2Button.defaultProps = {
  className: null,
  content: false,
  children: false,
  checked: false,
  disabled: false,
  options: null,
  value: null,
  onChange: () => {},
};

ExpandR2Button.propTypes = {
  className: PropTypes.any,
  content: PropTypes.any,
  children: PropTypes.any,
  checked: PropTypes.bool,
  disabled: PropTypes.bool,
  options: PropTypes.any,
  value: PropTypes.any,
  onChange: PropTypes.func,
};

export default ExpandR2Button;

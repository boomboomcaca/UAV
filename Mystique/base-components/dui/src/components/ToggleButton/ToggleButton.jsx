import React, { useState, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import styles from './index.module.less';

const ToggleButton = (props) => {
  const {
    className,
    style,
    children,
    tag,
    tooltip,
    checked,
    visible,
    disabled,
    twinkling,
    indicator,
    value,
    options,
    onClick,
    onDoubleClick,
    onPress,
    onSelectChange,
    mode,
    multiSelect,
  } = props;

  const pressDelayRef = useRef(1000);
  const clickDelayRef = useRef(200);
  const clickCountRef = useRef(0);
  const timerRef = useRef(null);
  const timeBeginRef = useRef(null);
  const timeEndRef = useRef(null);
  const isTouchRef = useRef(false);

  const SelectRef = useRef(null);

  const [show, setShow] = useState(false);

  useClickAway(() => {
    setShow(false);
  }, SelectRef);

  const onPressBegin = (bo) => {
    if (bo === isTouchRef.current) {
      if (disabled || !onPress) return;
      timeBeginRef.current = new Date();
      timerRef.current = setTimeout(() => {
        // 处理长按事件
        if (onPress) {
          setShow(true);
          onPress(tag);
        }
      }, pressDelayRef.current);
    }
  };

  const onPressEnd = (bo) => {
    if (bo === isTouchRef.current) {
      if (disabled || !onPress) return;
      timeEndRef.current = new Date();
      clearTimeout(timerRef.current);
      if (timeEndRef.current - timeBeginRef.current < pressDelayRef.current) {
        // 处理点击事件
        // onClick(checked, tag);
        toClick();
      }
    }
  };

  const toClick = () => {
    clickCountRef.current += 1;
    if (clickCountRef.current === 1) {
      setTimeout(() => {
        if (clickCountRef.current === 2) {
          clickCountRef.current = 0;
          setShow(true);
          onDoubleClick(tag);
        } else if (clickCountRef.current === 1) {
          clickCountRef.current = 0;
          onClick(checked, tag);
        }
      }, clickDelayRef.current);
    }
  };

  return (
    <div className={styles.outer720d} title={tooltip}>
      <div
        ref={SelectRef}
        className={classnames(
          mode === 'simple' ? styles.simple : styles.root,
          disabled ? (mode === 'simple' ? styles.simpleDisabled : styles.disabled) : null,
          visible ? null : styles.collapse,
          checked ? styles.checked : null,

          className,
        )}
        style={style}
        onClick={() => {
          if (!disabled && !onPress) toClick();
        }}
        onMouseDown={() => {
          onPressBegin(false);
        }}
        onMouseUp={() => {
          onPressEnd(false);
        }}
        onTouchStart={() => {
          isTouchRef.current = true;
          onPressBegin(true);
        }}
        onTouchEnd={() => {
          isTouchRef.current = true;
          onPressEnd(true);
        }}
      >
        {indicator ? <div className={styles.topLeft}>{indicator}</div> : null}
        {checked !== undefined && !indicator ? (
          <div
            className={classnames(
              styles.indicator,
              checked ? styles.indicator2 : styles.indicator1,
              twinkling ? styles.indicator3 : null,
              disabled ? styles.indicatorDisabled : null,
            )}
          >
            <div
              className={classnames(
                checked ? styles.indicator2before : styles.indicator1before,
                twinkling ? styles.indicator3before : null,
              )}
            />
          </div>
        ) : null}
        <div
          className={styles.child}
          title={tooltip !== null && tooltip !== undefined && tooltip !== '' ? null : children}
        >
          {children}
        </div>
        {onPress === null || mode === 'simple' ? null : (
          <div className={classnames(styles.more, disabled ? styles.moreDisabled : null)} />
        )}
        {mode === 'simple' ? <div className={styles.simpleBorder} /> : null}
      </div>
      {options === null ? null : (
        <div
          className={classnames(styles.hide, show ? styles.drop : null)}
          style={show ? { width: options?.length * 104 + 40 } : null}
        >
          <div className={styles.list}>
            {options?.map((c, idx) => {
              return (
                <div
                  key={c.key}
                  className={classnames(styles.item, value.includes(c.key) ? styles.select : null)}
                  onClick={(e) => {
                    if (!multiSelect) {
                      setTimeout(() => {
                        setShow(false);
                      }, 300);
                      onSelectChange(tag, [c.key], c);
                    } else {
                      const newVal = [...value];
                      newVal.push(c.key);
                      console.log(newVal);
                      onSelectChange(tag, newVal, c);
                    }

                    e.stopPropagation();
                  }}
                >
                  <div className={styles.ccc}>{c.value}</div>
                </div>
              );
            })}
          </div>
          <div className={styles.sbtriangle2} />
        </div>
      )}
    </div>
  );
};

ToggleButton.defaultProps = {
  className: null,
  style: null,
  children: null,
  tag: '',
  tooltip: '',
  value: [],
  options: null,
  checked: undefined,
  visible: true,
  disabled: false,
  twinkling: false,
  indicator: null,
  onClick: () => {},
  onDoubleClick: () => {},
  onSelectChange: () => {},
  onPress: null,
  mode: 'normal', // 'simple' 'normal'
  multiSelect: false,
};

ToggleButton.propTypes = {
  className: PropTypes.any,
  style: PropTypes.any,
  children: PropTypes.any,
  tag: PropTypes.string,
  tooltip: PropTypes.string,
  value: PropTypes.array,
  options: PropTypes.any,
  checked: PropTypes.bool,
  disabled: PropTypes.bool,
  twinkling: PropTypes.bool,
  indicator: PropTypes.any,
  visible: PropTypes.bool,
  onClick: PropTypes.func,
  onDoubleClick: PropTypes.func,
  onSelectChange: PropTypes.func,
  onPress: PropTypes.func,
  mode: PropTypes.any,
  multiSelect: PropTypes.bool,
};

export default ToggleButton;

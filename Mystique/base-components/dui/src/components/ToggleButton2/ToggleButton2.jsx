import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import Loading from '../Loading';
import styles from './index.module.less';

const ToggleButton2 = (props) => {
  const {
    className,
    style,
    children,
    tag,
    tooltip,
    visible,
    disabled,
    value,
    options,
    onClick,
    onDoubleClick,
    onPress,
    onSelectChange,
    mode,
    waiting,
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
  const loadingRef = useRef(false);
  const [loading, setLoading] = useState(false);

  useClickAway(() => {
    setShow(false);
  }, SelectRef);

  useEffect(() => {
    if (typeof waiting === 'boolean') {
      setLoading(waiting);
    }
  }, [waiting]);

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
          if (typeof waiting === 'boolean') {
            onClick(value);
          }
          if (typeof waiting === 'number') {
            if (!loadingRef.current) {
              onClick(value);
              setLoading(true);
              loadingRef.current = true;
              setTimeout(() => {
                setLoading(false);
                loadingRef.current = false;
              }, waiting || 0);
            }
          }
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
        <div className={styles.sbtag} style={disabled ? { opacity: '0.5' } : null}>
          {options?.find((o) => {
            return o.value === value;
          })?.label || ''}
        </div>
      </div>
      {options === null ? null : (
        <div
          className={classnames(styles.hide, show ? styles.drop : null)}
          style={show ? { width: options?.length * 104 + 40 } : null}
        >
          <div className={styles.list}>
            {options?.map((c) => {
              return (
                <div
                  key={c.key}
                  className={classnames(styles.item, c.value === value ? styles.select : null)}
                  onClick={(e) => {
                    setShow(false);
                    onSelectChange(c.value);
                    e.stopPropagation();
                  }}
                >
                  <div className={styles.ccc}>{c.label}</div>
                </div>
              );
            })}
          </div>
          <div className={styles.sbtriangle2} />
        </div>
      )}
      {loading ? <Loading className={styles.loading} /> : null}
    </div>
  );
};

ToggleButton2.defaultProps = {
  className: null,
  style: null,
  children: null,
  tag: '',
  tooltip: '',
  value: null,
  options: null,
  visible: true,
  disabled: false,
  onClick: () => {},
  onDoubleClick: () => {},
  onSelectChange: () => {},
  onPress: null,
  mode: 'normal', // 'simple' 'normal'
  waiting: false,
};

ToggleButton2.propTypes = {
  className: PropTypes.any,
  style: PropTypes.any,
  children: PropTypes.any,
  tag: PropTypes.string,
  tooltip: PropTypes.string,
  value: PropTypes.any,
  options: PropTypes.any,
  disabled: PropTypes.bool,
  visible: PropTypes.bool,
  onClick: PropTypes.func,
  onDoubleClick: PropTypes.func,
  onSelectChange: PropTypes.func,
  onPress: PropTypes.func,
  mode: PropTypes.any,
  waiting: PropTypes.any,
};

export default ToggleButton2;

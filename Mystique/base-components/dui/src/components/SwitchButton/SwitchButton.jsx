import React, { useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import { svg, drop, back, select } from './icons.jsx';
import styles from './index2.module.less';

const SwitchButton = (props) => {
  const {
    tag,
    title,
    tooltip,
    className,
    contentClassName,
    style,
    contentStyle,
    children,
    values,
    value,
    indicator,
    disabled,
    visible,
    onClick,
    mode,
  } = props;

  const SelectRef = useRef(null);

  const [index, setIndex] = useState(-1);

  const [show, setShow] = useState(false);

  useEffect(() => {
    let idx = -1;
    if (values.length > 0) {
      idx = values.indexOf(value);
      setIndex(idx);
    }
  }, [value, values]);

  const onSwitch = () => {
    if (values.length <= 3) {
      if (!disabled && values.length > 0) {
        let idx = index;
        if (idx + 1 >= values.length) {
          idx = 0;
        } else {
          idx += 1;
        }
        setIndex(idx);
        onClick(tag, idx, values[idx]);
      }
    } else {
      // eslint-disable-next-line no-lonely-if
      if (!disabled && values.length > 0 && show === false) {
        setShow(true);
      } else {
        setShow(false);
      }
    }
  };

  useClickAway(() => {
    setShow(false);
  }, SelectRef);

  return (
    <div
      ref={SelectRef}
      title={tooltip}
      className={classnames(
        mode === 'simple' ? styles.simple : styles.sbbase,
        disabled ? (mode === 'simple' ? styles.simpleDisabled : styles.sbbaseDisabled) : null,
        visible ? null : styles.sbcollapse,
        className,
      )}
      style={style}
      onClick={onSwitch}
    >
      {values.length <= 3 ? (
        <div className={classnames(styles.sbindicator, disabled ? styles.sbindicatorDisabled : null)}>
          {indicator || svg}
        </div>
      ) : null}
      <div className={classnames(styles.sbcontent, contentClassName)} style={contentStyle}>
        {title === null ? children[index] : title}
      </div>
      {title !== null && values.length > 0 && children.length > 0 ? (
        <div className={styles.sbtag} style={disabled ? { opacity: '0.5' } : null}>
          {children[values.indexOf(value)] || ''}
        </div>
      ) : null}
      {mode === 'simple' ? <div className={styles.simpleBorder} /> : null}
      {values.length > 3 ? (
        <div className={classnames(styles.sbhide, show ? styles.sbdrop : null)}>
          <div className={styles.sblist}>
            {children?.map((c, idx) => {
              return (
                <div
                  key={c}
                  className={classnames(
                    styles.sbitem,
                    values.length > 0 && value !== null && values.indexOf(value) === idx ? styles.sbselect : null,
                  )}
                  onClick={(e) => {
                    // setShow(false);
                    onClick(tag, idx, values[idx]);
                    e.stopPropagation();
                  }}
                >
                  {values.length > 0 && value !== null && values.indexOf(value) === idx ? select : back}
                  <div className={styles.sbccc}>{c}</div>
                </div>
              );
            })}
          </div>
          <div className={styles.sbtriangle2} />
        </div>
      ) : null}
    </div>
  );
};

SwitchButton.defaultProps = {
  tag: '',
  title: null,
  tooltip: null,
  className: null,
  contentClassName: null,
  style: null,
  contentStyle: null,
  children: null,
  values: [],
  value: null,
  indicator: null,
  disabled: false,
  visible: true,
  onClick: () => {},
  mode: 'normal',
};

SwitchButton.propTypes = {
  tag: PropTypes.string,
  title: PropTypes.string,
  tooltip: PropTypes.string,
  className: PropTypes.any,
  contentClassName: PropTypes.any,
  style: PropTypes.any,
  contentStyle: PropTypes.any,
  children: PropTypes.any,
  values: PropTypes.array,
  value: PropTypes.any,
  indicator: PropTypes.any,
  disabled: PropTypes.bool,
  visible: PropTypes.bool,
  onClick: PropTypes.func,
  mode: PropTypes.any,
};

export default SwitchButton;

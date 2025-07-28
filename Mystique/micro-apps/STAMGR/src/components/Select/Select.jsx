import React, { useState, useRef, useEffect } from 'react';
import { createPortal } from 'react-dom';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useClickAway } from 'ahooks';
import { getArea, pointOutArea } from './select';
import styles from './index.module.less';

const Select = (props) => {
  const { className, value, name, onChange, clickStop, style } = props;
  let { children } = props;
  const [val, setVal] = useState(value);
  const [open, setOpen] = useState(false);
  const [dropStyle, setDropStyle] = useState(null);
  const SelectRef = useRef(null);
  const refDropDiv = useRef(null);

  useClickAway(() => {
    setOpen(false);
  }, SelectRef);

  const handleChange = (str) => {
    if (onChange) {
      onChange(str);
    } else {
      setVal(str);
    }
    setOpen(false);
  };

  useEffect(() => {
    setVal(value);
  }, [value]);

  // 单一子元素是对象  转数组方便处理
  if (children && typeof children === 'object') {
    children = [children];
  }

  const currentLabel = children
    ?.flat(Infinity)
    .filter((option) => option && (option.props.value === undefined ? '' : option.props.value) === val)?.[0]
    ?.props.children;

  const currentTitle = children
    ?.flat(Infinity)
    .filter((option) => option && (option.props.value === undefined ? '' : option.props.value) === val)?.[0]
    ?.props.title;

  const onMouseUp = (e) => {
    const div1 = SelectRef.current;
    const div2 = refDropDiv.current;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    if (pointOutArea(point, area1) && pointOutArea(point, area2)) {
      setOpen(false);
      window.removeEventListener('mouseup', onMouseUp);
    }
  };

  return (
    <div
      className={classnames(className, styles.select)}
      ref={SelectRef}
      style={style}
      onClick={(e) => {
        if (clickStop) e.stopPropagation();
        setOpen(!open);
        window.addEventListener('mouseup', onMouseUp);
        if (SelectRef) {
          const area1 = getArea(SelectRef.current);
          const { point1, point2 } = area1;
          const left = point1.x;
          let top = point2.y;
          const width = point2.x - point1.x;
          // TODO from style.option.max-height && style.option.item.height;
          const maxh = 165;
          let h = maxh;
          const length = children?.flat(Infinity).length;
          if (32 * length < maxh) {
            h = 32 * length;
          }
          const bodyRect = document.body.getBoundingClientRect();
          if (top + h >= bodyRect.bottom - bodyRect.top) {
            top = point1.y - h;
          }
          setDropStyle({ left, top, width });
        }
      }}
    >
      <input type="search" className={styles.input} name={name} value={val} title={currentTitle} onChange={() => {}} />
      <div className={styles.text}>{currentLabel}</div>
      <svg
        className={classnames(styles.ab_arrow, { [styles.trun]: open })}
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <g opacity="0.3">
          <path
            d="M12.822 15.8136C12.4243 16.3876 11.5757 16.3876 11.178 15.8136L7.89123 11.0695C7.43175 10.4063 7.9064 9.5 8.71322 9.5L15.2868 9.5C16.0936 9.5 16.5683 10.4063 16.1088 11.0695L12.822 15.8136Z"
            fill="white"
          />
        </g>
      </svg>
      {open
        ? createPortal(
            <div ref={refDropDiv} className={classnames(styles.option, { [styles.open]: open })} style={dropStyle}>
              {children?.flat(Infinity).map((child) => {
                if (child) {
                  return React.cloneElement(child, {
                    className: val === (child.props.value === undefined ? '' : child.props.value) ? styles.checked : '',
                    key: child.key === undefined ? child.key : child.props.value,
                    onClick: (v) => handleChange(v),
                  });
                }
                return null;
              })}
            </div>,
            document.body,
          )
        : null}
    </div>
  );
};

Select.defaultProps = {
  className: '',
  style: {},
  value: '',
  onChange: null,
  name: '',
  clickStop: false,
  children: null,
};

Select.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  name: PropTypes.string,
  value: PropTypes.any,
  onChange: PropTypes.func,
  clickStop: PropTypes.bool,
  children: PropTypes.any,
};

export default Select;

/* eslint-disable prefer-destructuring */
/* eslint-disable react/no-array-index-key */
import React, { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { getArea, pointOutArea } from './cascader';
import styles from './index.module.less';

const defaultKEY = 'id';
const defaultLABEL = 'label';
const defaultChildren = 'children';
const defaultKM = { KEY: defaultKEY, LABEL: defaultLABEL, CHILDREN: defaultChildren };

const Cascader = (props) => {
  const { values, options, className, keyMap, splitter, onSelectValue } = props;

  const refMainDiv = useRef(null);

  const refDropDiv = useRef(null);

  const levelRef = useRef(0);

  const keyMapRef = useRef(defaultKM);

  const [dropdown, setDropdown] = useState(false);

  const [selectValues, setSelectValues] = useState(null);

  useEffect(() => {
    setSelectValues(values || null);
  }, [values]);

  useEffect(() => {
    if (keyMap) {
      if (keyMap.KEY) {
        keyMapRef.current = { ...keyMapRef.current, KEY: keyMap.KEY };
      }
      if (keyMap.LABEL) {
        keyMapRef.current = { ...keyMapRef.current, LABEL: keyMap.LABEL };
      }
      if (keyMap.CHILDREN) {
        keyMapRef.current = { ...keyMapRef.current, CHILDREN: keyMap.CHILDREN };
      }
    }
  }, [keyMap]);

  const getSub = () => {
    const { KEY, LABEL, CHILDREN } = keyMapRef.current;
    if (selectValues && selectValues.length > 0) {
      return selectValues.map((val, idx) => {
        let valnext = null;
        if (idx + 1 < selectValues.length) {
          valnext = selectValues[idx + 1];
        }
        return val[CHILDREN] && val[CHILDREN].length > 0 ? (
          <div key={idx} className={styles.dropdownsub}>
            {val[CHILDREN]?.map((option) => {
              return (
                <div
                  key={option[KEY]}
                  className={classnames(
                    styles.dropdownitem,
                    valnext && (valnext[KEY] === option[KEY] || valnext[LABEL] === option[LABEL])
                      ? styles.dropdownitemselect
                      : null,
                  )}
                  onClick={() => onSubItemClick(option, idx + 1)}
                >
                  {option[LABEL]}
                </div>
              );
            })}
          </div>
        ) : null;
      });
    }
    return null;
  };

  const onItemClick = (item) => {
    const { KEY } = keyMapRef.current;
    let res = null;
    if (selectValues === null || (selectValues && selectValues.length > 0 && selectValues[0][KEY] !== item[KEY])) {
      res = [item];
    }
    setSelectValues(res);
    onSelectValue(res);
    levelRef.current = 0;
  };

  const onSubItemClick = (item, idx) => {
    const { KEY } = keyMapRef.current;
    const res = [...selectValues];
    if (idx > levelRef.current) {
      res.push(item);
      levelRef.current = idx;
    } else if (idx === levelRef.current) {
      if (res.length === idx + 1 && res[res.length - 1][KEY] !== item[KEY]) {
        res.splice(res.length - 1, 1, item);
        levelRef.current = idx;
      } else {
        res.splice(res.length - 1, 1);
        levelRef.current = idx - 1;
      }
    } else if (res[idx][KEY] !== item[KEY]) {
      res.splice(idx, res.length - idx, item);
      levelRef.current = idx;
    } else {
      res.splice(idx, res.length - idx);
      levelRef.current = idx - 1;
    }
    setSelectValues(res);
    onSelectValue(res);
  };

  const getDisplayValues = () => {
    if (selectValues) {
      const { LABEL } = keyMapRef.current;
      const str = selectValues
        .map((s) => {
          return s[LABEL];
        })
        .join(splitter);
      return str;
    }
    return null;
  };

  const onHScroll = (e) => {
    const dom = e.currentTarget;
    const scrollWidth = 50;
    e.deltaY > 0 ? (dom.scrollLeft += scrollWidth) : (dom.scrollLeft -= scrollWidth);
  };

  const onMouseUp = (e) => {
    const div1 = refMainDiv.current;
    const div2 = refDropDiv.current;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    if (pointOutArea(point, area1) && pointOutArea(point, area2)) {
      setDropdown(false);
      window.removeEventListener('mouseup', onMouseUp);
    }
  };

  return (
    <div
      ref={refMainDiv}
      className={classnames(styles.root, className)}
      onClick={() => {
        setDropdown(!dropdown);
        window.addEventListener('mouseup', onMouseUp);
      }}
    >
      <div className={styles.value} onWheel={onHScroll}>
        {getDisplayValues()}
      </div>
      <svg
        className={classnames(styles.icon, dropdown ? styles.icondeg : null)}
        width="24"
        height="24"
        viewBox="0 0 24 24"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <g opacity="0.3">
          <path
            d="M12.822 15.8136C12.4243 16.3876 11.5757 16.3876 11.178 15.8136L7.89123 11.0695C7.43175 10.4063 7.9064 9.5 8.71322 9.5L15.2868 9.5C16.0936 9.5 16.5683 10.4063 16.1088 11.0695L12.822 15.8136Z"
            fill="var(--theme-font-100)"
          />
        </g>
      </svg>
      {options ? (
        <div
          ref={refDropDiv}
          className={classnames(styles.dropdown, dropdown ? null : styles.dropdownHide)}
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          <div className={styles.dropdownsub}>
            {options?.map((option) => {
              let valsel = null;
              if (selectValues && selectValues.length > 0) {
                valsel = selectValues[0];
              }
              const { KEY, LABEL } = keyMapRef.current;
              return (
                <div
                  key={option[KEY]}
                  className={classnames(
                    styles.dropdownitem,
                    valsel && (valsel[KEY] === option[KEY] || valsel[LABEL] === option[LABEL])
                      ? styles.dropdownitemselect
                      : null,
                  )}
                  onClick={() => onItemClick(option)}
                >
                  {option[LABEL]}
                </div>
              );
            })}
          </div>
          {getSub()}
        </div>
      ) : null}
    </div>
  );
};

Cascader.defaultProps = {
  values: null,
  options: null,
  className: null,
  keyMap: null,
  splitter: '/',
  onSelectValue: () => {},
};

Cascader.propTypes = {
  values: PropTypes.any,
  options: PropTypes.any,
  className: PropTypes.any,
  keyMap: PropTypes.any,
  splitter: PropTypes.any,
  onSelectValue: PropTypes.func,
};

export default Cascader;

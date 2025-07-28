import React, { useState, useRef, memo, useCallback, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { pointOutArea, getArea } from './graph';
import icons, { info, drop, colors2 } from './icons.jsx';
import ComboListItem from './ComboListItem.jsx';
import styles from './index.module.less';

const ComboList = (props) => {
  const { mainIcon, dropIcon, values, value, attachValues, maxLength, disabled, visible, className } = props;

  const refMainDiv = useRef(null);
  const refDropDiv = useRef(null);

  const [isDropDown, setIsDropDown] = useState(false);

  const [valueItems, setValueItems] = useState([]);

  const [twinkling, setTwinkling] = useState(false);

  const updateTwinkling = () => {
    // TODO leak
    setTwinkling(true);
    setTimeout(() => {
      setTwinkling(false);
    }, 1500);
  };

  useEffect(() => {
    if (values && values.length >= 0) {
      const now = new Date().getTime();
      setValueItems(
        values.map((v, i) => {
          return v.id ? v : { msg: v, id: now + i };
        }),
      );
      // updateTwinkling();
    }
  }, [values]);

  useEffect(() => {
    if (attachValues && attachValues.length > 0) {
      const now = new Date().getTime();
      const length = maxLength > 0 ? maxLength : 20;
      let items = [...valueItems, ...attachValues];
      if (items.length > length) {
        items = items.slice(-20);
      }
      setValueItems(
        items
          .map((v, i) => {
            const type = typeof v;
            if (v && type === 'object') return v.id ? v : { ...v, id: now + i };
            if (v && type === 'string') return { msg: v, id: now + i };
            return null;
          })
          .filter((i) => {
            return i !== null;
          }),
      );
      const has = attachValues.find((a) => {
        return a.type === 'error' || a.type === 'warning';
      });
      if (has) updateTwinkling();
    }
  }, [attachValues]);

  useEffect(() => {
    if (value) {
      const now = new Date().getTime();
      const length = maxLength > 0 ? maxLength : 20;
      let items = [...valueItems, value];
      if (items.length > length) {
        items = items.slice(-20);
      }
      setValueItems(
        items
          .map((v, i) => {
            const type = typeof v;
            if (v && type === 'object') return v.id ? v : { ...v, id: now + i };
            if (v && type === 'string') return { msg: v, id: now + i };
            return null;
          })
          .filter((i) => {
            return i !== null;
          }),
      );
      if (value.type === 'error' || value.type === 'warning') updateTwinkling();
    }
  }, [value]);

  const onMainClick = () => {
    if (!disabled) {
      if (!isDropDown) {
        setIsDropDown(true);
        window.addEventListener('mouseup', onMouseUp);
        if (refDropDiv.current) {
          refDropDiv.current.scrollTop = refDropDiv.current.scrollHeight;
        }
      } else {
        setIsDropDown(false);
      }
    }
  };

  const onMouseUp = useCallback((e) => {
    const div1 = refMainDiv.current;
    const div2 = refDropDiv.current;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    if (pointOutArea(point, area1) && pointOutArea(point, area2)) {
      setIsDropDown(false);
      window.removeEventListener('mouseup', onMouseUp);
    }
  }, []);

  return (
    <div
      ref={refMainDiv}
      className={classnames(
        styles.base,
        className,
        disabled ? styles.baseDisabled : null,
        visible ? null : styles.collapse,
        twinkling ? styles.twinkling : null,
      )}
      onClick={onMainClick}
    >
      {mainIcon === null ? (
        <div className={styles.icon}>
          {valueItems.length > 0 ? icons[valueItems[valueItems.length - 1].type] || info : info}
        </div>
      ) : (
        <div className={styles.icon}>{mainIcon}</div>
      )}

      <div className={classnames(styles.plate, disabled ? styles.plateDisabled : null)}>
        <div
          className={valueItems.length > 0 ? classnames(styles.content) : styles.nodata}
          style={{
            color:
              valueItems.length > 0 ? colors2[valueItems[valueItems.length - 1].type] || colors2.info : colors2.info,
          }}
        >
          {valueItems.length > 0 ? valueItems[valueItems.length - 1].msg : '暂无数据'}
        </div>
      </div>

      <div className={classnames(styles.dropToggle, isDropDown ? styles.dropdownToggle : styles.collapseToggle)}>
        {dropIcon || drop}
      </div>

      <div
        ref={refDropDiv}
        className={classnames(
          styles.dropdownCollapse,
          isDropDown ? styles.dropdownVisible : null,
          valueItems.length === 0 && isDropDown ? styles.dropdownNodata : null,
        )}
      >
        {valueItems.length > 0 ? (
          valueItems.map((val, idx) => {
            return (valueItems.length < 5 || idx < valueItems.length - 1) && val !== null ? (
              <ComboListItem key={val.id} item={val} />
            ) : null;
          })
        ) : (
          <div className={styles.nodata}>暂无数据</div>
        )}
      </div>
    </div>
  );
};

ComboList.defaultProps = {
  mainIcon: null,
  dropIcon: null,
  values: [],
  value: null,
  attachValues: null,
  maxLength: 20,
  disabled: false,
  visible: true,
  className: null,
};

ComboList.propTypes = {
  mainIcon: PropTypes.any,
  dropIcon: PropTypes.any,
  values: PropTypes.any,
  value: PropTypes.any,
  attachValues: PropTypes.any,
  maxLength: PropTypes.number,
  disabled: PropTypes.bool,
  visible: PropTypes.bool,
  className: PropTypes.any,
};

const areEquals = (prev, next) => {
  return prev.values === next.values && prev.value === next.value && prev.attachValues === next.attachValues;
};

export default memo(ComboList, areEquals);

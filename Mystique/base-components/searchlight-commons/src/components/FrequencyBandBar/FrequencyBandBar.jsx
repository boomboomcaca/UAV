import React, { useEffect, useState, useMemo, useRef } from 'react';
import PropTypes from 'prop-types';
import icons from './Icon';
import styles from './FrequencyBandBar.module.less';

const FrequencyBandBar = (props) => {
  const { frequencyList, onCheckChange } = props;
  const [validList, setValidList] = useState([]);
  const [validChooseList, setValidChooseList] = useState([]);
  const [maxFrequency, setMaxFrequency] = useState(null);
  const [minFrequency, setMinFrequency] = useState(null);
  const frequencyListRef = useRef(frequencyList);
  const barBoxRef = useRef(null);
  useEffect(() => {
    createList(frequencyListRef.current);
    window.addEventListener('resize', resizeFunc, false);
    return () => {
      window.removeEventListener('resize', resizeFunc, false);
    };
  }, []);
  const resizeFunc = () => {
    // resize监听
    createList(frequencyListRef.current);
  };
  const createList = (freList) => {
    const dataList = [];
    const chooseList = [];
    const barWidth = barBoxRef.current.clientWidth;
    let minFre = null;
    let maxFre = null;
    for (let i = 0; i < freList.length; i += 1) {
      const { id, choosed, startFrequency, stopFrequency } = freList[i];
      if (id !== undefined && startFrequency !== undefined && stopFrequency !== undefined) {
        if (!minFre || startFrequency < minFre) {
          minFre = startFrequency;
        }
        if (!maxFre || stopFrequency > maxFre) {
          maxFre = stopFrequency;
        }
        if (choosed) {
          chooseList.push(freList[i]);
        }
        dataList.push(freList[i]);
      }
    }
    dataList.sort((a, b) => a.startFrequency - b.startFrequency);
    dataList.map((item) => {
      const { startFrequency, stopFrequency } = item;
      item.relWidth = Math.round((Math.abs(stopFrequency - startFrequency) / Math.abs(maxFre - minFre)) * barWidth);
      item.relLeft = Math.round((Math.abs(startFrequency - minFre) / Math.abs(maxFre - minFre)) * barWidth);
      return item;
    });
    setMinFrequency(minFre);
    setMaxFrequency(maxFre);
    setValidList([...dataList]);
    setValidChooseList([...chooseList]);
    // 选择频段改变
    onCheckChange && onCheckChange(chooseList);
  };
  const onCheckBoxChange = () => {
    // 全选改变
    const newList = validList.reduce((pre, item) => {
      item.choosed = !(validChooseList.length === validList.length);
      return [...pre, item];
    }, []);
    setValidChooseList(validChooseList.length === validList.length ? [] : validList);
    setValidList([...newList]);
    // 选择频段改变
    onCheckChange && onCheckChange(validChooseList.length === validList.length ? [] : validList);
  };
  const onRadioChange = (item, index) => {
    const chooseList = [];
    validList[index].choosed = !item.choosed;
    for (let i = 0; i < validList.length; i += 1) {
      if (validList[i].choosed) {
        chooseList.push(validList[i]);
      }
    }
    setValidList([...validList]);
    setValidChooseList([...chooseList]);
    // 选择频段改变
    onCheckChange && onCheckChange(chooseList);
  };
  useMemo(() => {
    if (barBoxRef.current) {
      frequencyListRef.current = frequencyList;
      createList(frequencyList);
    }
  }, [frequencyList]);
  return (
    <div className={styles.container}>
      <div className={styles.container_bar}>
        <div className={styles.bar_left}>
          <div className={styles.bar_left_top}>
            <div className={styles.bar_left_top_wrap} ref={barBoxRef}>
              {validList.map((item, index) => (
                <div
                  key={`frequencyBand-${index + 1}`}
                  className={[styles.left_top_item, item.choosed ? styles.left_top_item_choose : ''].join(' ')}
                  style={{
                    width: item.relWidth,
                    left: item.relLeft,
                  }}
                >
                  <div
                    className={styles.left_top_item_tip}
                    style={{
                      left: index === 0 ? '100%' : '50%',
                      transform: `translate(${index === 0 ? '-25%' : '-50%'},-50%)`,
                    }}
                  >
                    <span>{`${item.startFrequency}~${item.stopFrequency}MHz`}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
          <div className={styles.bar_left_bottom}>
            <span>{minFrequency}MHz</span>
            <span>{maxFrequency}MHz</span>
          </div>
        </div>
        <div className={styles.bar_right}>
          <div
            className={[
              styles.bar_right_checkbox,
              validList.length === validChooseList.length
                ? styles.bar_right_checkbox_full
                : validChooseList.length > 0 && validChooseList.length < validList.length
                ? styles.bar_right_checkbox_half
                : styles.bar_right_checkbox_null,
            ].join(' ')}
            onClick={onCheckBoxChange}
          >
            {validList.length === validChooseList.length
              ? icons.check
              : validChooseList.length > 0 && validChooseList.length < validList.length
              ? icons.checkSome
              : ''}
          </div>
          <span>全选</span>
        </div>
      </div>
      <div className={styles.container_list}>
        {validList.map((item, index) => (
          <div
            className={[styles.list_item, item.choosed ? styles.list_item_choose : ''].join(' ')}
            key={`validList-${index + 1}`}
          >
            <div
              className={[
                styles.list_item_radio,
                item.choosed ? styles.list_item_radio_full : styles.list_item_radio_null,
              ].join(' ')}
              onClick={() => onRadioChange(item, index)}
            >
              {item.choosed ? icons.check : ''}
            </div>
            <div className={styles.list_item_band}>
              <span>{`${item.startFrequency}~${item.stopFrequency}`}</span>
              <span>MHz</span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
FrequencyBandBar.defaultProps = {
  frequencyList: [],
  onCheckChange: () => {},
};

FrequencyBandBar.propTypes = {
  frequencyList: PropTypes.array,
  onCheckChange: PropTypes.func,
};
export default FrequencyBandBar;

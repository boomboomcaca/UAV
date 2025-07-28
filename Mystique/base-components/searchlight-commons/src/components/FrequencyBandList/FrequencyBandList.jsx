import React, { useEffect, useState, useMemo, useRef } from 'react';
import PropTypes from 'prop-types';
import { ArrowLeftIcon, ArrowRightIcon } from 'dc-icon';
import styles from './FrequencyBandList.module.less';

const FrequencyBandList = (props) => {
  const { frequencyList, onChange } = props;
  const [validList, setValidList] = useState([]);
  const [isPageTurning, setIsPageTurning] = useState(false);
  const [currentPage, setCurrentPage] = useState(0);
  const [totalPage, setTotalPage] = useState(0);
  const frequencyListRef = useRef(frequencyList);
  const listBoxRef = useRef(null);
  useEffect(() => {
    frequencyListRef.current = frequencyList;
    createList(frequencyList);
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
    let minFre = null;
    let maxFre = null;
    const freWidth = 150;
    const intervalWidth = 8;
    const wrapWidth = listBoxRef.current.clientWidth;
    for (let i = 0; i < freList.length; i += 1) {
      const { id, startFrequency, stopFrequency } = freList[i];
      if (id !== undefined && startFrequency !== undefined && stopFrequency !== undefined) {
        if (!minFre || startFrequency < minFre) {
          minFre = startFrequency;
        }
        if (!maxFre || stopFrequency > maxFre) {
          maxFre = stopFrequency;
        }
        dataList.push(freList[i]);
      }
    }
    dataList.sort((a, b) => a.startFrequency - b.startFrequency);
    const isFull = dataList.length * freWidth + (dataList.length - 1) * intervalWidth - 60 > wrapWidth;
    const allPage = Math.ceil((dataList.length * freWidth + (dataList.length - 1) * intervalWidth) / wrapWidth) - 1;
    setTotalPage(allPage);
    setValidList([...dataList]);
    setIsPageTurning(isFull);
  };
  const onTurnPage = (type) => {
    // 列表平移
    console.log('列表平移--->type', type);
    if (type === 'up') {
      if (currentPage > 0) {
        setCurrentPage(currentPage - 1);
      }
    } else if (type === 'down') {
      if (currentPage < totalPage) {
        setCurrentPage(currentPage + 1);
      }
    }
  };
  const onItemCheck = (item, index) => {
    if (item.disabled) {
      return;
    }
    validList[index].choosed = !item.choosed;
    setValidList([...validList]);
    onChange && onChange(validList);
  };
  useMemo(() => {
    if (listBoxRef.current) {
      frequencyListRef.current = frequencyList;
      createList(frequencyList);
    }
  }, [frequencyList]);
  return (
    <div className={styles.container}>
      {isPageTurning && (
        <div
          className={[styles.container_left, currentPage === 0 ? styles.container_left_disabled : ''].join(' ')}
          onClick={() => onTurnPage('up')}
        >
          <ArrowLeftIcon iconSize={20} color="#3CE5D3" />
        </div>
      )}
      <div className={styles.container_list} ref={listBoxRef}>
        <div
          className={styles.list_wrap}
          style={{
            transform: `translateX(${listBoxRef.current ? -(currentPage * listBoxRef.current.clientWidth) : 0}px)`,
          }}
        >
          {validList.map((item, index) => (
            <div
              className={[
                styles.list_wrap_item,
                item.choosed && !item.disabled ? styles.list_wrap_item_choose : '',
                item.disabled ? styles.list_wrap_item_disabled : '',
              ].join(' ')}
              onClick={() => onItemCheck(item, index)}
            >
              <span>{`${item.startFrequency}~${item.stopFrequency}`}</span>
              <span>MHz</span>
            </div>
          ))}
        </div>
      </div>
      {isPageTurning && (
        <div
          className={[styles.container_right, currentPage === totalPage ? styles.container_right_disabled : ''].join(
            ' ',
          )}
          onClick={() => onTurnPage('down')}
        >
          <ArrowRightIcon iconSize={20} color="#3CE5D3" />
        </div>
      )}
    </div>
  );
};
FrequencyBandList.defaultProps = {
  frequencyList: [],
  onChange: () => {},
};

FrequencyBandList.propTypes = {
  frequencyList: PropTypes.array,
  onChange: PropTypes.func,
};
export default FrequencyBandList;

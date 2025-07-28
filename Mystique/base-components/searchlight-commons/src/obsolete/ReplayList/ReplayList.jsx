import React, { useRef, useState, useCallback, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Input, Empty, Table, message, Button } from 'dui';
import getColumns from './columns.jsx';
import { freq, ifbw, findParam } from './helper';
import { getTimeSpan, getTimeStamp } from '../../lib/timeHelper';
import icons from './icons';
import styles from './index.module.less';

const ReplayList = (props) => {
  const {
    data,
    syncData,
    type,
    // pageSize,
    sortLocal,
    showPage,
    onDeleteItems,
    onPageChange,
    onSortChange,
    onPlayback,
    onPlaysync,
    refreshKey,
    showSearch,
    onSearchChanged,
  } = props;

  const selectionsRef = useRef(null);

  const refreshRef = useRef(false);

  const [disabled, setDisabled] = useState(true);

  // const [isSelectMode, setIsSelectMode] = useState(false);

  const [displayList, setDisplayList] = useState(data.result);

  const [current, setCurrent] = useState(0);

  const [canOperate, setCanOperate] = useState(true);

  useEffect(() => {
    if (data && data.result) {
      selectionsRef.current = [];
      setDisabled(true);
      if (refreshRef.current || showPage) {
        setDisplayList(data.result);
      } else {
        const addNew = [];
        data.result.forEach((dr) => {
          let has = displayList.find((dl) => {
            return dl.id === dr.id;
          });
          if (has) {
            has = { ...dr };
          } else {
            addNew.push(dr);
          }
        });
        setDisplayList([...displayList, ...addNew]);
      }
      if (data.result.length > 0 && current === 0) {
        setCurrent(1);
      }
      if (data.result.length === 0) {
        setCurrent(0);
        // setIsSelectMode(false);
        setCanOperate(true);
      }
      refreshRef.current = false;
    }
  }, [data]);

  useEffect(() => {
    if (syncData && displayList) {
      const find = displayList.find((dr) => {
        return dr.sourceFile === syncData.sourceFile;
      });
      if (syncData.syncCode === 200 || syncData.syncCode === undefined) {
        if (find && find.percentage !== 101) {
          find.percentage = Number(syncData.rate.replace('%', ''));
        }
        if (find && find.percentage === 100) {
          setTimeout(() => {
            find.percentage = 101;
            setDisplayList([...displayList]);
            // setDisplayList((prev) => {
            //   const find = prev.find((dr) => {
            //     return dr.sourceFile === syncData.sourceFile;
            //   });
            //   if (find) {
            //     find.percentage = 101;
            //   }
            //   return [...prev];
            // });
          }, 500);
        }
        setDisplayList([...displayList]);
      } else {
        setTimeout(() => {
          find.percentage = -1;
          setDisplayList([...displayList]);
        }, 500);
        message.error('手动同步失败');
      }
    }
  }, [syncData]);

  const getData = useCallback(() => {
    const items = displayList?.map((item) => {
      let params = null;
      try {
        params = JSON.parse(item.params);
      } catch (error) {
        window.console.log(error);
      }
      const bo = params instanceof Array;
      const freqy = bo ? findParam(params, freq) : { value: '--' };
      const bandw = bo ? findParam(params, ifbw) : { value: '--' };

      const timeSpan = getTimeSpan(item.dataStopTime, item.dataStartTime);
      const frequency = freqy === undefined ? '--' : freqy.value;
      const bandwidth = bandw === undefined ? '--' : bandw.value;
      // const updateTime = new Date(item.updateTime).toLocaleString();
      const updateTime = item.updateTime || '--';
      const percentage = item.percentage !== undefined ? item.percentage : getPercent(item);

      const value = {
        ...item,
        frequency,
        bandwidth,
        timeSpan,
        updateTime,
        percentage,
      };

      return value;
    });
    return items;
  }, [displayList]);

  const getPercent = (item) => {
    if (item.syncTime) {
      if (getTimeStamp(item.syncTime) >= getTimeStamp(item.updateTime)) {
        return 101;
      }
    }
    return -1;
  };

  const onOrderByFrequency = (order) => {
    if (data.result.length > 0) {
      try {
        const list = data.result.sort((l1, l2) => {
          const p1 = JSON.parse(l1.params);
          const p2 = JSON.parse(l2.params);
          return order === 'asc'
            ? findParam(p1, freq).value - findParam(p2, freq).value
            : findParam(p2, freq).value - findParam(p1, freq).value;
        });
        setDisplayList([...list]);
      } catch (error) {
        window.console.log(error);
      }
    }
  };

  const onOrderByUpdateime = (order) => {
    if (data.result.length > 0) {
      const list = data.result.sort((l1, l2) => {
        return order === 'desc'
          ? getTimeStamp(l2.updateTime) - getTimeStamp(l1.updateTime)
          : getTimeStamp(l1.updateTime) - getTimeStamp(l2.updateTime);
      });
      setDisplayList([...list]);
    }
  };

  const onOrderByBandwidth = (order) => {
    if (data.result.length > 0) {
      try {
        const list = data.result.sort((l1, l2) => {
          const p1 = JSON.parse(l1.params);
          const p2 = JSON.parse(l2.params);
          return order === 'asc'
            ? findParam(p1, ifbw)?.value - findParam(p2, ifbw)?.value
            : findParam(p2, ifbw)?.value - findParam(p1, ifbw)?.value;
        });
        setDisplayList([...list]);
      } catch (error) {
        window.console.log(error);
      }
    }
  };

  const onOrderByTimespan = (order) => {
    const getDelta = (p) => {
      return getTimeStamp(p.dataStopTime) - getTimeStamp(p.dataStartTime);
    };
    if (data.result.length > 0) {
      const list = data.result.sort((l1, l2) => {
        return order === 'asc' ? getDelta(l1) - getDelta(l2) : getDelta(l2) - getDelta(l1);
      });
      setDisplayList([...list]);
    }
  };

  const onOrderBySegmentInfo = (order) => {
    if (data.result.length > 0) {
      const list = data.result.sort((l1, l2) => {
        return order === 'asc' ? (l1.segmentInfo > l2.segmentInfo ? 1 : -1) : l2.segmentInfo > l1.segmentInfo ? 1 : -1;
      });
      setDisplayList([...list]);
    }
  };

  const onOrderByTest = (order) => {
    if (data.result.length > 0) {
      const list = data.result.sort((l1, l2) => {
        return order === 'asc' ? (l1.test > l2.test ? 1 : -1) : l2.test > l1.test ? 1 : -1;
      });
      setDisplayList([...list]);
    }
  };

  const onOperate = (dat) => {
    if (canOperate) {
      if (dat.percentage === 101) {
        onPlayback(dat);
      } else {
        setSync(null, dat);
        onPlaysync(dat, (bo) => {
          setSync(bo, dat);
        });
      }
    }
  };

  function setSync(bo, dat) {
    const item = displayList.find((d) => {
      return dat.id === d.id;
    });
    if (item) {
      if (bo === true) {
        item.percentage = 200;
      } else if (bo === false) {
        item.percentage = 101;
      } else if (bo === null) {
        item.percentage = 0;
      }
      setDisplayList([...displayList]);
    }
  }

  const onSelectionChanged = (items) => {
    selectionsRef.current = items;
    if (!selectionsRef.current || selectionsRef.current.length === 0) {
      setDisabled(true);
    } else {
      setDisabled(false);
    }
  };

  const onColumnSort = (sort) => {
    const { key, state } = sort;
    if (sortLocal) {
      if (key) {
        if (key === 'frequency') {
          onOrderByFrequency(state);
        }
        if (key === 'updateTime') {
          onOrderByUpdateime(state);
        }
        if (key === 'bandwidth') {
          onOrderByBandwidth(state);
        }
        if (key === 'timeSpan') {
          onOrderByTimespan(state);
        }
        if (key === 'segmentInfo') {
          onOrderBySegmentInfo(state);
        }
        if (key === 'test') {
          onOrderByTest(state);
        }
      }
    } else {
      const condi = key === null ? null : { sort: key, order: state };
      onSortChange(condi);
    }
  };

  const onDeleteSelect = () => {
    // if (isSelectMode) {
    if (selectionsRef.current && selectionsRef.current.length > 0) {
      // if (isSelectMode) {
      onDeleteItems(selectionsRef.current);
      // }
    } else {
      message.info('未选择回放文件');
    }
    // }
  };

  // const onDeleteConfirm = () => {
  //   if (isSelectMode) {
  //     setIsSelectMode(false);
  //     selectionsRef.current = [];
  //     setDisabled(true);
  //     setCanOperate(true);
  //   } else {
  //     setIsSelectMode(true);
  //     setCanOperate(false);
  //   }
  // };

  const onRefresh = () => {
    refreshRef.current = true;
    selectionsRef.current = [];
    setDisplayList([]);
    setCurrent(1);
    onPageChange(1, 20);
  };

  useEffect(() => {
    if (refreshKey !== 0) {
      refreshRef.current = true;
      selectionsRef.current = [];
      setDisplayList([]);
      setCurrent(1);
      onPageChange(1, 20);
    }
  }, [refreshKey]);

  const [searchText, setSearchText] = useState(undefined);
  useEffect(() => {
    if (searchText !== undefined) {
      onSearchChanged(searchText);
    }
  }, [searchText]);

  return (
    <div className={styles.root}>
      <div className={styles.query}>
        <Input
          allowClear
          showSearch
          onSearch={(str) => setSearchText(str)}
          onPressEnter={(str) => setSearchText(str)}
          onChange={(val) => {
            if (val === '') {
              setSearchText('');
            }
          }}
          placeholder="搜索"
          style={{ width: 260, position: 'absolute', left: 0, opacity: showSearch ? 1 : 0 }}
        />
        <Button onClick={onRefresh}>刷新</Button>
        <Button disabled={disabled} onClick={onDeleteSelect}>
          <div className={styles.idel}>
            {icons.idel(disabled ? 0.2 : 1)}
            删除
          </div>
        </Button>
        {/* <Button onClick={onDeleteConfirm}>{isSelectMode ? '完成' : '选择'}</Button> */}
      </div>
      <div className={styles.list}>
        <Table
          columns={getColumns(type, canOperate, onOperate)}
          data={getData()}
          onSelectionChanged={onSelectionChanged}
          onColumnSort={onColumnSort}
          showSelection /* ={isSelectMode} */
          canLoadMore={!showPage}
          loadMore={() => {
            if (!showPage) {
              if (displayList.length < data.total) {
                const cur = current + 1;
                setCurrent(cur);
                onPageChange(cur, 20);
              }
            }
          }}
        />
        {displayList.length === 0 ? <Empty className={styles.empty} /> : null}
      </div>
      <div className={styles.pager} style={showPage ? null : { display: 'none' }}>
        {/* <Pagination
          current={current}
          pageSize={pageSize}
          total={data.total}
          onChange={(p, s) => {
            setCurrent(p);
            onPageChange(p, s);
          }}
        /> */}
      </div>
    </div>
  );
};

ReplayList.defaultProps = {
  data: { result: [], total: 0 },
  syncData: null,
  type: 'single',
  // pageSize: 20,
  sortLocal: true,
  showPage: false,
  onPageChange: () => {},
  onSortChange: () => {},
  onDeleteItems: () => {},
  onPlayback: () => {},
  onPlaysync: () => {},
  refreshKey: 0,
  showSearch: false,
  onSearchChanged: () => {},
};

ReplayList.propTypes = {
  data: PropTypes.any,
  syncData: PropTypes.any,
  type: PropTypes.any,
  // pageSize: PropTypes.number,
  sortLocal: PropTypes.bool,
  showPage: PropTypes.bool,
  onPageChange: PropTypes.func,
  onSortChange: PropTypes.func,
  onDeleteItems: PropTypes.func,
  onPlayback: PropTypes.func,
  onPlaysync: PropTypes.func,
  refreshKey: PropTypes.any,
  showSearch: PropTypes.bool,
  onSearchChanged: PropTypes.func,
};

export default ReplayList;

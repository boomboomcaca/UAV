import React, { useRef, useState, useCallback, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import { Input, Empty, message, Button, Calendar } from 'dui';
import { getTimeSpan, getTimeStamp } from '../../lib';
import Table from './Table';
import OperateFooter from './OperateFooter';
import icons from './Icon';
import useSortLocal from './hooks/useSortLocal';
import columnTemplate, { singleFunc, segmentFunc, segmentsFunc } from './Column/columnTemplate.jsx';
import { freq, ifbw, findParam } from './utils/helper';
import styles from './index.module.less';

const ReplayList = (props) => {
  const {
    className,
    footerClassName,
    type,
    segments,
    data,
    syncData,
    showPlay,
    sortLocal,
    showPage,
    onDeleteItems,
    onPageChange,
    onSortChange,
    onPlayback,
    onPlaysync,
    onDownload,
    refreshKey,
    showSearch,
    onSearchChanged,
    onTimeChange,
    updateRemark,
    style,
  } = props;

  const selectionsRef = useRef(null);

  const [curSels, setCurSels] = useState(null);

  const refreshRef = useRef(false);

  const [disabled, setDisabled] = useState(true);

  const [displayList, setDisplayList] = useState(data.result);

  const [current, setCurrent] = useState(0);

  const [canOperate, setCanOperate] = useState(true);

  const { sortLocalFunc } = useSortLocal(setDisplayList);

  useEffect(() => {
    if (data && data.result) {
      // selectionsRef.current = [];
      // setDisabled(true);
      if (refreshRef.current || showPage) {
        sortLocalFunc(sortRef.current, data.result);
      } else {
        const addNew = [];
        data.result.forEach((dr) => {
          let has = displayList.find((dl) => {
            return dl.id === dr.id;
          });
          if (has) {
            has = { ...dr };
          } else {
            addNew.push({ ...dr });
          }
        });
        sortLocalFunc(sortRef.current, [...displayList, ...addNew]);
      }
      if (data.result.length > 0 && current === 0) {
        setCurrent(1);
      }
      if (data.result.length === 0) {
        setCurrent(0);
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
          }, 500);
        }
        setDisplayList([...displayList]);
      } else {
        setTimeout(() => {
          find.percentage = -1;
          setDisplayList([...displayList]);
        }, 500);
        message.error(langT('commons', 'manualSyncFailure'));
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

      const timeSpan =
        item.dataStopTime === '1970-01-01 08:00:00' || item.recordCount === 0
          ? `00'00''`
          : getTimeSpan(item.dataStopTime, item.dataStartTime);
      const dataStartTime = item.dataStartTime || '--';
      const percentage = item.percentage !== undefined ? item.percentage : getPercent(item);

      const value = {
        ...item,
        timeSpan,
        dataStartTime,
        percentage,
      };

      const bo = params instanceof Array;

      if (type === 'single' || singleFunc.includes(type)) {
        const freqy = bo ? findParam(params, freq) : { value: '--' };
        const bandw = bo ? findParam(params, ifbw) : { value: '--' };
        value.frequency = freqy === undefined ? '--' : freqy.value;
        value.bandwidth = bandw === undefined ? '--' : bandw.value;
      } else if (type === 'segments' || segmentsFunc.includes(type)) {
        const segs = bo ? findParam(params, 'scanSegments') : { value: [] };
        value.segments = null;
        if (segs && segs.value) {
          value.segments = segs.value.map((sv) => {
            const find = segments.find((ss) => {
              return ss.startFrequency <= sv.startFrequency && ss.stopFrequency >= sv.stopFrequency;
            });
            if (find) {
              return { ...sv, type: find.name };
            }
            return { ...sv };
          });
        }
      } else if (type === 'segment' || segmentFunc.includes(type)) {
        const start = bo ? findParam(params, 'startFrequency') : { value: '--' };
        const stop = bo ? findParam(params, 'stopFrequency') : { value: '--' };
        const step = bo ? findParam(params, 'stepFrequency') : { value: '--' };
        value.segment = `${start?.value || '--'}MHz~${stop?.value || '--'}MHz@${step?.value || '--'}kHz`;
      } else if (type === 'mscan') {
        const mscanPoints = bo ? findParam(params, 'mscanPoints') : { parameters: null };
        value.mscanPoints = mscanPoints;
      }

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

  const onOperate = (dat) => {
    if (canOperate) {
      if (type === 'iqretri') {
        onPlayback(dat);
        return;
      }
      // TODO 可能需要这个判断
      if (dat.dataStopTime === '1970-01-01 08:00:00') {
        message.warning({ key: 'commons-main', content: langT('commons', 'syncDataAfterTaskStop') });
      } else if (dat.percentage === 101) {
        // TODO 数据列表中去掉数据回放功能，统一放在数据分析中
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
    setCurSels(selectionsRef.current);
    if (!selectionsRef.current || selectionsRef.current.length === 0) {
      setDisabled(true);
    } else {
      setDisabled(false);
    }
  };

  const sortRef = useRef({ key: 'dataStartTime', state: 'desc' });

  const onColumnSort = (sort) => {
    sortRef.current = sort;
    const { key, state } = sort;
    if (sortLocal) {
      sortLocalFunc(sort, displayList);
    } else {
      const condi = key === null ? null : { sort: key, order: state };
      onSortChange(condi);
    }
  };

  const onDeleteSelect = () => {
    if (selectionsRef.current && selectionsRef.current.length > 0) {
      onDeleteItems(selectionsRef.current);
    } else {
      message.info(langT('commons', 'noReplayFile'));
    }
  };

  const onDeleteCancel = () => {
    // TODO 取消
    selectionsRef.current = null;
    setCurSels(null);
    setDisabled(true);
  };

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
      setDisabled(true);
      setDisplayList([]);
      setCurrent(1);
      onPageChange(1, 20);
    }
  }, [refreshKey]);

  const [searchText, setSearchText] = useState(undefined);
  const [timeRange, setTimeRange] = useState([]);

  useEffect(() => {
    if (searchText !== undefined) {
      onSearchChanged(searchText);
    }
  }, [searchText]);

  return (
    <div className={classnames(styles.root, className)} style={style}>
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
          placeholder={langT('commons', 'inputHolderSearchStation')}
          style={{ width: 260, position: 'absolute', left: 0, opacity: showSearch ? 1 : 0 }}
        />
        <div style={{ position: 'absolute', left: showSearch ? 300 : 0, display: 'flex', alignItems: 'center' }}>
          <div style={{ color: 'var(--theme-font-30)', fontSize: 14 }}>{langT('commons', 'filterByTime')}</div>
          <Calendar.Range
            style={{
              marginLeft: 8,
            }}
            value={timeRange}
            position="right"
            onChange={(d) => {
              setTimeRange(d);
              onTimeChange(d);
            }}
          />
        </div>

        <div className={styles.refresh} onClick={onRefresh}>
          {icons.refresh}
          {langT('commons', 'refresh')}
        </div>
      </div>
      <div className={styles.list}>
        <Table
          columns={columnTemplate(type, onOperate, onDownload, updateRemark, showPlay)}
          defaultSort={{ key: 'dataStartTime', state: 'desc' }}
          data={getData()}
          onSelectionChanged={onSelectionChanged}
          onColumnSort={onColumnSort}
          showSelection
          currentSelections={curSels}
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

      <OperateFooter
        className={footerClassName}
        disabled={disabled}
        count={curSels?.length || 0}
        allChecked={data?.result.length === curSels?.length}
        onIconClick={() => {
          onSelectionChanged(data?.result.length === curSels?.length ? [] : data?.result);
        }}
      >
        <Button disabled={disabled} onClick={onDeleteSelect}>
          <div className={styles.idel}>
            {icons.remove(disabled ? 0.2 : 1)}
            {langT('commons', 'delete')}
          </div>
        </Button>
        <Button onClick={onDeleteCancel}> {langT('commons', 'cancel')}</Button>
      </OperateFooter>
    </div>
  );
};

ReplayList.Single = 'single';

ReplayList.Segments = 'segments';

ReplayList.Segment = 'segment';

ReplayList.defaultProps = {
  className: null,
  footerClassName: null,
  type: ReplayList.Single,
  segments: [],
  data: { result: [], total: 0 },
  syncData: null,
  showPlay: false,
  sortLocal: true,
  showPage: false,
  onPageChange: () => {},
  onSortChange: () => {},
  onDeleteItems: () => {},
  onPlayback: () => {},
  onPlaysync: () => {},
  onDownload: () => {},
  onTimeChange: () => {},
  refreshKey: 0,
  showSearch: true,
  onSearchChanged: () => {},
  updateRemark: () => {},
  style: null,
};

ReplayList.propTypes = {
  className: PropTypes.any,
  footerClassName: PropTypes.any,
  type: PropTypes.string,
  segments: PropTypes.any,
  data: PropTypes.any,
  syncData: PropTypes.any,
  showPlay: PropTypes.bool,
  sortLocal: PropTypes.bool,
  showPage: PropTypes.bool,
  onPageChange: PropTypes.func,
  onSortChange: PropTypes.func,
  onDeleteItems: PropTypes.func,
  onTimeChange: PropTypes.func,
  onPlayback: PropTypes.func,
  onPlaysync: PropTypes.func,
  onDownload: PropTypes.func,
  refreshKey: PropTypes.any,
  showSearch: PropTypes.bool,
  onSearchChanged: PropTypes.func,
  updateRemark: PropTypes.func,
  style: PropTypes.any,
};

export default ReplayList;

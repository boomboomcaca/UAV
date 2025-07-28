import React, { useState, useEffect, useRef, useCallback } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useThrottleFn } from 'ahooks';
import styles from './table2.module.less';

const svg1 = (bo, sta) => (
  <svg xmlns="http://www.w3.org/2000/svg" width="12" height="16" viewBox="0 0 5 16" fill="none">
    <path
      d="M 4 15 V 1 L 1 5"
      stroke={bo && sta === 'asc' ? '#3CE5D3' : 'var(--theme-font-30)'}
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      transform="translate(-3.5 0)"
    />
    <path
      d="M 1 1 V 15 L 4 11"
      stroke={bo && sta === 'desc' ? '#3CE5D3' : 'var(--theme-font-30)'}
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
      transform="translate(3.5 0)"
    />
  </svg>
);

const svg2 = (
  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="8" viewBox="0 0 10 8" fill="none">
    <path
      d="M9 1L3.74795 7L1 3.86779"
      stroke="#353D5B"
      strokeWidth="1.5"
      strokeLinecap="round"
      strokeLinejoin="round"
    />
  </svg>
);

const svg3 = (
  <svg xmlns="http://www.w3.org/2000/svg" width="10" height="2" viewBox="0 0 10 2" fill="none">
    <path d="M1 1L9 1" stroke="#353D5B" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

const state = ['none', 'asc', 'desc'];

const getNextState = (sta) => {
  const idx = state.indexOf(sta);
  let index = idx + 1;
  if (index > state.length - 1) {
    index = 0;
  }
  return state[index];
};

const Table = (props) => {
  const {
    className,
    rowKey,
    columns,
    data,
    showSelection,
    currentSelections,
    onSelectionChanged,
    selectRowIndex,
    selectRowKey,
    onRowSelected,
    onColumnSort,
    options,
    loadMore,
    canLoadMore,
  } = props;

  const [sortCol, setSortCol] = useState({ key: null, state: state[0] });

  const [selections, setSelections] = useState([]);

  const [selectRow, setSelectRow] = useState(-1);

  useEffect(() => {
    if (currentSelections === null || currentSelections === undefined || currentSelections.length === 0) {
      setSelections([]);
    } else {
      const ss = [];
      for (let i = 0; i < currentSelections.length; i += 1) {
        const element = currentSelections[i];
        const find = data?.find((d) => {
          return d[rowKey] === element[rowKey];
        });
        if (find) {
          ss.push(data.indexOf(find));
        }
      }
      setSelections(ss);
    }
  }, [currentSelections, JSON.stringify(data)]);

  // useEffect(() => {
  //   setSelections([]);
  // }, [JSON.stringify(data)]);

  useEffect(() => {
    setSelectRow(selectRowIndex);
  }, [selectRowIndex]);

  const onColumnClick = (col) => {
    if (col.sort) {
      let sortcol = {};
      if (col.key === sortCol.key) {
        const sta = getNextState(sortCol.state);
        sortcol = {
          key: sta === state[0] ? null : sortCol.key,
          state: sta,
        };
      } else {
        sortcol = { key: col.key, state: state[1] };
      }
      setSortCol(sortcol);
      onColumnSort(sortcol);
    }
  };

  const onCellClick = (col, item) => {
    window.console.log(col, item);
  };

  const onRowClick = (index) => {
    let ss = [];
    if (index === -1) {
      if (selections.length < data?.length) {
        for (let i = 0; i < data?.length; i += 1) {
          ss.push(i);
        }
      }
    } else {
      ss = [...selections];
      const idx = ss.indexOf(index);
      if (idx > -1) {
        ss.splice(idx, 1);
      } else {
        ss.push(index);
      }
    }
    setSelections(ss);
    onSelectionChanged(
      data?.filter((_d, idx) => {
        return ss.indexOf(idx) > -1;
      }),
    );
  };

  const bodyRef = useRef(null);
  const triggerRef = useRef(null);
  const onScrollListener = () => {
    if (triggerRef.current) {
      return;
    }
    const { current } = bodyRef;
    const { clientHeight, scrollTop, scrollHeight } = current;
    const atBottom = scrollTop + clientHeight >= scrollHeight - 20;
    if (atBottom) {
      triggerRef.current = true;
      if (atBottom) {
        loadMore();
      }
    }
  };
  useEffect(() => {
    triggerRef.current = false;
  }, [data?.length]);
  const { run: runScroll } = useThrottleFn(onScrollListener, { wait: 250 });

  const getBordered = useCallback((bordered, tag = 'inline') => {
    if (bordered) return bordered === true || bordered[tag] === true;
    return false;
  }, []);

  const getChecked = useCallback((checked, index) => {
    return checked.length > 0 && checked.indexOf(index) > -1;
  }, []);

  return (
    // <div className={styles.root}>
    <table
      className={classnames(styles.table, getBordered(options.bordered, 'outline') ? styles.outline : null, className)}
    >
      {columns ? (
        <thead className={classnames(styles.thead, getBordered(options.bordered) ? styles.theadBorder : null)}>
          <tr className={classnames(styles.trth, getBordered(options.bordered) ? styles.headBorder : null)}>
            {showSelection !== false ? (
              <th style={{ cursor: 'pointer', width: '45px' }}>
                <div
                  style={showSelection === 'hideAll' ? { opacity: 0, pointerEvents: 'none' } : null}
                  onClick={() => {
                    if (showSelection === true) onRowClick(-1);
                  }}
                >
                  <div
                    className={classnames(styles.check, showSelection === 'disable' ? styles.disable : null)}
                    style={selections.length === 0 ? null : { backgroundColor: '#3CE5D3' }}
                  >
                    {selections.length > 0 && selections.length === data?.length
                      ? svg2
                      : selections.length === 0
                      ? null
                      : svg3}
                  </div>
                </div>
              </th>
            ) : null}
            {columns.map((col) => {
              return (
                <th key={col.key} style={{ cursor: col.sort ? 'pointer' : 'default', ...col.style }}>
                  <div
                    onClick={() => {
                      onColumnClick(col);
                    }}
                    style={col.key === sortCol.key ? { color: '#3CE5D3' } : null}
                  >
                    {col.name}
                    {col.sort ? (
                      <div className={styles.sort}>{svg1(col.key === sortCol.key, sortCol.state)}</div>
                    ) : null}
                  </div>
                </th>
              );
            })}
          </tr>
        </thead>
      ) : null}
      <tbody
        className={options?.rowPitchEnable === false ? styles.tbody2 : styles.tbody}
        style={columns ? null : { height: '100%' }}
        ref={bodyRef}
        onScroll={() => {
          canLoadMore ? runScroll() : null;
        }}
        // onScroll={(e) => {
        //   window.console.log(e.target.scrollTop);
        // }}
      >
        {data?.map((item, index) => {
          const checked = getChecked(selections, index);
          const bordered = getBordered(options.bordered);
          return (
            <tr
              key={item[rowKey] || item.key || JSON.stringify(item)}
              className={classnames(
                styles.trtd,
                options.rowHover || options.rowHover === undefined ? styles.trtdHover : null,
                options.canRowSelect &&
                  (selectRow === index || (selectRowKey !== undefined && item[rowKey] === selectRowKey))
                  ? styles.trtdSelect
                  : null,
                bordered ? styles.bodyBorder : null,
              )}
              style={options.rowHeight ? { height: options.rowHeight } : null}
              onClick={
                (/* e */) => {
                  if (options.canRowSelect === true) {
                    let toSelect = false;
                    toSelect = (selectRowKey !== undefined && item[rowKey] === selectRowKey) || index === selectRow;
                    if (selectRowKey === undefined) {
                      setSelectRow(!toSelect ? index : -1);
                    }
                    onRowSelected(item, index, !toSelect);
                  }
                }
              }
            >
              {showSelection !== false ? (
                <td style={{ cursor: 'pointer', width: '45px' }}>
                  <div
                    onClick={(e) => {
                      if (showSelection !== false && showSelection !== 'disable') {
                        onRowClick(index);
                        e.stopPropagation();
                      }
                    }}
                  >
                    <div
                      className={classnames(styles.check, showSelection === 'disable' ? styles.disable : null)}
                      style={checked ? { backgroundColor: '#3CE5D3' } : null}
                    >
                      {checked ? svg2 : null}
                    </div>
                  </div>
                </td>
              ) : null}
              {columns.map((col /* , i */) => {
                return (
                  <td key={`${item[rowKey] || item.key || new Date().getTime()}${col.key}`} style={col.style}>
                    <div
                      onClick={() => {
                        onCellClick(col, item);
                      }}
                      style={checked ? { color: options?.rowCheckColor || null } : null}
                    >
                      {col.render ? col.render(item, index) : item[col.key]}
                    </div>
                  </td>
                );
              })}
            </tr>
          );
        })}
      </tbody>
    </table>
    // </div>
  );
};

Table.defaultProps = {
  // children: null,
  className: null,
  rowKey: 'id',
  columns: null,
  data: null,
  // hideAll true false disable
  showSelection: true,
  currentSelections: null,
  onSelectionChanged: () => {},
  selectRowIndex: -1,
  selectRowKey: undefined,
  onRowSelected: () => {},
  onColumnSort: () => {},
  options: {
    canRowSelect: false,
    rowHover: undefined,
    rowHeight: null,
    bordered: false,
    rowCheckColor: null,
    rowPitchEnable: undefined,
  },
  loadMore: () => {},
  canLoadMore: false,
};

Table.propTypes = {
  // children: PropTypes.any,
  className: PropTypes.any,
  rowKey: PropTypes.string,
  columns: PropTypes.any,
  data: PropTypes.any,
  showSelection: PropTypes.any,
  currentSelections: PropTypes.array,
  onSelectionChanged: PropTypes.func,
  selectRowIndex: PropTypes.any,
  selectRowKey: PropTypes.any,
  onRowSelected: PropTypes.func,
  onColumnSort: PropTypes.func,
  options: PropTypes.any,
  // 未虚拟化数据的加载更多
  loadMore: PropTypes.func,
  canLoadMore: PropTypes.bool,
};

export default React.memo(Table);

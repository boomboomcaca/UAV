import React, { useState, useEffect, useRef, useCallback } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useThrottleFn } from 'ahooks';
import { state, getNextState } from './table';
import { sortState, selectAll, selectSome } from './icons.jsx';
import styles from './index2.module.less';

const Table = (props) => {
  const {
    className,
    rowKey,
    columns,
    defaultSort,
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
    setSortCol(defaultSort);
  }, []);

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

  useEffect(() => {
    setSelectRow(selectRowIndex);
  }, [selectRowIndex]);

  const onColumnClick = (col) => {
    if (col.sort) {
      let sortcol = {};
      if (col.key === sortCol?.key) {
        const sta = getNextState(sortCol?.state);
        sortcol = {
          key: sta === state[0] ? null : sortCol?.key,
          state: sta,
        };
      } else {
        sortcol = { key: col.key, state: state[1] };
      }
      setSortCol(sortcol);
      onColumnSort(sortcol);
    }
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
    <div
      className={classnames(styles.root, getBordered(options.bordered, 'outline') ? styles.outline : null, className)}
    >
      <div className={classnames(styles.table1, getBordered(options.bordered, 'outline') ? styles.borderBottom : null)}>
        <table className={classnames(styles.table)}>
          {columns ? (
            <thead className={classnames(styles.thead)}>
              <tr className={getBordered(options.bordered) ? styles.headBorder : null}>
                {showSelection ? (
                  <th style={{ cursor: 'pointer', width: '45px', boxSizing: 'border-box' }}>
                    <div
                      onClick={() => {
                        onRowClick(-1);
                      }}
                    >
                      <div
                        className={styles.check}
                        style={selections.length === 0 ? null : { backgroundColor: '#3CE5D3' }}
                      >
                        {selections.length > 0 && selections.length === data?.length
                          ? selectAll
                          : selections.length === 0
                          ? null
                          : selectSome}
                      </div>
                    </div>
                  </th>
                ) : null}
                {columns.map((col) => {
                  return (
                    <th
                      key={col.key}
                      style={{
                        cursor: col.sort ? 'pointer' : 'default',
                        color: col.key === sortCol?.key ? '#3CE5D3' : null,
                        ...col.style,
                      }}
                      onClick={() => {
                        onColumnClick(col);
                      }}
                    >
                      <div
                        onClick={() => {
                          onColumnClick(col);
                        }}
                      >
                        {col.name}
                        {col.sort ? (
                          <div className={styles.sort}>{sortState(col.key === sortCol?.key, sortCol?.state)}</div>
                        ) : null}
                      </div>
                    </th>
                  );
                })}
              </tr>
            </thead>
          ) : null}
        </table>
      </div>
      <div
        className={styles.table2}
        ref={bodyRef}
        onScroll={() => {
          canLoadMore ? runScroll() : null;
        }}
      >
        <table className={classnames(styles.table)} style={{ height: 'auto' }}>
          <tbody className={styles.tbody} style={columns ? null : { height: '100%' }}>
            {data?.map((item, index) => {
              const checked = getChecked(selections, index);
              const bordered = getBordered(options.bordered);
              return (
                <tr
                  key={item[rowKey] || item.key || JSON.stringify(item)}
                  className={classnames(
                    options.rowHover || options.rowHover === undefined ? styles.trtdHover : null,
                    options.canRowSelect &&
                      (selectRow === index || (selectRowKey !== undefined && item[rowKey] === selectRowKey))
                      ? styles.trtdSelect
                      : null,
                    bordered ? styles.bodyBorder : null,
                  )}
                  style={options.rowHeight ? { height: options.rowHeight } : null}
                  onClick={() => {
                    if (options.canRowSelect === true && selectRow !== index) {
                      let toSelect = false;
                      toSelect = (selectRowKey !== undefined && item[rowKey] === selectRowKey) || index === selectRow;
                      if (selectRowKey === undefined) {
                        // setSelectRow(!toSelect ? index : -1);
                        setSelectRow(index);
                      }
                      onRowSelected(item, index, !toSelect);
                    }
                  }}
                >
                  {showSelection ? (
                    <td style={{ cursor: 'pointer', width: '45px', padding: options?.cellPadding || '16px 8px' }}>
                      <div
                        style={{ display: 'flex', justifyContent: 'center' }}
                        onClick={(e) => {
                          onRowClick(index);
                          e.stopPropagation();
                        }}
                      >
                        <div className={styles.check} style={checked ? { backgroundColor: '#3CE5D3' } : null}>
                          {checked ? selectAll : null}
                        </div>
                      </div>
                    </td>
                  ) : null}
                  {columns.map((col /* , i */) => {
                    return (
                      <td
                        key={`${item[rowKey] || item.key || new Date().getTime()}${col.key}`}
                        style={{
                          ...col.style,
                          ...{ color: checked ? options?.rowCheckColor || null : null },
                        }}
                      >
                        {col.render ? col.render(item, index) : item[col.key]}
                      </td>
                    );
                  })}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
};

Table.defaultProps = {
  className: null,
  rowKey: 'id',
  columns: null,
  defaultSort: null,
  data: null,
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
    bordered: false,
    rowCheckColor: null,
    cellPadding: null,
    rowHeight: null,
  },
  loadMore: () => {},
  canLoadMore: true,
};

Table.propTypes = {
  className: PropTypes.any,
  rowKey: PropTypes.string,
  columns: PropTypes.any,
  defaultSort: PropTypes.any,
  data: PropTypes.any,
  showSelection: PropTypes.bool,
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

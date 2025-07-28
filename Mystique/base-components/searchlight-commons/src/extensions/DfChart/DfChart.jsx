import React, { useEffect, useRef, useState, useLayoutEffect, useMemo } from 'react';
import propTypes from 'prop-types';
import { Table2 } from 'dui';
import { DeleteIcon } from 'dc-icon';
import CvsChart from './cvsChart';

import styles from './style.module.less';

export default function DfChart(props) {
  const { pointerColors, data, displayType, onDelete, defaultSelectedId, tableHeight, chartHeight } = props;
  const chartInstance = useRef(null);
  const bigRef = useRef(null);
  const [hh, setHH] = useState(0);
  const domRef = useRef(null);
  const [activeId, setActiveId] = useState(null);
  const [chartSize, setChartSize] = useState(400);
  useEffect(() => {
    const instance = new CvsChart({
      multiPointerColors: pointerColors,
      bearings: data,
      defaultSelectedId,
    });
    // instance.setSelectdedIndex(*)
    chartInstance.current = instance;
    if (defaultSelectedId) {
      setActiveId(defaultSelectedId);
    }
  }, []);

  const columns = useMemo(() => {
    return [
      {
        key: 'color',
        name: '',
        render: (item, index) => {
          return <div style={{ width: '16px', height: '16px', backgroundColor: pointerColors[index] }} />;
        },
      },
      {
        key: 'frequency',
        name: '频率',
        render: (item) => {
          return (item.frequency * 1000) % 1 === 0 ? item.frequency : item.frequency?.toFixed(4);
        },
      },
      {
        key: 'bearing',
        name: '示向度',
        render: (item) => {
          return <span>{(item.bearing * 1000) % 1 === 0 ? item.bearing : item.bearing.toFixed(2)}°</span>;
        },
      },
      {
        key: 'id',
        name: '',
        render: (item) => {
          return (
            <DeleteIcon
              onClick={() => {
                typeof onDelete === 'function' && onDelete(item);
              }}
            />
          );
        },
      },
    ];
  }, []);

  useLayoutEffect(() => {
    let resizeObserver;
    if (bigRef.current) {
      resizeObserver = new ResizeObserver((entries) => {
        if (entries.length > 0) {
          let timer = setTimeout(() => {
            if (chartInstance.current.hasMounted === false) {
              chartInstance.current.mount(domRef.current);
              chartInstance.current.init();

              if (typeof chartInstance.current.getChartSize === 'function') {
                setChartSize(chartInstance.current.getChartSize());
              }
            } else {
              chartInstance.current.resize();
              if (typeof chartInstance.current.getChartSize === 'function') {
                setChartSize(chartInstance.current.getChartSize());
              }
            }
            // cvsInstance.current.forceResize();
            clearTimeout(timer);
            timer = null;
          }, 100);
        }
      });
      resizeObserver.observe(bigRef.current);
    }

    return () => {
      if (resizeObserver) {
        resizeObserver.unobserve(bigRef.current);
        resizeObserver.disconnect();
      }
    };
  }, [bigRef]);

  useEffect(() => {
    if (chartInstance.current && chartInstance.current.hasMounted) {
      chartInstance.current.updateBearings(data);
    }
  }, [data]);

  useEffect(() => {
    if (chartInstance.current && chartInstance.current.hasMounted) {
      chartInstance.current.resize();
    }
  }, [displayType]);

  const centerBearing = useMemo(() => {
    let result = null;
    if (data instanceof Array && data.length > 0 && activeId) {
      result = data.find((d) => d.id === activeId)?.bearing;
    }

    return result;
  }, [data, activeId]);

  useEffect(() => {
    if (
      chartInstance.current &&
      chartInstance.current.hasMounted &&
      typeof chartInstance.current.setSelectdedIndex === 'function'
    ) {
      chartInstance.current.setSelectdedIndex(activeId);
    }
  }, [activeId]);

  return (
    <div className={styles.dfChartContainer} ref={bigRef}>
      <div
        className={styles.dfChartInner}
        style={{
          height: displayType.length === 2 ? chartHeight : '100%',
          display: (displayType || []).includes('chart') ? 'block' : 'none',
        }}
        ref={domRef}
      >
        {typeof centerBearing === 'number' && <span className={styles.centerBearing}>{centerBearing}°</span>}
      </div>

      {(displayType || []).includes('table') && (
        <div className={styles.dfTableContainer} style={{ height: displayType.length === 2 ? tableHeight : '100%' }}>
          <div style={{ height: '100%', width: `${chartSize}px` }}>
            <Table2
              columns={columns}
              data={data}
              rowKey="id"
              showSelection={false}
              options={{ canRowSelect: true, bordered: { inline: false, outline: true }, rowHeight: 36 }}
              onRowSelected={(p) => {
                if (p.id && chartInstance.current && typeof chartInstance.current.setSelectdedIndex === 'function') {
                  setActiveId(p.id);
                }
              }}
              selectRowKey={activeId}
            />
          </div>
        </div>
      )}
    </div>
  );
}

DfChart.propTypes = {
  data: propTypes.array,
  pointerColors: propTypes.array,
  displayType: propTypes.array,
  onDelete: propTypes.func,
  defaultSelectedId: propTypes.any,
  tableHeight: propTypes.string,
  chartHeight: propTypes.string,
};

DfChart.defaultProps = {
  pointerColors: ['#35E065', '#FFD118', '#3CE5D3', '#FF85C0', '#FE7A45'],
  data: [
    {
      bearing: 20,
      frequency: 101,
      id: 1,
    },
    {
      bearing: 80,
      frequency: 102,
      id: 2,
    },
    {
      bearing: 160,
      frequency: 103,
      id: 3,
    },
  ],
  displayType: ['chart', 'table'],
  onDelete: () => {},
  defaultSelectedId: null,
  tableHeight: '35%',
  chartHeight: '65%',
};

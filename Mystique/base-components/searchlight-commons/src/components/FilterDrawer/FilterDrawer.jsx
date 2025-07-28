import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import { Button, Checkbox, Drawer, Calendar } from 'dui';
import styles from './index.module.less';

const FilterDrawer = (props) => {
  const { isShow, timeRange, visibleHandle, title, options, okHandle, enableDelete, width, contentLeft, contentRight } =
    props;
  const [option, setOption] = useState([]);
  const [times, setTimes] = useState([]);

  useEffect(() => {
    setOption(options);
    if (timeRange) setTimes(timeRange);
  }, [isShow]);

  const [flag, setFlag] = useState(false);

  useEffect(() => {
    let bool = false;
    bool = option.some((i) => {
      return i.value.length > 0;
    });
    setFlag(bool);
  }, [option]);

  const visibleOptionHandle = (e) => {
    const arr = option.map((i) => {
      if (i.type === e) {
        return {
          ...i,
          visible: !i.visible,
        };
      }
      return i;
    });
    setOption(arr);
  };

  const selectHandle = (e, type) => {
    const arr = option.map((i) => {
      if (i.type === type) {
        return {
          ...i,
          value: e,
        };
      }
      return i;
    });
    setOption(arr);
  };

  const reset = (e) => {
    let arr = [];
    if (e) {
      arr = option.map((i) => {
        return {
          ...i,
          value: [],
        };
      });
    } else {
      arr = option.map((i) => {
        const a = i.option.map((it) => {
          return it.value;
        });
        return {
          ...i,
          value: a,
        };
      });
    }
    setOption(arr);
  };

  const deleteHandle = (a, b) => {
    const arr = options.map((i) => {
      if (i.type === b && i.value.includes(a)) {
        const c = i.value.filter((it) => {
          return it !== a;
        });
        return {
          ...i,
          value: c,
        };
      }
      return i;
    });
    setOption(arr);
    okHandle(arr, times);
  };
  const svg1 = (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        d="M5.00001 9.07102L12.0711 16.1421L19.1421 9.07102"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );

  const svg2 = (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        d="M5.00001 9.07102L12.0711 16.1421L19.1421 9.07102"
        stroke="var(--theme-font-100)"
        strokeWidth="1.5"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );

  const deleteSvg = (
    <svg width="10" height="10" viewBox="0 0 10 10" fill="none" xmlns="http://www.w3.org/2000/svg">
      <g opacity="0.5">
        <path
          d="M1 1L9 9"
          stroke="var(--theme-font-100)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M9 1L1 9"
          stroke="var(--theme-font-100)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </g>
    </svg>
  );

  const filterSvg = (
    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <g opacity="0.8">
        <path
          // eslint-disable-next-line max-len
          d="M8.79545 15.7603V11.2479L4.05542 6.72336C3.40284 6.10044 3.84374 5 4.7459 5H15.7541C16.6563 5 17.0972 6.10044 16.4446 6.72336L11.7045 11.2479V19"
          stroke="var(--theme-font-100)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M13.6787 12.6001H18.2501"
          stroke="var(--theme-font-100)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M13.6787 15H18.2501"
          stroke="var(--theme-font-100)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
        <path
          d="M13.6787 17.3999H18.2501"
          stroke="var(--theme-font-100)"
          strokeWidth="1.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </g>
    </svg>
  );
  return (
    <>
      <div className={styles.root}>
        {contentLeft}
        {enableDelete && (
          <div className={styles.headerCenter}>
            <div>
              {options.map((it) => {
                return it.option.map((item) => {
                  if (it.value && it.value.includes(item.value)) {
                    return (
                      <div key={item.value} className={styles.itemStyle}>
                        {item.label}
                        <div onClick={() => deleteHandle(item.value, it.type)}>{deleteSvg}</div>
                      </div>
                    );
                  }
                  return null;
                });
              })}
            </div>
          </div>
        )}
        <div className={styles.right}>
          {contentRight}
          <Button
            onClick={() => {
              visibleHandle(true);
            }}
          >
            <div className={styles.filterButton}>
              {filterSvg}
              筛选
            </div>
          </Button>
        </div>
      </div>
      <Drawer
        title={title}
        visible={isShow}
        onCancel={() => {
          okHandle(options, timeRange);
          visibleHandle(false);
        }}
        width={`${width}px`}
        bodyStyle={{ padding: '0' }}
      >
        <div className={styles.main}>
          {timeRange && (
            <div className={styles.time}>
              <div style={{ width: `calc(${width}px - 64px)` }}>
                <span>时间</span>
                <Calendar.Range position="right" value={times} onChange={(ddd) => setTimes(ddd)} />
              </div>
            </div>
          )}
          <div className={styles.content}>
            {option.map((it) => {
              if (it.option.length > 0) {
                return (
                  <div key={it.type} style={{ marginBottom: '8px' }}>
                    <div className={styles.title} onClick={() => visibleOptionHandle(it.type)}>
                      <span>{it.name}</span>
                      {it.visible ? <div>{svg1}</div> : <div style={{ transform: 'rotate(180deg)' }}>{svg2}</div>}
                    </div>
                    <div
                      id="idname"
                      className={it.child === 1 ? styles.checkbox1 : styles.checkbox2}
                      style={{
                        display: it.visible ? 'none' : '',
                        '--number': it.child > 4 ? 4 : it.child,
                        '--width':
                          it.option.length % (it.child > 4 ? 4 : it.child) === 0
                            ? '100%'
                            : `${
                                ((width - 32 * 2) / it.child - 8 / (it.child - (it.option.length % it.child))) *
                                (it.child - (it.option.length % it.child))
                              }px`,
                      }}
                    >
                      <Checkbox.Group value={it.value} onChange={(e) => selectHandle(e, it.type)} options={it.option} />
                    </div>
                  </div>
                );
              }
              return null;
            })}
          </div>
          <div className={styles.foot}>
            <Button onClick={() => reset(flag)}>{flag ? '清空' : '全选'}</Button>
            <Button
              onClick={() => {
                okHandle(option, times);
                visibleHandle(false);
              }}
              style={{ color: '#3CE5D3' }}
            >
              确认
            </Button>
          </div>
        </div>
      </Drawer>
    </>
  );
};

FilterDrawer.defaultProps = {
  width: 450,
  options: [],
  title: '筛选',
  isShow: false,
  enableDelete: true,
  okHandle: () => {},
  visibleHandle: () => {},
  contentLeft: null,
  contentRight: null,
  timeRange: null,
};

FilterDrawer.propTypes = {
  width: PropTypes.number,
  options: PropTypes.array,
  isShow: PropTypes.bool,
  enableDelete: PropTypes.bool,
  okHandle: PropTypes.func,
  timeRange: PropTypes.any,
  visibleHandle: PropTypes.func,
  title: PropTypes.string,
  contentLeft: PropTypes.element,
  contentRight: PropTypes.element,
};

export default FilterDrawer;

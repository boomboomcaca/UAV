/*
 * @Author: wangXueDong
 * @Date: 2022-04-21 11:31:46
 * @LastEditors: XYQ
 * @LastEditTime: 2022-10-10 16:39:16
 */
import React, { useEffect, useRef, useState, useMemo } from 'react';
import PropTypes from 'prop-types';
import 'dc-map/dist/main.css';
import { Input, Checkbox, Select, message } from 'dui';
import { useSetState, useUnmount } from 'ahooks';
import classnames from 'classnames';
import PubSub from 'pubsub-js';
import { pubSubKey, clickMapPubSubKey } from '../setting';
import styles from './stationSelector.module.less';

const { Option } = Select;
let first = 0;
let isFirst = true;
function StationSelector(props) {
  const { selectEdgeId, stations, selectEdges, onChange, number } = props;
  const [key, setKey] = useState(0);
  const [gCitylist, setgCitylist] = useState([]);
  const [groupedStations, setGroupedStations] = useState([]);
  const [searchWord, setSearchWord] = useState(''); // 关键词
  const [selEid, setSelEid] = useState(selectEdgeId);
  const [selStations, setSelStations] = useState(selectEdges || []);
  const selStationsRef = useRef(selectEdges);
  const [gindex, setGindex] = useState(0);
  const [gMapArr, setgapArr] = useState([]);

  let tempJson = null;
  const mydiv = useRef();
  const [gtype, setGType] = useSetState({
    index: null,
    data: false,
    selectList: ['stationaryCategory', 'mobileCategory', 'movableCategory'],
    rand: 0,
    station: null,
    tempData: null,
    deviceinfolist: [],
    deviceList: [],
    categoryArr: {
      stationaryCategory: {
        name: '固定站',
        value: '0',
        chrild: [
          {
            category: '1',
            name: '一类站',
            id: '2022021801',
          },
          {
            category: '2',
            name: '二类站',
            id: '2022021802',
          },
          {
            category: '3',
            name: '三类站',
            id: '2022021803',
          },
          {
            category: '4',
            name: '四类站',
            id: '2022021804',
          },
          {
            category: '0',
            name: '全部',
            id: '2022021805',
          },
        ],
      },
      mobileCategory: {
        name: '移动站',
        value: '0',
        chrild: [
          {
            category: '4',
            name: '水上站',
            id: '2022021806',
          },
          {
            category: '5',
            name: '空中站',
            id: '2022021807',
          },
          {
            category: '1',
            name: '陆地一类站',
            id: '2022021808',
          },
          {
            category: '2',
            name: '陆地二类站',
            id: '2022021809',
          },
          {
            category: '3',
            name: '陆地三类站',
            id: '2022021810',
          },
          {
            category: '0',
            name: '全部',
            id: '2022021811',
          },
        ],
      },
      movableCategory: {
        name: '搬移站',
      },
      // all: {
      //   name: '只看空闲',
      // },
    },
  });
  const { categoryArr, deviceinfolist } = gtype;
  const expandNames = useRef({}).current;
  tempJson = categoryArr;
  useMemo(() => {
    if (!selectEdgeId) return;

    if (selectEdgeId && gCitylist && first === 0) {
      /**
       * 城市下标、选择功能下标、选择功能列表
       */
      gCitylist.forEach((it, i) => {
        it?.data.forEach((item) => {
          if (item.id === selectEdgeId.edgeId) {
            setGindex(i);
            setGType({ station: item });
            first += 1;
          }
        });
      });
    } else {
      gCitylist[gindex]?.data.forEach((item) => {
        if (item.id === selectEdgeId?.edgeId) {
          setGType({ station: item });
        }
      });
    }
    setSelEid(selectEdgeId);
  }, [selectEdgeId, gCitylist]);

  useEffect(() => {
    PubSub.subscribe(clickMapPubSubKey, (evt, dat) => {
      const selId = dat.selEdgeIds;
      const selObj = stations.find((e) => e.id === selId);
      toSelect(selObj);
    });
    return () => {
      PubSub.unsubscribe(pubSubKey);
      PubSub.unsubscribe(clickMapPubSubKey);
    };
  }, [stations]);

  useEffect(() => {
    //  增加zoneStr属性，为市级地名
    const zStations = stations.map((ss) => {
      return { ...ss, zoneStr: ss.zone ? ss.zone.split(' ')?.[1] || '其它' : '其它' };
    });
    //  将站点按照省分组
    const gCity = groupBy(zStations, (item) => {
      return item.zone ? item.zone.split(' ')[0] : item.zoneStr?.split(' ')[0];
    });
    // 按照category进行分组
    getDataCity(gCity);
  }, [stations, gindex, gtype.data, gtype.selectList, categoryArr, gtype.rand]);

  const getDataCity = (list, bool = true) => {
    setgCitylist(list);
    //  根据按照category过滤list，
    list.forEach((item) => {
      const t = [];
      item.data.forEach((it) => {
        if (it.type.indexOf('Category') < 0) {
          it.type = `${it.type}Category`;
        }
        if (gtype.selectList.indexOf(it.type) > -1) {
          t.push(it);
        }
      });
      item.data = t;
    });
    if (list[gindex]) {
      setTimeout(() => {
        let gStations = list[gindex]?.data;
        //  输入框筛选条件
        if (searchWord) {
          gStations = gStations.filter((it) => it.name.indexOf(searchWord) > -1);
        }
        const { selectList } = gtype;
        const s = handleChangeType(gStations, selectList);
        setGroupedStations(s);
        // if (bool) {
        //   setgapArr(s);
        // }
      }, 300);
    }
  };
  /**
   *  选择类型处理
   * @param {*} arr //  当前选择的地区包含的站点列表
   * @param {*} e
   * @returns
   */
  const handleChangeType = (arr, e) => {
    let arrData = [...arr];
    //  只看空闲
    if (gtype.data) {
      const t = [];
      arrData.forEach((it) => {
        for (let i = 0; i < it.modules.length; i += 1) {
          if (it.modules[i].moduleState === 'idle' && it.modules[i].moduleType === 'driver') {
            t.push(it);
            break;
          }
        }
      });
      arrData = t;
    }
    if (e.length > 0) {
      const testlist = [...arrData];
      const tt = [];
      testlist.forEach((it) => {
        if (it.type.indexOf('Category') < 0) {
          it.type = `${it.type}Category`;
        }
        if (it.mytest) {
          tt.push(it);
        }
        if (
          e.indexOf(it.type) > -1 &&
          (it.category * 1 === categoryArr[it.type].value * 1 || categoryArr[it.type].value * 1 === 0)
        ) {
          it.mytest = 1;
          tt.push(it);
        }
        if (it.type === 'movableCategory') {
          it.mytest = 1;
          tt.push(it);
        }
      });
      return tt;
    }

    return arrData;
  };
  const groupBy = (array, f) => {
    const groups = {};
    array.forEach((o) => {
      const group = JSON.stringify(f(o));
      groups[group] = groups[group] || [];
      groups[group].push(o);
    });

    return Object.keys(groups).map((group) => {
      const name = JSON.parse(group);
      const ret = { name, data: groups[group] };

      const find = expandNames[name];
      if (find !== undefined) {
        ret.expand = find;
      }
      return ret;
    });
  };
  useUnmount(() => {
    first = 0;
    isFirst = true;
  });
  /**
   *
   * @returns 城市切换移动
   */
  const handleChange = () => {
    if (mydiv.current) {
      const w = mydiv.current.clientWidth;
      const old = mydiv.current.scrollWidth;
      return w < old ? gindex * 25 : 0;
    }
    return 0;
  };
  /**
   * 选择类型
   * @param {*} e
   * @param {*} it
   */
  const handleChangeTest = (e, it) => {
    e.stopPropagation();
    // setSelEid(null);
    const str = gtype.selectList;
    if (str.indexOf(it) > -1) {
      const t = str.filter((item) => item !== it);
      setGType({ selectList: t });
    } else {
      str.push(it);
      setGType({ selectList: JSON.parse(JSON.stringify(str)) });
    }
  };
  const toSelect = (e) => {
    let selArr = [...selStationsRef.current];
    const index = selArr.findIndex((ele) => ele.id === e.id);
    if (number === 1) {
      selArr = [];
      selArr.push(e);
    } else {
      if (index === -1 || (!index && index !== 0)) {
        if (number && selArr.length >= number) {
          message.warn(`最多选择${number}个监测站`);
          return;
        }
        selArr.push(e);
      } else {
        selArr.splice(index, 1);
      }
    }
    setSelStations(selArr);
    selStationsRef.current = selArr;
    onChange(selArr);
  };
  useEffect(() => {
    const edgeids = selStations.map((e) => {
      return e.id;
    });
    if (isFirst && selStations) {
      isFirst = false;
    } else {
      PubSub.publish(pubSubKey, {
        isFirst: false,
        selEdgeIds: edgeids || [],
      });
    }
  }, [selStations]);

  const isClude = (id) => {
    let is = false;
    for (let i = 0; i < selStations.length; i += 1) {
      if (selStations[i].id === id) {
        is = true;
        break;
      }
    }
    return is;
  };
  /**
   * 2J下拉分类
   * @param {*} it
   * @returns
   */
  const handleType = (it) => {
    return (
      <div>
        {categoryArr[it].chrild ? (
          <div>
            <Select
              value={categoryArr[it].value}
              onChange={(val) => {
                categoryArr[it].value = val;
                // setSelEid(null);
                setGType({ categoryArr: JSON.parse(JSON.stringify(categoryArr)) });
              }}
              style={{
                width: `${it === 'mobileCategory' ? '122px' : '92px'}`,
                background: 'var(--theme-input)',
              }}
            >
              {categoryArr[it].chrild?.map((its) => (
                <Option value={its.category} key={`${it}_${its.id}`}>
                  {its.name}
                </Option>
              ))}
            </Select>
          </div>
        ) : null}
      </div>
    );
  };
  return (
    <div className={styles.station_center_view}>
      <div className={styles.station_center_view_left}>
        {/** 城市+搜索 */}
        <div className={styles.station_center_view_left_fiex}>
          <div className={classnames(styles.station_center_view_left_city)} ref={mydiv}>
            {gCitylist?.map((item, i) => {
              return (
                <div
                  style={{ transform: [`translateX(-${handleChange()}px)`] }}
                  // eslint-disable-next-line react/no-array-index-key
                  key={`${item.name}_${i}`}
                  className={classnames(
                    styles.station_center_view_left_city_item,
                    i === gindex ? styles.station_center_view_left_city_item_active : null,
                  )}
                  onClick={(e) => {
                    e.stopPropagation();
                    if (i === gindex) return;
                    setGindex(i);
                    setGType({
                      data: false,
                      selectList: ['stationaryCategory', 'mobileCategory', 'movableCategory'],
                      categoryArr: tempJson,
                    });
                  }}
                >
                  <div>{item.name}</div>
                  <div
                    className={classnames(i === gindex ? styles.station_center_view_left_city_item_active_line : null)}
                  />
                </div>
              );
            })}
          </div>
          <Input
            allowClear
            showSearch
            placeholder="搜索"
            value={searchWord}
            maxLength={10}
            style={{ width: 240 }}
            onChange={(e) => setSearchWord(e)}
            onSearch={(e) => {
              //   setSelEid(null);
              //   setDevice(null);
              setGType({ rand: Math.random() });
            }}
          />
        </div>
        {/** 筛选类型 */}
        <div className={styles.station_center_view_left_type}>
          {Object.keys(categoryArr).map((it, i) => {
            if (it === 'all') {
              return (
                <Checkbox.Traditional
                  checked={gtype.data}
                  // eslint-disable-next-line react/no-array-index-key
                  key={`${it}_0000${i}`}
                  onChange={(bl) => {
                    // setSelEid(null);
                    // setDevice(null);
                    setGType({ data: bl });
                  }}
                >
                  {categoryArr[it].name}
                </Checkbox.Traditional>
              );
            }
            if (it === 'mobileCategory' || it === 'stationaryCategory') {
              return (
                // eslint-disable-next-line react/no-array-index-key
                <div className={styles.station_center_view_left_type_selectItem} key={`${it}_0000${i}`}>
                  <div
                    className={classnames(
                      gtype.selectList?.indexOf(it) < 0
                        ? styles.station_center_view_left_type_selectItem_item
                        : styles.station_center_view_left_type_selectItem_item_active,
                    )}
                    onClick={(e) => handleChangeTest(e, it)}
                  >
                    {categoryArr[it].name}
                  </div>
                  <div
                    className={classnames(
                      gtype.selectList?.indexOf(it) < 0
                        ? styles.station_center_view_left_type_selectItem_item_right
                        : styles.station_center_view_left_type_selectItem_item_active_right,
                    )}
                  >
                    {handleType(it)}
                  </div>
                </div>
              );
            }
            return (
              <div
                // eslint-disable-next-line react/no-array-index-key
                key={`${it}_0000${i}`}
                className={classnames(
                  gtype.selectList?.indexOf(it) < 0
                    ? styles.station_center_view_left_type_item
                    : styles.station_center_view_left_type_item_active,
                )}
                onClick={(e) => handleChangeTest(e, it)}
              >
                {categoryArr[it].name}
              </div>
            );
          })}
        </div>
        <div className={styles.station_center_view_left_list}>
          {groupedStations.map((s) => {
            return (
              <div
                className={classnames(styles.featureItem_new, {
                  [styles.active]: isClude(s.id),
                  // [styles.ban]: s.state !== 'idle',
                  // [styles.disabled]: disabled,
                })}
                key={s.id}
                id={s.id}
                onClick={() => {
                  toSelect(s);
                  //   setSelEid({ edgeId: s.id, featureId: null });
                  // setView(groupedStations, s);
                }}
              >
                <div className={styles.info}>
                  <div
                    style={{ fontWeight: isClude(s.id) ? 'bold' : '500' }}
                    className={classnames(styles.ename, isClude(s.id) ? styles.acFont : '')}
                  >
                    {s.name}
                  </div>
                  {s.frequency?.startFrequency && s.frequency?.stopFrequency ? (
                    <div className={classnames(styles.efreq, isClude(s.id) ? styles.acFont : '')}>
                      {s.frequency.startFrequency}MHz - {s.frequency.stopFrequency}MHz
                    </div>
                  ) : null}
                </div>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}

StationSelector.defaultProps = {
  selectEdgeId: {},
  stations: [],
  selectEdges: [],
  number: null,
  onChange: () => {},
};

StationSelector.propTypes = {
  selectEdgeId: PropTypes.object,
  stations: PropTypes.array,
  selectEdges: PropTypes.array,
  onChange: PropTypes.func,
  number: PropTypes.number,
};

export default StationSelector;

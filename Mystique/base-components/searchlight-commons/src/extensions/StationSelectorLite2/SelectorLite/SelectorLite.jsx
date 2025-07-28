/* eslint-disable max-len */
import React, { useEffect, useState, useRef, useLayoutEffect, useMemo } from 'react';
import PropTypes, { bool } from 'prop-types';
import classnames from 'classnames';
import initDcMap from 'dc-map';
import { useUpdateEffect, useSetState, useUnmount } from 'ahooks';
import { Input, Select, Radio, message } from 'dui';
import { GreenGoodIcon, BarIcon, CompassIcon } from 'dc-icon';
import StationIcons from '../../../components/StationIcons';
import styles from './styles.module.less';
import 'dc-map/dist/main.css';

const { Option } = Select;
const zoom = 3;
let first = 0;
let mapInstancestr = null;
const moduleState = ['offline', 'fault', 'disabled']; // 查找的条件

const StationSelectorLite = (props) => {
  const { onSelect, selectEdgeId, stations, mapOptions, onChangeApi, deviceDatas, selectType, onClose } = props;

  const selDivRef = useRef(null);
  const [selEid, setSelEid] = useState(selectEdgeId);
  const [key, setKey] = useState(0);
  const [searchWord, setSearchWord] = useState(''); // 关键词
  const [groupedStations, setGroupedStations] = useState([]);
  const [gCitylist, setgCitylist] = useState([]);
  const [gMapArr, setgapArr] = useState([]);
  const [gindex, setGindex] = useState(0);
  const [mapInstance, setmapInstance] = useState(null);
  const [top1, setTop] = useState('6%');
  const [device, setDevice] = useState({});
  const [deviceinfo, setDeviceInfo] = useState({});
  const [selectItem, SetSelectItem] = useState(null);
  const [check, setCheck] = useState(false);
  let tempJson = null;
  const mydiv = useRef();
  const alllist = useRef([]);

  const [gtype, setGType] = useSetState({
    index: null,
    keep: [],
    data: false,
    selectList: ['stationaryCategory', 'mobileCategory', 'movableCategory'],
    rand: 0,
    station: null,
    tempData: null,
    deviceshow: false,
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
    },
  });
  const { categoryArr, deviceinfolist } = gtype;
  const expandNames = useRef({}).current;
  tempJson = categoryArr;
  useLayoutEffect(() => {
    if (!mapInstance) return;
    mapInstance.mount();
    mapInstance.handleOAStationsClick('edgesStation1', (currentEdge) => {
      // SetSelectItem(currentEdge);
      addSelect(currentEdge);
      return currentEdge;
    });
  }, [mapInstance]);

  useLayoutEffect(() => {
    setGType({ deviceinfolist: deviceDatas });
  }, [deviceDatas]);
  useUnmount(() => {
    first = 0;
  });
  useEffect(() => {
    alllist.current = [...selectEdgeId];
  }, [selectEdgeId]);

  useUpdateEffect(() => {
    if (JSON.stringify(mapInstancestr) === JSON.stringify(mapOptions)) {
      return;
    }
    const options = {
      container: 'mapStationSelect2',
      zoom,
      coordinate: 'WGS84',
      mapType: mapOptions?.mapType || 'amap',
      customUrl: mapOptions?.webMapUrl || mapOptions?.customUrl || 'http://192.168.102.31:6088',
      fontUrl: mapOptions?.fontUrl || 'http://192.168.102.103:6066/public',
      type: 'grid',
      showBaseTool: false,
      showMeasureTool: false,
      showScaleControl: false,
      baseToolPosition: 'top-right',
      measureToolPosition: 'top-right',
    };
    window.mstatiom = initDcMap(options);
    setmapInstance(window.mstatiom);
    almostDone();
    mapInstancestr = mapOptions;
  }, [mapOptions]);

  useUpdateEffect(() => {
    if (mapInstance) {
      setView(gMapArr);
    }
  }, [mapInstance, gMapArr, gtype.keep]);

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

  useEffect(() => {
    const vh = document.documentElement.clientHeight;
    document.documentElement.style.setProperty('--h', `${vh / 2 + 200}px`);
    // setTop(`${(vh - (vh / 2 + 200)) / 3}px`);

    const zStations = stations.map((ss) => {
      return { ...ss, zoneStr: ss.zone ? ss.zone.split(' ')?.[1] || '其它' : '其它' };
    });
    const gCity = groupBy(zStations, (item) => {
      return item.zone ? item.zone.split(' ')[0] : item.zoneStr?.split(' ')[0]; // 按照category进行分组
    });
    getDataCity(gCity);
  }, [stations, gindex, gtype.data, gtype.selectList, categoryArr, gtype.rand]);

  //   useEffect(() => {
  //     getDataCity([...gCitylist], false);
  //   }, [stations, key]);

  /**
   *
   * @param {*} list
   * @param {*} bool
   */
  const getDataCity = (list, bool = true) => {
    const arrlist = [...list];
    console.log(arrlist, '%%%%%%%%%%%');
    setgCitylist(arrlist);
    if (arrlist[gindex]) {
      setTimeout(() => {
        const arr = arrlist[gindex]?.data;
        const gStations = groupBy(arr, (item) => {
          return item.zoneStr ? item.zoneStr : item.zone; // 按照category进行分组
        });

        // window.console.log(gStations);
        if (searchWord) {
          gStations.forEach((it) => {
            const t = [];
            it.data?.forEach((item) => {
              if (item.name.indexOf(searchWord) > -1) {
                t.push(item);
              }
            });
            it.data = t;
          });
        }
        const { selectList } = gtype;
        const s = handleChangeType(gStations, selectList);
        s.forEach((it) => {
          const arealist = groupBy(it.data, (item) => {
            return item.areacode;
          });
          arealist.forEach((item) => {
            const t = item.data[0].zone.split(' ');
            item.areaname = t[t.length - 1];
          });
          it.area = arealist;
          it.selectlist = [];
          it.code = '';
        });
        const ar = [...s].filter((item) => item.data.length > 0);

        setGroupedStations(ar);
        setCheck(true);
        for (let i = 0; i < ar.length; i += 1) {
          const show = ar[i].data.filter((item) => item.id === selEid?.id);
          if (show.length > 0) {
            setGType({ deviceshow: true });
            break;
          }
        }
        if (bool) {
          setgapArr(s);
        }
      }, 300);
    }
  };
  /**
   * 接收数据并组装数据
   */
  useLayoutEffect(() => {
    if (!alllist.current) return;

    if (alllist.current && groupedStations.length > 0 && check && first === 0) {
      /**
       * 城市下标、选择功能下标、选择功能列表
       */
      const list = JSON.parse(JSON.stringify(alllist.current));
      for (let i = 0; i < list.length; i += 1) {
        const item = list[i];
        let s = null;
        groupedStations.find((it) => {
          s = it.data.find((items) => items.id === item.id);
          return false;
        });

        if (s) {
          const s1 = intoType(s, false);
          const m = item.moduleIds;
          for (let j = 0; j < m.length; j += 1) {
            const k = m[j];
            Object.keys(k).forEach((res) => {
              const mid = s1.newItem.find((it) => it.name.indexOf(res) > -1);
              if (mid) {
                const code = mid.data.find((it) => it.id === k[res]);

                selectTypeOnclick(mid, code.id, s1, false);
              }
            });
          }
          handleChangeOnclick(s, false);
        }
      }
      setCheck(false);
    }
  }, [groupedStations, check]);

  /**
   *  选择类型处理
   * @param {*} arr
   * @param {*} e
   * @returns
   */
  const handleChangeType = ([...arr], e) => {
    arr?.forEach((item) => {
      item.data?.forEach((it) => {
        const temp = [];
        for (let i = 0; i < it.modules.length; i += 1) {
          const sameArr = [...it.modules[i].supportedFeatures].filter((items) =>
            selectType.find((its) => items === its),
          );
          if (sameArr.length > 0 && it.modules[i].moduleType === 'driver') {
            temp.push(it.modules[i]);
          }
        }
        it.modules = temp;
        let sameArr = it.modules.filter((itemid) => itemid.moduleState === 'idle');
        if (sameArr.length > 0) {
          it.state = 'idle';
          return;
        }
        sameArr = it.modules.filter((itemid) => itemid.moduleState === 'busy' || itemid.moduleState === 'deviceBusy');
        if (sameArr.length > 0) {
          it.state = 'busy';
          return;
        }
        sameArr = it.modules.filter((itemid) => itemid.moduleState === 'fault');
        if (sameArr.length > 0) {
          it.state = 'fault';
          return;
        }
        it.state = 'offline';
      });
    });
    if (gtype.data) {
      arr?.forEach((item) => {
        const t = [];
        item.data?.forEach((it) => {
          for (let i = 0; i < it.modules.length; i += 1) {
            const sameArr = [...it.modules[i].supportedFeatures].filter((items) =>
              selectType.find((its) => items === its),
            );
            if (sameArr.length > 0 && it.modules[i].moduleType === 'driver') {
              t.push(it);
              break;
            }
          }
        });
        item.data = t;
      });
    }

    if (e.length > 0) {
      const testlist = JSON.parse(JSON.stringify(arr));
      testlist.forEach((item) => {
        const t = [];
        item.data.forEach((it) => {
          if (it.type.indexOf('Category') < 0) {
            it.type = `${it.type}Category`;
          }
          if (e.indexOf(it.type) > -1) {
            t.push(it);
          }
        });
        item.data = t;
      });
      testlist.forEach((item) => {
        const t = [];
        item.data.forEach((it) => {
          if (it.mytest) {
            t.push(it);
          }
          if (
            e.indexOf(it.type) > -1 &&
            (it.category * 1 === categoryArr[it.type].value * 1 || categoryArr[it.type].value * 1 === 0)
          ) {
            it.mytest = 1;
            t.push(it);
          }
          if (it.type === 'movableCategory') {
            it.mytest = 1;
            t.push(it);
          }
        });
        item.data = t;
      });
      return testlist;
    }
    return arr;
  };

  const setView = (arr) => {
    mapInstance.map.setZoom(8);
    const json = [];
    if (arr && arr.length > 0) {
      const t = [];
      arr.forEach((it) => {
        it.data.forEach((i) => {
          t.push(i);
        });
      });
      const newlist = t.filter((item) => {
        return item.latitude;
      });
      const { keep } = gtype;
      newlist.forEach((it) => {
        const ar = keep?.filter((item) => item.id === it.id);
        json.push({
          ...it,
          status: it.state,
          edgeID: it.id,
          angle: 0,
          lineLength: 0,
          isMoveStation: it.type === 'mobile',
          selected: ar.length > 0 ? 'Y' : 'N',
          innerImage: StationIcons(it.type, it.state),
          prop: it,
          zIndex: ar.length > 0 ? 10 : 0,
          stationWidth: ar.length > 0 ? 24 : 18,
          stationHeight: ar.length > 0 ? 24 : 18,
          width: ar.length > 0 ? 70 : 60,
          height: ar.length > 0 ? 70 : 60,
          outLineColor: ar.length > 0 ? '#FF4C2B' : '',
          'icon-size': 1,
          'circle-size': 25,
          category: Number(it.category),
          textColor: 'white',
        });
      });
      if (mapInstance && json.length > 0 && json instanceof Array) {
        setTimeout(() => {
          try {
            mapInstance?.drawOAStations && mapInstance?.drawOAStations(json, 'edgesStation1');
            getPosition(json);
            if (keep.length > 0) {
              const positionList = [];
              keep.map((it) => positionList.push([it.longitude, it.latitude]));
              keep.length === 1
                ? mapInstance.setPosition(positionList[0], true)
                : mapInstance.fitBounds && mapInstance.fitBounds(positionList, 120);
            }
          } catch (error) {
            console.log(error);
          }
        }, 300);
      } else {
        mapInstance?.drawOAStations && mapInstance?.drawOAStations(json, 'edgesStation1');
      }
      // console.log(mapInstance.map?.getZoom());
      // setTimeout(() => {
      //   mapInstance.map.setZoom(12);
      // }, 3000);
    }
  };
  const getPosition = (edges) => {
    let minLat = 90;
    let maxLat = -90;
    let minLng = 180;
    let maxLng = -180;
    edges.forEach((e) => {
      if (e.latitude < minLat) minLat = e.latitude;
      if (e.latitude > maxLat) maxLat = e.latitude;
      if (e.longitude < minLng) minLng = e.longitude;
      if (e.longitude > maxLng) maxLng = e.longitude;
    });

    let lat = 0;
    let lng = 0;

    if (maxLat > 0 && minLat < 0) {
      lat = (maxLat - Math.abs(minLat)) / 2;
    } else {
      lat = (minLat + maxLat) / 2;
    }

    if (maxLng > 0 && minLng < 0) {
      lng = (maxLng - Math.abs(minLng)) / 2;
    } else {
      lng = (minLng + maxLng) / 2;
    }
    const list = [];
    edges.forEach((it) => {
      list.push([it.longitude * 1, it.latitude * 1]);
    });
    mapInstance.setPosition([lng, lat], true);
    if (list.length > 1) mapInstance.fitBounds(list, 30);
  };

  const almostDone = () => {
    if (selDivRef.current) {
      setTimeout(() => {
        const scroll = document.getElementById(selDivRef.current);
        scroll?.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        });
      }, 300);
    }
  };

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
   * 组合+显示类型
   * @param {*} e
   * @returns
   */
  const handleChangeTypeItem = (e) => {
    let str = '';
    Object.keys(categoryArr).forEach((it) => {
      if (it === e.type) {
        if (categoryArr[it].chrild) {
          const arr = categoryArr[it].chrild;
          for (let i = 0; i < arr.length; i += 1) {
            if (arr[i].category === e.category) {
              str = `${categoryArr[it].name} ${arr[i].name}`;
            }
          }
        } else {
          str = categoryArr[it].name;
        }
      }
    });
    return str;
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
                setSelEid(null);
                setDevice(null);
                setGType({ categoryArr: JSON.parse(JSON.stringify(categoryArr)) });
              }}
              style={{
                width: `${it === 'mobileCategory' ? '122px' : '92px'}`,
                background: `${gtype.selectList.indexOf(it) > -1 ? 'rgba(4, 5, 27, 0.2)' : ''}`,
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

  /**
   * 选择类型
   * @param {*} e
   * @param {*} it
   */
  const handleChangeTest = (e, it) => {
    e.stopPropagation();
    // setSelEid(null);
    // setDevice(null);
    SetSelectItem(null);
    setGType({ deviceshow: false });
    const str = gtype.selectList;
    if (str.indexOf(it) > -1) {
      const t = str.filter((item) => item !== it);
      setGType({ selectList: t });
    } else {
      str.push(it);
      setGType({ selectList: JSON.parse(JSON.stringify(str)) });
    }
  };

  /**
   * 筛选 匹配的数据
   */
  const getDataType = (arr) => {
    const t = [];
    arr?.modules?.forEach((it) => {
      const sameArr = [...it.supportedFeatures].filter((item) => selectType.find((its) => item === its));
      if (sameArr.length > 0 && it.moduleType === 'driver') {
        t.push(it);
      }
    });
    return t;
  };

  /**
   * 选站监测站
   * @param {*} item
   * @returns
   */
  const handleChangeOnclick = (item, boolen = true) => {
    const val = selectItem;
    const { keep } = gtype;
    const arr = JSON.parse(JSON.stringify(groupedStations));
    const newItem = intoType(item);
    const r = keep.findIndex((it) => it.id === newItem.id);

    if (keep.length === 0 || (r === -1 && keep.length < 4)) {
      keep.push(newItem);
    } else {
      if (keep.length === 4 && r === -1) {
        message.info('最多选中4个监测站');
        return;
      }
      if (boolen) {
        keep.splice(
          keep.findIndex((it) => it.id === newItem.id),
          1,
        );
      }
    }
    arr.forEach((it) => {
      it.selectlist = keep;
    });

    setGroupedStations([...arr]);
    for (let i = 0; i < keep.length; i += 1) {
      if (keep[i]?.id === val?.id) {
        keep[i] = val;
        break;
      }
    }
    setGType({ keep: JSON.parse(JSON.stringify(keep)) });
    const i = alllist.current.findIndex((it) => it.id === keep[0]?.id);
    alllist.current.splice(i, 1);
  };
  /**
   * 显示选中站点下的设备
   */
  useEffect(() => {
    if (device) {
      const t = getDataType(device);
      /**
       *  默认+空闲=默认选中
       */
      for (let i = 0; i < t.length; i += 1) {
        if (t[0].moduleState === 'idle' && !deviceinfo.id) {
          setDeviceInfo(t[0]);
        }
        break;
      }
      setGType({ deviceList: t });
      onChangeApi(t);
    }
  }, [device]);

  /**
   * 站点 类型对应颜色
   * @param {*} s
   * @returns
   */
  const edgeState = {
    online: { name: '在线' },
    busy: { name: '在用', color: '#FFD118' },
    offline: { name: '离线', color: '#FFFFFF80' },
    fault: { name: '故障', color: '#FF4C2B' },
    disabled: { name: '禁用', color: '#787878' },
  };
  const handleChangeColor = (s) => {
    const t = getDataType(s);
    let idle = false;
    for (let i = 0; i < t.length; i += 1) {
      if (t[i].moduleState === 'idle') {
        idle = true;
        break;
      }
    }
    return idle ? null : edgeState[String(s.state)]?.color;
  };
  /**
   *
   * @param {设备分类} it
   */
  const addSelect = (it) => {
    const { keep } = gtype;
    // console.log(it);
    if (keep.length > 0) {
      const val = keep.find((i) => i.id === it.id);
      if (val) {
        SetSelectItem(val);
        return;
      }
    }
    const t = intoType(it);
    SetSelectItem(t);
  };

  const intoType = (it, boolen = true) => {
    const ar = it?.modules || [];
    const m = groupBy(ar, (item) => {
      return item.supportedFeatures; // 按照category进行分组
    });

    const t = { ffm: '中频测量', scan: '频段扫描', fdf: '单频侧向', aoa: '交汇定位' };
    // const stateCaptions = {
    //   none: { name: '未知', color: '#787878' },
    //   idle: { name: '空闲', color: '#35E065' },
    //   busy: { name: '忙碌', color: '#FFD118' },
    //   deviceBusy: { name: '占用', color: '#FFD118' },
    //   offline: { name: '离线', color: '#FFFFFF80' },
    //   fault: { name: '故障', color: '#FF4C2B' },
    //   disabled: { name: '禁用', color: '#787878' },
    // };
    const stateCaptions = ['deviceBusy', 'offline', 'fault', 'disabled'];

    Object.keys(t).forEach((item) => {
      const egidArr = [];
      m.forEach((its) => {
        if (its.name.indexOf(item) > -1) {
          its.value = t[item];
        }
        const options = [];
        const myMap = new Map();
        const narr3 = its.data.filter((i) => {
          return !myMap.has(i.id) && myMap.set(i.id, 1);
        });
        let b = '';
        narr3.forEach((i) => {
          // debugger;
          egidArr.push(i.id);
          b = i.id;
          options.push({
            value: stateCaptions.includes(i.moduleState) ? null : i.id,
            label: i.deviceName,
            disabled: stateCaptions.includes(i.moduleState),
          });
        });
        its.options = options;
        its.id = Math.random();
        its.code = egidArr.filter((i) => i === b).length === 1 ? (boolen ? b : null) : null;
      });
    });
    it.newItem = m;
    return it;
  };
  /**
   * 选择设备
   * @param {*} it
   */
  const selectTypeOnclick = (row, it, temp = null, boolen = true) => {
    const { keep } = gtype;
    const list = [...keep];
    const val = temp || selectItem;

    // const deid = val.modules.find((i) => i.id === it)?.deviceId;
    // for (let i = 0; i < list.length; i += 1) {
    //   const its = list[i];
    //   const l = its.newItem;
    //   for (let j = 0; j < l.length; j += 1) {
    //     const element = l[j];
    //     if (element.code) {
    //       const ss = its.modules.find((is) => is.id === it)?.deviceId;
    //       if (ss === deid && deid) {
    //         message.info('该设备只能选择一个功能');
    //         return;
    //       }
    //     }
    //   }
    // }

    // if (t.length > 0 && boolen) {
    //   message.info('该设备只能选择一个功能');
    //   return;
    // }
    val.newItem.forEach((item) => {
      if (item.value === row.value) {
        item.code = item.code === it ? null : it;
      }
    });
    if (list.length >= 0) {
      for (let i = 0; i < list.length; i += 1) {
        if (list[i].id === val.id) {
          list[i] = val;
        }
      }
    }

    for (let i = 0; i < list.length; i += 1) {
      const item = list[i];
      setCode(item.newItem, item);
    }
    setGType({ keep: [...list] });
    if (bool) {
      SetSelectItem(JSON.parse(JSON.stringify(val)));
    }
  };

  const setCode = (list, item) => {
    const m = [];
    list?.forEach((it) => {
      if (it.code) {
        const t = it.data.find((its) => its.id === it.code);
        const [k] = it.name;
        const json = {};
        json[k] = t.id;
        m.push(json);
      }
    });

    const i = alllist.current.findIndex((it) => it.id === item.id);
    if (i >= 0) {
      alllist.current[i].moduleIds = m;
    }
    return m;
  };
  return (
    <div className={styles.station_center_view}>
      <div
        id="mapStationSelect2"
        className={styles.station_center_view_map}
        // style={{ visibility: 'hidden' }}
      />
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
                    SetSelectItem(null);
                    setGType({
                      data: false,
                      selectList: ['stationaryCategory', 'mobileCategory', 'movableCategory'],
                      categoryArr: tempJson,
                      deviceshow: false,
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
              setGType({ rand: Math.random() });
            }}
          />
        </div>
        {/** 筛选类型 */}
        <div className={styles.station_center_view_left_type}>
          {Object.keys(categoryArr).map((it, i) => {
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
        <div className={styles.box} style={{ display: `${selectItem ? 'block' : 'none'}` }}>
          <div className={styles.box_head}>{selectItem?.name}</div>
          <div className={styles.box_list}>
            {selectItem?.newItem.map((item) => (
              <div
                className={classnames(styles.box_Items, {
                  // [styles.box_Items_none]: moduleState.find((e) => e === selectItem.state),
                })}
                key={`${item.id}_oo1`}
              >
                <div className={styles.box_Items_lableBox}>{item.value}</div>
                <div className={styles.box_Items_buttonsBox}>
                  {item.options.map((e) => (
                    <div
                      key={e.value}
                      onClick={() => {
                        !e.disabled && selectTypeOnclick(item, e.value);
                      }}
                      className={classnames(styles.box_Items_buttonsBox_buttons, {
                        [styles.box_Items_buttonsBox_buttonsAc]: item.code === e.value,
                        [styles.box_Items_buttonsBox_buttonsBan]: e.disabled,
                      })}
                    >
                      {e.label}
                    </div>
                  ))}

                  {/* <Radio
                      options={item.options}
                      value={item.code}
                      onChange={(e) => {
                        console.log(item);
                        selectTypeOnclick(item, e);
                      }}
                    /> */}
                </div>
              </div>
            ))}
          </div>
        </div>
        <div className={styles.station_center_view_left_list}>
          {groupedStations.map((gs) => {
            const { name, data, expand, area, code, selectlist } = gs;
            return (
              <div key={name}>
                <div
                  key={`${name}_1`}
                  className={classnames(styles.title, expand === true || expand === undefined ? styles.isExpand : null)}
                >
                  <div className={styles.title_left}>
                    <div style={{ marginRight: '8px' }}> {name}</div>
                    <Select
                      value={code}
                      onChange={(val) => {
                        groupedStations.forEach((it) => {
                          if (it.name === name) {
                            it.code = val;
                          }
                        });
                        SetSelectItem(null);
                        setGroupedStations(JSON.parse(JSON.stringify(groupedStations)));
                      }}
                    >
                      <Option value="">全部</Option>
                      {area.map((its) => (
                        <Option value={its.name * 1} key={its.name}>
                          {its.areaname}
                        </Option>
                      ))}
                    </Select>
                  </div>
                </div>
                <div
                  key={`${name}_2`}
                  style={expand === true || expand === undefined ? null : { height: 0, overflow: 'hidden' }}
                >
                  {(code !== ''
                    ? area.filter((it) => {
                        return it.name * 1 === code;
                      })[0].data
                    : data
                  ).map((s) => {
                    return (
                      <div
                        className={classnames(styles.featureItem_new, {
                          [styles.active]: s.id === selectItem?.id,
                          // [styles.ban]: s.state !== 'idle',
                          // [styles.disabled]: disabled,
                        })}
                        key={s.id}
                        id={s.id}
                        onClick={() => {
                          addSelect(s);
                        }}
                      >
                        <div
                          className={classnames(styles.info, {
                            [styles.infoactive]: gtype.keep?.filter((it) => it.id === s.id).length > 0,
                          })}
                        >
                          <div className={styles.ename}>
                            <div className={styles.infohead}>{s.name}</div>
                            <div className={styles.seticon}>
                              {s.modules.filter((it) => it.supportedFeatures.indexOf('ffm') > -1).length > 0 ? (
                                <BarIcon iconSize={14} color="var(--theme-station-text-color)" />
                              ) : null}
                              {s.modules.filter((it) => it.supportedFeatures.indexOf('ffdf') > -1).length > 0 ? (
                                <CompassIcon iconSize={14} color="var(--theme-station-text-color)" />
                              ) : null}
                            </div>
                          </div>
                          {s.isBetterEdge ? <div className={styles.info_fix}>荐</div> : null}
                          <div
                            className={styles.info_state}
                            style={{
                              background: handleChangeColor(s),
                            }}
                          />
                        </div>
                        <div
                          className={styles.info_af}
                          onClick={(e) => {
                            e.stopPropagation();
                            if (!moduleState.includes(s.state)) handleChangeOnclick(s);
                          }}
                        >
                          <GreenGoodIcon
                            iconSize={20}
                            color={
                              gtype.keep?.filter((it) => it.id === s.id).length > 0
                                ? '#3CE5D3'
                                : 'rgba(255, 255, 255, 0.2)'
                            }
                          />
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>
        <div className={styles.stations_foot}>
          <div
            onClick={(e) => {
              e.stopPropagation();
              onClose();
              // unsuccessful();
            }}
          >
            放弃
          </div>
          <div
            onClick={(e) => {
              e.stopPropagation();
              const { keep } = gtype;
              const res = [];
              keep.forEach((item) => {
                const m = setCode(item.newItem, item);
                res.push({ id: item.id, moduleIds: m });
              });

              const hash = {};
              const temp = res.concat(alllist.current).reduceRight((item, next) => {
                if (!hash[next.id]) hash[next.id] = true && item.push(next);
                return item;
              }, []);

              onSelect(temp || []);
            }}
          >
            确定
          </div>
        </div>
      </div>
    </div>
  );
};

StationSelectorLite.defaultProps = {
  onSelect: () => {},
  stations: [],
  selectEdgeId: {},
  mapOptions: {},
  onChangeApi: () => {},
  deviceDatas: [],
  selectType: [],
  onClose: () => {},
};

StationSelectorLite.propTypes = {
  onSelect: PropTypes.func,
  selectEdgeId: PropTypes.array,
  stations: PropTypes.array,
  mapOptions: PropTypes.object,
  onChangeApi: PropTypes.func,
  deviceDatas: PropTypes.array,
  selectType: PropTypes.array,
  onClose: PropTypes.func,
};

export default StationSelectorLite;

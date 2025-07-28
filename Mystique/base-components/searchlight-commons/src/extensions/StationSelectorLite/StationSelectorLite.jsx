/* eslint-disable max-len */
import React, { useEffect, useState, useRef, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import initDcMap from 'dc-map';
import { useUpdateEffect, useSetState, useUnmount } from 'ahooks';
import { Modal, Input, Select, Checkbox } from 'dui';
import { GreenGoodIcon } from 'dc-icon';
import StationIcons from '../../components/StationIcons';
// import { StationIcons } from 'searchlight-commons';
import styles from './lite.module.less';
import 'dc-map/dist/main.css';

const { Option } = Select;

const StationSelectorLite = (props) => {
  const {
    onSelect,
    selectEdgeId,
    stations,
    visible,
    disabled,
    mapOptions,
    deviceDatas,
    selectType,
    axios,
    moduleState,
    multiple,
    arrayEdgeId,
  } = props;

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
  const [edgeInfo, setEdgeInfo] = useState([]);
  const [numberid, setNumberid] = useSetState(0);
  let tempJson = null;
  const mydiv = useRef();
  const citys = useRef();
  const mapInstancestr = useRef();
  const first = useRef(0);
  const zoom = useRef(8);
  const oldData = useRef(null);
  const stationArry = useRef([]);
  const [stationArray, setStationArray] = useState([]);

  const [gtype, setGType] = useSetState({
    keep: {},
    index: null,
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
      all: {
        name: '只看空闲',
      },
    },
  });
  const { categoryArr } = gtype;
  const expandNames = useRef({}).current;
  tempJson = categoryArr;
  useLayoutEffect(() => {
    if (!mapInstance) return;
    mapInstance.mount();
    mapInstance.handleOAStationsClick('edgesStation', (currentEdge) => {
      const e = currentEdge.prop;
      try {
        mapInstance.removePopup('nodes_popup');
        mapInstance.drawPopup(
          { id: e.id, latitude: e.latitude, longitude: e.longitude },
          { popupText: e.name, offset: 50 },
          'nodes_popup',
        );
        setSelEid({ edgeId: e.id, featureId: null });
        setDevice(e);
        setGType({ station: e, deviceshow: true });
        autoChooseDevice(e);
      } catch (error) {
        console.log(error);
      }
      return currentEdge;
    });
  }, [mapInstance]);

  useLayoutEffect(() => {
    setGType({ deviceinfolist: deviceDatas });
  }, [deviceDatas]);

  useUpdateEffect(() => {
    if (!visible) {
      zoom.current = window.mstatiom?.map.getZoom();
      mapInstancestr.current = null;
      first.current = 0;
      return;
    }
    if (JSON.stringify(mapInstancestr.current) === JSON.stringify(mapOptions)) {
      return;
    }
    const options = {
      container: 'mapStationSelect',
      zoom: zoom.current,
      coordinate: 'WGS84',
      mapType: mapOptions?.mapType || 'amap',
      customUrl: mapOptions?.webMapUrl || 'http://192.168.102.31:6088',
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
    mapInstancestr.current = mapOptions;
  }, [mapOptions, visible]);

  useUpdateEffect(() => {
    if (mapInstance && citys.current) {
      setView(citys.current, device);
    }
  }, [mapInstance, gMapArr, device]);

  const onNameClick = (name) => {
    const find = expandNames[name];
    if (find === undefined || find === true) {
      expandNames[name] = false;
    } else {
      expandNames[name] = true;
    }
    setKey(key + 1);
  };

  const groupBy = (array, f) => {
    const groups = {};
    array.forEach((o) => {
      if (f(o)) {
        const group = JSON.stringify(f(o));
        groups[group] = groups[group] || [];
        groups[group].push(o);
      }
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
  }, [stations, gindex, visible, gtype.data, gtype.selectList, categoryArr, gtype.rand]);

  useEffect(() => {
    getDataCity([...gCitylist], false);
  }, [stations, key]);

  // useEffect(() => {
  //   getDataCity(gCity);
  // }, [numberid]);

  const getDataCity = (list, bool = true) => {
    const arrlist = [...list];
    setgCitylist(arrlist);
    if (arrlist[gindex] && visible) {
      // setTimeout(() => {
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
      citys.current = ar;
      setGType({ city: ar });
      for (let i = 0; i < ar.length; i += 1) {
        const show = ar[i].data.filter((item) => item.id === selEid?.edgeId);
        if (show.length > 0) {
          setGType({ deviceshow: true });
          break;
        }
      }

      if (bool) {
        setgapArr(s);
      }
      // }, 300);
    }
  };
  useLayoutEffect(() => {
    if (!selectEdgeId) return;
    if (selectEdgeId && first.current === 0 && citys.current) {
      /**
       * 城市下标、选择功能下标、选择功能列表
       */
      gCitylist.forEach((it, i) => {
        it?.data.forEach((item) => {
          if (item.id === selectEdgeId.edgeId) {
            setGindex(i);
            setDevice(item);
            setGType({ station: item, deviceshow: true });
            first.current += 1;
            item.modules.forEach((res) => {
              if (selectEdgeId.featureId === res.id) {
                setDeviceInfo(res);
                const des = { keep: { device: item, feature: res } };
                setGType(des);
                oldData.current = des;
              }
            });
            setSelEid(selectEdgeId);
            setNumberid(i + 1);
          }
        });
      });
    }
  }, [selectEdgeId, gCitylist, citys.current]);

  /**
   * 多选初始化
   */
  useEffect(() => {
    stationArry.current = arrayEdgeId;
    setStationArray(arrayEdgeId);
  }, [arrayEdgeId]);
  useEffect(() => {
    first.current = 0;
  }, [visible]);
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
            if (it.modules[i].moduleState === 'idle' && it.modules[i].moduleType === 'driver' && sameArr.length > 0) {
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

  const setView = (arr, val = null) => {
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
      newlist.forEach((it) => {
        json.push({
          ...it,
          status: it.state,
          edgeID: it.id,
          angle: 0,
          lineLength: 0,
          isMoveStation: it.type === 'mobile',
          selected: val?.id === it.id ? 'Y' : 'N',
          innerImage: StationIcons(it.type, it.state),
          prop: it,
          zIndex: val?.id === it.id ? 10 : 0,
          stationWidth: val?.id === it.id ? 24 : 18,
          stationHeight: val?.id === it.id ? 24 : 18,
          width: val?.id === it.id ? 70 : 60,
          height: val?.id === it.id ? 70 : 60,
          outLineColor: val?.id === it.id ? '#FF4C2B' : '',
          'icon-size': 1,
          'circle-size': 25,
          category: Number(it.category),
          textColor: 'white',
        });
      });
      if (mapInstance && json.length > 0 && json instanceof Array) {
        setTimeout(() => {
          try {
            mapInstance?.removePopup('nodes_popup');
            mapInstance?.drawOAStations && mapInstance?.drawOAStations(json, 'edgesStation');
            getPosition(json);
            // console.log(json, val);
            if (val) {
              mapInstance.setPosition([val.longitude, val.latitude], true);
              mapInstance.removePopup('nodes_popup');
              mapInstance.drawPopup(
                { id: val.id, latitude: val.latitude, longitude: val.longitude },
                { popupText: val.name, offset: 50 },
                'nodes_popup',
              );
            }
          } catch (error) {
            console.log(error);
          }
        }, 300);
      } else {
        mapInstance?.drawOAStations && mapInstance?.drawOAStations(json, 'edgesStation');
      }
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
  const stateCaptions = {
    none: { name: '未知', color: '#787878' },
    idle: { name: '空闲', color: '#35E065' },
    busy: { name: '在用', color: '#FFD118' },
    deviceBusy: { name: '在用', color: '#FFD118' },
    offline: { name: '离线', color: '#FFFFFF80' },
    fault: { name: '故障', color: '#FF4C2B' },
    disabled: { name: '禁用', color: '#787878' },
  };

  const edgeState = {
    online: { name: '在线' },
    busy: { name: '忙碌', color: '#FFD118' },
    offline: { name: '离线', color: '#FFFFFF80' },
    fault: { name: '故障', color: '#FF4C2B' },
    disabled: { name: '禁用', color: '#787878' },
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
    if (mydiv.current && visible) {
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
   * 根据配置显示对应的数据
   * @param {*} it
   * @returns
   */
  const handleView = (it) => {
    const newIt = { ...it };
    if (it.capability) {
      const [min, max, ifBandwidth] = it.capability.split('|');
      newIt.frequency = {
        minimum: min,
        maximum: max,
      };
      newIt.ifBandwidth = ifBandwidth;
    }
    return (
      <div
        className={classnames(styles.box_Items_right, {
          [styles.box_Items_head_ban]:
            newIt.moduleState === 'idle' || newIt.moduleState === 'busy' || newIt.moduleState === 'deviceBusy',
        })}
        key={`${newIt.id}`}
      >
        <div className={styles.box_Items_right_view}>
          <div style={{ width: '100%' }}>
            <div>
              频率范围：{newIt?.frequency?.minimum || ''}-{newIt?.frequency?.maximum || ''} MHz
            </div>
            <div>
              中频带宽：{newIt?.ifBandwidth > 1000 ? newIt?.ifBandwidth / 1000 : newIt?.ifBandwidth || ' '}
              {newIt?.ifBandwidth > 1000 ? 'MHz' : 'kHz'}
            </div>
          </div>
          {/* <div className={styles.box_Items_right_icon}>
            {it.moduleState === 'idle' || it.moduleState === 'busy' || it.moduleState === 'deviceBusy' ? (
              <GreenGoodIcon
                iconSize={20}
                color={
                  deviceinfo.id === it.id || it.id === gtype.keep?.feature?.id ? '#3CE5D3' : 'rgba(255, 255, 255, 0.2)'
                }
              />
            ) : (
              <></>
            )}
          </div> */}
        </div>
      </div>
    );
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
   * 站点 类型对应颜色
   * @param {*} s
   * @returns
   */
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
   * 显示选中站点下的设备
   */
  useEffect(() => {
    if (device) {
      const t = getDataType(device);
      /**
       *  默认+空闲=默认选中
       */
      const r = [];
      for (let i = 0; i < t.length; i += 1) {
        if (moduleState.find((is) => is === t[i].moduleState)) {
          try {
            axios(`/rmbt/device/getBusyInfo?deviceId=${t[i].deviceId}`).then((res) => {
              const str = res.result[0];
              r.push({ ...str, id: t[i].deviceId });
              setEdgeInfo(r);
            });
          } catch (error) {
            console.log(error);
          }
        }
      }
      setGType({ deviceList: t });
    }
  }, [device]);
  //  自动选择空闲或忙碌的设备
  const autoChooseDevice = (s) => {
    const devices = getDataType(s);
    const it = devices.find(
      (e) => e.moduleState === 'idle' || e.moduleState === 'deviceBusy' || e.moduleState === 'busy',
    );
    if (it && moduleState.find((items) => items === it.moduleState)) {
      setDeviceInfo(it);
      setGType({
        keep: { device: s, feature: it },
      });
      if (!multiple) return;
      const index = stationArry.current.findIndex((item) => (item?.id || item?.station) === it.edgeId);
      const arr = [...stationArry.current];
      if (index > -1) {
        if (stationArry.current[index].feature?.id === it.id) {
          arr.splice(index, 1);
        } else {
          const j = { ...s, feature: it };
          arr.splice(index, 1, j);
        }

        stationArry.current = arr;
        setStationArray(arr);
        return;
      }
      const j = { ...s, feature: it };
      arr.push(j);
      stationArry.current = arr;
      setStationArray(arr);
    }
  };
  const unsuccessful = () => {
    setSelEid(null);
    setDevice(null);
    if (multiple) {
      onSelect(arrayEdgeId);
      return;
    }
    if (oldData.current) {
      setGType(oldData.current);
      const { keep } = oldData.current;
      onSelect({
        station: keep?.device || null,
        feature: keep?.feature || null,
      });
      return;
    }
    onSelect({
      station: null,
      feature: null,
    });
    setGType({ keep: { device: null, feature: null } });
  };

  const distinct = (arr) => {
    const obj = {};
    const result = arr.reduce((pre, cur) => {
      if (!obj[cur.station]) {
        obj[cur.station] = true;
        return [...pre, cur];
      }
      return pre;
    }, []);
    return result;
  };

  return (
    <Modal
      visible={visible}
      title="选择监测站和通道"
      bodyStyle={{ padding: '0 0' }}
      style={{ width: '1560px' }}
      footer={null}
      onCancel={() => unsuccessful()}
    >
      <div className={styles.station_center_view}>
        <div
          id="mapStationSelect"
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
                      // setSelEid(null);
                      setDevice(null);
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
                      className={classnames(
                        i === gindex ? styles.station_center_view_left_city_item_active_line : null,
                      )}
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
                setSelEid(null);
                setDevice(null);
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
                      setGType({ data: bl, deviceshow: false });
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

          {selEid && device && gtype.deviceshow ? (
            <div className={styles.box}>
              <div className={styles.box_head}>
                <div className={styles.box_head_1}>
                  <div>{device.name}</div>
                  <div>{handleChangeTypeItem(device)}</div>
                </div>
                <div>IP地址：{device.ip}</div>
              </div>

              <div className={styles.box_view}>
                {gtype.deviceList?.map((it, i) => {
                  if (gtype.data && it.moduleState !== 'idle') {
                    return null;
                  }

                  return (
                    <div
                      key={it.id}
                      className={classnames(styles.box_Items, {
                        [styles.box_Items_active]: multiple
                          ? stationArray.find((e) => (e.feature?.id || e?.feature) === it.id)
                          : it.id === deviceinfo.id || it.id === gtype.keep?.feature?.id,

                        // [styles.disabled]: disabled,
                      })}
                      onClick={(e) => {
                        e.stopPropagation();
                        if (moduleState.find((items) => items === it.moduleState)) {
                          setDeviceInfo(it);
                          setGType({
                            keep: { device, feature: it },
                          });
                          if (!multiple) return;
                          const index = stationArry.current.findIndex(
                            (item) => (item?.id || item?.station) === it.edgeId,
                          );
                          const arr = [...stationArry.current];
                          if (index > -1) {
                            if (stationArry.current[index].feature?.id === it.id) {
                              arr.splice(index, 1);
                            } else {
                              const j = { ...device, feature: it };
                              arr.splice(index, 1, j);
                            }

                            stationArry.current = arr;
                            setStationArray(arr);
                            return;
                          }
                          const j = { ...device, feature: it };
                          arr.push(j);
                          stationArry.current = arr;
                          setStationArray(arr);
                        }
                      }}
                    >
                      {it.moduleState === 'busy' || it.moduleState === 'deviceBusy' ? (
                        <div className={styles.box_show_fix}>
                          <div className={styles.box_Items_show}>
                            <div>
                              功能：
                              <span>
                                {edgeInfo?.filter((item) => item.id === it.deviceId)[0]?.displayName || '未知'}
                              </span>
                            </div>
                            <div>
                              操作者：
                              <span>{edgeInfo?.filter((item) => item.id === it.deviceId)[0]?.username || '未知'}</span>
                            </div>
                          </div>
                        </div>
                      ) : null}

                      <div className={styles.box_Items_left}>
                        <div className={styles.box_Items_head}>
                          <div
                            className={classnames(styles.box_Items_head_number, {
                              [styles.box_Items_head_ban]: moduleState.find((items) => items === it.moduleState),
                            })}
                          >
                            {i + 1}
                          </div>
                          <div
                            className={styles.box_Items_head_state}
                            style={{
                              color: (stateCaptions[String(it.moduleState)] || stateCaptions.none).color,
                            }}
                          >
                            {(stateCaptions[String(it.moduleState)] || stateCaptions.none).name}
                          </div>
                        </div>
                        <div
                          className={classnames(styles.box_Items_head_foot, {
                            [styles.box_Items_head_ban]: moduleState.find((items) => items === it.moduleState),
                          })}
                        >
                          {it.deviceName || it.displayName}
                        </div>
                      </div>
                      {handleView(it)}
                    </div>
                  );
                })}
              </div>
            </div>
          ) : null}
          <div className={styles.station_center_view_left_list}>
            {groupedStations.map((gs) => {
              const { name, data, expand, area, code } = gs;
              return (
                <div key={name}>
                  <div
                    key={`${name}_1`}
                    className={classnames(
                      styles.title,
                      expand === true || expand === undefined ? styles.isExpand : null,
                    )}
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
                          setGroupedStations(JSON.parse(JSON.stringify(groupedStations)));
                          setGType({ deviceshow: false });
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
                      if (s.id === selEid?.edgeId) {
                        selDivRef.current = s.id;
                      }
                      return (
                        <div
                          className={classnames(styles.featureItem_new, {
                            [styles.active]: s.id * 1 === selEid?.edgeId * 1 || s.id === selEid?.edgeId,
                            // [styles.ban]: s.state !== 'idle',
                            // [styles.disabled]: disabled,
                            [styles.featureItem_new_ac]: multiple
                              ? stationArray.find((it) => (it?.id || it?.station) === s.id)
                              : gtype.keep?.device?.id === s.id,
                          })}
                          key={s.id}
                          id={s.id}
                          onClick={() => {
                            setSelEid({ edgeId: s.id, featureId: null });
                            setDevice(s);
                            setGType({ station: s, deviceshow: true });
                            setDeviceInfo({});
                            autoChooseDevice(s); //  自动选择空闲或忙碌的设备
                          }}
                        >
                          {/* {gtype.keep?.device?.id === s.id ? <GreenGoodIcon iconSize={26} color="#3CE5D3" /> : <></>} */}
                          <div className={styles.info}>
                            <div
                              className={classnames(styles.ename, {
                                [styles.ename_ac]: multiple
                                  ? stationArray.find((it) => (it?.id || it?.station) === s.id)
                                  : gtype.keep?.device?.id === s.id,
                              })}
                              style={{ fontWeight: `${gtype.keep?.device?.id === s.id ? 700 : 400}` }}
                            >
                              {s.name}
                            </div>
                            {/* <div
                              className={styles.info_state}
                              style={{
                                background: handleChangeColor(s),
                              }}
                            /> */}
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
                unsuccessful();
              }}
            >
              放弃
            </div>
            <div
              onClick={(e) => {
                e.stopPropagation();
                if (!multiple) {
                  const { feature } = gtype.keep;
                  oldData.current = { ...gtype.keep };
                  onSelect({
                    station: gtype.keep.device || null,
                    feature: feature || null,
                  });
                } else {
                  const arr = [];
                  const li = [...stationArry.current];
                  li.map((it) =>
                    arr.push({
                      station: it?.id || it?.station,
                      feature: it.feature?.id || it?.feature,
                    }),
                  );
                  const a = distinct(arr);
                  onSelect(a);
                }
              }}
            >
              确定
            </div>
          </div>
        </div>
      </div>
    </Modal>
  );
};

StationSelectorLite.defaultProps = {
  visible: false,
  disabled: false,
  onSelect: () => {},
  stations: [],
  selectEdgeId: {},
  mapOptions: {},
  deviceDatas: [],
  selectType: ['ffm'],
  moduleState: ['idle'],
  axios: () => {},
  multiple: false,
  arrayEdgeId: [],
};

StationSelectorLite.propTypes = {
  visible: PropTypes.bool,
  disabled: PropTypes.bool,
  onSelect: PropTypes.func,
  selectEdgeId: PropTypes.object,
  stations: PropTypes.array,
  mapOptions: PropTypes.object,
  deviceDatas: PropTypes.array,
  selectType: PropTypes.array,
  axios: PropTypes.func,
  moduleState: PropTypes.array,
  multiple: PropTypes.bool,
  arrayEdgeId: PropTypes.array,
};

export default StationSelectorLite;

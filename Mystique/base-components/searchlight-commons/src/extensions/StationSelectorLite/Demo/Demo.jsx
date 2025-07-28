/*
 * @Author: XYQ
 * @Date: 2022-01-24 18:09:27
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-12-08 15:31:40
 * @Description: file content
 */
import React, { useEffect, useState } from 'react';
import { Button } from 'dui';
import Axios from 'axios';
import res from './res';
import StationSelectorLite from '../index';

export default function StationViewDemo() {
  const [showSelector, setShowSelector] = useState(false);

  const [modules, setModules] = useState({ edgeId: '40005', featureId: '371678f0-5406-11ed-b38a-3ff40986c160' }); //
  const [listinfo, setListInfo] = useState([]);

  // const t = new Theme(Mode.light);
  // t.getMode();
  // t.getTheme();
  // t.setTheme();
  // t.switchTheme();

  // useEffect(() => {
  //   const moduleData = [];
  //   res.result.forEach((item) => {
  //     item.modules.forEach((d) => {
  //       if (d.moduleType === 'driver' && d.supportedFeatures.includes('ffm')) {
  //         moduleData.push({
  //           edgeId: d.edgeId,
  //           featureId: d.id,
  //           edgeName: item.name,
  //           featureName: d.displayName,
  //           parameters: d.parameters || null,
  //           state: d.moduleState,
  //           edgeType: item.type,
  //           edgeCategory: item.category, // 新增属性
  //           longitude: item.longitude,
  //           latitude: item.latitude,
  //           zone: item.zone,
  //           deviceName: d.deviceName,
  //         });
  //       }
  //     });
  //   });
  //   setModules(moduleData);
  // }, [showSelector]);
  // const getDeviceConf = (it) => {
  //   return axios({
  //     url: `/rmbt/edge/getFuncParams?id=${it.id}&edgeId=${it.edgeId}`,
  //     method: 'get',
  //   });
  // };
  // const stateCaptions = {
  //   none: { name: '未知', color: '#787878' },
  //   idle: { name: '空闲', color: '#35E065' },
  //   busy: { name: '忙碌', color: '#FFD118' },
  //   deviceBusy: { name: '占用', color: '#FFD118' },
  //   offline: { name: '离线', color: '#FFFFFF80' },
  //   fault: { name: '故障', color: '#FF4C2B' },
  //   disabled: { name: '禁用', color: '#787878' },
  // };
  useEffect(() => {
    console.log('%%%%%%', modules);
  }, [modules]);
  return (
    <div>
      <Button onClick={() => setShowSelector(true)}>选择功能</Button>
      <StationSelectorLite
        visible={showSelector}
        stations={res}
        mapOptions={{
          apiBaseUrl: '',
          mapType: 'amap',
          customUrl: undefined,
          fontUrl: undefined,
        }}
        selectType={['scan']}
        // multiple
        // moduleState={['offline', 'idle']}
        selectEdgeId={modules} // 单站
        // arrayEdgeId={listinfo} // 多选站
        // deviceDatas={listinfo}
        onSelect={(x) => {
          console.log('--->', x);
          // setListInfo(x);
          setModules({ edgeId: x.station?.id || null, featureId: x.feature?.id || null });
          setShowSelector(false);
        }}
        // onChangeApi={(x) => {
        //   const t = [];
        //   x.forEach((it) => {
        //     // getDeviceConf({ id: it.id, edgeId: it.edgeId }).then((res) => {
        //     //   t.push(res.result);
        //     // });
        //     Axios.get(`http://192.168.102.16:11001/rmbt/device/getBusyInfo?=${it}`).then((e) => {
        //       console.log(e);
        //       t.push(e);
        //     });
        //   });
        //   Promise.all(t).then((e) => {
        //     let temp = [];
        //     if (e instanceof Array) {
        //       temp = e.map((r) => r.result);
        //     }
        //     setListInfo(temp);
        //   });
        // }}
      />
    </div>
  );
}

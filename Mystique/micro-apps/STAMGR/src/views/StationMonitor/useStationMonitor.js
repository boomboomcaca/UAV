import { useState, useEffect } from 'react';
import { getEdgesList, getList } from '@/api/cloud';
import useDictionary from '@/hooks/useDictionary';
import { getState, stationType, stationaryCategory, mobileCategory, fmAddrType } from '@/hooks/dictKeys';
import { edgeUrl } from '@/api/path';

const defaultParams = {
  page: 1,
  rows: 1000,
  sort: 'desc',
  order: 'createTime',
};

const getModules = (modules, filter) => {
  if (modules) {
    return modules.filter((sm) => {
      return sm.moduleType === filter;
    });
  }
  return null;
};

function useStationMonitor(edgeID) {
  const [key, setKey] = useState(0);

  const [station, setStation] = useState(0);

  const [devices, setDevices] = useState([]);

  const [drivers, setDrivers] = useState([]);

  const { dictionary, getDictValue } = useDictionary();

  const getTudeNum = (tude) => {
    if (typeof tude === 'number') {
      const num = Number(tude?.toFixed(6));
      return num || '--';
    }
    return tude || '--';
  };

  useEffect(() => {
    if (dictionary) {
      const pros = [];
      pros.push(getEdgesList());
      pros.push(getList(edgeUrl, { ...defaultParams, id: edgeID }));
      Promise.allSettled(pros).then((res) => {
        const [res1, res2] = res;
        const { result: egs } = res1.value;
        const { result } = res2.value;
        if (result) {
          const ret = result.map((r) => {
            const find = egs?.find((e) => {
              return e.id === r.id;
            });
            return {
              ...r,
              categoryStr: getDictValue(
                dictionary,
                r.type === 'mobileCategory'
                  ? mobileCategory
                  : r.type === 'stationaryCategory'
                  ? stationaryCategory
                  : stationType,
                r.type === 'mobileCategory' || r.type === 'stationaryCategory' ? r.category : r.type,
              ),
              fmaddrtypeStr: getDictValue(dictionary, fmAddrType, r.fmaddrtype),
              typeStr: getDictValue(dictionary, stationType, r.type),
              state: find?.state,
              stateStr: getState(find?.state).tag,
              stateColor: getState(find?.state).color,
              modules: find?.modules,
              location: `经度:${getTudeNum(find?.longitude)} ° 纬度:${getTudeNum(find?.latitude)} °`,
              altitudeStr: `${r.altitude} m`,
            };
          });
          setStation(ret[0]);
          setDevices(getModules(ret[0].modules, 'device'));
          setDrivers(getModules(ret[0].modules, 'driver'));
        }
      });
    }
  }, [key, dictionary]);

  return { key, setKey, station, devices, drivers };
}

export default useStationMonitor;

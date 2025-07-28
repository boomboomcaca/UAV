import { useState, useEffect } from 'react';
import { getDictionary } from '@/api/cloud';
import {
  stationType,
  stationaryCategory,
  mobileCategory,
  movableCategory,
  sensorCategory,
  portableCategory,
  airCategory,
  mcsType,
  fmAddrType,
} from './dictKeys';

const getDictValue = (dict, type, key) => {
  if (dict) {
    const find = dict.find((d) => {
      return d.dicNo === type;
    });
    if (find) {
      const has = find.data.find((f) => {
        return f.key === key;
      });
      if (has) {
        return has.value;
      }
    }
  }
  return null;
};

function useDictionary(keys = null) {
  const [dictionary, setDictionary] = useState(null);

  useEffect(() => {
    const dicts = keys || [
      stationType,
      stationaryCategory,
      mobileCategory,
      movableCategory,
      sensorCategory,
      portableCategory,
      airCategory,
      mcsType,
      fmAddrType,
    ];
    getDictionary(dicts).then((res) => {
      console.log('get dicts;;;;;', res);
      setDictionary(res.result);
    });
  }, []);

  return { dictionary, getDictValue };
}

export default useDictionary;

/* eslint-disable no-param-reassign */
import { useState, useEffect, useRef } from 'react';
import { getList } from '@/api/cloud';
import { deviceUrl } from '../api/path';

function useDeviceListLite(edgeId, filter = null, reverse = false, more = null) {
  const [devices, setDevices] = useState([]);

  const defaultParams = useRef({
    page: 1,
    rows: 1000,
    sort: 'desc',
    order: 'displayName',
    moduleType: 'device',
  }).current;

  useEffect(() => {
    getList(deviceUrl, { ...defaultParams, edgeId }).then((res) => {
      const { result } = res;
      if (filter) {
        const rf = result.filter((r) => {
          const bo1 = r.moduleCategory.includes(filter);
          const bo2 = reverse ? !bo1 : bo1;
          if (more) {
            const bo3 = r.model === more.model;
            return bo2 && more.reverse ? !bo3 : bo3;
          }
          return bo2;
        });
        setDevices(rf);
      } else {
        setDevices(result);
      }
    });
  }, []);

  return { devices };
}

export default useDeviceListLite;

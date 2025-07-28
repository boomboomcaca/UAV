/* eslint-disable no-param-reassign */
import { useState, useEffect } from 'react';
import { Modal } from 'dui';
import { getList, del, update } from '@/api/cloud';
import useDictionary from '@/hooks/useDictionary';
import { deviceUrl } from '../api/path';

const defaultParams = {
  page: 1,
  rows: 1000,
  sort: 'desc',
  // order: 'createTime',
  order: 'displayName',
  moduleType: 'device',
};

function useDeviceList(edgeID, filter = null, reverse = false) {
  const [key, setKey] = useState(0);

  const { dictionary } = useDictionary(['moduleCategory']);

  const [params, setParams] = useState(defaultParams);

  const [devices, setDevices] = useState([]);

  const [selectDevices, setSelectDevices] = useState([]);

  const updateSelectDevices = (item) => {
    const find = selectDevices?.find((s) => {
      return s.id === item.id;
    });
    if (find) {
      const idx = selectDevices.indexOf(find);
      selectDevices.splice(idx, 1);
      setSelectDevices([...selectDevices]);
    } else {
      setSelectDevices([...selectDevices, item]);
    }
  };

  const hasSelected = (id) => {
    const find = selectDevices?.find((s) => {
      return s.id === id;
    });
    if (find) return true;
    return false;
  };

  const refresh = () => {
    setKey(key + 1);
  };

  const updateParams = (param) => {
    const ps = {};
    Object.keys(param).forEach((k) => {
      if (param[k] !== null) {
        ps[k] = param[k];
      }
    });
    setParams({ ...defaultParams, ...ps });
  };

  const onDelete = () => {
    if (selectDevices && selectDevices.length > 0) {
      Modal.confirm({
        title: '监测设备删除',
        closable: false,
        content: '确定要删除这些监测设备吗？',
        onOk: () => {
          const pro = [];
          selectDevices.forEach((s) => {
            pro.push(del(deviceUrl, { id: s.id }));
          });
          Promise.allSettled(pro).then((res) => {
            window.console.log(res);
            refresh();
            setSelectDevices([]);
          });
        },
      });
    }
  };

  const onActive = (device) => {
    const param = {
      id: device.id,
      edgeId: edgeID,
      moduleState: device.moduleState === 'disabled' ? 'idle' : 'disabled',
    };
    update(deviceUrl, param).then((res) => {
      if (res.result) {
        const find = devices.find((d) => {
          return d.id === device.id;
        });
        if (find) {
          find.moduleState = param.moduleState;
        }
        setDevices([...devices]);
      }
    });
  };

  useEffect(() => {
    if (dictionary) {
      getList(deviceUrl, { ...params, edgeId: edgeID }).then((res) => {
        const { result } = res;
        if (dictionary) {
          result.forEach((r) => {
            const filts = dictionary[0]?.data
              ?.filter((d) => {
                return r.moduleCategory.includes(d.key);
              })
              ?.map((m) => {
                return m.value;
              });
            r.moduleCategoryStr = filts?.join(',') || '未知';
            const find1 = r.parameters?.find((p) => {
              return p.name === 'ipAddress';
            });
            const find2 = r.parameters?.find((p) => {
              return p.name === 'port';
            });
            r.iport = `${find1?.value || '--'}:${find2?.value || '--'}`;
          });
        }
        if (filter) {
          const rf = result.filter((r) => {
            const bo = r.moduleCategory.includes(filter);
            return reverse ? !bo : bo;
          });
          setDevices(rf);
        } else {
          setDevices(result);
        }
      });
    }
  }, [key, params, dictionary]);

  return { refresh, updateParams, devices, selectDevices, updateSelectDevices, hasSelected, onDelete, onActive };
}

export default useDeviceList;

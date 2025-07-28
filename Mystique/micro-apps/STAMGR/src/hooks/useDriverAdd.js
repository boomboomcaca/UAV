/* eslint-disable no-param-reassign */

import { useState, useEffect } from 'react';
import { message } from 'dui';
import { createTimeID } from '@/utils/random';
import { getTemplate } from '@/api/template';
import { add, update } from '@/api/cloud';
import { driverUrl } from '../api/path';

// TODO error type:add,mod,none

const defaultID = '00000000-0000-0000-0000-000000000000';

function useDriverAdd(edgeID) {
  const [tempVisible, setTempVisible] = useState(false);

  const [drivers, setDrivers] = useState([]);

  const [select, setSelect] = useState(null);

  const [loading, setLoading] = useState(false);

  const onDelete = (item) => {
    const find = drivers.find((d) => {
      return d.rowKey === item.rowKey;
    });
    if (find) {
      const idx = drivers.indexOf(find);
      drivers.splice(idx, 1);
      setDrivers([...drivers]);
      if (select.rowKey === find.rowKey) {
        setSelect(null);
      }
    }
  };

  const onAdd = (temps) => {
    const newds = [
      ...drivers,
      ...temps.map((t) => {
        return { ...t, rowKey: `${t.id}-${createTimeID()}` };
      }),
    ];
    setDrivers(newds);
  };

  const onSelect = async (device) => {
    const find = drivers.find((d) => {
      return d.rowKey === device.rowKey;
    });
    if (find) {
      if (!find.template) {
        setLoading(true);
        const ret = await getTemplate(device.id);
        if (ret.result) {
          find.template = ret.result.template;
        }
        setLoading(false);
      }
      find.template.moduleCategoryStr = find.moduleCategoryStr;
      setSelect(find);
    }
  };

  const onSelectChange = (args) => {
    const { key, value } = args;
    const find = drivers.find((d) => {
      return d.rowKey === select.rowKey;
    });
    if (find) {
      const temp = { ...find.template };
      temp[key] = value;
      select.template = temp;
      find.template = temp;
      setSelect({ ...select });
    }
  };

  const onSave = async (action) => {
    // TODO before save
    const prosBefore = [];
    const temp = [];
    drivers.forEach((d) => {
      if (d.template === null) {
        temp.push(d);
        prosBefore.push(getTemplate(d.id));
      }
    });
    if (prosBefore.length > 0) {
      const res = await Promise.allSettled(prosBefore);
      res.forEach((r, i) => {
        const { status, value, reason } = r;
        if (status === 'fulfilled' && value.result) {
          temp[i].template = value.result.template;
        }
        // if (r.error) {
        //   temp[i].error = true;
        // }
      });
    }

    // TODO ok to save,error to wait
    let hasError = false;
    const newds = drivers.map((d) => {
      if (d.template) {
        if (d.template.displayName === '') {
          d.error = '未设置名称';
          hasError = true;
        }
      } else {
        d.error = '没有模板';
        hasError = true;
      }
      return d;
    });

    if (hasError) {
      setDrivers(newds);
    } else {
      const pros = [];
      drivers.forEach((d) => {
        const device = { ...d.template };
        // TODO search moduleCategoryStr , then put in=> []
        device.edgeId = edgeID;
        device.templateId = d.id;
        delete device.moduleCategoryStr;
        if (d.error === false || device.id !== defaultID) {
          const ud = {
            id: device.id,
            displayName: device.displayName,
            description: device.description,
            parameters: device.parameters,
            edgeId: edgeID,
          };
          pros.push(update(driverUrl, ud));
        } else {
          delete device.id;
          device.constraintScript = device.constraintScript !== '' ? JSON.stringify(device.constraintScript) : '';
          pros.push(add(driverUrl, device));
        }
      });
      Promise.allSettled(pros).then((res) => {
        window.console.log(res);
        let hasErr = false;
        res.forEach((r, i) => {
          const { status, value, reason } = r;
          if (status === 'rejected') {
            hasErr = true;
            drivers[i].error = reason;
          }
          if (status === 'fulfilled' && value.result) {
            drivers[i].error = false;
            if (Array.isArray(value.result)) {
              const [id] = value.result;
              drivers[i].template.id = id;
            }
          }
        });
        if (hasErr) {
          message.error({ key: 'tip', content: '部分设备添加失败,需要重新设置这些设备！' });
        }
        action?.(hasErr === false);
        setDrivers([...drivers]);
      });
    }
  };

  useEffect(() => {}, []);

  return { loading, tempVisible, setTempVisible, drivers, onDelete, onAdd, select, onSelect, onSelectChange, onSave };
}

export default useDriverAdd;

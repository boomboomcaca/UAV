/* eslint-disable no-param-reassign */

import { useState, useEffect } from 'react';
import { update } from '@/api/cloud';
import { getDeviceParam } from '@/api/template';
import useDictionary from '@/hooks/useDictionary';
import { driverUrl } from '../api/path';

function useDriverMod(device) {
  const { dictionary } = useDictionary(['moduleCategory']);

  const [loading, setLoading] = useState(false);

  const [select, setSelect] = useState(null);

  useEffect(() => {
    if (device && dictionary) {
      getDeviceParam(device.id).then((res) => {
        window.console.log(res);
        const d = res.result;
        if (dictionary) {
          const mcates = [
            ...dictionary[0].data.map((dd) => {
              dd.templates = [];
              return dd;
            }),
          ];
          if (mcates) {
            const filter = mcates.filter((m) => {
              return d.moduleCategory.includes(m.key);
            });
            if (filter) {
              d.moduleCategoryStr = filter
                .map((f) => {
                  return f.value;
                })
                .join(',');
            }
          }
        }
        setSelect({ ...device, ...d });
      });
    } else {
      setSelect(null);
    }
  }, [device, dictionary]);

  const onSelectChange = (args) => {
    const { key, value } = args;
    if (select) {
      select[key] = value;
      setSelect({ ...select });
    }
  };

  const onSave = async (action) => {
    setLoading(true);
    const ud = {
      id: select.id,
      displayName: select.displayName,
      description: select.description,
      parameters: select.parameters,
      edgeId: select.edgeId,
    };
    console.log('save driver::::', ud);
    update(driverUrl, ud).then((res) => {
      window.console.log(res);
      action?.(!!res.result);
    });
  };

  return { loading, select, onSave, onSelectChange };
}

export default useDriverMod;

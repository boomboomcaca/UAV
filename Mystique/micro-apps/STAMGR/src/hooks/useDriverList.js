import { useState, useEffect } from 'react';
import { message, Modal } from 'dui';
import { getList, del, update } from '@/api/cloud';
import { getDeviceParam } from '@/api/template';
import { driverUrl } from '../api/path';

const defaultParams = {
  page: 1,
  rows: 1000,
  sort: 'desc',
  // order: 'createTime',
  order: 'displayName',
  moduleType: 'driver',
};

const defaultID = '00000000-0000-0000-0000-000000000000';

function useDriverList(edgeID) {
  const [key, setKey] = useState(0);

  const [params, setParams] = useState(defaultParams);

  const [drivers, setDrivers] = useState([]);

  const [selectDrivers, setSelectDrivers] = useState([]);

  const updateSelectDrivers = (item) => {
    const find = selectDrivers?.find((s) => {
      return s.id === item.id;
    });
    if (find) {
      const idx = selectDrivers.indexOf(find);
      selectDrivers.splice(idx, 1);
      setSelectDrivers([...selectDrivers]);
    } else {
      setSelectDrivers([...selectDrivers, item]);
    }
  };

  const hasSelected = (id) => {
    const find = selectDrivers?.find((s) => {
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

  const getAntennaParams = (atennaParams) => {
    const find = atennaParams.find((ap) => {
      return ap.name === 'antennaSelectionMode';
    });

    if (find) {
      // TODO warning 固定为manual
      find.value = 'manual';

      if (find.value === 'auto') {
        return atennaParams.filter((ap) => {
          return ap.isInstallation || ap.name === 'isActive';
        });
      }
      if (find.value === 'manual') {
        return atennaParams.filter((ap) => {
          return ap.name === 'antennaID' || ap.isInstallation || ap.name === 'isActive';
        });
      }
      if (find.value === 'polarization') {
        return atennaParams.filter((ap) => {
          return ap.name === 'polarization' || ap.isInstallation || ap.name === 'isActive';
        });
      }
    }
    return atennaParams.filter((ap) => {
      return ap.isInstallation || ap.name === 'isActive';
    });
  };

  const cullingAntennaParams = (parameters) => {
    const find = parameters.find((ap) => {
      return ap.name === 'antennaSelectionMode';
    });
    if (find) {
      if (find.value === 'manual') {
        return parameters.filter((ap) => {
          return ap.name !== 'polarization';
        });
      }
      if (find.value === 'polarization') {
        return parameters.filter((ap) => {
          return ap.name !== 'antennaID';
        });
      }
    }
    return parameters.filter((ap) => {
      return ap.name !== 'antennaID' && ap.name !== 'polarization';
    });
  };

  const onDelete = () => {
    if (selectDrivers && selectDrivers.length > 0) {
      Modal.confirm({
        title: '监测功能删除',
        closable: false,
        content: '确定要删除这些监测功能吗？',
        onOk: () => {
          const pro = [];
          selectDrivers.forEach((s) => {
            pro.push(del(driverUrl, { id: s.id }));
          });
          Promise.allSettled(pro).then((res) => {
            window.console.log(res);
            refresh();
            setSelectDrivers([]);
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
    update(driverUrl, param).then((res) => {
      if (res.result) {
        const find = drivers.find((d) => {
          return d.id === device.id;
        });
        if (find) {
          find.moduleState = param.moduleState;
        }
        setDrivers([...drivers]);
      }
    });
  };

  const onInitDriver = (item) => {
    const find = drivers.find((d) => {
      return d.id === item.id;
    });
    if (find) {
      find.parameters = item.parameters;
    }
  };

  const unionclude = (arr1, arr2) => {
    if (arr1 && arr2 && arr1.length > 0 && arr2.length > 0) {
      for (let i = 0; i < arr1.length; i += 1) {
        const e = arr1[i];
        const find = arr2.find((a) => {
          return a === e;
        });
        if (find) {
          return true;
        }
      }
    }
    return false;
  };

  // TODO ？？？ 合并参数
  const onDriverRelative = async (item, param) => {
    const find = drivers.find((d) => {
      return d.id === item.id;
    });
    window.console.log('=============f', find);
    if (find && find.parameters) {
      // TODO 关联设备为主设备信息
      const p = find.parameters.find((x) => {
        return x.name === param.name;
      });
      window.console.log('=============p', p);
      if (p) {
        const old = p.value;
        p.value = param.value;
        const res = await getDeviceParam(param.value);
        if (res.result) {
          const originParams = [...find.parameters];
          let attachParams = res.result.parameters.filter((rrp) => {
            window.console.log(rrp.supportedFeatures, p.needFeature, unionclude(rrp.supportedFeatures, p.needFeature));
            return (
              // !rrp.isInstallation &&
              (rrp.name !== 'antennaSet' &&
                // (rrp.supportedFeatures.includes(item.supportedFeatures[0]) || rrp.supportedFeatures[0] === 'none')
                // rrp.supportedFeatures.includes(p.needFeature[0]) /* || rrp.supportedFeatures[0] === 'none' */
                unionclude(rrp.supportedFeatures, p.needFeature)) ||
              rrp.name === 'isActive'
            );
          });

          if (param.name === 'antennaController') {
            attachParams = getAntennaParams(attachParams);
            const aset = res.result.parameters.find((rrp) => {
              return rrp.name === 'antennaSet';
            });
            const as = attachParams.find((apf) => {
              return apf.name === 'antennas';
            });
            const aid = attachParams.find((apf) => {
              return apf.name === 'antennaID';
            });
            if (aset && as) {
              const set = [...aset.parameters];
              as.parameters = set;
              const a00 = as.parameters.find((app) => {
                return app.id === defaultID;
              });
              if (!a00) {
                as.parameters.unshift({ id: defaultID, displayName: 'autoAntenna' });
              }

              const values = [];
              const displayValues = [];
              set.forEach((s) => {
                if (s.id !== defaultID) {
                  values.push(s.id);
                  displayValues.push(s.displayName || s.name);
                }
              });
              aid.values = values;
              aid.displayValues = displayValues;
              if (aid.value === defaultID || !values.includes(aid.value)) {
                aid.value = values?.[0] || defaultID;
              }
            }
          }

          let originParamsAfter = [];

          if (old) {
            originParams.forEach((op) => {
              const idx = op.owners.indexOf(old);
              if (idx > -1) {
                op.owners.splice(idx, 1);
                if (op.owners.length > 0) {
                  originParamsAfter.push(op);
                }
              } else {
                originParamsAfter.push(op);
              }
            });
          } else {
            originParamsAfter = originParams;
          }

          attachParams.forEach((ap) => {
            const has = originParamsAfter.find((opa) => {
              return opa.name === ap.name;
            });
            if (has) {
              if (typeof p.value === 'string' && !has.owners.includes(p.value)) {
                has.owners.push(p.value);
              } else {
                p.value.forEach((v) => {
                  if (!has.owners.includes(v)) has.owners.push(v);
                });
              }

              // has.owners = has.owners.contact(p.value);
            } else {
              // ap.owners.push(p.value);
              if (typeof p.value === 'string' && !ap.owners.includes(p.value)) {
                ap.owners.push(p.value);
              } else {
                p.value.forEach((v) => {
                  if (!ap.owners.includes(v)) ap.owners.push(v);
                });
              }

              // ap.owners = ap.owners.contact(p.value);
              originParamsAfter.push(ap);
            }
          });

          let { constraintScript } = find || {};
          if (constraintScript === '') {
            constraintScript = {};
          } else {
            constraintScript = JSON.parse(constraintScript);
          }
          if (param && param.oldValue) {
            delete constraintScript[param.oldValue];
          }
          if (param && param.value && res.result.constraintScript !== '') {
            constraintScript[param.value] = JSON.parse(res.result.constraintScript);
          }
          window.console.log('constraintScript--->', constraintScript);
          const up = {
            id: find.id,
            edgeId: edgeID,
            parameters: originParamsAfter,
            deviceId: p.isPrimaryDevice ? param.value || '' : find.deviceId,
            deviceName: p.isPrimaryDevice ? param.deviceName || '' : find.deviceName,
            capability: p.isPrimaryDevice ? param.capability || '' : find.capability,
            constraintScript: JSON.stringify(constraintScript),
          };
          console.log('update feature settings:::', up);
          const r = await update(driverUrl, up);
          if (r.result) {
            refresh();
            message.success({ key: 'tip', content: '已关联设备' });
            return;
          }
        }
      }
    }
    message.error({ key: 'tip', content: '未成功关联设备' });
  };

  const onDriverAntenna = async (item, antParams) => {
    window.console.log(item);
    window.console.log(antParams);
    const drv = drivers.find((d) => {
      return d.id === item.id;
    });
    const { parameters } = drv;
    antParams.forEach((ap) => {
      if (ap.name === 'antennas') {
        const a00 = ap.parameters.find((app) => {
          return app.id === defaultID;
        });
        if (!a00) {
          ap.parameters.unshift({ id: defaultID, displayName: 'autoAntenna' });
        }
      }
      if (ap.name === 'antennaID') {
        if (!ap.values?.includes(ap.value)) {
          // eslint-disable-next-line no-param-reassign
          ap.value = ap.values?.[0] || defaultID;
        }
      }
      const find = parameters.find((p) => {
        return ap.name === p.name;
      });
      if (find) {
        const idx = parameters.indexOf(find);
        parameters.splice(idx, 1, { ...find, ...ap });
      } else {
        parameters.push({ ...ap });
      }
    });

    const up = {
      id: item.id,
      edgeId: edgeID,
      parameters: cullingAntennaParams(parameters),
    };
    const r = await update(driverUrl, up);
    if (r.result) {
      refresh();
      message.success({ key: 'tip', content: '天线相关参数更新成功' });
    }
  };

  useEffect(() => {
    getList(driverUrl, { ...params, edgeId: edgeID }).then((res) => {
      const { result } = res;
      setDrivers(result);
    });
  }, [key, params]);

  return {
    refresh,
    updateParams,
    drivers,
    selectDrivers,
    updateSelectDrivers,
    hasSelected,
    onDelete,
    onActive,
    onInitDriver,
    onDriverRelative,
    onDriverAntenna,
  };
}

export default useDriverList;

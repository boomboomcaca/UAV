import React, { useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Switch, Loading, Modal, Select, MultipleSelect } from 'dui';
import { ParamSettingsIcon, RoundInfoIcon } from 'dc-icon';
import { getDeviceParam } from '@/api/template';
import Check from '@/components/Check';
// import Select from '@/components/Select';
import icons from './icons';
import ShowAntenna from './ShowAntenna.jsx';
import styles from './index.module.less';

const { Option } = Select;
const { Option: MOption } = MultipleSelect;

const getDriverIcon = (driver) => {
  let src = icons.none;
  if (driver && driver.supportedFeatures)
    if (driver.supportedFeatures.length > 0) {
      const p = driver.supportedFeatures[0];
      if (icons[p]) {
        src = icons[p];
      }
    }
  return <img style={{ width: '100%', height: '100%' }} alt="" src={src} />;
};

const Item = (props) => {
  const { className, item, devices, checked, onChange } = props;

  const antennaParams = useRef([]).current;

  const [parameters, setParameters] = useState(null);

  const [showAntenna, setShowAntenna] = useState(false);
  const [antennaID, setAntennaID] = useState(null);

  const getDevicesParams = (params) => {
    const filters = params?.filter((p) => {
      return Array.isArray(p.needModuleCategory) && p.needModuleCategory.length > 0 && p.isInstallation;
    });
    return filters.sort((f) => {
      return !f.isPrimaryDevice ? 1 : -1;
    });
  };

  const hasIntersection = (arr1, arr2) => {
    let flag = false;
    if (arr1.length > 0 && arr2.length > 0) {
      arr1.forEach((a1) => {
        if (arr2.includes(a1)) {
          flag = true;
        }
      });
    }
    return flag;
  };
  // hasIntersection(rrp.supportedFeatures || [], p.needFeature || [])

  // TODO ？？？ 获取设备
  const getDevicesByCategory = (devs, param) => {
    let ds = null;

    const [cate] = param.needModuleCategory;
    const [feat] = param.needFeature;

    if (feat && feat !== 'none' && cate && cate !== 'none') {
      ds = devs.filter((d) => {
        return (
          hasIntersection(d.supportedFeatures || [], param.needFeature || []) &&
          hasIntersection(d.moduleCategory || [], param.needModuleCategory || [])
        );
      });
    } else if (cate && cate !== 'none') {
      ds = devs.filter((d) => {
        return hasIntersection(d.moduleCategory || [], param.needModuleCategory || []);
      });
    }

    return ds;
  };

  useEffect(() => {
    if (item && item.parameters === null) {
      getDeviceParam(item.id).then((res) => {
        if (res.result) {
          setParameters(res.result.parameters);
          onChange({ tag: 'init', item: res.result });
        }
      });
    } else {
      setParameters(item.parameters);
    }
  }, [item]);

  const onEditAntenna = (param) => {
    // window.console.log(param);
    if (param.value && param.value !== '') {
      setShowAntenna(true);
      setAntennaID(param.value);
    }
  };

  const onModalOK = () => {
    setShowAntenna(false);
    onChange({ tag: 'antenna', item, params: antennaParams });
  };

  const onAntennaParamChanged = (p) => {
    const find = antennaParams.find((ap) => {
      return ap.name === p.name;
    });
    if (find) {
      const idx = antennaParams.indexOf(find);
      antennaParams.splice(idx, 1);
    }
    antennaParams.push(p);
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.operate}>
        <Check
          checked={checked}
          onCheck={() => {
            onChange({ tag: 'check', item });
          }}
        />
        <Switch
          selected={item.moduleState !== 'disabled'}
          onChange={() => {
            onChange({ tag: 'switch', item });
          }}
        />
      </div>
      <div className={styles.title}>
        <div className={styles.icon}>{getDriverIcon(item)}</div>
        <div className={styles.name}>
          <div>{item?.displayName}</div>
          <div>{item?.version}</div>
        </div>
        <ParamSettingsIcon
          style={{ opacity: 0.5, cursor: 'pointer' }}
          onClick={() => {
            onChange({ tag: 'param', item });
          }}
        />
      </div>
      <div className={styles.info}>
        <div className={styles.infotitle}>绑定设备</div>
        <div className={styles.infolist}>
          {parameters === null ? (
            <Loading loadingMsg="加载相关参数中..." className={styles.loading} />
          ) : (
            getDevicesParams(parameters)?.map((pa, idx) => {
              // console.log('papapa :::', item.displayName, pa.displayName, pa);

              return (
                <div className={styles.infoitem}>
                  <div title={pa.displayName}>{pa.displayName}</div>
                  <div>
                    {pa.isPrimaryDevice || pa.name === 'antennaController' || pa.type !== 'list' ? (
                      <Select
                        value={pa.value}
                        onChange={(e) => {
                          // window.console.log('=============e', e, devices);
                          const dev = devices.find((d) => {
                            return d.id === e;
                          });
                          onChange({
                            tag: 'relative',
                            item,
                            param: {
                              name: pa.name,
                              value: e,
                              deviceName: idx === 0 ? dev?.displayName || null : null,
                              capability: idx === 0 ? dev?.capability || null : null,
                              oldValue: pa?.value || null,
                            },
                          });
                        }}
                      >
                        {getDevicesByCategory(devices, pa)?.map((d) => (
                          <Option value={d.id} key={`${item.id}-${pa.name}-${d.id}`}>
                            {d.displayName}
                          </Option>
                        ))}
                      </Select>
                    ) : (
                      <MultipleSelect
                        value={typeof pa.value === 'string' ? [pa.value] : pa.value || []}
                        onChange={(val) => {
                          onChange({
                            tag: 'relative',
                            item,
                            param: {
                              name: pa.name,
                              value: val || [],
                              // deviceName: idx === 0 ? dev?.displayName || null : null,
                              // capability: idx === 0 ? dev?.capability || null : null,
                              oldValue: pa?.value || [],
                            },
                          });
                        }}
                      >
                        {getDevicesByCategory(devices, pa)?.map((d) => (
                          <MOption value={d.id} key={`${item.id}-${pa.name}-${d.id}`}>
                            {d.displayName}
                          </MOption>
                        ))}
                      </MultipleSelect>
                    )}
                    {pa && pa.needModuleCategory[0] === 'antennaControl' ? (
                      <RoundInfoIcon
                        style={{ opacity: 0.5, cursor: 'pointer', marginLeft: 5 }}
                        onClick={() => {
                          onEditAntenna(pa);
                        }}
                      />
                    ) : null}
                  </div>
                </div>
              );
            })
          )}
        </div>
      </div>
      <Modal
        visible={showAntenna}
        title="天线参数设置"
        onCancel={() => {
          setShowAntenna(false);
        }}
        onOk={onModalOK}
      >
        <ShowAntenna antennaID={antennaID} driverParams={parameters} onParamChanged={onAntennaParamChanged} />
      </Modal>
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  devices: null,
  checked: false,
  onChange: () => {},
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  devices: PropTypes.any,
  checked: PropTypes.bool,
  onChange: PropTypes.func,
};

export default Item;

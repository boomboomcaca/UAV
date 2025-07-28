import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Loading } from 'dui';
import { getDeviceParam } from '@/api/template';
import icons from './icons';
import styles from './index.module.less';

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
  const { className, item, devices } = props;

  const [parameters, setParameters] = useState(null);

  const getDevicesParams = (params) => {
    const filters = params?.filter((p) => {
      return Array.isArray(p.needModuleCategory) && p.needModuleCategory.length > 0 && p.isInstallation;
    });
    return filters;
  };

  const getDeviceNameByID = (devs, id) => {
    const ds = devs.find((d) => {
      return d.id === id;
    });
    return ds?.displayName;
  };

  useEffect(() => {
    if (item && (item.parameters === null || item.parameters === undefined)) {
      getDeviceParam(item.id).then((res) => {
        if (res.result) {
          setParameters(res.result.parameters);
        }
        if (res.error) {
          setParameters([]);
        }
      });
    } else {
      setParameters(item.parameters);
    }
  }, [item]);

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.operate}>??</div>
      <div className={styles.title}>
        <div className={styles.icon}>{getDriverIcon(item)}</div>
        <div className={styles.name}>
          <div>{item?.displayName}</div>
          <div>{item?.version}</div>
        </div>
      </div>
      <div className={styles.info}>
        <div className={styles.infotitle}>绑定设备</div>
        <div className={styles.infolist}>
          {parameters === null ? (
            <Loading loadingMsg="加载相关参数中..." className={styles.loading} />
          ) : (
            getDevicesParams(parameters)?.map((pa) => {
              return (
                <div className={styles.infoitem}>
                  <div>{pa.displayName}</div>
                  <div>{getDeviceNameByID(devices, pa.value)}</div>
                </div>
              );
            })
          )}
        </div>
      </div>
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  devices: null,
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  devices: PropTypes.any,
};

export default Item;

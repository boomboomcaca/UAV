import React, { useState } from 'react';
import PropTypes from 'prop-types';
import { ListView, Input, Button } from 'dui';
import { DeleteIcon, AddIcon } from 'dc-icon';
import ContentButton from '@/components/ContentButton';
import useDriverList from '@/hooks/useDriverList';
import useDeviceList from '@/hooks/useDeviceList';
import Item from '../Item';
import Add from '../Add';
import Mod from '../Mod';
import styles from './index.module.less';

const Driver = (props) => {
  const { edgeID } = props;

  const [showAdd, setShowAdd] = useState(false);

  const [showMod, setShowMod] = useState(false);
  const [driver, setDriver] = useState(null);

  const {
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
  } = useDriverList(edgeID);

  const { devices } = useDeviceList(edgeID);

  const onItemChanged = (args) => {
    const { tag, item, param, params } = args;
    switch (tag) {
      case 'check':
        updateSelectDrivers(item);
        break;
      case 'param':
        setShowMod(true);
        setDriver(item);
        break;
      case 'switch':
        onActive(item);
        break;
      case 'init':
        onInitDriver(item);
        break;
      case 'relative':
        // window.console.log('=========relative');
        onDriverRelative(item, param);
        break;
      case 'antenna':
        onDriverAntenna(item, params);
        break;
      default:
        break;
    }
  };

  return (
    <div className={styles.root}>
      <div className={styles.query}>
        <Input
          allowClear
          showSearch
          onSearch={(str) => updateParams({ 'displayName.lk': str })}
          onPressEnter={(str) => updateParams({ 'displayName.lk': str })}
          onChange={(val) => {
            if (val === '') {
              updateParams({ 'displayName.lk': null });
            }
          }}
          placeholder="搜索"
          style={{ width: 260 }}
        />
        <div className={styles.filters} />
        <div className={styles.operate}>
          <Button onClick={refresh}>刷新</Button>
          <ContentButton
            icon={<AddIcon />}
            text="新增"
            onClick={() => {
              setShowAdd(true);
            }}
          />
          <ContentButton
            icon={<DeleteIcon />}
            text="删除"
            disabled={selectDrivers === null || selectDrivers.length === 0}
            onClick={onDelete}
          />
        </div>
      </div>
      <ListView
        className={styles.list}
        baseSize={{ width: 340, height: 360 }}
        dataSource={drivers}
        itemTemplate={(item) => {
          return (
            <Item
              className={styles.item}
              item={item}
              devices={devices}
              checked={hasSelected(item.id)}
              onChange={onItemChanged}
            />
          );
        }}
      />
      {showAdd ? (
        <Add
          edgeID={edgeID}
          className={styles.popup}
          onReturn={() => {
            refresh();
            setShowAdd(false);
          }}
        />
      ) : null}
      {showMod ? (
        <Mod
          driver={driver}
          className={styles.popup}
          onReturn={() => {
            refresh();
            setShowMod(false);
          }}
        />
      ) : null}
    </div>
  );
};

Driver.defaultProps = {
  edgeID: null,
};

Driver.propTypes = {
  edgeID: PropTypes.any,
};

export default Driver;

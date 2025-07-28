import React, { useState } from 'react';
import PropTypes from 'prop-types';
import { ListView, Input, Button } from 'dui';
import { DeleteIcon, AddIcon } from 'dc-icon';
import ContentButton from '@/components/ContentButton';
import useDeviceList from '@/hooks/useDeviceList';
import Item from '../Item';
import Add from '../Add';
import Mod from '../Mod';
import styles from './index.module.less';

const Device = (props) => {
  const { edgeID, filter, reverse } = props;

  const [showAdd, setShowAdd] = useState(false);

  const [showMod, setShowMod] = useState(false);
  const [device, setDevice] = useState(null);

  const { refresh, updateParams, devices, selectDevices, updateSelectDevices, hasSelected, onDelete, onActive } =
    useDeviceList(edgeID, filter, reverse);

  const onItemChanged = (args) => {
    const { tag, item } = args;
    switch (tag) {
      case 'check':
        updateSelectDevices(item);
        break;
      case 'param':
        setShowMod(true);
        setDevice(item);
        break;
      case 'switch':
        onActive(item);
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
            disabled={selectDevices === null || selectDevices.length === 0}
            onClick={onDelete}
          />
        </div>
      </div>
      <ListView
        className={styles.list}
        baseSize={{ width: 260, height: 280 }}
        dataSource={devices}
        itemTemplate={(item) => {
          return <Item className={styles.item} item={item} checked={hasSelected(item.id)} onChange={onItemChanged} />;
        }}
      />
      {showAdd ? (
        <Add
          edgeID={edgeID}
          filter={filter}
          reverse={reverse}
          className={styles.popup}
          onReturn={() => {
            refresh();
            setShowAdd(false);
          }}
        />
      ) : null}
      {showMod ? (
        <Mod
          device={device}
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

Device.defaultProps = {
  edgeID: null,
  filter: null,
  reverse: false,
};

Device.propTypes = {
  edgeID: PropTypes.any,
  filter: PropTypes.any,
  reverse: PropTypes.bool,
};

export default Device;

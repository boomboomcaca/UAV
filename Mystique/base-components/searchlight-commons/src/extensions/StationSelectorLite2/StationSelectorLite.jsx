/*
 * @Author: wangXueDong
 * @Date: 2022-10-15 10:14:41
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-12-09 16:53:50
 */
/* eslint-disable max-len */
import React, { useEffect, useState, useRef, useLayoutEffect } from 'react';
import PropTypes, { bool } from 'prop-types';
import initDcMap from 'dc-map';
import { useUpdateEffect, useSetState, useUnmount } from 'ahooks';
import { Modal, Select, message } from 'dui';
import SelectorLite from './SelectorLite/SelectorLite.jsx';

const StationSelectorLite = (props) => {
  const { onSelect, selectEdgeId, stations, visible, mapOptions, onChangeApi, deviceDatas, selectType, onClose } =
    props;

  return (
    <Modal
      visible={visible}
      title="选择监测站"
      bodyStyle={{ padding: '0 0' }}
      style={{ width: '1560px' }}
      footer={null}
      onCancel={() => onClose()}
    >
      {visible && (
        <SelectorLite
          onSelect={onSelect}
          stations={stations}
          selectEdgeId={selectEdgeId}
          mapOptions={mapOptions}
          onChangeApi={onChangeApi}
          deviceDatas={deviceDatas}
          selectType={selectType}
          onClose={onClose}
        />
      )}
    </Modal>
  );
};

StationSelectorLite.defaultProps = {
  visible: false,
  onSelect: () => {},
  stations: [],
  selectEdgeId: {},
  mapOptions: {},
  onChangeApi: () => {},
  deviceDatas: [],
  selectType: [],
  onClose: () => {},
};

StationSelectorLite.propTypes = {
  visible: PropTypes.bool,
  onSelect: PropTypes.func,
  selectEdgeId: PropTypes.array,
  stations: PropTypes.array,
  mapOptions: PropTypes.object,
  onChangeApi: PropTypes.func,
  deviceDatas: PropTypes.array,
  selectType: PropTypes.array,
  onClose: PropTypes.func,
};

export default StationSelectorLite;

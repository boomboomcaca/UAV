/*
 * @Author: wangXueDong
 * @Date: 2022-05-13 10:04:00
 * @LastEditors: XYQ
 * @LastEditTime: 2022-10-18 10:20:31
 */
import React, { useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import { Modal } from 'dui';
import StationSelector from './StationSelector/StationSelector.jsx';
import Map from './Map';
import styles from './EdgesModal.module.less';

function EdgesModal(props) {
  const { visible, onCancel, selectEdges, onEdgesChange, mapOptions, stations, number } = props;
  const [selStations, setSelStations] = useState([]);
  const [edges, setEdges] = useState([]);

  useEffect(async () => {
    const vh = document.documentElement.clientHeight;
    document.documentElement.style.setProperty('--h', `${vh / 2 + 200}px`);
    if (visible) {
      setEdges(stations);
    }
  }, [visible]);
  useEffect(() => {
    setSelStations(selectEdges);
  }, [selectEdges]);

  const onSelectStationChange = (e) => {
    setSelStations(e);
  };
  const toCancel = () => {
    onCancel();
    if (selStations) {
      onEdgesChange([...selStations]);
    } else {
      onEdgesChange([]);
    }
  };
  return (
    <Modal
      visible={visible}
      title="选择站点"
      bodyStyle={{ padding: '0 0' }}
      style={{ width: '1560px' }}
      footer={null}
      onCancel={toCancel}
    >
      <div className={styles.mainBox}>
        <div className={styles.mapBox}>
          <Map mapOptions={mapOptions} selectEdges={selectEdges || []} stations={edges} />
        </div>
        <div className={styles.edgesBox}>
          <StationSelector
            onChange={onSelectStationChange}
            selectEdges={selectEdges || []}
            stations={edges}
            number={number}
          />
        </div>
      </div>
    </Modal>
  );
}

EdgesModal.defaultProps = {
  visible: false,
  onCancel: () => {},
  selectEdges: [],
  onEdgesChange: () => {},
  mapOptions: {},
  stations: [],
  number: null,
};

EdgesModal.propTypes = {
  visible: PropTypes.bool,
  onCancel: PropTypes.func,
  selectEdges: PropTypes.array,
  onEdgesChange: PropTypes.func,
  mapOptions: PropTypes.object,
  stations: PropTypes.array,
  number: PropTypes.number,
};

export default EdgesModal;

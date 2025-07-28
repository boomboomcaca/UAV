import React, { useRef, useEffect, useState, useMemo } from 'react';
import PropTypes from 'prop-types';
import initDcMap from 'dc-map';
import 'dc-map/dist/main.css';
import classnames from 'classnames';
import notifilter from 'notifilter';
import { useThrottleFn } from 'ahooks';
import { Select } from 'dui';
import { getEdgesList } from '@/api/cloud';
import getConfig from '@/config';
import { mapBoxId, getMapOptions, getMapEdges, updateEdges, getPosition } from './map';
import styles from './index.module.less';

const { wsNotiUrl } = getConfig();

const { Option } = Select;

const Map = (props) => {
  const { className, onInitialized, onEdgesChanged, onStationSelected } = props;

  const edgesRef = useRef(null);
  const [key, setKey] = useState(0);
  const mapBoxRef = useRef(initDcMap(getMapOptions()));

  const [selectNode, setSelectNode] = useState(null);

  const { run } = useThrottleFn(
    () => {
      if (edgesRef.current) {
        const edges = getMapEdges(edgesRef.current, selectNode);
        mapBoxRef.current?.drawEdges?.(edges, true);
      }
    },
    { wait: 2000 },
  );

  const selectEdge = (edgeID) => {
    const selectedEdge = edgesRef.current?.find((item) => item.id === edgeID);
    if (selectedEdge) {
      mapBoxRef.current?.setPosition([selectedEdge.longitude, selectedEdge.latitude], true);
    }
    setSelectNode(selectedEdge);
  };

  useEffect(() => {
    mapBoxRef.current.mount();
    mapBoxRef.current.onEdgeSelectedChanged((_es, e) => {
      selectEdge(e.id);
    });

    getEdgesList().then((res) => {
      if (res.result) {
        edgesRef.current = res.result;
        setKey(key + 1);
        onInitialized(edgesRef.current, selectEdge);
        mapBoxRef.current?.setPosition(getPosition(res.result), true);
        run();
      }
    });

    const unregister = notifilter.register({
      url: wsNotiUrl,
      onmessage: (res) => {
        const { result } = res;
        onEdgesChanged(result);
        if (edgesRef.current) {
          edgesRef.current = updateEdges(edgesRef.current, result);
          run();
        }
      },
      dataType: ['gps', 'moduleStateChange', 'edgeStateChange'],
    });

    return () => {
      unregister();
    };
  }, []);

  const selects = useMemo(() => {
    return (
      <Select style={{ width: '100%', marginBottom: 8 }} value={selectNode?.id || ''} onChange={selectEdge}>
        {edgesRef.current?.map((edge) => (
          <Option key={edge.id} value={edge.id}>
            {edge.name}
          </Option>
        ))}
      </Select>
    );
  }, [selectNode, key]);

  return (
    <div className={classnames(styles.root, className)}>
      <div id={mapBoxId} className={classnames(styles.box)} />
      {/* {selectNode && ( */}
      <div className={styles.popup}>
        {selects}
        <div className={styles.name}>
          <span style={{ opacity: selectNode ? 1 : 0.5 }}>{selectNode?.name || '未选择站点'}</span>
          {selectNode && (
            <span
              onClick={() => {
                onStationSelected(selectNode);
              }}
            >{`查看详情 >>`}</span>
          )}
        </div>
        <div>
          <span>纬度：</span>
          <span>{selectNode?.latitude ? Number(selectNode?.latitude.toFixed(6)) : '--'}</span>
        </div>
        <div>
          <span>经度：</span>
          <span>{selectNode?.longitude ? Number(selectNode?.longitude.toFixed(6)) : '--'}</span>
        </div>
      </div>
      {/* )} */}
    </div>
  );
};

Map.defaultProps = {
  className: null,
  onInitialized: () => {},
  onEdgesChanged: () => {},
  onStationSelected: () => {},
};

Map.propTypes = {
  className: PropTypes.any,
  onInitialized: PropTypes.func,
  onEdgesChanged: PropTypes.func,
  onStationSelected: PropTypes.func,
};

export default Map;

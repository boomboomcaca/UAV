import React, { useRef, useState } from 'react';
import PropTypes from 'prop-types';
import { Button } from 'dui';
import { useThrottleFn } from 'ahooks';
import Map from '@/components/Map';
import Status from '@/components/Status';
import { getModules, updateModules, getStatus, getStatistics } from './preview';
import styles from './index.module.less';

const Preview = (props) => {
  const { hasRight, onPreviewClick } = props;

  const edgeStatesRef = useRef(null);

  const [status, setStatus] = useState(null);

  const { run } = useThrottleFn(
    () => {
      if (edgeStatesRef.current) {
        // setStatus(getStatus(edgeStatesRef.current));
        getStatistics().then((res) => {
          setStatus(res);
        });
      }
    },
    { wait: 2000 },
  );

  const onInitialized = (edges /* , toselect */) => {
    edgeStatesRef.current = getModules(edges);
    run();
  };

  const onEdgesChanged = (res) => {
    edgeStatesRef.current = updateModules(edgeStatesRef.current, res);
    run();
  };

  const onStationSelected = (node) => {
    onPreviewClick({ tag: 'detail', id: node.id });
  };

  return (
    <div className={styles.root}>
      <div className={styles.head}>
        <Status className={styles.status} values={status} />
        {hasRight ? (
          <Button
            style={{ border: '1px solid rgba(255, 255, 255, 0.2)', boxShadow: 'none' }}
            className={styles.primary}
            type="primary"
            onClick={() => {
              onPreviewClick({ tag: 'rest' });
            }}
          >
            重启环境监控系统
          </Button>
        ) : null}
        {hasRight ? (
          <Button
            onClick={() => {
              onPreviewClick({ tag: 'edit' });
            }}
          >
            站点维护
          </Button>
        ) : null}
      </div>
      <Map
        className={styles.map}
        onInitialized={onInitialized}
        onEdgesChanged={onEdgesChanged}
        onStationSelected={onStationSelected}
      />
    </div>
  );
};

Preview.defaultProps = {
  hasRight: true,
  onPreviewClick: () => {},
};

Preview.propTypes = {
  hasRight: PropTypes.bool,
  onPreviewClick: PropTypes.func,
};

export default Preview;

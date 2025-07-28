/*
 * @Author: XYQ
 * @Date: 2022-02-22 13:47:00
 * @LastEditors: XYQ
 * @LastEditTime: 2022-02-25 10:49:06
 * @Description: file content
 */
import React, { useCallback, useState, useRef } from 'react';
import { createPortal } from 'react-dom';
import PropTypes from 'prop-types';
import Waves from '../waves/Index.jsx';
import ssd from '../ssd.jsx';
import memory from '../memory.jsx';
import cpu from '../cpu.jsx';
import styles from './index.module.less';

export default function Srm(props) {
  const { type, data } = props;
  const [showPopover, setShowPopover] = useState(false);

  const ssdRef = useRef(null);
  const memRef = useRef(null);
  const cpuRef = useRef(null);

  const getPosition = (dr) => {
    if (dr && dr.current) {
      const pos = dr.current.getBoundingClientRect();
      const { bottom, left } = pos;
      return { top: bottom + 8, left: left - 53 };
    }
    return null;
  };

  const getCom = useCallback(() => {
    const color = data.percent > 60 ? '#FF4C2B' : '#35E065';
    switch (type) {
      case 'ssd':
        return (
          <>
            <div className={styles.ssd} ref={ssdRef}>
              {ssd(data.percent)}
            </div>
            <Waves
              style={{
                width: 32,
                height: 28,
                position: 'absolute',
                top: '2px',
                left: '3px',
                zIndex: -1,
                transform: 'scale(1)',
                clipPath: 'polygon(5px 0,0 100%,100% 100%,calc(100% - 5px) 0)',
              }}
              color="#35E065"
              number={data.percent}
            />
            {showPopover &&
              createPortal(
                <div className={styles.visible} style={getPosition(ssdRef)}>
                  <div className={styles.arrow} />
                  <div>边缘服务器硬盘资源</div>
                  <div>
                    已占用：<span style={{ color }}>{data.used}</span>G/
                    <span>{data.total}</span>G
                  </div>
                </div>,
                document.body,
              )}
          </>
        );
      case 'memory':
        return (
          <>
            {showPopover &&
              createPortal(
                <div className={styles.visible} style={getPosition(memRef)}>
                  <div className={styles.arrow} />
                  <div>边缘服务器内存资源</div>
                  <div>
                    已占用：<span style={{ color }}>{data.used}</span>G/
                    <span>{data.total}</span>G
                  </div>
                </div>,
                document.body,
              )}
            <div className={styles.ssd} ref={memRef}>
              {memory()}
            </div>
            <Waves
              style={{
                width: 27,
                height: 30,
                position: 'absolute',
                top: '3px',
                left: '6px',
                borderTopRightRadius: '6px',
                overflow: 'hidden',
              }}
              color="#3CE5D3"
              number={data.percent}
            />
          </>
        );
      case 'cpu':
        return (
          <>
            <div className={styles.ssd} ref={cpuRef}>
              {cpu()}
            </div>
            <Waves
              style={{
                width: 28,
                height: 28,
                position: 'absolute',
                top: '5px',
                left: '5px',
              }}
              number={data.percent}
            />
            {showPopover &&
              createPortal(
                <div className={styles.visible} style={getPosition(cpuRef)}>
                  <div className={styles.arrow} />
                  <div>边缘服务器CPU使用率</div>
                  <div style={{ color }}>{`${data.percent}%`}</div>
                </div>,
                document.body,
              )}
          </>
        );
      default:
        return null;
    }
  }, [showPopover, data]);

  return (
    <div
      className={styles.srmItem}
      id={`id-${type}`}
      onMouseEnter={() => setShowPopover(true)}
      onMouseLeave={() => setShowPopover(false)}
    >
      {getCom()}
    </div>
  );
}

Srm.propTypes = {
  type: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
};

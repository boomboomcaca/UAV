import React, { useState, useLayoutEffect } from 'react';
import PropTypes from 'prop-types';
import { Modal } from 'dui';
import { RightOutlined } from '@ant-design/icons';
// import defaultPng from './defaultPng.png';
import styles from './index.module.less';

const TaskInfoModal = (props) => {
  const { visible, info, onCancel, moreClick, dcRequest } = props;
  // 站类型
  const [typeObject, setTypeObject] = useState({});
  // 站分类
  const [categoryObject, setCategoryObject] = useState({});

  const [ipList, setipList] = useState([]);

  useLayoutEffect(() => {
    dcRequest?.({
      url: '/dic/dictionary/getDic',
      method: 'get',
      params: { dicNo: 'stationType' },
    }).then((ret) => {
      if (ret.result) {
        const snap = {};
        ret.result[0].data.forEach((item) => {
          snap[item.key] = item.value;
        });
        setTypeObject(snap);
      }
    });
  }, []);

  useLayoutEffect(() => {
    if (info.type) {
      dcRequest?.({
        url: '/dic/dictionary/getDic',
        method: 'get',
        params: { dicNo: info.type },
      }).then((ret) => {
        if (ret.result) {
          const snap = {};
          ret.result[0].data.forEach((item) => {
            snap[item.key] = item.value;
          });
          setCategoryObject(snap);
        }
      });
    }
  }, [info.type]);

  useLayoutEffect(() => {
    if (info.edgeId) {
      dcRequest?.({
        url: '/rmbt/device/getList',
        method: 'get',
        params: { moduleType: 'device', edgeId: info.edgeId },
      }).then((ret) => {
        if (ret.result) {
          const snap = [];
          ret.result.forEach((item) => {
            const findIp = item.parameters.find((param) => param.name === 'ipAddress');
            const findPort = item.parameters.find((param) => param.name === 'port');
            if (findIp) {
              snap.push({
                name: item.displayName,
                ip: findIp?.value || '',
                port: findPort?.value || '',
              });
            }
          });
          setipList(snap);
        }
      });
    }
  }, [info.edgeId]);

  const getStatus = (status) => {
    if (status === 'idle') return { name: '空闲', color: '#35E065' };
    if (status === 'busy') return { name: '忙碌', color: '#FFD118' };
    if (status === 'deviceBusy') return { name: '设备占用', color: '#FFD118' };
    if (status === 'offline') return { name: '离线', color: '#FE0000' };
    if (status === 'fault') return { name: '故障', color: '#E3E34A' };
    if (status === 'disabled') return { name: '禁用', color: '#787878' };
    return { name: '未知', color: '#787878' };
  };

  const ssstatus = getStatus(info.moduleState);
  const taskCreator = sessionStorage.getItem('userName') || '';

  return (
    <Modal
      visible={visible}
      title="信息"
      footer={null}
      onCancel={onCancel}
      reHeightKey={ipList.length}
      bodyStyle={{ padding: 16 }}
      style={{ width: 1320 }}
    >
      <div className={styles.taskinfonew2}>
        <div className={styles.maincontent}>
          <div className={styles.left}>
            <div className={styles.lefthead}>
              <div className={styles.boldtext}>基本信息</div>
              <div className={styles.baseinfo}>
                <div className={styles.baseleft}>
                  <div>操作员</div>
                  <div className={styles.boldtext}>{taskCreator}</div>
                </div>
                <div className={styles.baseRight}>
                  <div className={styles.baseItem}>
                    <div>功能启动时间</div>
                    <div className={styles.boldtext}>{info.creatTime || ''}</div>
                  </div>
                  <div className={styles.baseItem}>
                    <div>功能运行时间</div>
                    <div className={styles.boldtext}>{info.runTime || ''}</div>
                  </div>
                </div>
              </div>
              <div className={styles.boldtext}>站点信息</div>
              <div className={styles.edgeInfo}>
                <div className={styles.edgeInfoItem}>
                  <div>站点名称</div>
                  <div className={styles.boldtext}>{info.edgeName}</div>
                </div>
                <div className={styles.edgeInfoItem}>
                  <div>站点编码</div>
                  <div className={styles.boldtext}>{info.mfid || ''}</div>
                </div>
                <div className={styles.edgeInfoItem}>
                  <div>站点类型</div>
                  <div className={styles.boldtext}>
                    <span>{typeObject[info.type]}</span>
                    {categoryObject[info.category] && (
                      <span style={{ color: 'var(--theme-font-50)', marginLeft: '5px' }}>
                        （{categoryObject[info.category]}）
                      </span>
                    )}
                  </div>
                </div>
                <div className={styles.edgeInfoItem}>
                  <div>站点位置</div>
                  <div className={styles.boldtext}>
                    <span>{info.longitude ? `${info.longitude.toFixed(1)}°E` : ''}</span>
                    <span style={{ marginLeft: '10px' }}>{info.latitude ? `${info.latitude.toFixed(1)}°N` : ''}</span>
                  </div>
                </div>
                <div className={styles.edgeInfoItem}>
                  <div>设备名称</div>
                  <div className={styles.boldtext}>
                    <span>{info.deviceName || info.featureName}</span>
                    <span style={{ color: ssstatus.color, marginLeft: '16px' }}>{ssstatus.name}</span>
                  </div>
                </div>
              </div>
            </div>
            <div className={styles.leftfoot}>
              <div className={styles.boldtext}>网络信息</div>
              <div className={styles.iplistarea}>
                {ipList.map((item) => (
                  <div className={styles.ipitem} key={`${item.ip} ${item.port}`}>
                    <div className={styles.ipitemname}>
                      <span className={styles.boldtext} title={item.name}>
                        {item.name}
                      </span>
                      <span>：</span>
                    </div>
                    <div>{item.ip}</div>
                  </div>
                ))}
              </div>
            </div>
          </div>
          <div className={styles.right}>{/* <img src={defaultPng} alt="" /> */}</div>
        </div>
        <div className={styles.footer} onClick={moreClick}>
          <span>查看更多站点信息 </span>
          <RightOutlined />
        </div>
      </div>
    </Modal>
  );
};

TaskInfoModal.defaultProps = {
  info: {},
  visible: false,
  onCancel: () => {},
  moreClick: () => {},
  dcRequest: null,
};

TaskInfoModal.propTypes = {
  visible: PropTypes.bool,
  info: PropTypes.any,
  onCancel: PropTypes.func,
  moreClick: PropTypes.func,
  dcRequest: PropTypes.any,
};

export default TaskInfoModal;

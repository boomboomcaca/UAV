import React, { memo, useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Modal, message, Input } from 'dui';
import { useUpdateLayoutEffect } from 'ahooks';
import Mode1 from './Mode1/index.jsx';
import Mode2 from './Mode2/index.jsx';
import Mode3 from './Mode3/index.jsx';
import SegList from './SegList/index.jsx';
import styles from './index.module.less';

/**
 * 生成全局唯一标识符GUId
 * @returns {string}
 */
export const createGUID = () => {
  function S4() {
    return Math.floor((1 + Math.random()) * 0x10000)
      .toString(16)
      .substring(1);
  }
  return `${S4() + S4()}-${S4()}-${S4()}-${S4()}-${S4()}${S4()}${S4()}`;
};

const SegEditorModal = (props) => {
  const { segments, onChange, segparameters, axios, limit, maxLength, disabled } = props;
  const defaultSegRef = useRef();

  const [mode, setmode] = useState(1);
  const [visible, setVisible] = useState(false);
  // 本地频段数据
  const [segs, setsegs] = useState([]);
  // 选择频段idx
  const [activeIdx, setactiveIdx] = useState(-1);
  // 另存时文本确认框
  const [showConfirm, setshowConfirm] = useState(false);
  // 另存时文本
  const [name, setname] = useState('');
  // mode2刷新key
  const [mode2ReKey, setmode2ReKey] = useState('');

  const onMode2IPT = (segList) => {
    const newSegs = segList.map((item) => ({
      ...defaultSegRef.current,
      ...item,
      id: createGUID(),
    }));
    setsegs(newSegs);
  };

  const onSelectSeg = (idx) => {
    if (idx === activeIdx) {
      setactiveIdx(-1);
      return;
    }
    if (mode === 2) {
      setmode(1);
    }
    setactiveIdx(idx);
  };

  const onDeleteSeg = (idx) => {
    if (idx === activeIdx) {
      setactiveIdx(-1);
    }
    const nne = [...segs];
    nne.splice(idx, 1);
    setsegs(nne);
  };

  const onNewSeg = (seg) => {
    const nne = [...segs];
    // 转进为替换
    if (activeIdx > -1) {
      nne[activeIdx] = {
        ...nne[activeIdx],
        ...seg,
        name: '',
      };
      setsegs(nne);
      return;
    }
    const findSame = nne.find(
      (item) =>
        item.startFrequency === seg.startFrequency &&
        item.stopFrequency === seg.stopFrequency &&
        item.stepFrequency === seg.stepFrequency,
    );
    if (findSame) {
      message.info({ key: 'SegEditorModal', content: `该频段已添加` });
      return;
    }
    if (segs.length === maxLength) {
      message.info({ key: 'SegEditorModal', content: `最多选择${maxLength}组` });
      return;
    }
    nne.push({
      ...defaultSegRef.current,
      ...seg,
    });
    setsegs(nne);
  };

  const onOk = () => {
    if (segs.length === 0) {
      message.info({ key: 'SegEditorModal', content: `请新增频段` });
      return;
    }
    if (maxLength === 1) {
      const firstSeg = segs[0];
      if (firstSeg.startFrequency === null || firstSeg.startFrequency === '') {
        message.info({ key: 'Mode1', content: '请输入起始频率' });
        return;
      }
      if (firstSeg.stopFrequency === null || firstSeg.stopFrequency === '') {
        message.info({ key: 'Mode1', content: '请输入结束频率' });
        return;
      }
      if (firstSeg.stopFrequency - firstSeg.startFrequency < 0) {
        message.info({ key: 'Mode1', content: '结束频率必须大于起始频率' });
        return;
      }
      if (firstSeg.stopFrequency - firstSeg.startFrequency < 1) {
        message.info({ key: 'Mode1', content: '扫描带宽至少为1M' });
        return;
      }
    }
    const nne = [...segs];
    nne.sort((a, b) => a.stopFrequency - b.stopFrequency).sort((a, b) => a.startFrequency - b.startFrequency);
    setVisible(false);
    onChange(nne);
  };

  const onConfirmOk = () => {
    if (name === '') {
      message.info({ key: 'confirmText', content: `请输入列表名称` });
      return;
    }
    axios?.({
      url: '/sys/scanInfo/add',
      method: 'post',
      data: {
        name,
        scanInfo: segs.map((item) => ({
          startFrequency: item.startFrequency,
          stepFrequency: item.stepFrequency,
          stopFrequency: item.stopFrequency,
        })),
      },
    }).then(() => {
      message.info('保存成功');
      setshowConfirm(false);
      setmode2ReKey(new Date().getTime());
    });
  };

  useUpdateLayoutEffect(() => {
    const defaultSeg = {};
    if (segparameters instanceof Array && segparameters.length > 0) {
      segparameters.forEach((temp) => {
        defaultSeg[temp.name] = temp.value;
      });
    }
    defaultSegRef.current = defaultSeg;
  }, [segparameters]);

  useEffect(() => {
    if (visible) {
      setsegs(segments);
      if (maxLength === 1) {
        setactiveIdx(0);
      } else {
        setactiveIdx(-1);
      }
    }
  }, [visible]);

  return (
    <>
      <div
        className={classnames(styles.specbtn, { [styles.disabled]: disabled })}
        onClick={() => {
          if (!disabled) {
            setVisible(true);
          }
        }}
      >
        <div className={styles.text}>频段编辑</div>
        {maxLength > 1 && <div className={styles.num}>{segments.length || 0}</div>}
      </div>
      <Modal
        title="频段编辑"
        visible={visible}
        onCancel={() => setVisible(false)}
        onOk={onOk}
        style={{ width: 1320 }}
        bodyStyle={{ padding: '8px 16px 0' }}
        heterotypic
        heterotypicChild={
          maxLength === 1 ? null : (
            <div className={styles.footerBtnArea}>
              <div
                className={styles.btn}
                onClick={() => {
                  setshowConfirm(true);
                  setname();
                }}
              >
                列表另存
              </div>
              {showConfirm && (
                <div className={styles.confirmText}>
                  <Input
                    style={{ width: 220 }}
                    placeholder="请输入名称"
                    value={name}
                    maxLength={12}
                    onChange={(val) => setname(val)}
                  />
                  <div className={styles.confirmSvg} onClick={onConfirmOk}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <g opacity="0.6">
                        <path
                          d="M5 11L9.29412 16L19 7"
                          stroke="white"
                          strokeWidth="1.5"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </g>
                    </svg>
                  </div>
                  <div className={styles.confirmSvg} onClick={() => setshowConfirm(false)}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                      <g opacity="0.6">
                        <path
                          d="M6 6C10.6863 10.491 18 18 18 18M18 6L6 18"
                          stroke="white"
                          strokeWidth="1.5"
                          strokeLinecap="round"
                          strokeLinejoin="round"
                        />
                      </g>
                    </svg>
                  </div>
                </div>
              )}
              {showConfirm && <div className={styles.confirmArrow} />}
            </div>
          )
        }
      >
        <div className={styles.modalcontent}>
          <div className={styles.left}>
            <div className={styles.lefthead}>
              <div className={classnames(styles.headitem, { [styles.active]: mode === 1 })} onClick={() => setmode(1)}>
                {activeIdx > -1 ? '编辑频段' : '添加频段'}
              </div>
              {maxLength !== 1 && (
                <div
                  className={classnames(styles.headitem, { [styles.active]: mode === 2, [styles.ban]: activeIdx > -1 })}
                  onClick={() => {
                    if (activeIdx > -1) {
                      return;
                    }
                    setmode(2);
                  }}
                >
                  导入频段表
                </div>
              )}
              <div className={classnames(styles.headitem, { [styles.active]: mode === 3 })} onClick={() => setmode(3)}>
                {activeIdx > -1 ? '更换业务频段' : '选择业务频段'}
              </div>
            </div>
            {mode === 1 && (
              <Mode1
                limit={limit}
                segparameters={segparameters}
                segment={activeIdx > -1 ? segs[activeIdx] : defaultSegRef.current}
                isEdit={activeIdx > -1}
                maxLength={maxLength}
                onMode1Sure={(seg) => {
                  onNewSeg(seg);
                }}
                onMode1Cancel={() => setactiveIdx(-1)}
              />
            )}
            {mode === 2 && <Mode2 axios={axios} limit={limit} onMode2IPT={onMode2IPT} mode2ReKey={mode2ReKey} />}
            {mode === 3 && <Mode3 axios={axios} limit={limit} segments={segs} onClickSeg={onNewSeg} />}
          </div>
          <SegList
            segments={segs}
            activeIdx={activeIdx}
            maxLength={maxLength}
            onSelectSeg={onSelectSeg}
            onDeleteSeg={onDeleteSeg}
          />
        </div>
      </Modal>
    </>
  );
};

SegEditorModal.defaultProps = {
  segments: [],
  onChange: () => {},
  segparameters: [],
  limit: {
    min: 20,
    max: 8000,
    stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  },
  maxLength: 8,
  disabled: false,
};

SegEditorModal.propTypes = {
  axios: PropTypes.func.isRequired,
  segments: PropTypes.array,
  onChange: PropTypes.func,
  segparameters: PropTypes.array,
  limit: PropTypes.object,
  maxLength: PropTypes.number,
  disabled: PropTypes.bool,
};

export default memo(SegEditorModal);

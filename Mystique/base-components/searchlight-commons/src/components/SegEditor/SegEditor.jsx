import React, { useState, useEffect, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import { Modal, Button, message } from 'dui';
import { svg1, svg2, CloseIcon, SignIcon, PenEditIcon } from './Svg.jsx';
// eslint-disable-next-line import/extensions
import NumberInput from '@/components/NumberInput';
// eslint-disable-next-line import/extensions
import BubbleSelector from '@/components/BubbleSelector';
import { sortPro, createGUID, limitHandle, getStepList } from './utils';
import styles from './index.module.less';

const SegEditor = (props) => {
  const { data, getSegmentList, limit, segmentList, disabled, maxLength } = props;
  const list = sortPro(data, ['group'], limit);
  const dataSource = getStepList(limit.stepItems);
  const [visible, setVisible] = useState(false);
  const prevSegsRef = useRef();
  const [selectSegmentList, setSelectSegmentList] = useState([]);
  const [historyList, setHistoryList] = useState([]);
  const [selfSegment, setSelfSegment] = useState({
    startFrequency: 88,
    stopFrequency: 108,
    stepFrequency: 25,
  });
  const [isEdit, setIsEdit] = useState({
    id: '',
    flag: false,
  });

  useEffect(() => {
    setSelectSegmentList(
      segmentList.sort((a, b) => a.stopFrequency - b.stopFrequency).sort((a, b) => a.startFrequency - b.startFrequency),
    );
  }, [segmentList]);

  useEffect(() => {
    if (visible) {
      if (maxLength === 1 && segmentList.length > 0) {
        setSelfSegment(segmentList[0]);
        setIsEdit({
          id: segmentList[0].id,
          flag: true,
        });
      }
      const arr = JSON.parse(window.localStorage.getItem('SEGMENT_LIST'));
      if (arr) {
        setHistoryList(limitHandle(limit, arr));
      }
    }
  }, [visible]);

  const isSegementSame = (a, b) => {
    return (
      a.startFrequency === b.startFrequency &&
      a.stopFrequency === b.stopFrequency &&
      a.stepFrequency === b.stepFrequency
    );
  };

  const isSame = (b) => {
    const a = selectSegmentList.find((i) => {
      return isSegementSame(i, b);
    });
    return a;
  };

  const addSegementHandle = () => {
    if (selfSegment.stopFrequency - selfSegment.startFrequency < 10) {
      message.error('结束频率必须比起始频率大10MHz');
      return;
    }
    // 是否有相同频率
    if (isSame(selfSegment)) {
      message.warning('该频段已添加！');
      return;
    }
    if (isEdit.flag) {
      const li = [...selectSegmentList].map((it) => {
        if (it.id === isEdit.id) {
          const guid = createGUID();
          setIsEdit({
            ...isEdit,
            id: guid,
          });
          return {
            id: guid,
            startFrequency: selfSegment.startFrequency,
            stopFrequency: selfSegment.stopFrequency,
            stepFrequency: selfSegment.stepFrequency,
          };
        }
        return it;
      });
      setSelectSegmentList(li);
    } else {
      if (selectSegmentList.length === maxLength) {
        message.info(`最多选择${maxLength}组`);
        return;
      }
      const guId = createGUID();
      setSelectSegmentList([
        ...selectSegmentList,
        {
          ...selfSegment,
          id: guId,
        },
      ]);
      setSelfSegment({
        startFrequency: 88,
        stopFrequency: 108,
        stepFrequency: 25,
      });
    }
  };

  const segmentSelect = (e) => {
    if (isEdit.flag) {
      changeSelectId(e);
    } else {
      addSelectSegmentList(e);
    }
  };

  const addSelectSegmentList = (e) => {
    if (isSame(e)) {
      if (selectSegmentList.length === 1) {
        return message.info('最少选择1组');
      }
      return cancelSelect(e);
    }
    if (selectSegmentList.length === maxLength) {
      return message.info(`最多选择${maxLength}组`);
    }
    return setSelectSegmentList([...selectSegmentList, e]);
  };

  const changeSelectId = (e) => {
    if (isSame(e)) {
      message.warning('该频段已添加！');
      return;
    }
    const li = [...selectSegmentList].map((it) => {
      if (it.id === isEdit.id) return e;
      return it;
    });
    setSelectSegmentList(li);
    setIsEdit({
      ...isEdit,
      id: e.id,
    });
  };

  const cancelSelect = (e) => {
    const a = [...selectSegmentList].filter((it) => {
      return !isSegementSame(it, e);
    });
    setSelectSegmentList(a);
  };

  const deleteHandle = (e, item) => {
    e.stopPropagation();
    if (item.id === isEdit.id) {
      init();
    }
    cancelSelect(item);
  };

  const editSegment = (e) => {
    if (maxLength === 1) return;
    if (e.id === isEdit.id) {
      if (isEdit.flag) {
        init();
      } else {
        setSelfSegment(e);
        setIsEdit({
          id: e.id,
          flag: true,
        });
      }
    } else {
      setSelfSegment(e);
      setIsEdit({
        id: e.id,
        flag: true,
      });
    }
  };
  const init = () => {
    setIsEdit({
      id: '',
      flag: false,
    });
    setSelfSegment({
      startFrequency: 88,
      stopFrequency: 108,
      stepFrequency: 25,
    });
  };
  const itemChange = (values) => {
    setSelfSegment({
      ...selfSegment,
      ...values,
    });
  };

  const outHandle = () => {
    setVisible(false);
    const li = JSON.parse(window.localStorage.getItem('SEGMENT_LIST'));
    let arr = [];
    let arr1 = [];
    if (li) {
      arr = li.filter((i) => {
        return i.isLock;
      });
      arr1 = li.filter((i) => {
        return !i.isLock;
      });
    }
    let arr2 = [...arr, ...selectSegmentList, ...arr1];
    for (let i = 0; i < arr2.length - 1; i += 1) {
      for (let j = 1; j < arr2.length; j += 1) {
        if (i !== j) {
          if (isSegementSame(arr2[i], arr2[j]) || arr2[i].id === arr2[j].id) {
            arr2.splice(j, 1);
          }
        }
      }
    }
    if (arr2.length > 9) {
      arr2 = arr2.slice(0, 9);
    }
    window.localStorage.setItem('SEGMENT_LIST', JSON.stringify(arr2));
    getSegmentList(
      selectSegmentList
        .sort((a, b) => a.stopFrequency - b.stopFrequency)
        .sort((a, b) => a.startFrequency - b.startFrequency),
    );
    init();
  };

  const changeLock = (e, i) => {
    e.stopPropagation();
    const arr = historyList.map((it) => {
      if (it.id === i.id) {
        return i;
      }
      return it;
    });
    setHistoryList(arr);
    window.localStorage.setItem('SEGMENT_LIST', JSON.stringify(arr));
  };

  return (
    <div className={styles.SegEditor}>
      <div
        className={classnames(styles.specbtn, { [styles.disabled]: disabled })}
        onClick={() => {
          if (!disabled) {
            prevSegsRef.current = JSON.stringify(segmentList);
            setVisible(true);
          }
        }}
      >
        <div className={styles.text}>
          <PenEditIcon iconSize={24} />
        </div>
        <div className={styles.num}>{segmentList.length}</div>
      </div>
      <Modal
        visible={visible}
        title={langT('commons', 'spectrumEditor001')}
        footer={null}
        onCancel={() => {
          setVisible(false);
          setSelectSegmentList(JSON.parse(prevSegsRef.current));
        }}
        onOk={outHandle}
        style={{ width: 1080 }}
        heterotypic
        heterotypicChild={<></>}
        bodyStyle={{ padding: 8, height: window.screen.height - 480, background: 'var(--theme-background-primary)' }}
      >
        <div className={styles.segEditorContentBody}>
          <div className={styles.left}>
            <div style={{ marginBottom: '32px' }} className={styles.item}>
              <div className={styles.title}>{langT('commons', 'spectrumEditor002')}</div>
              <div className={styles.self}>
                <div className={styles.inputSegment}>
                  <NumberInput
                    minValue={limit.min}
                    maxValue={limit.max}
                    style={{ width: '140px' }}
                    value={selfSegment.startFrequency}
                    suffix="MHz"
                    unavailableKeys={['+/-']}
                    onValueChange={(val) => itemChange({ startFrequency: val })}
                  />
                  <span style={{ padding: '0 8px' }}>-</span>
                  <NumberInput
                    minValue={limit.min}
                    maxValue={limit.max}
                    style={{ width: '140px' }}
                    value={selfSegment.stopFrequency}
                    suffix="MHz"
                    unavailableKeys={['+/-']}
                    onValueChange={(val) => itemChange({ stopFrequency: val })}
                  />
                  <span style={{ padding: '0 8px' }}>-</span>
                  <BubbleSelector
                    width={140}
                    dataSource={dataSource}
                    value={selfSegment.stepFrequency}
                    position="center"
                    onValueChange={(e) => {
                      itemChange({ stepFrequency: e.value });
                    }}
                    keyBoardType="simple"
                  />
                </div>
                <Button onClick={addSegementHandle}>
                  {isEdit.flag ? langT('commons', 'confirm') : langT('commons', 'spectrumEditor003')}
                </Button>
              </div>
            </div>
            {historyList.length > 0 && (
              <div className={styles.item}>
                <div className={styles.title}>{langT('commons', 'spectrumEditor004')}</div>
                <div className={styles.segments}>
                  {historyList.map((it, index) => {
                    const flag = (index + 1) % 3;
                    const flag1 = isSame(it);
                    return (
                      <div
                        onClick={() => segmentSelect(it)}
                        className={styles.segmentItem1}
                        style={{
                          margin: flag ? '0 16px 16px 0' : '0 0 16px',
                          color: flag1 ? '#3CE5D3' : 'var(--theme-font-100)',
                          cursor: flag1 && isEdit.flag ? 'not-allowed' : 'pointer',
                        }}
                        key={it.id}
                      >
                        <div style={{ margin: '0 8px', height: 20, cursor: 'pointer' }}>
                          <SignIcon
                            onClick={(e) =>
                              changeLock(e, {
                                ...it,
                                isLock: !it.isLock,
                              })
                            }
                            iconSize={20}
                            color={it.isLock ? '#FFD118' : '#C4C4C4'}
                          />
                        </div>
                        <div className={styles.segmentInfo}>
                          <div>
                            <span className={styles.valueColor}>{it.startFrequency}</span>
                            <span style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>MHz - </span>
                            <span className={styles.valueColor}>{it.stopFrequency}</span>
                            <span style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>MHz @ </span>
                            <span className={styles.valueColor}>{it.stepFrequency}</span>
                            <span style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>kHz</span>
                          </div>
                          <div style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>{it.name}</div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            )}
            {list.map((i, idx) => {
              return (
                <div key={`${idx + 1}`} className={styles.item}>
                  {i.children.length > 0 && (
                    <>
                      <div className={styles.title}>{i.group}</div>
                      <div className={styles.segments}>
                        {i.children.map((it, index) => {
                          const flag = (index + 1) % 3;
                          const flag1 = isSame(it);
                          return (
                            <div
                              onClick={() => segmentSelect(it)}
                              className={styles.segmentItem}
                              style={{
                                margin: flag ? '0 16px 16px 0' : '0 0 16px',
                                color: flag1 ? '#3CE5D3' : 'var(--theme-font-100)',
                                cursor: flag1 && isEdit.flag ? 'not-allowed' : 'pointer',
                              }}
                              key={it.id}
                            >
                              <div className={styles.segmentInfo}>
                                <span className={styles.valueColor}>{it.startFrequency}</span>
                                <span style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>MHz - </span>
                                <span className={styles.valueColor}>{it.stopFrequency}</span>
                                <span style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>MHz @ </span>
                                <span className={styles.valueColor}>{it.stepFrequency}</span>
                                <span style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>kHz</span>
                              </div>
                              <div style={{ color: flag1 ? '#3CE5D3' : 'var(--theme-font-50)' }}>{it.name}</div>
                            </div>
                          );
                        })}
                      </div>
                    </>
                  )}
                </div>
              );
            })}
          </div>
          <div className={styles.right}>
            <div className={styles.top}>
              <div className={styles.leftFilter}>{svg1()}</div>
              {langT('commons', 'spectrumEditor005')}
              <div className={styles.rightFilter}>{svg2()}</div>
            </div>
            <div className={styles.center}>{langT('commons', 'spectrumEditor006')}</div>
            <div className={styles.bottom}>
              {selectSegmentList.map((it, index) => {
                const isSelect = it.id === isEdit.id;
                return (
                  <div
                    onClick={() => editSegment(it)}
                    style={{
                      border: isSelect ? '1px solid rgba(60, 229, 211, 1)' : '1px solid #202741',
                      background: isSelect ? 'var(--theme-switch-border)' : 'var(--theme-background-light)',
                    }}
                    className={styles.segmentItem1}
                    key={it.id}
                  >
                    <div className={styles.index}>{index + 1}</div>
                    <div className={styles.segmentInfo}>
                      <div>
                        <span className={styles.valueColor}>{it.startFrequency}</span>
                        <span className={styles.labelColor}>MHz - </span>
                        <span className={styles.valueColor}>{it.stopFrequency}</span>
                        <span className={styles.labelColor}>MHz @ </span>
                        <span className={styles.valueColor}>{it.stepFrequency}</span>
                        <span className={styles.labelColor}>kHz</span>
                      </div>
                      <div className={styles.labelColor}>{it.name}</div>
                    </div>
                    <div className={styles.close}>
                      {selectSegmentList.length > 1 && (
                        <CloseIcon onClick={(e) => deleteHandle(e, it)} iconSize={20} color="var(--theme-font-100)" />
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </Modal>
    </div>
  );
};

SegEditor.defaultProps = {
  segmentList: [],
  disabled: false,
  limit: {
    min: 20,
    max: 8000,
    stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  },
  maxLength: 8,
};

SegEditor.propTypes = {
  data: PropTypes.array.isRequired,
  getSegmentList: PropTypes.func.isRequired,
  limit: PropTypes.object,
  segmentList: PropTypes.array,
  disabled: PropTypes.bool,
  maxLength: PropTypes.number,
};

export default SegEditor;

/* eslint-disable react/no-array-index-key */
import React, { useEffect, useMemo, useRef, useState } from 'react';
import PropTypes from 'prop-types';
import langT from 'dc-intl';
import classnames from 'classnames';
import { MinusIcon, RemoveIcon, AddIcon, FinishIcon } from 'dc-icon';
import Icon, { LeftOutlined, RightOutlined } from '@ant-design/icons';
import { createGUID } from '../../lib/random';
import useClick from '../../hooks/useClick';
import useLongTouch from '../../hooks/useLongTouch';
import AddSeg from './AddSeg/index.jsx';

import styles from './style.module.less';

const SegmentsEditor = (props) => {
  const {
    initSegmentData,
    treeData,
    tableData,
    onTreeSelect,
    onSegChanged,
    onViewChanged,
    editable,
    maxCount,
    rightWidth,
  } = props;
  // 当前选择频段额索引号
  const [selectedIndx, setSelectedIndx] = useState(-1);
  // 单个编辑时的seg
  const [activeSeg, setActiveSeg] = useState({});
  // 当前是否处于删除状态
  const [isUnderRemove, setIsUnderRemove] = useState(false);
  // 频率规划库弹窗
  const [showAdd, setShowAdd] = useState(false);
  // 滚动num
  const [num, setNum] = useState(0);

  const [segLen, setSegLen] = useState(-1);
  // 保存当前频段信息，因为自定义hook回调获取不到初始频段信息，不知道为什么
  const segsRef = useRef([]);
  const editRef = useRef();
  const delingRef = useRef();
  // 添加频段按钮
  const btnAdd = useRef();

  const removeSegment = (e, index) => {
    e.stopPropagation();
    const ss = [...segsRef.current];
    ss.splice(index, 1);
    segsRef.current = ss;
    setSelectedIndx(-1);
    setSegLen(ss.length);
  };

  // 点击+号添加
  const addOneZDY = (allsegs) => {
    const ss = [...allsegs];
    const newSeg = {
      id: createGUID(),
      startFrequency: 87,
      stopFrequency: 108,
      stepFrequency: 25,
    };
    ss.push(newSeg);
    segsRef.current = ss;
    setSegLen(ss.length);

    setSelectedIndx(-1);
    onViewChanged({ segment: undefined });
    onSegChanged?.({
      action: 'add',
      current: newSeg,
      segment: ss,
    });
  };

  const editOne = (e, seg) => {
    e.stopPropagation();
    setActiveSeg(seg);
    setShowAdd(true);
  };

  const clickLeftArrow = () => {
    if (num === 0) {
      return;
    }
    setNum(num - 1);
  };

  const clickRightArrow = () => {
    if (num === segsRef.current.length - 6) {
      return;
    }
    setNum(num + 1);
  };

  // 双击弹窗添加、编辑某段
  const addOneYS = (newSeg) => {
    const ss = [...segsRef.current];
    if (activeSeg.startFrequency) {
      const index = selectedIndx === -1 ? 0 : selectedIndx;
      ss[index] = newSeg;
      onSegChanged?.({
        action: 'edit',
        index,
        current: newSeg,
        segment: ss,
      });
    } else {
      ss.push(newSeg);
      setSelectedIndx(-1);
      onViewChanged({ segment: undefined });
      onSegChanged?.({
        action: 'add',
        current: newSeg,
        segment: ss,
      });
    }
    segsRef.current = ss;
    setSegLen(ss.length);
    setShowAdd(false);
  };

  // 外部设置频段信息
  useEffect(() => {
    if (initSegmentData) {
      segsRef.current = initSegmentData;
      if (initSegmentData.length > 6 && num > initSegmentData.length - 6) {
        setNum(initSegmentData.length - 6);
      }
      if (initSegmentData.length <= 6) {
        setNum(0);
      }
      setSegLen(initSegmentData.length);
    }
  }, [JSON.stringify(initSegmentData)]);

  useEffect(() => {
    editRef.current = editable;
  }, [editable]);

  const translateXCss = useMemo(() => {
    return `translateX(-${num * (100 / 6)}%)`;
  }, [num]);

  useEffect(() => {
    let removeClick;
    let remoLongTouch;
    if (btnAdd.current) {
      removeClick = useClick(btnAdd.current, (e) => {
        if (editable && segsRef.current.length < maxCount) {
          if (e.dblClick) {
            setShowAdd(true);
            setActiveSeg({});
          } else {
            addOneZDY(segsRef.current);
          }
        }
      });

      remoLongTouch = useLongTouch(btnAdd.current, (e) => {
        if (editable && segsRef.current.length < maxCount) {
          setShowAdd(true);
          setActiveSeg({});
        }
      });
    }
    return () => {
      if (removeClick) {
        removeClick();
      }
      if (remoLongTouch) {
        remoLongTouch();
      }
    };
  }, [editable, btnAdd.current]);

  return (
    <div className={styles.segRoot}>
      <div className={styles.segLeft}>
        {segLen > 1 && (
          <div
            className={classnames(styles.allSeeBtn, { [styles.ban]: !editable, [styles.active]: selectedIndx === -1 })}
            onClick={() => {
              if (selectedIndx !== -1) {
                setSelectedIndx(-1);
                onViewChanged({ segment: undefined });
              }
            }}
          >
            <div className={styles.btntext1}>全景显示</div>
            <div className={styles.btntext2}>显示所有频段</div>
          </div>
        )}
      </div>

      <div className={styles.segCenter}>
        {segsRef.current.length > 6 && !isUnderRemove && (
          <div className={styles.leftArrow} onClick={clickLeftArrow}>
            <LeftOutlined color="var(--theme-font-80)" style={{ fontSize: '18px' }} />
          </div>
        )}
        {segsRef.current.length > 6 && !isUnderRemove && (
          <div className={styles.rightArrow} onClick={clickRightArrow}>
            <RightOutlined color="var(--theme-font-80)" style={{ fontSize: '18px' }} />
          </div>
        )}
        <div className={styles.segArea} style={{ transform: translateXCss }}>
          {segLen <= 0 ? (
            <div className={styles.emptyText}>{langT('commons', 'selectseg')}</div>
          ) : (
            segsRef.current.map((seg, index) => (
              <div
                className={classnames(styles.segitem, {
                  [styles.see]: index === selectedIndx || segsRef.current.length === 1,
                })}
                key={`${seg.id}-${index}`}
                style={{ width: `${100 / segsRef.current.length}%` }}
                onClick={() => {
                  if (index !== selectedIndx) {
                    setSelectedIndx(index);
                    onViewChanged({ segment: seg }, index);
                  }
                }}
              >
                <div className={styles.marginArea}>
                  {seg.name ? (
                    <div className={styles.twofloorSeg}>
                      <div className={styles.btntext1}>{seg.name}</div>
                      <div className={styles.btntext2}>
                        {seg.startFrequency}MHz - {seg.stopFrequency}
                        MHz
                      </div>
                    </div>
                  ) : (
                    <div className={classnames(styles.onlyseg, styles.overflowtext)}>
                      {seg.startFrequency}MHz - {seg.stopFrequency}
                      MHz
                    </div>
                  )}
                  {isUnderRemove && (
                    <div
                      className={styles.removeSegIcon}
                      onClick={(e) => {
                        removeSegment(e, index);
                      }}
                    >
                      <MinusIcon color="var(--theme-font-80)" iconSize={25} />
                    </div>
                  )}
                  {!isUnderRemove && editable && (index === selectedIndx || segLen === 1) && (
                    <div className={styles.removeSegIcon} onClick={(e) => editOne(e, seg)}>
                      <Icon component={ChangeSvg} />
                    </div>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      </div>

      <div className={styles.segRight} style={{ width: `${rightWidth}px` }}>
        {isUnderRemove ? (
          <div
            className={styles.rightbtn}
            onClick={() => {
              if (editable) {
                setIsUnderRemove(false);
                delingRef.current = false;
                setSelectedIndx(-1);

                onViewChanged({ segment: undefined });
                onSegChanged?.({
                  action: 'remove',
                  current: undefined,
                  index: -1,
                  segment: [...segsRef.current],
                });
              }
            }}
          >
            <FinishIcon iconSize={25} color="var(--theme-font-100)" />
          </div>
        ) : (
          <div
            className={styles.rightbtn}
            onClick={() => {
              if (editable && segLen > 1) {
                setIsUnderRemove(true);
                delingRef.current = true;
              }
            }}
          >
            <RemoveIcon
              iconSize={25}
              color={editable && segLen > 1 ? 'var(--theme-font-100)' : 'var(--theme-font-30)'}
            />
          </div>
        )}
        <div className={styles.rightbtn} ref={btnAdd}>
          <AddIcon
            iconSize={25}
            color={isUnderRemove || segLen >= maxCount || !editable ? 'var(--theme-font-30)' : 'var(--theme-font-100)'}
          />
        </div>
      </div>

      <AddSeg
        visible={showAdd}
        treeData={treeData}
        tableData={tableData}
        segmentData={activeSeg}
        onOpen={() => setShowAdd(true)}
        onCancel={() => setShowAdd(false)}
        onTreeSelect={onTreeSelect}
        onSelectChange={addOneYS}
      />
    </div>
  );
};

SegmentsEditor.defaultProps = {
  initSegmentData: [],
  treeData: [],
  tableData: [],
  onTreeSelect: () => {},
  onSegChanged: () => {},
  onViewChanged: () => {},
  editable: false,
  maxCount: 10,
  rightWidth: 114,
};

SegmentsEditor.propTypes = {
  initSegmentData: PropTypes.array,
  treeData: PropTypes.array, // 频段池的树结构数据
  tableData: PropTypes.array, // 频段列表
  onTreeSelect: PropTypes.func,
  onSegChanged: PropTypes.func,
  onViewChanged: PropTypes.func,
  editable: PropTypes.bool,
  maxCount: PropTypes.number,
  rightWidth: PropTypes.number,
};

const ChangeSvg = () => (
  <svg width="25" height="24" viewBox="0 0 25 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <g opacity="0.6">
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M2.75 5C2.75 4.58579 3.08579 4.25 3.5 4.25H11C11.4142 4.25 11.75 4.58579 11.75 5C11.75 5.41421 11.4142 5.75 11 5.75H4.25V18.25H7.5C7.91421 18.25 8.25 18.5858 8.25 19C8.25 19.4142 7.91421 19.75 7.5 19.75H3.5C3.08579 19.75 2.75 19.4142 2.75 19V5ZM16.75 5C16.75 4.58579 17.0858 4.25 17.5 4.25H21.5C21.9142 4.25 22.25 4.58579 22.25 5V19C22.25 19.4142 21.9142 19.75 21.5 19.75H14C13.5858 19.75 13.25 19.4142 13.25 19C13.25 18.5858 13.5858 18.25 14 18.25H20.75V5.75H17.5C17.0858 5.75 16.75 5.41421 16.75 5ZM14.2745 16.7153C13.9623 16.6169 13.75 16.3274 13.75 16V5C13.75 4.58579 14.0858 4.25 14.5 4.25C14.9142 4.25 15.25 4.58579 15.25 5L15.25 13.6207L15.8856 12.7128C16.1231 12.3734 16.5908 12.2909 16.9301 12.5284C17.2694 12.766 17.352 13.2336 17.1144 13.573L15.1144 16.4301C14.9267 16.6983 14.5867 16.8137 14.2745 16.7153ZM11.25 8C11.25 7.67265 11.0377 7.38311 10.7255 7.2847C10.4133 7.18628 10.0733 7.30173 9.88558 7.5699L7.88558 10.427C7.64804 10.7664 7.73057 11.234 8.0699 11.4716C8.40924 11.7091 8.87689 11.6266 9.11442 11.2872L9.75 10.3793V19C9.75 19.4142 10.0858 19.75 10.5 19.75C10.9142 19.75 11.25 19.4142 11.25 19V8Z"
        fill="var(--theme-font-100)"
      />
    </g>
  </svg>
);

export default SegmentsEditor;

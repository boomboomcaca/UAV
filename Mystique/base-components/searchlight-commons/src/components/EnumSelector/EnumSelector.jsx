import React, { useState, useEffect, useRef, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { useUpdateEffect, useClickAway } from 'ahooks';
import { NewArrowLeftSetIcon, NewArrowRightSetIcon } from 'dc-icon';
import { message } from 'dui';
import langT from 'dc-intl';
import CenterDrop from './components/CenterDrop.jsx';
import { back, select } from './icons.jsx';
import useWindowSize from './useWindowSize';
import styles from './index.module.less';
import triangle from './iconsPng/triangle.png';

const EnumSelector = (props) => {
  // caption 标签文本信息，如设置channelItems则设置此属性无效
  // value 默认选中的值  如：150
  // items  带宽选择：[{value,display}]
  // levelItems 二级选择 [{caption, items:[{value,display}]}]
  // onValueChanged 值变更事件
  //  position 组件位于页面靠左还是靠右，用于决定弹出框方向（“left”,"right"）
  // lightUp 控制开关灯与否
  const {
    caption,
    value,
    items,
    levelItems,
    onValueChanged,
    onCaptionValueChanged,
    options,
    disable,
    dealSame,
    position,
    lightUp,
    keyBoardType,
    onClickSearch,
  } = props;

  // 当前选中的一级选项
  const [captionSelIndx, setCaptionSelIndx] = useState(-1);

  // 当前选中的二级选项
  const [levelSelIndx, setLevelSelIndx] = useState(-1);
  // 当前是否为二级选择
  const [subLevel, setSubLevel] = useState(levelItems !== undefined);
  // 显示弹出层
  const [showDrawer, setShowDrawer] = useState(false);
  // 显示弹出层2
  const [showPop, setShowPop] = useState(false);
  const subRootRef = useRef(null);

  const [height, setHeight] = useState(0);

  // const [lightUp, setLightUp] = useState(false);

  const [captionShow, setCaptionShow] = useState(false);

  const [levelShow, setLevelShow] = useState(false);

  //  把items值按从小到大的顺序重新组合排列
  const [sortItems, setSortItems] = useState([]);

  const selectRef = useRef(null);

  const levelRef = useRef(null);

  const captionRef = useRef(null);
  const dropLeftStyles = {
    left: '-24px',
    top: '36px',
  };
  const dropRightStyles = {
    right: '0px',
    top: '36px',
  };
  const dropBottomStyles = {
    left: '-154px',
    bottom: '36px',
  };
  const sinLeftStyles = {
    left: '88px',
    top: '-11px',
  };
  const sinRightStyles = {
    right: '72px',
    top: '-11px',
  };
  const sinBottomStyles = {
    right: '50%',
    bottom: '-11px',
    transform: 'rotate(180deg)',
  };
  const typeNameColor = [
    { color: '#353d5b', background: '#FFD118' },
    { color: '#353d5b', background: '#35E065' },
    { color: '#353d5b', background: '#BAE637' },
    { color: '#353d5b', background: '#69C0FF' },
    { color: '#353d5b', background: '#5CDBD3' },
  ];
  useEffect(() => {
    // 处理默认值
    let hasSub = false;
    if (levelItems) {
      hasSub = true;
      let newIndx = -1;
      let newCapIndx = captionSelIndx;
      if (levelItems.length === 0) {
        newCapIndx = -1;
      }
      if (newCapIndx > -1) {
        newIndx = levelItems[newCapIndx]?.items?.findIndex((v) => v.value === value);
      }
      if (newIndx < 0) {
        newCapIndx = -1;
        for (let i = 0; i < levelItems.length; i += 1) {
          const index = levelItems[i]?.items?.findIndex((v) => v.value === value);
          if (index > -1) {
            newCapIndx = i;
            newIndx = index;
          }
        }
      }
      //  判断是否亮灯
      // setLightUp(newIndx > -1 && newCapIndx > -1);
      setCaptionSelIndx(newCapIndx);
      setLevelSelIndx(newIndx);
    } else if (items && items.length > 0) {
      items.sort((a, b) => {
        return a.value - b.value;
      });
      setSortItems(items);
      const indx = items.findIndex((v) => {
        return v.value === value;
      });
      setLevelSelIndx(indx);
    }

    setSubLevel(hasSub);
  }, [levelItems, items, value]);
  /**
   * 监听内部二级菜单选择索引
   * 出发事件，通知选择变更
   */
  useUpdateEffect(() => {
    if (onValueChanged && levelSelIndx > -1) {
      const val = subLevel
        ? captionSelIndx > -1 && levelItems[captionSelIndx]?.items && levelItems[captionSelIndx]?.items.length > 0
          ? levelItems[captionSelIndx]?.items[levelSelIndx].value
          : null
        : items[levelSelIndx].value;
      if (dealSame) {
        if (val && val !== value) {
          onValueChanged(levelSelIndx, val);
        }
      } else if (val) {
        onValueChanged(levelSelIndx, val);
      }
    }
  }, [levelSelIndx]);
  useUpdateEffect(() => {
    if (
      onCaptionValueChanged &&
      captionSelIndx >= 0 &&
      levelItems[captionSelIndx]?.items &&
      levelItems[captionSelIndx]?.items.length > 0
    ) {
      onCaptionValueChanged(captionSelIndx, levelItems[captionSelIndx].caption);
      if (levelSelIndx > -1) {
        const val = subLevel ? levelItems[captionSelIndx]?.items[levelSelIndx].value : items[levelSelIndx].value;
        if (onValueChanged) {
          if (dealSame) {
            if (val !== value) {
              onValueChanged(levelSelIndx, val);
            }
          } else {
            onValueChanged(levelSelIndx, val);
          }
        }
      }
    }
  }, [captionSelIndx]);

  const backStep = () => {
    if (subLevel) {
      const curCaptionIndx = captionSelIndx < 0 ? 0 : captionSelIndx;
      if (captionSelIndx < 0) {
        setCaptionSelIndx(curCaptionIndx);
      }
      if (curCaptionIndx >= 0 && levelSelIndx > 0) {
        const newVal = levelSelIndx - 1;
        setLevelSelIndx(newVal);
      } else {
        const newCaptionSel = curCaptionIndx > 0 && levelSelIndx === 0 ? curCaptionIndx - 1 : levelItems.length - 1;
        setCaptionSelIndx(newCaptionSel);
        setLevelSelIndx(levelItems[newCaptionSel]?.items.length - 1);
      }
    } else if (levelSelIndx > 0) {
      const newVal = levelSelIndx - 1;
      setLevelSelIndx(newVal);
    }
  };

  const nextStep = () => {
    if (subLevel) {
      const curCaptionIndx = captionSelIndx < 0 ? 0 : captionSelIndx;
      if (captionSelIndx < 0) {
        setCaptionSelIndx(curCaptionIndx);
      }
      if (levelSelIndx < levelItems[curCaptionIndx]?.items?.length - 1) {
        const newVal = levelSelIndx + 1;
        setLevelSelIndx(newVal);
      } else {
        const newCaptionSel = curCaptionIndx + 1 >= levelItems.length ? 0 : curCaptionIndx + 1;
        setCaptionSelIndx(newCaptionSel);
        setLevelSelIndx(0);
      }
    } else if (levelSelIndx < items.length - 1) {
      const newVal = levelSelIndx < items.length - 1 ? levelSelIndx + 1 : 0;
      setLevelSelIndx(newVal);
    }
  };
  const iconSize = 24;

  const selDivRef = useRef(null);
  const almostDone = () => {
    if (selDivRef.current) {
      setTimeout(() => {
        getSubHeight();
        const scroll = document.getElementById(selDivRef.current);
        scroll?.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        });
      }, 300);
    }
  };

  const getSubHeight = () => {
    if (subRootRef.current && levelItems && levelItems.length > 0) {
      let he = subRootRef.current.offsetHeight - levelItems.length * 57;
      if (he < 500) {
        he = 500;
      }
      setHeight(he);
    }
  };
  //  因为显示要按类型分组，所以重构数据
  const gooditem = useMemo(() => {
    sortItems.map((e, index) => {
      e.itemIdx = index;
      return e;
    });
    const newItems = [];
    const type1 = sortItems.filter((e) => e.value < 15);
    type1.length > 0 && newItems.push({ typeNum: 0, type: langT('enumSelector', '8000'), data: type1 });
    const type2 = sortItems.filter((e) => e.value >= 15 && e.value < 120);
    type2.length > 0 && newItems.push({ typeNum: 1, type: langT('enumSelector', '8001'), data: type2 });
    const type3 = sortItems.filter((e) => e.value >= 120 && e.value < 500);
    type3.length > 0 && newItems.push({ typeNum: 2, type: langT('enumSelector', '8002'), data: type3 });
    const type4 = sortItems.filter((e) => e.value >= 500 && e.value < 40000);
    type4.length > 0 && newItems.push({ typeNum: 3, type: langT('enumSelector', '8003'), data: type4 });
    const type5 = sortItems.filter((e) => e.value >= 40000);
    type5.length > 0 && newItems.push({ typeNum: 4, type: langT('enumSelector', '8004'), data: type5 });
    return newItems;
  }, [sortItems]);

  const size = useWindowSize();

  useEffect(() => {
    getSubHeight();
  }, [size]);

  //  点击别的地方蒙版消失
  useClickAway(() => {
    popDisappear();
  }, selectRef);
  const popDisappear = () => {
    setTimeout(() => {
      setShowPop(false);
    }, 150);
  };
  useClickAway(() => {
    captionDisappear();
  }, captionRef);
  useClickAway(() => {
    levelDisappear();
  }, levelRef);
  const onCaptionChange = (e) => {
    if (captionSelIndx === e) {
      captionDisappear();
      return;
    }
    setCaptionSelIndx(e);
    //  选中信道业务以后值默认选第一个
    const selindex = levelItems[e]?.items?.length > 0 ? 0 : -1;
    setLevelSelIndx(selindex);
    captionDisappear();
  };
  const captionDisappear = () => {
    setTimeout(() => {
      setCaptionShow(false);
    }, 150);
  };
  const levelDisappear = () => {
    setTimeout(() => {
      setLevelShow(false);
    }, 150);
  };
  const onLevelChange = (e) => {
    setLevelSelIndx(e);
    levelDisappear();
  };
  return (
    <div className={classnames(styles.dongGeRoot, disable && styles.dongRootDisable)}>
      {!subLevel ? <div className={styles.titleBox}>{caption}</div> : undefined}
      <div style={{ gap: '8px' }} className={styles.rootContent}>
        <div
          className={styles.icon}
          style={{ opacity: disable ? 0.5 : 1 }}
          onClick={() => {
            if (!disable) backStep();
          }}
        >
          {options.leftIcon ? (
            <div className={levelSelIndx > 0 ? styles.pager : styles.pagerDisable}>
              <image src={options.leftIcon} alt="" />
            </div>
          ) : (
            <NewArrowLeftSetIcon
              iconSize={iconSize}
              style={{ alignItems: 'center' }}
              color={
                (subLevel && levelItems.length > 0) || (!subLevel && items.length > 0 && levelSelIndx > 0)
                  ? 'var(--theme-primary)'
                  : 'var(--theme-primary-20)'
              }
            />
          )}
        </div>
        {subLevel ? (
          // 业务频段和信道中心频率设置
          <div className={styles.levelContentBox}>
            {/* lightUp */}
            <div
              style={{
                background: lightUp ? 'var(--theme-enumSelector-lightup-bg)' : 'var(--theme-enumSelector-lightdown-bg)',
              }}
              className={styles.leftLight}
            />
            <div className={styles.levelContent}>
              <div className={styles.levelLight} />

              <div
                onClick={() => {
                  // if (!disable) {
                  //   setShowDrawer(true);
                  //   setCaptionSelecting(captionSelIndx);
                  //   almostDone();
                  // }
                }}
                className={styles.levelMain}
              >
                <div style={{ position: 'relative' }}>
                  <div
                    ref={captionRef}
                    onClick={() => {
                      if (!disable) setCaptionShow(!captionShow);
                    }}
                    className={styles.mainName}
                  >
                    {captionSelIndx > -1 && levelItems.length > 0 && captionSelIndx <= levelItems.length - 1
                      ? levelItems[captionSelIndx]?.caption
                      : '--'}
                  </div>
                  <CenterDrop
                    position={position}
                    dataSource={levelItems}
                    type="caption"
                    visiable={captionShow}
                    value={captionSelIndx}
                    onChange={(e) => onCaptionChange(e)}
                  />
                </div>

                <div className={styles.driver1} />
                <div ref={levelRef} className={styles.mainTexts}>
                  <div
                    onClick={() => {
                      if (!disable) {
                        if (captionSelIndx >= 0) {
                          if (levelItems[captionSelIndx].items) {
                            setLevelShow(!levelShow);
                          }
                        } else {
                          message.warning(langT('enumSelector', '8005'));
                        }
                      }
                    }}
                    className={styles.mainTextsCon}
                  >
                    {captionSelIndx > -1 &&
                    levelSelIndx > -1 &&
                    levelItems.length > 0 &&
                    levelItems[captionSelIndx]?.items &&
                    levelItems[captionSelIndx]?.items[levelSelIndx]?.name ? (
                      <>
                        <div className={styles.mainText1}>
                          {captionSelIndx > -1 && levelSelIndx > -1 && levelItems.length > 0
                            ? levelItems[captionSelIndx]?.items[levelSelIndx]?.name || '--'
                            : '--'}
                        </div>
                        <div className={styles.driver2} />
                      </>
                    ) : undefined}

                    <div className={styles.mainText2}>
                      {captionSelIndx > -1 &&
                      levelSelIndx > -1 &&
                      levelItems.length > 0 &&
                      levelItems[captionSelIndx]?.items
                        ? levelItems[captionSelIndx]?.items[levelSelIndx]?.display || '--'
                        : '--'}
                    </div>
                  </div>
                  <CenterDrop
                    onClickSearch={() => {
                      onClickSearch(
                        captionSelIndx > -1 && levelItems.length > 0 && levelItems[captionSelIndx]
                          ? levelItems[captionSelIndx]
                          : null,
                      );
                    }}
                    position={position}
                    dataSource={
                      captionSelIndx > -1 && levelItems.length > 0 && levelItems[captionSelIndx]
                        ? levelItems[captionSelIndx]?.items
                        : []
                    }
                    type="level"
                    isSearch={
                      captionSelIndx > -1 && levelItems.length > 0 && levelItems[captionSelIndx]
                        ? levelItems[captionSelIndx]?.isSearch
                        : false
                    }
                    visiable={levelShow}
                    value={levelSelIndx}
                    onChange={(e) => onLevelChange(e)}
                  />
                </div>
              </div>
            </div>
            <div
              style={{
                background: lightUp ? 'var(--theme-enumSelector-lightup-bg)' : 'var(--theme-enumSelector-lightdown-bg)',
              }}
              className={styles.rightLight}
            />
          </div>
        ) : (
          // 带宽设置
          <div ref={selectRef} className={styles.normalContent}>
            <div className={styles.normalSpace}>
              <span
                className={styles.valuelabel}
                onClick={() => {
                  if (!disable) {
                    setShowPop(!showPop);
                    almostDone();
                  }
                }}
              >
                {levelSelIndx > -1 && sortItems.length > 0 ? sortItems[levelSelIndx]?.display || '--' : '--'}
              </span>
            </div>
            {/* 弹出框选择带宽 start  */}
            <div
              className={classnames(styles.hide, showPop ? styles.drop : null)}
              style={position === 'left' ? dropLeftStyles : position === 'right' ? dropRightStyles : dropBottomStyles}
            >
              {/* {position === 'left' ? dropLeft : dropRight} */}

              <div
                style={{
                  marginTop: position === 'bottom' ? 'unset' : '11px',
                  marginBottom: position === 'bottom' ? '11px' : '',
                }}
                className={styles.dropBox}
              >
                <div
                  style={position === 'left' ? sinLeftStyles : position === 'right' ? sinRightStyles : sinBottomStyles}
                  className={styles.imgTriangle}
                  src={triangle}
                />
                <div className={styles.boxCon}>
                  {keyBoardType === 'simple' ? (
                    <div className={styles.concon}>
                      <div style={{ flexWrap: 'wrap' }} className={styles.buttons}>
                        {sortItems.map((op, index) => {
                          if (levelSelIndx === index) {
                            selDivRef.current = op.display;
                          }
                          return (
                            <div
                              onClick={() => {
                                setLevelSelIndx(index);
                                popDisappear();
                              }}
                              id={op.display}
                              key={op.display}
                              className={styles.button}
                              style={{
                                color: levelSelIndx === index ? 'var(--theme-primary)' : 'var(--theme-font-100)',
                                fontWeight: levelSelIndx === index ? 'bold' : 'normal',
                              }}
                            >
                              {levelSelIndx === index ? (
                                <div className={classnames(styles.selButton1, styles.selButton1Check)} />
                              ) : (
                                <div className={classnames(styles.selButton1, styles.selButton1NoCheck)} />
                              )}
                              <div className={styles.buttonText}>{op.display}</div>
                            </div>
                          );
                        })}
                      </div>
                    </div>
                  ) : (
                    <div className={styles.concon}>
                      {gooditem.map((e, index) => (
                        <div id={e.type} key={e.type} className={styles.buttonLine}>
                          <div className={styles.buttons}>
                            {e.data?.map((op) => {
                              if (levelSelIndx === op.itemIdx) {
                                selDivRef.current = op.display;
                              }
                              return (
                                <div
                                  onClick={() => {
                                    setLevelSelIndx(op.itemIdx);
                                    popDisappear();
                                  }}
                                  id={op.display}
                                  key={op.display}
                                  className={styles.button}
                                  style={{
                                    color:
                                      levelSelIndx === op.itemIdx ? 'var(--theme-primary)' : 'var(--theme-font-100)',
                                    fontWeight: levelSelIndx === op.itemIdx ? 'bold' : 'normal',
                                  }}
                                >
                                  {levelSelIndx === op.itemIdx ? (
                                    <div className={classnames(styles.selButton1, styles.selButton1Check)} />
                                  ) : (
                                    <div className={classnames(styles.selButton1, styles.selButton1NoCheck)} />
                                  )}
                                  <div className={styles.buttonText}>{op.display}</div>
                                </div>
                              );
                            })}
                          </div>
                          <div style={typeNameColor[e.typeNum]} className={styles.typeName}>
                            {e.type}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </div>
            {/* 弹出框选择带宽 end  */}
          </div>
        )}
        <div
          className={styles.icon}
          style={{ opacity: disable ? 0.5 : 1 }}
          onClick={() => {
            if (!disable) nextStep();
          }}
        >
          {options.rightIcon ? (
            <div>
              <image src={options.rightIcon} alt="" />
            </div>
          ) : (
            <NewArrowRightSetIcon
              iconSize={iconSize}
              style={{ alignItems: 'center' }}
              color={
                (subLevel && levelItems.length > 0) ||
                (!subLevel && items.length > 0 && levelSelIndx < items.length - 1)
                  ? 'var(--theme-primary)'
                  : 'var(--theme-primary-20)'
              }
            />
          )}
        </div>
      </div>
    </div>
  );
};

EnumSelector.defaultProps = {
  caption: '',
  value: -99999,
  items: [],
  levelItems: undefined,
  onValueChanged: () => {},
  options: {},
  disable: false,
  dealSame: false,
  position: 'center',
  lightUp: false,
  onCaptionValueChanged: () => {},
  keyBoardType: 'complex',
  onClickSearch: () => {},
};

EnumSelector.propTypes = {
  caption: PropTypes.string,
  value: PropTypes.any,
  items: PropTypes.array,
  levelItems: PropTypes.array,
  onValueChanged: PropTypes.func,
  options: PropTypes.object,
  disable: PropTypes.bool,
  dealSame: PropTypes.bool,
  lightUp: PropTypes.bool,
  position: PropTypes.string,
  onCaptionValueChanged: PropTypes.func,
  keyBoardType: PropTypes.any,
  onClickSearch: PropTypes.func,
};

export default EnumSelector;

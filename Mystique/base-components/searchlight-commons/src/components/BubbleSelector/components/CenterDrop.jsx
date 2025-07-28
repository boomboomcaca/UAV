/*
 * @Author: wangXueDong
 * @Date: 2022-02-14 15:53:38
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-07-07 17:23:27
 */
import React, { useEffect, useState, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import langT from 'dc-intl';
import { back, select } from '../icons.jsx';
import styles from './center.module.less';
import triangle from '../iconsPng/triangle.png';
import empty from '../iconsPng/empty-icon.png';

const centerDrop = (props) => {
  const { visiable, position, dataSource, value, onClose, onChange, keyBoardType } = props;
  const [sortItems, setSortItems] = useState([]);
  const [selIndx, setSelIndx] = useState(0);
  const dropLeftStyles = {
    left: '0px',
  };
  const dropRightStyles = {
    right: '0px',
  };
  const dropCenterStyles = {
    left: `calc(-232px + 50%)`,
  };
  const sinLeftStyles = {
    left: '38px',
  };
  const sinRightStyles = {
    right: '38px',
  };
  const sinCenterStyles = {
    right: '50%',
  };
  useEffect(() => {
    const datas = [...dataSource];
    if (datas && datas.length > 0) {
      datas.sort((a, b) => {
        return a.value - b.value;
      });
      setSortItems(datas);
      const indx = datas.findIndex((v) => {
        return v.value === value;
      });
      setSelIndx(indx);
    }
  }, [dataSource, value]);
  //  因为显示要按类型分组，所以重构数据
  const gooditem = useMemo(() => {
    sortItems.map((e, index) => {
      e.itemIdx = index;
      return e;
    });
    const newItems = [];
    const type1 = sortItems.filter((e) => e.value < 50);
    type1.length > 0 && newItems.push({ typeNum: 0, type: langT('enumSelector', '8006'), data: type1 });
    const type2 = sortItems.filter((e) => e.value >= 50 && e.value < 1000);
    type2.length > 0 && newItems.push({ typeNum: 1, type: langT('enumSelector', '8007'), data: type2 });
    const type3 = sortItems.filter((e) => e.value >= 1000);
    type3.length > 0 && newItems.push({ typeNum: 2, type: langT('enumSelector', '8008'), data: type3 });
    return newItems;
  }, [sortItems]);
  const typeNameColor = [
    { color: '#353d5b', background: '#FFD118' },
    { color: '#353d5b', background: '#35E065' },
    { color: '#353d5b', background: '#BAE637' },
    { color: '#353d5b', background: '#69C0FF' },
    { color: '#353d5b', background: '#5CDBD3' },
  ];
  return (
    <>
      {/* 弹出框选择带宽 start  */}
      <div
        className={classnames(
          styles.hide,
          visiable ? styles.drop : null,
          visiable ? styles.unsetHeight : styles.setHeight,
        )}
        style={position === 'left' ? dropLeftStyles : position === 'right' ? dropRightStyles : dropCenterStyles}
      >
        <div className={styles.dropBox}>
          <div
            style={position === 'left' ? sinLeftStyles : position === 'right' ? sinRightStyles : sinCenterStyles}
            className={styles.imgTriangle}
          />
          <div className={styles.boxCon}>
            {dataSource.length > 0 ? (
              <div className={styles.concon}>
                {keyBoardType === 'simple' ? (
                  <>
                    <div style={{ flexWrap: 'wrap' }} className={styles.buttons}>
                      {sortItems.map((op, index) => {
                        return (
                          <div
                            onClick={() => {
                              onChange(op);
                              onClose();
                            }}
                            id={op.display}
                            key={op.display}
                            className={styles.button}
                            style={{
                              color: selIndx === index ? 'var(--theme-primary)' : 'var(--theme-font-100)',
                              fontWeight: selIndx === index ? 'bold' : 'normal',
                            }}
                          >
                            {selIndx === index ? (
                              <div className={classnames(styles.selButton1, styles.selButton1Check)} />
                            ) : (
                              <div className={classnames(styles.selButton1, styles.selButton1NoCheck)} />
                            )}
                            <div className={styles.buttonText}>{op.display}</div>
                          </div>
                        );
                      })}
                    </div>
                  </>
                ) : (
                  <>
                    {gooditem.map((e, index) => (
                      <div id={e.type} key={e.type} className={styles.buttonLine}>
                        <div className={styles.buttons}>
                          {e.data?.map((op) => {
                            return (
                              <div
                                onClick={() => {
                                  onChange(op);
                                  onClose();
                                }}
                                id={op.display}
                                key={op.display}
                                className={styles.button}
                                style={{
                                  color: selIndx === op.itemIdx ? 'var(--theme-primary)' : 'var(--theme-font-100)',
                                  fontWeight: selIndx === op.itemIdx ? 'bold' : 'normal',
                                }}
                              >
                                {selIndx === op.itemIdx ? (
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
                  </>
                )}
              </div>
            ) : (
              <div className={styles.emptyBox}>
                <img src={empty} alt="" />
              </div>
            )}
          </div>
        </div>
      </div>
      {/* 弹出框选择带宽 end  */}
    </>
  );
};

centerDrop.defaultProps = {
  dataSource: [],
  visiable: false,
  type: 'caption',
  value: '',
  onClose: () => {},
  onChange: () => {},
  position: 'left',
  keyBoardType: 'complex',
};

centerDrop.propTypes = {
  position: PropTypes.string,
  dataSource: PropTypes.any,
  visiable: PropTypes.any,
  type: PropTypes.any,
  value: PropTypes.any,
  onClose: PropTypes.func,
  onChange: PropTypes.func,
  keyBoardType: PropTypes.string,
};

export default centerDrop;

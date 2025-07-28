/*
 * @Author: wangXueDong
 * @Date: 2022-02-14 15:53:38
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-06-29 17:38:21
 */
import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ToSearchIcon } from 'dc-icon';
import { back1, select1, back21, back22, select2 } from '../icons.jsx';
import styles from './center.module.less';
import triangle from '../iconsPng/triangle.png';

const centerDrop = (props) => {
  const { dataSource, visiable, type, value, onChange, position, isSearch, onClickSearch } = props;
  const captionStyles = {
    dropLeftStyles: {
      left: '0px',
    },
    dropRightStyles: {
      right: '0px',
    },
    dropCenterStyles: {
      left: '-247px',
    },
    sinLeftStyles: {
      left: '38px',
    },
    sinRightStyles: {
      right: '38px',
    },
    sinCenterStyles: {
      right: '50%',
    },
  };
  const levelStyles = {
    dropLeftStyles: {
      left: '0px',
    },
    dropRightStyles: {
      right: '0px',
    },
    dropCenterStyles: {
      left: 'calc(-508px + 50%)',
    },
    sinLeftStyles: {
      left: '38px',
    },
    sinRightStyles: {
      right: '38px',
    },
    sinCenterStyles: {
      right: '50%',
    },
  };
  return (
    <>
      {type === 'caption' ? (
        // 业务选择
        <div
          style={
            position === 'center'
              ? captionStyles.dropCenterStyles
              : position === 'left'
              ? captionStyles.dropLeftStyles
              : captionStyles.dropRightStyles
          }
          className={classnames(styles.hide, styles.width1, visiable ? styles.drop : null)}
        >
          <div
            alt=""
            style={
              position === 'center'
                ? captionStyles.sinCenterStyles
                : position === 'left'
                ? captionStyles.sinLeftStyles
                : captionStyles.sinRightStyles
            }
            className={styles.imgTriangle}
            src={triangle}
          />
          <div className={styles.dropCapBox}>
            <div className={styles.boxCapCon}>
              <div className={styles.conCapcon}>
                <div className={styles.capButtons}>
                  {dataSource.map((op, index) => {
                    return (
                      <div
                        onClick={() => {
                          onChange(index);
                        }}
                        id={op.caption}
                        key={op.caption}
                        className={styles.button}
                      >
                        {value === index ? (
                          <div className={classnames(styles.selButton3, styles.selButton3Check)} />
                        ) : (index + 1) % 2 === 0 ? (
                          <div className={classnames(styles.selButton3, styles.selButton3NoCheck_2)} />
                        ) : (
                          <div className={classnames(styles.selButton3, styles.selButton3NoCheck_1)} />
                        )}
                        <div style={{ fontSize: '16px' }} className={styles.buttonText}>
                          <div
                            style={{
                              color:
                                value === index
                                  ? 'var(--theme-primary)'
                                  : (index + 1) % 2 === 0
                                  ? 'var(--theme-enumSelector-captionTtilte1)'
                                  : 'var(--theme-enumSelector-captionTtilte2)',
                            }}
                            className={styles.capBox}
                          >
                            {op.caption}
                          </div>
                          <div
                            style={{
                              color: value === index ? 'var(--theme-primary)' : 'var(--theme-font-80)',
                            }}
                            className={styles.remarkBox}
                          >
                            <div
                              className={classnames(
                                styles.remarkBoxCon,
                                (index + 1) % 2 === 0 ? styles.yellowCon : styles.blueCon,
                              )}
                            >
                              {op.remark}
                            </div>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          </div>
        </div>
      ) : (
        // 频率选择
        <div
          style={
            position === 'center'
              ? levelStyles.dropCenterStyles
              : position === 'left'
              ? levelStyles.dropLeftStyles
              : levelStyles.dropRightStyles
          }
          className={classnames(styles.hide, styles.width2, visiable ? styles.drop : null)}
        >
          <div
            alt=""
            style={
              position === 'center'
                ? levelStyles.sinCenterStyles
                : position === 'left'
                ? levelStyles.sinLeftStyles
                : levelStyles.sinRightStyles
            }
            className={styles.imgTriangle}
            src={triangle}
          />
          <div className={styles.dropBox}>
            <div className={styles.boxCon}>
              <div className={styles.concon}>
                <div className={styles.buttons}>
                  {isSearch ? (
                    <div
                      onClick={() => {
                        onClickSearch();
                      }}
                      className={styles.button}
                      style={{
                        color: '#FFFFFF',
                        fontWeight: 'normal',
                      }}
                    >
                      {back1}
                      <div style={{ fontSize: '14px' }} className={styles.buttonText}>
                        <div
                          style={{
                            color: 'rgba(255, 255, 255, 0.8)',
                            fontSize: '14px',
                            display: 'flex',
                            alignItems: 'center',
                          }}
                        >
                          <ToSearchIcon color="#3CE5D3" />
                          <span style={{ marginLeft: '6px' }}>搜索</span>
                        </div>
                      </div>
                    </div>
                  ) : undefined}

                  {dataSource.map((op, index) => {
                    return (
                      <div
                        onClick={() => {
                          onChange(index);
                        }}
                        id={op.display}
                        key={op.display}
                        className={styles.button}
                        style={{
                          color: value === index ? 'var(--theme-primary)' : 'var(--theme-font-100)',
                          fontWeight: value === index ? 'bold' : 'normal',
                        }}
                      >
                        {value === index ? (
                          <div className={classnames(styles.selButton2, styles.selButton2Check)} />
                        ) : (
                          <div className={classnames(styles.selButton2, styles.selButton2NoCheck)} />
                        )}
                        <div style={{ fontSize: '14px' }} className={styles.buttonText}>
                          <div>{op.display}</div>

                          <div
                            style={{ color: value === index ? 'var(--theme-primary)' : 'var(--theme-font-50)' }}
                            className={styles.textP}
                          >
                            {op.name}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </>
  );
};

centerDrop.defaultProps = {
  dataSource: [],
  visiable: false,
  type: 'caption',
  value: '',
  position: 'center',
  isSearch: false,
  onClickSearch: () => {},
};

centerDrop.propTypes = {
  dataSource: PropTypes.any,
  visiable: PropTypes.any,
  type: PropTypes.any,
  value: PropTypes.any,
  position: PropTypes.string,
  isSearch: PropTypes.any,
  onClickSearch: PropTypes.func,
};

export default centerDrop;

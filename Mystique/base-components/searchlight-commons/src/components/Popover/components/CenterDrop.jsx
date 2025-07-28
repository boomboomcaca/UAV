/*
 * @Author: wangXueDong
 * @Date: 2022-02-14 15:53:38
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-04-28 17:56:57
 */
import React, { useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './center.module.less';
import triangle from '../iconsPng/triangle.png';

const centerDrop = (props) => {
  const { visiable, position, onClose, content, popoverWidth, popoverHeight } = props;
  const [dropStyle, setDropStyle] = useState({});
  const dropRef = useRef(null);
  useEffect(() => {
    const dropLeftStyles = {
      left: '0px',
      top: `${popoverHeight}px`,
    };
    const dropRightStyles = {
      right: '0px',
      top: `${popoverHeight}px`,
    };
    const dropCenterStyles = {
      left: `calc(-${dropRef.current.clientWidth / 2}px + 50%)`,
      top: `${popoverHeight}px`,
    };
    const sinLeftStyles = {
      left: `${popoverWidth / 2}px`,
    };
    const sinRightStyles = {
      right: `${popoverWidth / 2}px`,
    };
    const sinCenterStyles = {
      right: '50%',
    };
    setDropStyle({
      dropLeftStyles,
      dropRightStyles,
      dropCenterStyles,
      sinLeftStyles,
      sinRightStyles,
      sinCenterStyles,
    });
  }, [popoverWidth, popoverHeight, dropRef]);

  return (
    <>
      <div
        ref={dropRef}
        className={classnames(
          styles.hide,
          visiable ? styles.drop : null,
          visiable ? styles.heightUnset : styles.heightNo,
        )}
        style={
          position === 'left'
            ? dropStyle.dropLeftStyles
            : position === 'right'
            ? dropStyle.dropRightStyles
            : dropStyle.dropCenterStyles
        }
      >
        <div className={styles.dropBox}>
          <img
            alt=""
            style={
              position === 'left'
                ? dropStyle.sinLeftStyles
                : position === 'right'
                ? dropStyle.sinRightStyles
                : dropStyle.sinCenterStyles
            }
            className={styles.imgTriangle}
            src={triangle}
          />
          <div className={styles.boxCon}>{content}</div>
        </div>
      </div>
      {/* 弹出框选择带宽 end  */}
    </>
  );
};

centerDrop.defaultProps = {
  visiable: false,
  onClose: () => {},
  position: 'left',
  content: null,
  popoverWidth: null,
  popoverHeight: null,
};

centerDrop.propTypes = {
  popoverWidth: PropTypes.any,
  popoverHeight: PropTypes.any,
  position: PropTypes.string,
  visiable: PropTypes.any,
  onClose: PropTypes.func,
  content: PropTypes.any,
};

export default centerDrop;

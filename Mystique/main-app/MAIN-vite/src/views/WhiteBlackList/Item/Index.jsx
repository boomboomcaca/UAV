import React, { useState } from "react";
import PropTypes from "prop-types";
import { DelIcon } from "../../SystemConfig/WhiteRegions/RegionList/Icons";
import styles from "./style.module.less";

/**
 *
 * @param {{recordInfo:{id:String,sn:String,productor:String,model:String,type:Number,admin:String,remark:String},onDelete:Function}} props
 * @returns
 */
const CardItem = (props) => {
  const { recordInfo, title, onDelete } = props;

  return (
    <div className={styles.itemRoot}>
      <div className={styles.header}>
        <div className={styles.title}>
          <div
            className={`${styles.iconbase} ${
              recordInfo.type === 0 ? styles.iconBlack : styles.iconWhite
            }`}
          />
          <div className={styles.titleText}> {title}</div>
        </div>
        <div className={styles.buttons} onClick={() => onDelete(recordInfo.id)}>
          <DelIcon size="100%" opacity="1" />
        </div>
      </div>
      <div className={styles.infoTable}>
        <div className={styles.infoRow}>
          <div className={styles.infoCell}>
            <span>电子指纹</span>
            <div>{recordInfo.sn || ""}</div>
          </div>
          <div className={styles.infoCell}>
            <span>生产厂商</span>
            <div>{recordInfo.productor || ""}</div>
          </div>
        </div>
        <div className={styles.infoRow}>
          <div className={styles.infoCell}>
            <span>型号</span>
            <div>{recordInfo.model || ""}</div>
          </div>
          <div className={styles.infoCell}>
            <span>性质</span>
            <div>{recordInfo.type === 1 ? "白名单" : "黑名单"}</div>
          </div>
        </div>
        <div className={styles.infoRow}>
          {recordInfo.type === 1 && (
            <div className={styles.infoCell}>
              <span>管理员</span>
              <div>{recordInfo.admin}</div>
            </div>
          )}
          <div className={styles.infoCell}>
            <span>备注</span>
            <div>{recordInfo.remark || ""}</div>
          </div>
        </div>
      </div>
    </div>
  );
};

CardItem.defaultProps = {
  recordInfo: {},
  onDelete: () => {},
};

CardItem.propTypes = { recordInfo: PropTypes.object, onDelete: PropTypes.func };

export default CardItem;

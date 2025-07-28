import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import { Tooltip } from "react-tooltip";

import { AddIcon, DelIcon } from "./Icons";

import styles from "./style.module.less";

/**
 *
 * @param {{dataList:Array<{name:String}>,visibles:Array<String>}} props
 * @returns
 */
const RegionList = (props) => {
  const { dataList, visibles, onShow, onDel, onAdd, editing } = props;

  return (
    <div className={styles.items}>
      <div
        className={`${styles.itemCon} ${
          (!dataList || dataList.length === 0) && styles.nodata
        }`}
      >
        {dataList.map((item) => {
          return (
            <div
              className={`${styles.item} ${
                visibles.includes(item.name) && styles.show
              }`}
              onClick={() => onShow(item.name)}
            >
              <span className={styles.name}>{item.name}</span>
              {/* <div
                className={`${styles.view} ${
                  visibles.includes(item.name) && styles.show
                }`}
                onClick={() => onShow(item.name)}
              /> */}
              <div className={styles.del} onClick={() => onDel(item.name)}>
                <DelIcon />
              </div>
            </div>
          );
        })}
      </div>
      <div
        className={`${styles.add} ${editing && styles.disable}`}
        onClick={() => onAdd()}
      >
        <div>
          <AddIcon />
        </div>
        添加
      </div>

      <Tooltip id="my-tooltip2" />
    </div>
  );
};

RegionList.defaultProps = {
  dataList: [],
  visibles: [],
  editing: false,
  onShow: () => {},
  onDel: () => {},
  onAdd: () => {},
};

RegionList.propTypes = {
  dataList: PropTypes.array,
  visibles: PropTypes.array,
  onShow: PropTypes.func,
  onDel: PropTypes.func,
  onAdd: PropTypes.func,
  editing: PropTypes.bool,
};

export default RegionList;

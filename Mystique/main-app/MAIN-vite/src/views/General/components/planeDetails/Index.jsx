import React, { useEffect, useRef, useState } from "react";
import PropTypes from "prop-types";
import classNames from "classnames";
import { Button1, message } from "dui";
// import { getColumns, getData } from "./demo.jsx";
import Modal from "../../../../components/Modal/Index.jsx";
import { mainConf } from "../../../../config/index.js";
import { addWhiteList } from "../../../../api/newServer.js";
import styles from "./style.module.less";

const PlaneDetails = (props) => {
  const { detailInfo, show, className, onClose } = props;
  const [addingWhite, setAddingWhite] = useState(false);
  const [renderDetail, setRenderDetail] = useState(
    mainConf.mock
      ? {
          productTypeStr: "dji3",
          droneLatitude: 30.715841987656674,
          droneLongitude: 104.093110817389,
          droneSerialNum: "12342134",
          altitude: 0,
          height: 500,
          homeLatitude: 30.71679727281,
          homeLongitude: 104.094510129794,
          eastSpeed: 10,
          northSpeed: 140,
          pilotLatitude: 30.7167981,
          pilotLongitude: 104.094129794,
        }
      : {}
  );

  useEffect(() => {
    console.log("detailInfo change::::", detailInfo);
    if (!mainConf.mock) {
      setRenderDetail(detailInfo);
    }
    // TODO
  }, [detailInfo]);

  return (
    <div
      className={classNames(
        styles.alarmList,
        show ? styles.show : styles.hide,
        className
      )}
    >
      <Modal
        title={`型号${renderDetail.productTypeStr}`}
        onClose={() => onClose()}
        headChild={
          <Button1
            // size="large"
            disabled={addingWhite}
            style={{ margin: 0, padding: "0 8px", width: "72px" }}
            type="primary"
            onClick={() => {
              if (renderDetail?.droneSerialNum) {
                setAddingWhite(true);
                addWhiteList(renderDetail.droneSerialNum)
                  .then((res) => {
                    message.info("已添加");
                  })
                  .catch((er) => {
                    console.log("add white list error:::", er);
                    message.error("添加失败");
                  });
                setTimeout(() => {
                  setAddingWhite(false);
                }, 1000);
              }
            }}
          >
            加入白名单
          </Button1>
        }
      >
        <div className={styles.tablCon}>
          {/* <div className={styles.tbTitle}>型号(Air 2s)</div> */}
          <div className={styles.tbrow}>
            <div className={styles.tbcell}>
              电子指纹：<span>{renderDetail.droneSerialNum}</span>
            </div>
            <div className={styles.tbcell}>
              当前位置：
              <span>{`${renderDetail.droneLongitude?.toFixed(
                6
              )},${renderDetail.droneLatitude?.toFixed(6)}`}</span>
            </div>
          </div>
          <div className={styles.tbrow}>
            <div className={styles.tbcell}>
              飞手位置：
              <span>{`${renderDetail.pilotLongitude?.toFixed(
                6
              )},${renderDetail.pilotLatitude?.toFixed(6)}`}</span>
            </div>
            <div className={styles.tbcell}>
              返航位置：
              <span>{`${renderDetail.homeLongitude?.toFixed(
                6
              )},${renderDetail.homeLatitude?.toFixed(6)}`}</span>
            </div>
          </div>
          <div className={styles.tbrow}>
            <div className={styles.tbcell}>
              {/* 飞行速度：<span>垂直(1m/s),水平(10m/s)</span> */}
              飞行速度：
              <span>{`东(${renderDetail.eastSpeed}m/s)，北(${renderDetail.northSpeed}m/s)`}</span>
            </div>
            <div className={styles.tbcell}>
              飞行高度：<span>{renderDetail.height}m</span>
            </div>
          </div>
        </div>
      </Modal>
    </div>
  );
};

PlaneDetails.defaultProps = {
  show: false,
  detailInfo: {},
  fit: "",
  onClose: () => {},
};

PlaneDetails.prototype = {
  show: PropTypes.bool,
  detailInfo: PropTypes.object,
  fit: PropTypes.any,
  onClose: PropTypes.func,
};

export default PlaneDetails;

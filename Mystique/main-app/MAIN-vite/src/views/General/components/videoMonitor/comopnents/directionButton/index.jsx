import React, { useState } from "react";
import PropTypes from "prop-types";
import {
  DownOutlined,
  UpOutlined,
  LeftOutlined,
  RightOutlined,
} from "@ant-design/icons";

import { TargetIcon, PlayIcon, StopIcon } from "dc-icon";
import { Tooltip } from "react-tooltip";
import styles from "./style.module.less";

const DirectionButton = (props) => {
  const { onChange, tracking } = props;
  const [isTracking, setTracking] = useState(true); // useState(tracking);
  return (
    <div className={styles.buttongroup}>
      <div className={styles.outtercircle}>
        <div
          className={`${styles.innerparts} ${styles.brown}`}
          onMouseDown={() => {
            onChange({ action: "up" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
        >
          <span className={styles.rotate}>
            <UpOutlined />
          </span>
        </div>
        <div
          className={`${styles.innerparts} ${styles.silver}`}
          onMouseDown={() => {
            onChange({ action: "right" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
        >
          <span className={styles.rotate}>
            <RightOutlined />
          </span>
        </div>
        <div
          className={`${styles.innerparts} ${styles.blue}`}
          onMouseDown={() => {
            onChange({ action: "left" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
        >
          <span className={styles.rotate}>
            <LeftOutlined />
          </span>
        </div>
        <div
          className={`${styles.innerparts} ${styles.gold}`}
          onMouseDown={() => {
            onChange({ action: "down" });
          }}
          onMouseUp={() => {
            onChange({ action: "stop" });
          }}
        >
          <span className={styles.rotate}>
            <DownOutlined />
          </span>
        </div>
      </div>
      <div
        className={styles.innercircle}
        onClick={(e) => {
          if (isTracking) {
            // setTracking(false);
            onChange({
              action: "stoptrack",
            });
          }
        }}
      >
        <div className={styles.okbutton}>
          <div className={styles.trackIcon}>
            <TargetIcon
              iconSize={42}
              color={isTracking ? "lightgray" : "gray"}
            />
          </div>
          {isTracking && (
            <div
              className={styles.statusIcon}
              data-tooltip-id="my-tooltip"
              data-tooltip-content="停止跟踪"
              data-tooltip-place="top"
            >
              <StopIcon iconSize={20} color="red" />
            </div>
          )}
        </div>
      </div>
      <Tooltip id="my-tooltip" />
    </div>
  );
};

DirectionButton.defaultProps = {
  onChange: () => {},
  tracking: true, // 让他随便点停止吧
};

DirectionButton.propTypes = {
  onChange: PropTypes.func,
  tracking: PropTypes.any,
};

export default DirectionButton;

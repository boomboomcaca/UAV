import React, { useState, useEffect } from "react";
import { Modal, message, Select, TimeScroll } from "dui";

import { RobotIcon, UserIcon } from "./Icons";
import styles from "./style.module.less";

const { Option } = Select;
const SwitchMode = (props) => {
  const [showModal, setShowModal] = useState(false);
  const [robotMode, setRobotMode] = useState(true);
  const [onDuty, setOnDuty] = useState();
  const [onDuty1, setOnDuty1] = useState();

  const [value, setValue] = useState({
    h: 5,
    m: 5,
    h1: 0,
    m1: 0,
  });

  const onChange = (val) => {
    if (val.h * 60 + val.m > val.h1 * 60 + val.m1) {
    }
    setValue(val);
  };

  return (
    <div className={styles.root}>
      <div
        className={`${styles.item} ${robotMode && styles.sel}`}
        onClick={() => {
          if (!robotMode) {
            setRobotMode(true);
            setOnDuty1(undefined);
            message.success("已切换到无人值守模式");
          } else {
            message.info("当前已经是无人值守模式");
          }
        }}
      >
        <div>
          <RobotIcon color={robotMode ? "#148BED" : null} />
        </div>
        <span>无人值守</span>
      </div>
      <div
        className={`${styles.item} ${!robotMode && styles.sel}`}
        onClick={() => {
          setOnDuty(onDuty1);
          setShowModal(true);
        }}
      >
        <div>
          <UserIcon color={!robotMode ? "#148BED" : null} />
        </div>
        <span>{onDuty1 ? `李查查${onDuty1}` : "--"}</span>
      </div>
      <Modal
        visible={showModal}
        title="设置值守信息"
        onCancel={() => {
          setShowModal(false);
        }}
        closable={false}
        onOk={(e) => {
          console.log("switch mode:::", e, onDuty);
          setRobotMode(false);
          setShowModal(false);
          setOnDuty1(onDuty);
        }}
      >
        <div className={styles.dutyConfig}>
          <div>
            <span>值守人员</span>
            <Select value={onDuty} onChange={(val) => setOnDuty(val)}>
              {[4, 1, 2, 3].map((item) => (
                <Option value={item}>{`李查查${item}`}</Option>
              ))}
            </Select>
          </div>
          <div>
            <span>交班时间</span>
            <TimeScroll
              valueList={value}
              onChange={onChange}
              rangeSelection={false}
            />
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default SwitchMode;

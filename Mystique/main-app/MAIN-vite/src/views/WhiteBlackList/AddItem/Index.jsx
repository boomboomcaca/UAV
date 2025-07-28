import React, { useState, useRef } from "react";
import PropTypes from "prop-types";
import { Input, Modal, message } from "dui";
import Switch from "../../General/components/videoMonitor/comopnents/switch/Index";

import styles from "./style.module.less";

const AddItem = (props) => {
  const { onOk, show, onCancel } = props;
  const firingOkRef = useRef(false);
  // productor:String,model:String,type:Number,admin:String,remark
  const inputTempRef = useRef();
  const valueRef = useRef({ id: new Date().getTime().toString() });
  const [isWhite, setIsWhite] = useState(false);
  const [fields] = useState([
    {
      name: "sn",
      label: "电子指纹",
    },
    { name: "productor", label: "生产商" },
    { name: "model", label: "型号" },
    { name: "type", label: "性质" },
    { name: "admin", label: "管理员" },
    { name: "remark", label: "备注" },
  ]);
  return (
    <Modal
      closable={false}
      visible={show}
      title="添加黑白名单"
      onCancel={() => onCancel()}
      onOk={() => {
        if (!valueRef.current["sn"]) {
          message.info("请输入无人机电子指纹");
          return;
        }
        if (!firingOkRef.current) {
          firingOkRef.current = true;
          setTimeout(() => {
            onOk(valueRef.current);
          }, 200);
        }
      }}
    >
      <div className={styles.addRoot}>
        {fields.map((item) => {
          return (
            <div className={styles.editItem}>
              <span>{item.label}</span>
              {item.name === "type" ? (
                <Switch
                  labels={["黑名单", "白名单"]}
                  onChange={(e) => {
                    setIsWhite(e);
                    valueRef.current[item.name] = e ? 1 : 0;
                  }}
                  value={isWhite}
                />
              ) : item.name === "remark" ? (
                <textarea
                  className={styles.txtArea}
                  rows="4"
                  cols="33"
                  onChange={(e) => (inputTempRef.current = e.target.value)}
                  onBlur={() =>
                    (valueRef.current[item.name] = inputTempRef.current)
                  }
                />
              ) : (
                <Input
                  className={styles.input00}
                  onChange={(e) => (inputTempRef.current = e)}
                  onBlur={() =>
                    (valueRef.current[item.name] = inputTempRef.current)
                  }
                />
              )}
            </div>
          );
        })}
      </div>
    </Modal>
  );
};

AddItem.defaultProps = {
  onOk: () => {},
  show: false,
  onCancel: () => {},
};

AddItem.propTypes = {
  onCancel: PropTypes.func,
  onOk: PropTypes.func,
  show: PropTypes.bool,
};

export default AddItem;

import React, { useEffect, useState } from "react";
import PropTypes from "prop-types";
import { Button, InputNumber, IconButton, Radio } from "dui";
import styles from "./item.module.less";

const ItemEditor = (props) => {
  const { type, config, onDraw, onOK } = props;

  const [editing, setEditing] = useState(false);
  const [editType, setEditType] = useState("");
  const [desLabel, setDesLabel] = useState("");
  useEffect(() => {
    setEditing(!config);
    if (config) {
      let description = "已设置";
      const { outRadius, alti, type } = config;
      if (type) {
        description =
          type === "refer"
            ? `保护区外围${outRadius}m，高度${alti}m`
            : "自定义区域";
      }
      setDesLabel(description);
    }
    console.log("region config:::", config);
  }, [config]);

  useEffect(() => {
    if (onDraw) {
      console.log(editType);
      onDraw(editType === "地图绘制" ? "ok" : undefined);
    }
  }, [editType, onDraw]);

  return (
    <div className={styles.itemRoot}>
      <div className={styles.content}>
        <div className={styles.des}>
          <span>
            {!config ? "请设置保护区" : editing ? "编辑中..." : desLabel}
          </span>
          {!editing && config && (
            <Button
              disabled={editing}
              onClick={() => setEditing(!editing)}
              style={{ color: "#FF0000e0", border: "solid thin #FF0000a0" }}
            >
              修改
            </Button>
          )}
        </div>
        <div className={styles.form}>
          <Radio
            options={
              type === "region0"
                ? ["导入GPS", "地图绘制"]
                : ["区域参数", "导入GPS", "地图绘制"]
            }
            value={editType}
            disabled={!editing}
            onChange={(e) => setEditType(e)}
          />
          {type !== "region0" && (
            <div className={styles.setItems}>
              <div className={styles.setItem}>
                <span>保护区外延（m）:</span>
                <InputNumber
                  value={
                    type === "region2" ? 2000 : type === "region3" ? 3000 : 5000
                  }
                  min={500}
                  max={8000}
                  step={100}
                  disabled={editType !== "区域参数" || !editing}
                />
              </div>
              <div className={styles.setItem}>
                <span>区域高度（m）:</span>
                <InputNumber
                  value={1000}
                  min={100}
                  max={2000}
                  step={100}
                  disabled={editType !== "区域参数" || !editing}
                />
              </div>
            </div>
          )}
        </div>
      </div>
      <div className={styles.btns}>
        {editing && (
          <>
            <Button
              style={{ width: "60px" }}
              onClick={() => {
                if (onOK) onOK(false);
                setEditType("");
                setEditing(false);
              }}
            >
              取消
            </Button>
            <Button
              style={{ width: "60px", marginLeft: "16px" }}
              onClick={() => {
                if (onOK) {
                  if (onOK(true)) {
                    setEditType("");
                    setEditing(false);
                  }
                }
              }}
            >
              确认
            </Button>
          </>
        )}
      </div>
    </div>
  );
};

ItemEditor.defaultProps = {
  type: "region0",
  config: undefined,
  onDraw: () => {},
  onOK: () => {},
};

ItemEditor.prototype = {
  type: PropTypes.string,
  config: undefined,
  onDraw: PropTypes.func,
  onOK: PropTypes.func,
};

export default ItemEditor;

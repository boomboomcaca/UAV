import React from "react";
import PropTypes from "prop-types";
import { Button1 } from "dui";

import styles from "./style.module.less";

/**
 *
 * @param {{steps:Array<{name:String,title:String}>,children:any}} props
 * @returns
 */
const StepWizard = (props) => {
  const { steps, stepValue, children, onPrev, onNext, onOk, onCancel } = props;
  return (
    <div className={styles.stepRoot}>
      <div className={styles.header}>
        <div className={styles.no}>
          {steps.map((item, index) => {
            return (
              <div
                className={`${styles.stepItem}`}
                style={{
                  flex: index === 0 || index === steps.length - 1 ? 2 : 3,
                }}
              >
                {index > 0 && (
                  <div
                    className={`${styles.stepLine} ${
                      stepValue.index >= index && styles.sel
                    }`}
                  />
                )}
                <div
                  className={`${styles.stepNo} ${
                    stepValue.index >= index && styles.sel
                  }`}
                >
                  {index + 1}
                </div>
                {index < steps.length - 1 && (
                  <div
                    className={`${styles.stepLine} ${
                      stepValue.index > index && styles.sel
                    }`}
                  />
                )}
              </div>
            );
          })}
        </div>
        <div className={styles.title}>
          {steps.map((item, index) => (
            <div
              style={{
                flex: index === 0 || index === steps.length - 1 ? 2 : 3,
              }}
            >
              {item.title}
            </div>
          ))}
        </div>
      </div>
      <div className={styles.stepContent}>{children}</div>
      <div className={styles.footer}>
        {/* <Button1 size="large">编辑</Button1> */}
        <span />
        <div className={styles.right}>
          <Button1
            size="large"
            disabled={stepValue.index === 0}
            style={{ width: "64px" }}
            onClick={() => {
              onCancel();
            }}
          >
            取消
          </Button1>
          <Button1
            size="large"
            style={{ width: "64px" }}
            disabled={stepValue.index === 0}
            onClick={() => {
              onPrev({ index: stepValue.index - 1 });
            }}
          >
            上一步
          </Button1>
          {stepValue.index < steps.length - 1 && (
            <Button1
              size="large"
              type="primary"
              disabled={stepValue.index >= steps.length - 1}
              style={{ width: "64px" }}
              onClick={() => {
                onNext({ index: stepValue.index + 1 });
              }}
            >
              下一步
            </Button1>
          )}

          {stepValue.index >= steps.length - 1 && (
            <Button1
              size="large"
              type="primary"
              style={{ width: "64px" }}
              onClick={() => {
                onOk();
              }}
            >
              完成
            </Button1>
          )}
        </div>
      </div>
    </div>
  );
};

StepWizard.defaultProps = {
  steps: [
    { name: "step1", title: "保护区(点)", index: 0 },
    { name: "step2", title: "识别处置区" },
    { name: "step3", title: "警戒区" },
    { name: "step4", title: "预警区" },
  ],
  stepValue: { index: 0 },
  //   children,
  onPrev: () => {},
  onNext: () => {},
  onOk: () => {},
  onCancel: () => {},
};

StepWizard.prototype = {
  steps: PropTypes.array,
  //   children,
  stepValue: PropTypes.any,
  onPrev: PropTypes.func,
  onNext: PropTypes.func,
  onOk: PropTypes.func,
  onCancel: PropTypes.func,
};

export default StepWizard;

import React from 'react';
import PropTypes from 'prop-types';
import { MinusIcon } from 'dc-icon';
import { Button, InputNumber } from 'dui';
import styles from './SegmentList.module.less';

const SegmentList = (props) => {
  const { segmentList, onValueChange, limit } = props;

  const segParameterChange = (a, b) => {
    const arr = segmentList.map((segment, idx) => {
      if (b === idx) {
        return {
          ...segment,
          ...a,
        };
      }
      return segment;
    });
    onValueChange(arr);
  };
  const segmentDemo = {
    stepFrequency: limit.stepFrequency.value,
    stopFrequency: limit.stopFrequency.value,
    startFrequency: limit.startFrequency.value,
  };

  const deleteHandle = (idx) => {
    const a = segmentList.filter((it, index) => {
      return idx !== index;
    });
    onValueChange(a);
  };
  const addSegmentHandle = () => {
    const a = [...segmentList, segmentDemo];
    onValueChange(a);
  };

  return (
    <div className={styles.segmentListCommons}>
      <div className={styles.addSegment}>
        <div className={styles.label}>参数设置</div>
        <Button onClick={addSegmentHandle}>添加频段</Button>
      </div>
      <div className={styles.list}>
        {segmentList.map((it, idx) => {
          return (
            <div key={`segment_${idx + 1}`} className={styles.item}>
              <div className={styles.idx}>{`频段${idx + 1}`}</div>
              <div className={styles.ipt}>
                <div className={styles.iptItem}>
                  <div className={styles.iptItemLabel}>
                    频率范围 {it.stopFrequency - it.startFrequency <= 0.1 && <span>终止频率应大于起始频率0.1MHz</span>}
                  </div>
                  <div>
                    <InputNumber
                      suffix={limit.startFrequency.suffix}
                      min={limit.startFrequency.min}
                      style={{ width: 110 }}
                      max={limit.startFrequency.max}
                      value={it.startFrequency}
                      onChange={(e) => {
                        segParameterChange({ startFrequency: e }, idx);
                      }}
                    />
                    <span style={{ color: 'var(--theme-font-50)' }}> - </span>
                    <InputNumber
                      suffix={limit.stopFrequency.suffix}
                      min={limit.stopFrequency.min}
                      style={{ width: 110 }}
                      max={limit.stopFrequency.max}
                      value={it.stopFrequency}
                      onChange={(e) => {
                        segParameterChange({ stopFrequency: e }, idx);
                      }}
                    />
                  </div>
                </div>
                <div className={styles.iptItem}>
                  <div className={styles.iptItemLabel}>{limit.stepFrequency.displayName}</div>
                  <div className={styles.stepFrequency}>
                    <InputNumber
                      suffix={limit.stepFrequency.suffix}
                      min={limit.stepFrequency.min}
                      style={{ width: 230 }}
                      max={limit.stepFrequency.max}
                      value={it.stepFrequency}
                      onChange={(e) => {
                        segParameterChange({ stepFrequency: e }, idx);
                      }}
                    />
                  </div>
                </div>
              </div>
              {segmentList.length > 1 && (
                <div className={styles.minus}>
                  <MinusIcon onClick={() => deleteHandle(idx)} iconSize={24} color="var(--theme-font-80)" />
                </div>
              )}
            </div>
          );
        })}
      </div>
    </div>
  );
};
SegmentList.defaultProps = {
  segmentList: [
    {
      stepFrequency: 25,
      stopFrequency: 108,
      startFrequency: 87,
    },
  ],
  onValueChange: PropTypes.func,
  limit: {
    startFrequency: {
      value: 87,
      min: 20,
      max: 888888,
      suffix: 'MHz',
      displayName: '起始频率',
    },
    stopFrequency: {
      value: 108,
      min: 20,
      max: 888888,
      suffix: 'MHz',
      displayName: '终止频率',
    },
    stepFrequency: {
      value: 25,
      min: 0,
      max: 888888,
      suffix: 'kHz',
      displayName: '步进带宽',
    },
  },
};
SegmentList.propTypes = {
  segmentList: PropTypes.array,
  onValueChange: PropTypes.func,
  limit: PropTypes.object,
};

export default SegmentList;

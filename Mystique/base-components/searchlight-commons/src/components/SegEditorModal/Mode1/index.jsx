import React, { memo, useState, useEffect, useMemo } from 'react';
import PropTypes from 'prop-types';
import { message } from 'dui';
import NumberInput from '../../NumberInput';
import BubbleSelector from '../../BubbleSelector';
import ParametersSettings from './ParametersSettings/index.jsx';
import styles from './index.module.less';

const Mode1 = (props) => {
  const { limit, segparameters, segment, isEdit, maxLength, onMode1Sure, onMode1Cancel } = props;

  // 本地seg数据
  const [seg, setseg] = useState({});

  const changeSeg = (nesSeg) => {
    if (maxLength === 1) {
      onMode1Sure(nesSeg);
    } else {
      setseg(nesSeg);
    }
  };

  const onOk = () => {
    if (seg.startFrequency === null || seg.startFrequency === '') {
      message.info({ key: 'Mode1', content: '请输入起始频率' });
      return;
    }
    if (seg.stopFrequency === null || seg.stopFrequency === '') {
      message.info({ key: 'Mode1', content: '请输入结束频率' });
      return;
    }
    if (seg.stopFrequency - seg.startFrequency < 0) {
      message.info({ key: 'Mode1', content: '结束频率必须大于起始频率' });
      return;
    }
    if (seg.stopFrequency - seg.startFrequency < 1) {
      message.info({ key: 'Mode1', content: '扫描带宽至少为1M' });
      return;
    }
    onMode1Sure(seg);
  };

  useEffect(() => {
    setseg(segment);
  }, [segment]);

  const dataSource = useMemo(() => {
    return limit.stepItems.map((item) => ({ value: item, display: `${item} kHz` }));
  }, [limit]);

  return (
    <>
      <div className={styles.Mode1}>
        <div className={styles.mode1head}>
          <NumberInput
            minValue={limit.min}
            maxValue={limit.max}
            decimals={3}
            value={seg.startFrequency}
            placeholder="请输入起始频率"
            unavailableKeys={['+/-']}
            style={{ width: 180 }}
            suffix="MHz"
            onValueChange={(val) => changeSeg({ ...seg, startFrequency: val })}
          />
          <div style={{ color: 'rgba(255,255,255,0.5)' }}>—</div>
          <NumberInput
            minValue={limit.min}
            maxValue={limit.max}
            decimals={3}
            value={seg.stopFrequency}
            placeholder="请输入结束频率"
            unavailableKeys={['+/-']}
            style={{ width: 180 }}
            suffix="MHz"
            onValueChange={(val) => changeSeg({ ...seg, stopFrequency: val })}
          />
          <div style={{ color: 'rgba(255,255,255,0.5)' }}>—</div>
          <BubbleSelector
            width={180}
            dataSource={dataSource}
            value={seg.stepFrequency}
            position="right"
            onValueChange={(e) => changeSeg({ ...seg, stepFrequency: e.value })}
            keyBoardType="simple"
          />
        </div>
        <ParametersSettings
          paramValues={seg}
          parameters={segparameters}
          onChange={(name, val) => changeSeg({ ...seg, [name]: val })}
        />
      </div>
      {maxLength !== 1 && (
        <div className={styles.footerbtn}>
          {isEdit ? (
            <>
              <div className={styles.btn} onClick={onMode1Cancel}>
                取消
              </div>
              <div className={styles.btn} onClick={onOk}>
                确定
              </div>
            </>
          ) : (
            <div className={styles.btn} onClick={onOk}>
              添加
            </div>
          )}
        </div>
      )}
    </>
  );
};

Mode1.propTypes = {
  limit: PropTypes.object.isRequired,
  segment: PropTypes.object.isRequired,
  segparameters: PropTypes.array.isRequired,
  isEdit: PropTypes.bool.isRequired,
  maxLength: PropTypes.number.isRequired,
  onMode1Sure: PropTypes.func.isRequired,
  onMode1Cancel: PropTypes.func.isRequired,
};

export default memo(Mode1);

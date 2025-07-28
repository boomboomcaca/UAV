/* eslint-disable react/no-array-index-key */
import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
// import classnames from 'classnames';
import NP from 'number-precision';
import { message } from 'dui';
import FrequencyInput from '../../FrequencyInput';
import EnumSelector from '@/components/EnumSelector';
import styles from './singleSegment.module.less';

const SingleSegment = (props) => {
  const { startFrequency, stopFrequency, stepFrequency, minFrequency, maxFrequency, stepItems, disable, onChange } =
    props;
  const [options, setOptions] = useState([]);

  useEffect(() => {
    const arr = stepItems.map((i) => {
      if (i >= 1000) {
        return { value: i, display: `${i / 1000} MHz` };
      }
      return { value: i, display: `${i} kHz` };
    });
    setOptions(arr);
  }, [stepItems]);

  const createFour = () =>
    // eslint-disable-next-line no-bitwise
    (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);

  const createGUID = () => {
    // eslint-disable-next-line
    return `${createFour()}${createFour()}-${createFour()}-${createFour()}-${createFour()}-${createFour()}${createFour()}${createFour()}`;
  };

  return (
    <>
      <div className={styles.ssroot}>
        <div className={styles.input}>
          <FrequencyInput
            miniValue={minFrequency}
            maxValue={maxFrequency}
            value={startFrequency}
            hideKeys={['+/-']}
            hideLight
            disable={disable}
            onValueChange={(e) => {
              if (e !== startFrequency) {
                // 通知外部
                const snap = NP.minus(stopFrequency, startFrequency);
                const bw = stopFrequency - e;
                if (bw < 1 && bw >= 0) {
                  message.info('扫描带宽至少为1M');
                  return;
                }
                let st = stopFrequency;
                if (bw < 0) {
                  const addnewval = NP.plus(e, snap);
                  if (addnewval <= maxFrequency) {
                    st = addnewval;
                  }
                  if (addnewval > maxFrequency) {
                    if (maxFrequency - e >= 1) {
                      st = maxFrequency;
                    } else {
                      return;
                    }
                  }
                }
                onChange({
                  segment: { stepFrequency, startFrequency: e, stopFrequency: st, id: createGUID() },
                });
              }
            }}
          />
        </div>
        <EnumSelector
          position="bottom"
          keyBoardType="simple"
          caption="步进"
          items={options}
          value={stepFrequency}
          onValueChanged={(index, value) => {
            if (value !== stepFrequency) {
              onChange({
                segment: { startFrequency, stopFrequency, stepFrequency: value, id: createGUID() },
              });
            }
          }}
        />
        <div className={styles.input}>
          <FrequencyInput
            disable={disable}
            miniValue={minFrequency}
            maxValue={maxFrequency}
            hideKeys={['+/-']}
            hideLight
            value={stopFrequency}
            onValueChange={(e) => {
              if (e !== stopFrequency) {
                // 通知外部
                const snap = NP.minus(stopFrequency, startFrequency);
                const bw = e - startFrequency;
                if (bw < 1 && bw >= 0) {
                  message.info('扫描带宽至少为1M');
                  return;
                }
                let st = startFrequency;
                if (bw < 0) {
                  const addnewval = NP.minus(e, snap);
                  if (addnewval >= minFrequency) {
                    st = addnewval;
                  }
                  if (addnewval < minFrequency) {
                    if (e - minFrequency >= 1) {
                      st = minFrequency;
                    } else {
                      return;
                    }
                  }
                }
                onChange({
                  segment: { startFrequency: st, stepFrequency, stopFrequency: e, id: createGUID() },
                });
              }
            }}
          />
        </div>
      </div>
    </>
  );
};

SingleSegment.defaultProps = {
  startFrequency: 87,
  stopFrequency: 108,
  stepFrequency: 25,
  minFrequency: 20,
  maxFrequency: 8000,
  stepItems: [12.5, 25, 50, 100, 200, 500, 1000],
  disable: false,
  onChange: () => {},
};

SingleSegment.propTypes = {
  startFrequency: PropTypes.number,
  stopFrequency: PropTypes.number,
  stepFrequency: PropTypes.number,
  minFrequency: PropTypes.number,
  maxFrequency: PropTypes.number,
  stepItems: PropTypes.array,
  disable: PropTypes.bool,
  onChange: PropTypes.func,
};

export default SingleSegment;

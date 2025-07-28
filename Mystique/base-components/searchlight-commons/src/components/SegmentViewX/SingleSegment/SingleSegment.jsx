/* eslint-disable react/no-array-index-key */
import React, { useState, useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import NP from 'number-precision';
import { message, Drawer } from 'dui';
import FrequencyInput from '../../FrequencyInput';
import styles from './singleSegment.module.less';

const SingleSegment = (props) => {
  const { startFrequency, stopFrequency, stepFrequency, minFrequency, maxFrequency, stepItems, disable, onChange } =
    props;

  const selDivRef = useRef(null);
  const [stepsVisible, setStepsVisible] = useState(false);

  const almostDone = () => {
    if (selDivRef.current) {
      setTimeout(() => {
        const scroll = document.getElementById(selDivRef.current);
        scroll?.scrollIntoView({
          behavior: 'smooth',
          block: 'center',
        });
      }, 300);
    }
  };

  return (
    <>
      <div className={styles.ssroot}>
        <div className={styles.input}>
          <FrequencyInput.Modal
            miniValue={minFrequency}
            maxValue={maxFrequency}
            value={startFrequency}
            hideKeys={['+/-']}
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
                  action: 'startFrequency',
                  value: e,
                  segment: { stepFrequency, startFrequency: e, stopFrequency: st },
                });
              }
            }}
          />
        </div>
        <div
          className={classnames(styles.valuelabel, { [styles.disable]: disable })}
          onClick={() => {
            if (!disable) {
              setStepsVisible(true);
              almostDone();
            }
          }}
        >
          {stepFrequency >= 1000 ? `${stepFrequency / 1000}MHz` : `${stepFrequency}kHz`}
        </div>
        <div className={styles.input}>
          <FrequencyInput.Modal
            disable={disable}
            miniValue={minFrequency}
            maxValue={maxFrequency}
            hideKeys={['+/-']}
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
                  segment: { startFrequency: st, stepFrequency, stopFrequency: e },
                });
              }
            }}
          />
        </div>
      </div>
      <Drawer
        visible={stepsVisible}
        width="420px"
        title="选择步进"
        onCancel={() => setStepsVisible(false)}
        bodyStyle={{ padding: '20px 0' }}
      >
        {stepItems.map((item, index) => {
          if (stepFrequency === item) {
            selDivRef.current = `stepItems${index}`;
          }
          return (
            <div
              id={`stepItems${index}`}
              key={`stepItems${index}`}
              className={stepFrequency === item ? styles.selectItemSel : styles.selectItem}
              onClick={() => {
                setStepsVisible(false);
                onChange({
                  action: 'stepFrequency',
                  value: item,
                  segment: { startFrequency, stopFrequency, stepFrequency: item },
                });
                selDivRef.current = `stepItems${index}`;
              }}
            >
              <span>{item >= 1000 ? `${item / 1000}MHz` : `${item}kHz`}</span>
            </div>
          );
        })}
      </Drawer>
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

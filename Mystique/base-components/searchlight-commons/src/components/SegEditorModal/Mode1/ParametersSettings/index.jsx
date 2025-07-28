import React, { memo, useMemo } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Switch } from 'dui';
import BubbleSelector from '../../../BubbleSelector';
import ScaleSliderBar from '../../../ScaleSliderBar';
import styles from './index.module.less';

const ParametersSettings = (props) => {
  const { paramValues, parameters, onChange } = props;

  const dealChild = (item = {}) => {
    if (item.style === 'radio') {
      return (
        <>
          <div className={styles.label}>{item.displayName}</div>
          <div className={styles.radioArea}>
            {item.values.map((option, idx) => (
              <div
                key={option}
                className={classnames(styles.radioItem, { [styles.active]: paramValues[item.name] === option })}
                onClick={() => onChange(item.name, option)}
              >
                {item.displayValues[idx]}
              </div>
            ))}
          </div>
        </>
      );
    }
    if (item.style === 'switch') {
      return (
        <>
          <div className={styles.label}>{item.displayName}</div>
          <div className={styles.switchArea}>
            <div className={styles.text}>{item.displayValues[paramValues[item.name] === true ? 0 : 1]}</div>
            <Switch
              selected={paramValues[item.name]}
              disable={item.readonly}
              onChange={(val) => onChange(item.name, val)}
            />
          </div>
        </>
      );
    }
    if (item.style === 'slider') {
      return (
        <>
          <div className={styles.label}>{item.displayName}</div>
          <div className={styles.sliderArea}>
            {item.selectOnly ? (
              <ScaleSliderBar
                scaleData={item.values}
                labelData={item.displayValues}
                disable={item.readonly}
                value={paramValues[item.name]}
                unitName={item.unit}
                isSet={false}
                onSliderValueChange={(nne) => {
                  if (nne.end) onChange(item.name, nne.value);
                }}
              />
            ) : (
              <ScaleSliderBar
                disable={item.readonly}
                value={paramValues[item.name]}
                minimum={item.minimum}
                maximum={item.maximum}
                unitName={item.unit}
                isSet
                step={item.step}
                onSliderValueChange={(nne) => {
                  if (nne.end) onChange(item.name, nne.value);
                }}
              />
            )}
          </div>
        </>
      );
    }
    if (item.style === 'bandwidth') {
      const { values, displayValues } = item;
      const dItems = displayValues.map((dv, index) => {
        return { value: values[index], display: dv };
      });
      return (
        <>
          <div className={styles.label}>{item.displayName}</div>
          <BubbleSelector
            disable={item.readonly}
            dataSource={dItems}
            keyBoardType="complex"
            value={paramValues[item.name]}
            position="right"
            onValueChange={(nne) => {
              onChange(item.name, nne.value);
            }}
          />
        </>
      );
    }
    return null;
  };

  const showParams = useMemo(() => {
    let childNames = [];
    parameters.forEach((item) => {
      if (item.children && item.children instanceof Array) {
        childNames = [...childNames, ...item.children];
      }
    });
    const nne = parameters.filter((item) => item.browsable && !item.isInstallation && !childNames.includes(item.name));
    return nne;
  }, [parameters]);

  return (
    <div className={styles.params}>
      {showParams.map((item) => (
        <div className={styles.oneItem}>
          <div className={styles.noumenon}>{dealChild(item)}</div>
          {item.relatedValue?.includes?.(paramValues[item.name]) &&
            parameters.map((child) => {
              if (item.children?.includes?.(child.name)) {
                return <div className={styles.dhildmon}>{dealChild(child)}</div>;
              }
              return null;
            })}
        </div>
      ))}
    </div>
  );
};

ParametersSettings.propTypes = {
  paramValues: PropTypes.object.isRequired,
  parameters: PropTypes.array.isRequired,
  onChange: PropTypes.func.isRequired,
};

export default memo(ParametersSettings);

import React from 'react';
import { Form } from 'dui';
import styles from './index.module.less';

const { Field } = Form;
const SpecConf = () => {
  const options = [
    {
      label: '实时曲线颜色',
      name: 'main',
    },
    {
      label: '最大保持曲线颜色',
      name: 'max',
    },
    {
      label: '平均值曲线颜色',
      name: 'avg',
    },
    {
      label: '最小保持曲线颜色',
      name: 'min',
    },
    {
      label: '上门限颜色',
      name: 'limitTop',
    },
    {
      label: '下门限颜色',
      name: 'limitBottom',
    },
    {
      label: '频谱模板颜色',
      name: 'tmp',
    },
    {
      label: '解调宽带颜色',
      name: 'audioBandBackgroud',
    },
  ];

  return (
    <>
      <div className={styles.title}>颜色设置</div>
      <div className={styles.spectrogram}>
        {options.map((option) => {
          return (
            <div key={option.name} className={styles.colorLabel}>
              <Field label={option.label} name={option.name} key={option.name}>
                <input type='color' className={styles.colorInput} />
              </Field>
            </div>
          );
        })}
      </div>
    </>
  );
};

export default SpecConf;

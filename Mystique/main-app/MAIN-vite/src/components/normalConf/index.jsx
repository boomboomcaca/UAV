import React from 'react';
import { Form, Select } from 'dui';
import styles from './index.module.less';

const { Field } = Form;
const { Option } = Select;
const NormalConf = () => {
  const options = [
    {
      label: 'marker值背景颜色',
      name: 'background',
    },
    {
      label: 'marker值字体颜色',
      name: 'color',
    },
  ];
  return (
    <>
      <div className={styles.title}>marker设置</div>
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
      <div className={styles.title}>电平设置</div>
      <div className={styles.spectrogram}>
        <Field name='gradient' label='电平色带'>
          <Select>
            <Option value={0}>
              <div
                style={{
                  width: '110px',
                  height: '20px',
                  background:
                    'linear-gradient(90deg, #0075FF 0%, #56E34A 32.15%, #FAFF00 66.53%, #FF1818 100%)',
                }}
              />
            </Option>
            <Option value={1}>
              <div
                style={{
                  width: '110px',
                  height: '20px',
                  background:
                    ' linear-gradient(90deg, #000000 0%, #FFFFFF 100%)',
                }}
              />
            </Option>
          </Select>
        </Field>
      </div>
    </>
  );
};

export default NormalConf;

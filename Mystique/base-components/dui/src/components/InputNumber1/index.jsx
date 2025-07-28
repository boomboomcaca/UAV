import React, { useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { PlusOutlined, MinusOutlined } from '@ant-design/icons';
import styles from './style.module.less';

const InputNumber1 = (props) => {
  const { disable, value, suffix, height, onChange, minimum, maximum, step, inputClass } = props;

  const toSetValue = (type) => {
    if (disable) {
      return;
    }
    let num = value;
    if (type === 'add') {
      num += step;
    } else {
      num -= step;
    }
    num = minimum > num ? minimum : maximum < num ? maximum : num;
    onChange?.(num);
  };
  const handalBlur = (e) => {
    console.log('on blur:::', e);
  };

  useEffect(() => {
    console.log('ddddddf', minimum);
  }, [minimum]);

  return (
    <div className={styles.setBox}>
      <div className={styles.setButton}>
        <MinusOutlined
          onClick={() => toSetValue('cut')}
          style={{ fontSize: { height }, color: 'var(--theme-primary)', fontWeight: 800 }}
        />
      </div>
      <div className={styles.setValue}>
        <input
          autoComplete="off"
          className={classnames(inputClass, styles.input)}
          value={value}
          disabled={disable}
          min={minimum}
          max={maximum}
          onChange={onChange}
          onBlur={handalBlur}
          type="number"
        />
        <span>{suffix}</span>
      </div>
      <div className={styles.setButton}>
        <PlusOutlined
          onClick={() => toSetValue('add')}
          style={{ fontSize: { height }, color: 'var(--theme-primary)' }}
        />
      </div>
    </div>
  );
};

InputNumber1.defaultProps = {
  minimum: 0,
  maximum: 100,
  disable: false,
  value: -999,
  suffix: '',
  height: 24,
  onChange: () => {},
  step: 1,
  inputClass: '',
};

InputNumber1.propTypes = {
  minimum: PropTypes.number,
  maximum: PropTypes.number,
  height: PropTypes.any,
  value: PropTypes.number,
  suffix: PropTypes.string,
  onChange: PropTypes.func,
  step: PropTypes.number,
  disable: PropTypes.bool,
  inputClass: PropTypes.string,
};

export default InputNumber1;

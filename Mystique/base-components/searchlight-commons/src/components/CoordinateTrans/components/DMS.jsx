import React, { useEffect, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { InputNumber } from 'dui';
import { getDDMMSS, getFloat } from '../weapon';
import styles from './dms.module.less';

const DMS = (props) => {
  const { className, maximum, minimum, value, units, onChange } = props;

  const [transData, setTransData] = useState({
    degree: '',
    minute: '',
    second: '',
  });

  const [nonDegDisabled, setNonDegDisabled] = useState(false);

  const [negtive, setNegtive] = useState(false);

  useEffect(() => {
    if (value !== null && value !== '') {
      setNegtive(value < 0);
      setTransData(getDDMMSS(value));
    } else {
      setNegtive(false);
      setTransData(null);
    }
  }, [value]);

  const handleUpdateTransData = (val, type) => {
    const temp = { ...transData };
    try {
      if (Array.isArray(type)) {
        type.forEach((t, i) => {
          temp[t] = val[i];
        });
      } else {
        temp[type] = val;
      }
      setTransData(temp);

      const fval = getFloat(temp, negtive);
      onChange(fval);
    } catch (err) {
      window.console.error(err);
    }
  };

  return (
    <div className={classnames(styles.root, className)}>
      <InputNumber
        value={transData?.degree || ''}
        style={{ width: 75, marginRight: 5 }}
        digits={0}
        max={maximum}
        min={0}
        suffix="°"
        placeholder="DD"
        onChange={(val) => {
          if (val !== '') {
            if (val >= maximum || val <= minimum) {
              handleUpdateTransData([val, 0, 0], ['degree', 'minute', 'second']);
              setNonDegDisabled(true);
            } else {
              handleUpdateTransData(val, 'degree');
              setNonDegDisabled(false);
            }
          }
        }}
      />
      <InputNumber
        value={transData?.minute || ''}
        style={{ width: 75, marginRight: 5 }}
        digits={0}
        max={60}
        min={0}
        suffix="′"
        placeholder="MM"
        disabled={nonDegDisabled}
        onChange={(val) => {
          handleUpdateTransData(val, 'minute');
        }}
      />
      <InputNumber
        value={transData?.second || ''}
        style={{ width: 75, marginRight: 5 }}
        digits={2}
        max={60}
        min={0}
        suffix="″"
        placeholder="SS"
        disabled={nonDegDisabled}
        onChange={(val) => {
          handleUpdateTransData(val, 'second');
        }}
      />
      <div
        className={styles.unit}
        onClick={() => {
          const neg = !negtive;
          setNegtive(neg);
          const val = getFloat(transData, neg);
          onChange(val);
        }}
      >
        {negtive ? units[0] : units[1]}
      </div>
    </div>
  );
};

DMS.defaultProps = {
  className: null,
  maximum: 90,
  minimum: -90,
  value: null,
  units: ['-', '+'],
  onChange: () => {},
};

DMS.propTypes = {
  className: PropTypes.any,
  maximum: PropTypes.any,
  minimum: PropTypes.any,
  value: PropTypes.any,
  units: PropTypes.any,
  onChange: PropTypes.func,
};

export default DMS;

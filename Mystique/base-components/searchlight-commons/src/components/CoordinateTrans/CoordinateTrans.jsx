import React, { useEffect, useState } from 'react';
import classnames from 'classnames';
import PropTypes from 'prop-types';
import { Format, getNextFormat, getFormatStr } from './weapon';
import tools from './components';
import styles from './index.module.less';

// 经度（正:东经E; 负:西经W）
// 纬度（正:北纬N; 负:南纬S）

const { DDD, DMS, DM, NWSE } = tools;

const CoordinateTrans = (props) => {
  const { maximum, minimum, value, units, onChange } = props;

  const [showFormat, setShowFormat] = useState(Format.DMS);

  const [dddValue, setValue] = useState(null);

  const onChanged = (val) => {
    if (val !== '') {
      onChange(Number(val));
      setValue(Number(val));
    } else {
      onChange(val);
      setValue(null);
    }
  };

  useEffect(() => {
    if (value !== null && value !== '') {
      setValue(Number(value.toFixed(6)));
    } else {
      setValue(null);
    }
  }, [value]);

  return (
    <div className={styles.root}>
      <DDD
        className={classnames(styles.container, showFormat === Format.DDD ? null : styles.hide)}
        maximum={maximum}
        minimum={minimum}
        value={dddValue}
        onChange={onChanged}
      />
      <DMS
        className={classnames(styles.container, showFormat === Format.DMS ? null : styles.hide)}
        maximum={maximum}
        minimum={minimum}
        units={units}
        value={dddValue}
        onChange={onChanged}
      />
      <DM
        className={classnames(styles.container, showFormat === Format.DM ? null : styles.hide)}
        maximum={maximum}
        minimum={minimum}
        units={units}
        value={dddValue}
        onChange={onChanged}
      />
      <NWSE
        className={classnames(styles.container, showFormat === Format.NWSE ? null : styles.hide)}
        maximum={maximum}
        minimum={minimum}
        units={units}
        value={dddValue}
        onChange={onChanged}
      />
      <div
        className={styles.switch}
        onClick={() => {
          setShowFormat(getNextFormat(showFormat));
        }}
      >
        {getFormatStr(showFormat)}
      </div>
    </div>
  );
};

CoordinateTrans.defaultProps = {
  maximum: 90,
  minimum: -90,
  value: '',
  units: ['-', '+'],
  onChange: () => {},
};

CoordinateTrans.propTypes = {
  maximum: PropTypes.any,
  minimum: PropTypes.any,
  value: PropTypes.any,
  units: PropTypes.any,
  onChange: PropTypes.func,
};

export default CoordinateTrans;

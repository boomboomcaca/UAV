import React, { useState, useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Switch } from 'dui';
import styles from './item.module.less';

const Item = (props) => {
  const { className, item, disabled, onSwitchChanged } = props;

  const [power, setPower] = useState({ volt: null, curr: null });

  const fixColor = (val, min, max) => {
    if (val > max) return 'red';
    if (val < min) return 'blue';
    return 'white';
  };

  useEffect(() => {
    if (item) {
      const volt = item.info.find((i) => {
        return i.name === 'voltage';
      });
      const curr = item.info.find((i) => {
        return i.name === 'current';
      });
      const vcp = { volt, curr };
      setPower(vcp);
    }
  }, [JSON.stringify(item)]);

  const onSwitch = (checked) => {
    onSwitchChanged(checked, item);
  };

  return (
    <div className={classnames(styles.channel, className)}>
      <div className={styles.title}>
        <div>{item.display}</div>
        <Switch selected={item.state === 'on'} disable={disabled} onChange={onSwitch} />
      </div>
      <div
        className={styles.infoItem}
        style={{
          color: fixColor(power.state !== 'off' && power.volt ? power.volt.value : '--', 200, 250),
        }}
      >
        <div>类型：</div>
        <div>{item?.switchType || '未知'}</div>
      </div>
      <div
        className={styles.infoItem}
        style={{
          color: fixColor(power.state !== 'off' && power.volt ? power.volt.value : '--', 200, 250),
        }}
      >
        <div>电压：</div>
        <div>{`${power.state !== 'off' && power.volt ? power.volt.value.toFixed(2) : '--'} ${
          power.volt ? power.volt.unit : 'V'
        }`}</div>
      </div>
      <div
        className={styles.infoItem}
        style={{
          color: fixColor(power.curr ? power.curr.value : '--', 0.1, 2),
        }}
      >
        <div>电流：</div>
        <div>{`${power.state !== 'off' && power.curr ? power.curr.value.toFixed(2) : '--'} ${
          power.curr ? power.curr.unit : 'A'
        }`}</div>
      </div>
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  disabled: false,
  onSwitchChanged: () => {},
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  disabled: PropTypes.bool,
  onSwitchChanged: PropTypes.func,
};

export default Item;

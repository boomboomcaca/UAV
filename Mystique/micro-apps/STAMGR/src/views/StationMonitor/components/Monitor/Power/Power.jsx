import React, { useEffect } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { ListView, message } from 'dui';
import Task from '@/utils/task';
import Item from './Item.jsx';
import styles from './index.module.less';

const Power = (props) => {
  const { className, main, data, disabled } = props;

  const onSwitchItemChanged = (checked, info) => {
    const params = {
      moduleId: info.moduleId,
      parameters: [
        {
          name: info.name,
          value: checked ? 'on' : 'off',
        },
      ],
    };
    Task.SetParam(params, (e) => {
      if (e.message) {
        message.error(e.message);
      }
    });
  };

  useEffect(() => {
    Task.InitTask((args) => {
      window.console.log(args);
    });
    return () => {
      Task.CloseTask();
    };
  }, []);

  const getMain = (md, tag, unit) => {
    const volt = md?.info?.find((i) => {
      return i.name === tag;
    });
    return volt ? `${volt.value.toFixed(2)} ${volt.unit}` : `-- ${unit}`;
  };

  return (
    <div className={classnames(styles.root, className)}>
      <div>电源</div>
      <div className={styles.subtitle}>
        <span>总电压：</span>
        <span>{getMain(main, 'voltage', 'V')}</span>
        <span>总电流：</span>
        <span>{getMain(main, 'current', 'A')}</span>
      </div>
      <ListView
        className={styles.list}
        baseSize={{ width: 200, height: 160 }}
        dataSource={data}
        itemTemplate={(item) => {
          return <Item className={styles.item} item={item} disabled={disabled} onSwitchChanged={onSwitchItemChanged} />;
        }}
      />
    </div>
  );
};

Power.defaultProps = {
  className: null,
  main: null,
  data: null,
  disabled: false,
};

Power.propTypes = {
  className: PropTypes.any,
  main: PropTypes.any,
  data: PropTypes.any,
  disabled: PropTypes.bool,
};

export default Power;

import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { Switch } from 'dui';
import { ParamSettingsIcon } from 'dc-icon';
import Check from '@/components/Check';
import styles from './index.module.less';

const infos = [
  { key: 'moduleCategoryStr', label: '类型', title: true },
  { key: 'version', label: '版本', title: true },
  { key: 'iport', label: '地址', title: true },
];

const Item = (props) => {
  const { className, item, checked, onChange } = props;
  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.operate}>
        <Check
          checked={checked}
          onCheck={() => {
            onChange({ tag: 'check', item });
          }}
        />
        <ParamSettingsIcon
          style={{ opacity: 0.5, cursor: 'pointer' }}
          onClick={() => {
            onChange({ tag: 'param', item });
          }}
        />
      </div>
      <div className={styles.title}>{item?.displayName}</div>
      <div className={styles.info}>
        {infos.map((info) => {
          return (
            <div className={styles.infoItem}>
              <div>{info.label}</div>
              <div title={info.title ? item?.[info.key] : null}>{item?.[info.key]}</div>
            </div>
          );
        })}
      </div>
      <div className={styles.switch}>
        <Switch
          selected={item.moduleState !== 'disabled'}
          onChange={() => {
            onChange({ tag: 'switch', item });
          }}
        />
      </div>
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
  checked: false,
  onChange: () => {},
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
  checked: PropTypes.bool,
  onChange: PropTypes.func,
};

export default Item;

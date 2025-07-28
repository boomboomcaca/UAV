/* eslint-disable max-len */
import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Bubble from '@/components/Bubble';
import styles from './index.module.less';

const moduleStates = {
  none: { value: '未知', color: '#404040' },
  idle: { value: '空闲', color: '#a0a0a0' },
  busy: { value: '忙碌', color: '#a0a020' },
  offline: { value: '离线', color: '#e00000' },
  fault: { value: '故障', color: '#e00000' },
  disabled: { value: '禁用', color: '#808080' },
};

const svg1 = (
  <svg width="32" height="32" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
    <path
      d="M4.9943 18C4.62982 18 4.29749 17.8073 4.13668 17.5076C3.95444 17.208 3.95444 16.8119 4.13668 16.5122L11.1049 4.49235C11.2871 4.19266 11.6194 4 11.9625 4C12.327 4 12.6379 4.19266 12.8201 4.49235L19.8633 16.4801C20.0456 16.7798 20.0456 17.1758 19.8633 17.4755C19.6811 17.7752 19.3487 17.9679 19.0057 17.9679L4.9943 18Z"
      fill="#FFD118"
    />
    <path d="M12 9V13" stroke="black" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
    <circle cx="12" cy="16" r="1" fill="#04051B" />
  </svg>
);

const svg2 = (
  <svg width="27" height="27" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M1 13 L8 18 L23 5 L24 6 L9 24 L7 24 L0 14"
      fill="#35E06580"
      stroke="none"
    />
  </svg>
);

const Item = (props) => {
  const { className, item } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <Bubble
        className={classnames(styles.state)}
        bubble={item.moduleState === 'idle' || item.moduleState === 'busy' ? Bubble.Normal : Bubble.Exception}
      >
        <div style={{ marginTop: 24, color: moduleStates[item.moduleState].color }}>
          {moduleStates[item.moduleState].value}
        </div>
      </Bubble>
      <div className={styles.name}>
        <div>{item?.displayName}</div>
        <div>{item?.model}</div>
      </div>
      <div className={styles.shadow} />
    </div>
  );
};

Item.defaultProps = {
  className: null,
  item: null,
};

Item.propTypes = {
  className: PropTypes.any,
  item: PropTypes.any,
};

export default Item;

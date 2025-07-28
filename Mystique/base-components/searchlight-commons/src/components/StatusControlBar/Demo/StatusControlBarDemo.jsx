import React, { useState } from 'react';
import { IconButton } from 'dui';
import { EyeOpenIcon, EyeCloseIcon } from 'dc-icon';
import ComboList from '../../ComboList';
import StatusControlBar from '../StatusControlBar.jsx';
import styles from './index.module.less';

const StatusControlBarDemo = () => {
  const [startChecked, setStartChecked] = useState(false);

  const onIconClick = (checked, tag) => {
    switch (tag) {
      case 'start':
        setStartChecked(!checked);
        break;

      default:
        break;
    }
  };
  return (
    <div className={styles.main}>
      <div className={styles.content}>123</div>
      <StatusControlBar className={styles.bottom}>
        <StatusControlBar.Main>
          <div style={{ width: 160, background: 'red' }}>hello world!</div>
        </StatusControlBar.Main>
        <StatusControlBar.Action>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start">
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
        </StatusControlBar.Action>
      </StatusControlBar>
      <StatusControlBar className={styles.bottom}>
        <StatusControlBar.Message>
          <ComboList
            mainIcon={<EyeOpenIcon color="var(--theme-font-100)" />}
            dropIcon={<EyeCloseIcon color="var(--theme-font-100)" />}
            values={[
              'ddddd',
              'dddd0',
              '电磁波在自由空间的传播速率是光速，即3x108m/s;电磁波在铜线电缆中的传播速率约为2.5x108m/s;电磁波在光纤中的传播速率约为2x108m/s',
            ]}
          />
        </StatusControlBar.Message>
      </StatusControlBar>
      <StatusControlBar className={styles.bottom}>
        <StatusControlBar.Message>
          <ComboList
            mainIcon={<EyeOpenIcon color="var(--theme-font-100)" />}
            dropIcon={<EyeCloseIcon color="var(--theme-font-100)" />}
            values={[
              'ddddd',
              'dddd0',
              '电磁波在自由空间的传播速率是光速，即3x108m/s;电磁波在铜线电缆中的传播速率约为2.5x108m/s;电磁波在光纤中的传播速率约为2x108m/s',
            ]}
          />
        </StatusControlBar.Message>
        <StatusControlBar.Action>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start">
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
        </StatusControlBar.Action>
      </StatusControlBar>
      <StatusControlBar className={styles.bottom}>
        <StatusControlBar.Main>
          <div style={{ width: 160, background: 'red' }}>hello world!</div>
        </StatusControlBar.Main>
        <StatusControlBar.Message>
          <ComboList
            mainIcon={<EyeOpenIcon color="var(--theme-font-100)" />}
            dropIcon={<EyeCloseIcon color="var(--theme-font-100)" />}
            values={[
              'ddddd',
              'dddd0',
              '电磁波在自由空间的传播速率是光速，即3x108m/s;电磁波在铜线电缆中的传播速率约为2.5x108m/s;电磁波在光纤中的传播速率约为2x108m/s',
            ]}
          />
        </StatusControlBar.Message>
        <StatusControlBar.Action>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start" checked={startChecked} onClick={onIconClick}>
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
          <IconButton tag="start">
            <EyeCloseIcon color={startChecked ? 'red' : 'var(--theme-font-100)'} size={24} />
          </IconButton>
        </StatusControlBar.Action>
      </StatusControlBar>
    </div>
  );
};

StatusControlBarDemo.defaultProps = {};

StatusControlBarDemo.propTypes = {};

export default StatusControlBarDemo;

import React, { useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { message, Modal } from 'dui';
import useLocation from '@/hooks/useLocation';
import useDeviceMod from '@/hooks/useDeviceMod';
import Button from '@/components/Button';
import Detail from '../Detail';
import styles from './index.module.less';

const Mod = (props) => {
  const { className, onReturn, device } = props;

  const savedRef = useRef(true);

  const { updateLocation, updateStepable } = useLocation('device/mod', () => {
    if (savedRef?.current === false) {
      Modal.confirm({
        title: '提示',
        closable: false,
        content: '数据还未保存，是否退出？',
        onOk: () => {
          saved(true);
          onReturn();
        },
      });
    } else {
      saved(true);
      onReturn();
    }
  });

  const saved = (bo) => {
    if (bo) {
      savedRef.current = true;
      updateLocation('edit');
      updateStepable(true);
    } else {
      savedRef.current = false;
      updateLocation('device/mod');
      updateStepable(false);
    }
  };

  const { select, onSave, onSelectChange } = useDeviceMod(device);

  return (
    <div className={classnames(styles.root, className)}>
      <div
        className={styles.devs}
        onClick={() => {
          saved(true);
          onReturn();
        }}
      />
      <div className={styles.detl}>
        <Detail data={select} onChange={onSelectChange} />
        <div className={styles.btns}>
          <Button
            primary={false}
            className={styles.btn}
            onClick={() => {
              saved(true);
              onReturn();
            }}
          >
            取消
          </Button>
          <Button
            className={styles.btn}
            onClick={() => {
              onSave((bo) => {
                saved(bo);
                if (bo) {
                  message.success({ key: 'tip', content: '当前设备更新成功！' });
                  onReturn();
                }
              });
            }}
          >
            保存
          </Button>
        </div>
      </div>
    </div>
  );
};

Mod.defaultProps = {
  className: null,
  onReturn: () => {},
  device: null,
};

Mod.propTypes = {
  className: PropTypes.any,
  onReturn: PropTypes.func,
  device: PropTypes.any,
};

export default Mod;

import React, { useRef } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { message, Modal } from 'dui';
import useLocation from '@/hooks/useLocation';
import useDeviceAdd from '@/hooks/useDeviceAdd';
import Button from '@/components/Button';
import Template from '@/components/Template';
import Item from './Item.jsx';
import Detail from '../Detail';
import styles from './index.module.less';

const Add = (props) => {
  const { className, onReturn, edgeID, filter, reverse } = props;

  const savedRef = useRef(true);

  const { updateLocation, updateStepable } = useLocation('device/add', () => {
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
      updateLocation('device/add');
      updateStepable(false);
    }
  };

  const { tempVisible, setTempVisible, devices, onDelete, onAdd, select, onSelect, onSelectChange, onSave } =
    useDeviceAdd(edgeID);

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.edit}>
        <div className={styles.devs}>
          <div className={styles.title}>
            <div>设备</div>
            <div
              onClick={() => {
                setTempVisible(true);
              }}
            />
          </div>
          <div className={styles.list}>
            {devices.map((d) => {
              return (
                <Item
                  item={d}
                  checked={d.rowKey === select?.rowKey}
                  error={d.error && d.error !== false}
                  onDelete={onDelete}
                  onClick={onSelect}
                />
              );
            })}
          </div>
        </div>
        <div className={styles.detl}>
          <Detail data={select?.template} onChange={onSelectChange} />
        </div>
      </div>
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
                message.success({ key: 'tip', content: '全部设备保存成功！' });
                onReturn();
              }
            });
          }}
        >
          保存
        </Button>
      </div>

      <Template
        visible={tempVisible}
        type="device"
        filter={filter}
        reverse={reverse}
        onCancel={() => {
          setTempVisible(false);
        }}
        onConfirm={(temps) => {
          saved(false);
          onAdd(temps);
        }}
      />
    </div>
  );
};

Add.defaultProps = {
  className: null,
  onReturn: () => {},
  edgeID: '',
  filter: null,
  reverse: false,
};

Add.propTypes = {
  className: PropTypes.any,
  onReturn: PropTypes.func,
  edgeID: PropTypes.any,
  filter: PropTypes.any,
  reverse: PropTypes.bool,
};

export default Add;

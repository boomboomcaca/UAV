import React, { useEffect } from 'react';
import PropTypes from 'prop-types';
import { Button, message, Modal } from 'dui';
import classnames from 'classnames';
import Popup from '@/components/Popup';
import useStationDetail from '@/hooks/useStationDetail';
import useStep from '@/hooks/useStep';
import useDictionary from '@/hooks/useDictionary';
import useLocation from '@/hooks/useLocation';
import Steps, { Step } from '@/components/Steps';
import Detail from '../Station/Detail';
import Device from '../Device';
import Driver from '../Driver';
import Monitor from '../Monitor';
import styles from './index.module.less';

const Edit = (props) => {
  const { editype, editParam, visible, onReturn } = props;

  const { dictionary } = useDictionary();

  const onStationSave = async (values, tag = null) => {
    const newvals = { ...station, ...values };
    setNewStation(newvals);
    const ret = await toSaveStation(newvals);
    if (ret && ret.result) {
      message.success(editype === 'new' ? '添加站点成功！' : '站点信息保存成功！');
      setStation({ ...newvals });
      if (tag) {
        setStep(tag);
      }
      savedRef.current = true;
    }
  };

  const { station, setStation, newStation, setNewStation, toSaveStation } = useStationDetail(editype, editParam);

  const { step, setStep, onNextStep, savedRef } = useStep(
    visible,
    editype,
    station,
    newStation,
    setNewStation,
    onStationSave,
  );

  const { updateLocation, stepable } = useLocation('edit', () => {
    if (savedRef?.current === false) {
      Modal.confirm({
        title: '提示',
        closable: false,
        content: '数据还未保存，是否退出？',
        onOk: onReturn,
      });
    } else {
      onReturn();
    }
  });

  useEffect(() => {
    updateLocation(visible ? 'edit' : '');
  }, [visible]);

  const onStationChange = (name, value) => {
    if (Array.isArray(name) && Array.isArray(value)) {
      name.forEach((n, i) => {
        newStation[n] = value?.[i] || null;
      });
    } else {
      newStation[name] = value;
    }
    savedRef.current = false;
  };

  const onStepClick = (tag) => {
    onNextStep(tag, () => {
      return stepable;
    });
  };

  return (
    <Popup visible={visible} mask={false} usePortal={false}>
      <div
        className={styles.back}
        onClick={() => {
          if (onReturn) {
            onReturn();
          }
        }}
      >
        返回
      </div>
      <div className={styles.root}>
        <Steps step={step} onClick={onStepClick}>
          <Step tag={1} title="基本信息" />
          <Step tag={2} title="监测设备" />
          <Step tag={3} title="监测功能" />
          <Step tag={4} title="环境监控" />
        </Steps>
        <div className={classnames(styles.content, step > 1 ? styles.trans : null)}>
          {step === 1 && (
            <Detail
              editype={editype}
              data={newStation}
              dictionary={dictionary}
              onFinish={onStationSave}
              onChange={onStationChange}
            />
          )}
          {step === 2 && <Device edgeID={newStation.id} filter="control" reverse />}
          {step === 3 && <Driver edgeID={newStation.id} />}
          {step === 4 && <Monitor edgeID={newStation.id} filter="control" />}
        </div>
      </div>
    </Popup>
  );
};

Edit.defaultProps = {
  editype: 'mod',
  editParam: null,
  visible: false,
  onReturn: () => {},
};

Edit.propTypes = {
  editype: PropTypes.oneOf('mod', 'new'),
  editParam: PropTypes.any,
  visible: PropTypes.bool,
  onReturn: PropTypes.func,
};

export default Edit;

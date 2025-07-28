import React, { useState } from 'react';
import Modal from '../index';
import Input from '../../Input/index';

export default function Demo() {
  const [show, setShow] = useState(false);
  const [show2, setShow2] = useState(false);

  const confirm = () => {
    Modal.confirm({
      title: '提示',
      closable: false,
      content: (
        <div>
          <Input placeholder="请输入业务名称" size="large" style={{ width: '100%' }} />
        </div>
      ),
      onOk: () => {
        window.console.log(11);
      },
      onCancel: () => {
        console.log('ddddddddd');
      },
    });
  };

  return (
    <div>
      <div onClick={() => setShow(true)}>显示Modal </div>
      <br />
      <br />
      <div onClick={() => setShow2(true)}>显示异形Modal </div>
      <br />
      <br />
      <div onClick={confirm}> Modal.confirm</div>
      <Modal visible={show} title="任务信息" onCancel={() => setShow(false)} closable>
        <div>11111111111111111</div>
      </Modal>
      <Modal
        visible={show2}
        title="任务信息"
        onCancel={() => setShow2(false)}
        style={{ width: '80%' }}
        heterotypic
        heterotypicChild={
          <>
            <div>111</div>
            <div>111</div>
          </>
        }
      >
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
        <div>大大大modal才用异形</div>
      </Modal>
    </div>
  );
}

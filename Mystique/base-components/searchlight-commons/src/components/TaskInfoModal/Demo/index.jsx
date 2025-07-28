import React, { useState } from 'react';
import { Button } from 'dui';
import TaskInfoModal from '..';

const TaskInfoModalTest = () => {
  const [show, setShow] = useState(false);
  const info = {
    creatTime: '2021.03.22 17:47:11',
    runTime: '00:23:10',

    edgeName: 'D0.7移动站01',
    edgeId: '10001',
    type: 'mobile',
    category: '3',
    moduleState: 'deviceBusy',
    deviceName: 'DF0001A',
    featureName: '单频测量',
    latitude: 30.550624,
    longitude: 104.072941,
  };
  const myaxios = null;

  return (
    <div>
      <Button onClick={() => setShow(true)}>显示modal</Button>

      <TaskInfoModal visible={show} onCancel={() => setShow(false)} info={info} dcRequest={myaxios} />
    </div>
  );
};

export default TaskInfoModalTest;

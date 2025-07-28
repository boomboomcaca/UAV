import React, { useState } from 'react';
import TimeSlider from '../TimeSlider.jsx';
import styles from './index.module.less';

export default () => {
  const [time, setTime] = useState({
    createTime: '2022-09-15 16:54:10',
  });
  const [timeData, setTimeData] = useState([
    {
      longitude: 104.03342040775209,
      latitude: 29.987211751532406,
      altitude: 8014.810546875,
      horizontalSpeed: 820.228759765625,
      verticalSpeed: 4.200727462768555,
      azimuth: 224.17242521233857,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:10',
    },
    {
      longitude: 104.01971848800257,
      latitude: 29.97264269544844,
      altitude: 8014.810546875,
      horizontalSpeed: 824.1818237304688,
      verticalSpeed: 6.343491554260254,
      azimuth: 223.24315648712218,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:11',
    },
    {
      longitude: 104.00608152522425,
      latitude: 29.958012820323113,
      altitude: 8014.810546875,
      horizontalSpeed: 782.96533203125,
      verticalSpeed: 2.3246164321899414,
      azimuth: 222.98823248781264,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:12',
    },
    {
      longitude: 103.99254602844783,
      latitude: 29.94328901894522,
      altitude: 8014.810546875,
      horizontalSpeed: 786.5111083984375,
      verticalSpeed: 4.9350786209106445,
      azimuth: 222.59212874807417,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:13',
    },
    {
      longitude: 103.97878700823686,
      latitude: 29.92877387640943,
      altitude: 8014.810546875,
      horizontalSpeed: 807.9536743164062,
      verticalSpeed: 6.710480690002441,
      azimuth: 223.46813165582716,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:14',
    },
    {
      longitude: 103.96491289699611,
      latitude: 29.914368702433418,
      altitude: 8014.810546875,
      horizontalSpeed: 750.6854248046875,
      verticalSpeed: 9.708027839660645,
      azimuth: 223.92415677942336,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:15',
    },
    {
      longitude: 103.95080528005467,
      latitude: 29.900192132177057,
      altitude: 8014.810546875,
      horizontalSpeed: 794.6036376953125,
      verticalSpeed: 5.614928722381592,
      azimuth: 224.8603203240782,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:16',
    },
    {
      longitude: 103.93683729404307,
      latitude: 29.885877965530696,
      altitude: 8014.810546875,
      horizontalSpeed: 753.3407592773438,
      verticalSpeed: 1.401464819908142,
      azimuth: 224.298720324412,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:17',
    },
    {
      longitude: 103.92263158135569,
      latitude: 29.871799694079858,
      altitude: 8014.810546875,
      horizontalSpeed: 805.2535400390625,
      verticalSpeed: 3.022132158279419,
      azimuth: 225.25816007889807,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:18',
    },
    {
      longitude: 103.90863222691917,
      latitude: 29.857516204472603,
      altitude: 8014.810546875,
      horizontalSpeed: 757.5819091796875,
      verticalSpeed: 1.8529051542282104,
      azimuth: 224.4244143757969,
      transponderCode: 'UEA9001test',
      planeAddress: '90001',
      createTime: '2022-09-15 16:54:19',
    },
  ]);

  return (
    <div className={styles.container}>
      <TimeSlider
        timeRange={timeData}
        timeData={time}
        recall={(e) => {
          if (e.end) {
            setTime(e.value);
            console.log('recall--->', e.value);
          }
        }}
      />
    </div>
  );
};

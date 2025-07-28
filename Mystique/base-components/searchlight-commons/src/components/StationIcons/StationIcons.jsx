import mobileoffline from './imgs/mobile-offline.png';
import mobiledisabled from './imgs/mobile-disable.png';
import mobilefault from './imgs/mobile-fault.png';
import mobileidle from './imgs/mobile-idle.png';
import mobilebusy from './imgs/mobile-busy.png';
import mobileunknow from './imgs/mobile-unknow.png';

import portableoffline from './imgs/portable-offline.png';
import portabledisabled from './imgs/portable-disable.png';
import portablefault from './imgs/portable-fault.png';
import portableidle from './imgs/portable-idle.png';
import portablebusy from './imgs/portable-busy.png';
import portableunknow from './imgs/portable-unknow.png';

import stationaryoffline from './imgs/stationary-offline.png';
import stationarydisabled from './imgs/stationary-disable.png';
import stationaryfault from './imgs/stationary-fault.png';
import stationaryidle from './imgs/stationary-idle.png';
import stationarybusy from './imgs/stationary-busy.png';
import stationaryunknow from './imgs/stationary-unknow.png';

import sensoroffline from './imgs/sensor-offline.png';
import sensordisabled from './imgs/sensor-disabled.png';
import sensorfault from './imgs/sensor-fault.png';
import sensoridle from './imgs/sensor-idle.png';
import sensorbusy from './imgs/sensor-busy.png';
// import sensorunknow from './imgs/sensor-unknow.png';

import airoffline from './imgs/air-offline.png';
import airdisabled from './imgs/air-disabled.png';
import airfault from './imgs/air-fault.png';
import airidle from './imgs/air-idle.png';
import airbusy from './imgs/air-busy.png';
// import airunknow from './imgs/air-unknow.png';

const stationTypes = [
  'airCategory',
  'portableCategory',
  'sensorCategory',
  'movableCategory',
  'mobileCategory',
  'stationaryCategory',
];

const stationStatus = ['idle', 'busy', 'offline', 'disabled', 'fault', 'online'];

const Icons = {
  mobileoffline,
  mobiledisabled,
  mobilefault,
  mobileidle,
  mobilebusy,
  mobileunknow,
  portableoffline,
  portabledisabled,
  portablefault,
  portableidle,
  portablebusy,
  portableunknow,
  stationaryoffline,
  stationarydisabled,
  stationaryfault,
  stationaryidle,
  stationarybusy,
  stationaryunknow,
  sensoroffline,
  sensordisabled,
  sensorfault,
  sensoridle,
  sensorbusy,
  // sensorunknow,
  airoffline,
  airdisabled,
  airfault,
  airidle,
  airbusy,
  // airunknow,
};

export default function getStationIcons(type, status) {
  let typeResult = type;
  let statusResult = status;
  if (stationTypes.indexOf(typeResult) < 0) {
    typeResult = 'stationary';
  } else if (typeResult === 'movableCategory') {
    // movableCategory 和 portableCategory 图标一样
    typeResult = 'portableCategory';
  }
  typeResult = typeResult.replace('Category', '');
  if (stationStatus.indexOf(statusResult) < 0) {
    statusResult = 'unknow';
  }
  if (statusResult === 'online') {
    statusResult = 'idle';
  }
  const iconName = `${typeResult}${statusResult}`;
  return Icons[iconName];
}

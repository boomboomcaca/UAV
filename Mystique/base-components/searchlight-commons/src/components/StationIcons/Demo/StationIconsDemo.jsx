import React from 'react';
import getStationIcons from '../StationIcons';

const stationTypes = [
  'airCategory',
  'portableCategory',
  'sensorCategory',
  'movableCategory',
  'mobileCategory',
  'stationaryCategory',
];

const stationStatus = ['idle', 'busy', 'offline', 'disabled', 'fault'];

export default function StationIconDemo() {
  return (
    <div>
      {stationTypes.map((type) => {
        return stationStatus.map((status) => {
          return (
            <div key={`${type}-${status}`} style={{ marginRight: '10px', display: 'inline-block' }}>
              {type}-{status}:
              <img src={getStationIcons(type, status)} key={`${type}-${status}`} alt="暂未设计图标" />
            </div>
          );
        });
      })}
    </div>
  );
}

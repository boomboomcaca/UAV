import React from 'react';
import StationButton from '../index';
import {
  StationBuss,
  StationDisable,
  StationFault,
  StationFree,
  StationOffLine,
  StationUnknown,
} from '../Icon/index.jsx';

export default function Demo() {
  const onClick = () => {
    console.log('111');
  };

  return (
    <>
      <h3>监测站按钮</h3>
      <StationButton icon={<StationBuss />} size="small" onClick={onClick} />
      <StationButton icon={<StationDisable />} size="small" onClick={onClick} />
      <StationButton icon={<StationFault />} size="small" onClick={onClick} />
      <StationButton icon={<StationFree />} size="small" onClick={onClick} />
      <StationButton icon={<StationOffLine />} size="small" onClick={onClick} />
      <StationButton icon={<StationUnknown />} size="small" onClick={onClick} />
      <br />
      <StationButton icon={<StationBuss />} onClick={onClick} />
      <StationButton icon={<StationDisable />} onClick={onClick} />
      <StationButton icon={<StationFault />} onClick={onClick} />
      <StationButton icon={<StationFree />} onClick={onClick} />
      <StationButton icon={<StationOffLine />} onClick={onClick} />
      <StationButton icon={<StationUnknown />} onClick={onClick} />
      <StationButton icon={<StationDisable />} size="large" onClick={onClick} />
      <StationButton size="large" onClick={onClick} />
    </>
  );
}

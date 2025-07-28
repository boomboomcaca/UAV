import React from 'react';
import SelectButton from '../index';

export default function Demo() {
  const onChange = (state) => {
    console.log(state);
  };

  return (
    <SelectButton onChange={onChange} selected={true}>
      瀑布图
    </SelectButton>
  );
}

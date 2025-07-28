import React, { memo } from 'react';

export default memo(function IconCard(props) {
  return (
    <div
      className={`${props.selIcon === props.title ? 'IconItemSel' : 'IconItem'}`}
      onClick={() => {
        props.callback(props.title);
      }}
    >
      <div className="IconArea">{props.icon}</div>
      <div className="TextArea">{props.title}</div>
    </div>
  );
});

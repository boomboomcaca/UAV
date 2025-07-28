import React, { useState, useEffect } from 'react';
import TimeFrame from '../TimeFrame.jsx';

export default () => {
  const [timeData, setTimeData] = useState([8, 9, 4]);
  return (
    <div>
      {/* <TimeFrame
        recall={(e) => console.log(e)}
        type="号"
        timeRange={[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]}
        timeData={timeData}
        disable={true}
      /> */}
      <TimeFrame
        recall={(e) => console.log(e)}
        type="号"
        timeRange={[1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]}
        timeData={timeData}
      />
    </div>
  );
};

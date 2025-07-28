import React, { useState, useEffect } from 'react';
import Progress from '../index';

export default function Demo() {
  const [progress, setProgress] = useState(65);
  const closeClick = (val) => {
    console.log(val);
  };
  return (
    <div>
      <button
        onClick={() => {
          const a = progress + 2;
          if (a < 100) {
            setProgress(a);
          } else if (a >= 100) setProgress(100);
        }}
      >
        add
      </button>
      <Progress value={progress} closeClick={closeClick} />
    </div>
  );
}

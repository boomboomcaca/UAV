import React, { useEffect, useRef } from 'react';
import Img from '../assets/play.png';
import CanvasBase from './canvaseBase';

function CanvasBorderTest() {
  const canvasInstance = useRef(null);
  useEffect(() => {
    canvasInstance.current = new CanvasBase(document.getElementById('chart'));

    canvasInstance.current.drawImage(Img);
  }, []);

  return (
    <div>
      <div
        style={{ border: '1px solid #ddd' }}
        onClick={() => {
          canvasInstance.current.drawExpandBorder();
        }}
      >
        drawExpandBorder
      </div>
      <div style={{ width: '160px', height: '160px' }} id="chart" />
    </div>
  );
}

export default CanvasBorderTest;

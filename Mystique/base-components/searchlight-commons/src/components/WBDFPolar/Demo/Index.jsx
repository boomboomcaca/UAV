import React, { useState, useEffect, useRef } from 'react';

import WBDFPolar from '../WBDFPolar.jsx';

export default () => {
  const [quality, setQuality] = useState(12);
  const [bearings, setbearings] = useState([]);
  const setterRef = useRef();

  useEffect(() => {
    const t = setInterval(() => {
      if (setterRef.current) {
        const datas = {
          data: [
            {
              lng: Math.random() * 50 + 20,
              lat: Math.random() * 50 + 20,
            },
            {
              lng: Math.random() * 50 + 20,
              lat: Math.random() * 50 + 20,
            },
            {
              lng: Math.random() * 50 + 20,
              lat: Math.random() * 50 + 20,
            },
            {
              lng: Math.random() * 50 + 20,
              lat: Math.random() * 50 + 20,
            },
            {
              lng: Math.random() * 50 + 20,
              lat: Math.random() * 50 + 20,
            },
          ],
          centerGPS: { x: 50, y: 50 },
          radiusX: 50,
          radiusY: 50,
        };
        setterRef.current(datas);
      }
    }, 1000);
    return () => {
      clearInterval(t);
    };
  }, []);

  return (
    <div
      style={{
        width: '700px',
        height: '500px',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        border: 'blue',
      }}
    >
      <WBDFPolar
        tickInside
        showTickLabel={false}
        onLoaded={(e) => {
          setterRef.current = e;
        }}
      />
      <input
        type="number"
        max="100"
        min="0"
        onChange={(e) => {
          setQuality(Number(e.target.value));
        }}
      />
    </div>
  );
};

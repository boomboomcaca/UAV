import { useEffect, useRef } from 'react';

function useFrameTimer() {
  const timerRef = useRef(null);

  const lastValueRef = useRef(null);

  const valueQueue = useRef([]).current;

  const Enqueue = (val) => {
    if (!valueQueue.includes(val)) {
      valueQueue.push(val);
      // window.console.log(valueQueue);
    }
  };

  const Dequeue = () => {
    while (valueQueue.length > 1) {
      valueQueue.shift();
    }
    if (valueQueue.length > 0) {
      const val = valueQueue.shift();
      if (val !== lastValueRef.current) {
        lastValueRef.current = val;
        return val;
      }
    }
    return null;
  };

  const StartTimer = (callback) => {
    timerRef.current = setInterval(() => {
      const val = Dequeue();
      if (val) {
        callback(val);
      }
    }, 20);
  };

  const ExitTimer = () => {
    clearInterval(timerRef.current);
    timerRef.current = null;
  };

  useEffect(() => {}, []);

  return { Enqueue, Dequeue, StartTimer, ExitTimer };
}

export default useFrameTimer;

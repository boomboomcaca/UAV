import { useState, useEffect } from 'react';

function useWindowSize(callback = null) {
  const getWindowSize = (tag) => ({
    innerHeight: window.innerHeight,
    innerWidth: window.innerWidth,
    tag,
  });

  let resizeTimer = null;

  const [windowSize, setWindowSize] = useState(getWindowSize(0));

  useEffect(() => {
    callback?.(windowSize);
  }, [windowSize]);

  const handleResize = () => {
    if (resizeTimer) {
      clearTimeout(resizeTimer);
    }
    resizeTimer = setTimeout(() => {
      setWindowSize(getWindowSize(1));
    }, 100);
  };

  useEffect(() => {
    // 监听
    window.addEventListener('resize', handleResize);

    // 销毁
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  return windowSize;
}

export default useWindowSize;

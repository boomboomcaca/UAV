/**
 *
 * @param {HTMLElement} element
 * @param {Function} callback
 */
const useClick = (element, callback) => {
  let start;
  const touchStart = () => {
    start = new Date().getTime();
  };

  const touchEnd = (e) => {
    const end = new Date().getTime();
    if (end - start < 500) {
      // 触摸
      callback(e);
    }
  };
  // 是否移动端或支持touch
  const haveTouch = 'ontouchstart' in window;
  if (element) {
    element.addEventListener('touchstart', touchStart);
    element.addEventListener('touchend', touchEnd);
  }
  let preMouseDown = 0;
  let downTime = 0;
  const mouseDown = (e) => {
    const dTime = new Date().getTime();
    if (preMouseDown === 0 || dTime - preMouseDown < 300) {
      downTime += 1;
    }
    preMouseDown = dTime;
    if (downTime === 1) {
      setTimeout(() => {
        if (downTime === 1) {
          // 触发单击
          const args = {
            dblClick: false,
            event: e,
          };
          callback(args);
        } else {
          // 触发双击
          const args = {
            dblClick: true,
            event: e,
          };
          callback(args);
        }
        downTime = 0;
        preMouseDown = 0;
      }, 301);
    }
  };

  // 有触摸事件，则只是用触摸事件
  if (element && !haveTouch) {
    element.addEventListener('mousedown', mouseDown);
  }

  return () => {
    if (element) {
      element.removeEventListener('touchstart', touchStart);
      element.removeEventListener('touchend', touchEnd);
      if (!haveTouch) {
        element.removeEventListener('mousedown', mouseDown);
      }
    }
  };
};

export default useClick;

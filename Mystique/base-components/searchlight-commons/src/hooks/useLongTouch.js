export default function useLongTouch(target, onLongTouch) {
  let tmr;
  let pressed = false;
  const touchStart = (event) => {
    pressed = true;
    tmr = setTimeout(() => {
      if (pressed) {
        onLongTouch(event);
      }
    }, 750);
  };

  const touchEnd = () => {
    pressed = false;
    if (tmr) {
      clearTimeout(tmr);
    }
  };
  // 是否移动端或支持touch
  const haveTouch = 'ontouchstart' in window;
  if (target && haveTouch) {
    target.addEventListener('touchstart', touchStart);
    target.addEventListener('touchend', touchEnd);
  }
  return () => {
    if (target && haveTouch) {
      target.removeEventListener('touchstart', touchStart);
      target.removeEventListener('touchend', touchEnd);
    }
  };
}

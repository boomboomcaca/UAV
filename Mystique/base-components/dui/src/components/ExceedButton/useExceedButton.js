import { useRef, useState, useEffect, useCallback } from 'react';
import { getArea, pointOutArea } from './utils/graph';

function useExceedButton(checked, disable, waiting, value, options, onChange, onChangeTrigger, switchTrigger) {
  const rootRef = useRef(null);
  const popupRef = useRef(null);

  const [showOptions, setShowOptions] = useState(false);

  const [loading, setLoading] = useState(false);
  useEffect(() => {
    if (typeof waiting === 'boolean') {
      setLoading(waiting);
    }
  }, [waiting]);

  const [valueLabel, setvalueLabel] = useState(null);
  useEffect(() => {
    const find = options?.find((o) => {
      return o.value === value;
    });
    setvalueLabel(find?.label);
  }, [value, options]);

  const onMouseUp = useCallback((e) => {
    const div1 = rootRef.current;
    const div2 = popupRef.current.div;
    const point = { x: e.clientX, y: e.clientY };
    const area1 = getArea(div1);
    const area2 = getArea(div2);
    if (pointOutArea(point, area1) && pointOutArea(point, area2)) {
      setShowOptions(false);
      window.removeEventListener('mouseup', onMouseUp);
    }
  }, []);

  const onRootClick = () => {
    if (disable || loading) {
      return;
    }
    if (!showOptions && options) {
      if (options.length > switchTrigger) {
        setShowOptions(true);
        window.addEventListener('mouseup', onMouseUp);
      } else {
        let idx = 0;
        if (value) {
          idx = options.indexOf(
            options.find((o) => {
              return o.value === value;
            }),
          );
          idx += 1;
          if (idx > options.length - 1) {
            idx = 0;
          }
        }
        onChange({ event: 'valueChange', args: options[idx] });
      }
    }
    if (!options) {
      if (checked !== null && checked !== undefined) {
        onChange({ event: 'checkedChange', args: { checked: !checked } });
      } else {
        onChange({ event: 'click' });
      }
      onLoading();
    }
  };

  const onItemClick = (e) => {
    setShowOptions(false);
    window.removeEventListener('mouseup', onMouseUp);
    if ((onChangeTrigger === 'valueChange' && e.value !== value) || onChangeTrigger === 'itemClick') {
      onChange({ event: 'valueChange', args: e });
      onLoading();
    }
  };

  const onLoading = () => {
    if (typeof waiting === 'number') {
      setLoading(true);
      setTimeout(() => {
        setLoading(false);
      }, waiting || 0);
    }
  };

  return { rootRef, popupRef, showOptions, valueLabel, loading, onRootClick, onItemClick };
}

export default useExceedButton;

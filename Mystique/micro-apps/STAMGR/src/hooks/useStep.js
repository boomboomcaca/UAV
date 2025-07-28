import { useState, useEffect, useRef } from 'react';
import { message, Modal } from 'dui';

function useStep(visible, editype, station, newStation, setNewStation, onStationSave) {
  const [step, setStep] = useState(0);

  const savedRef = useRef(true);

  const onNextStep = (tag, callback = null) => {
    if (step === 1) {
      if (savedRef.current === true) {
        setStep(tag);
      } else if (editype === 'new') {
        message.warning({ key: 'tip', content: '数据还未保存，不能进行其它步骤！' });
      } else {
        Modal.confirm({
          title: '提示',
          closable: false,
          content: '数据还未保存，是否保存？',
          onOk: () => {
            onStationSave(newStation, tag);
          },
          onCacel: () => {
            setNewStation({ ...station });
          },
        });
      }
    } else {
      callback === null || callback?.()
        ? setStep(tag)
        : message.warning({ key: 'tip', content: '数据还未保存，不能进行其它步骤！' });
    }
  };

  useEffect(() => {
    if (visible) {
      setStep(1);
    } else {
      setStep(0);
    }
    savedRef.current = editype !== 'new';
  }, [visible, editype]);

  return { step, setStep, onNextStep, savedRef };
}

export default useStep;

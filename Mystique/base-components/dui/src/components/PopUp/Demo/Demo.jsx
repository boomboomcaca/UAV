import React, { useState } from 'react';
import PopUp from '../index';
import styles from './index.module.less';

export default function Demo() {
  const [show, setShow] = useState(false);
  const [show2, setShow2] = useState(false);
  const [popupTransition, setPopupTransition] = useState('rtg-slide-right');

  const toshow = (type) => {
    setShow(true);
    setPopupTransition(type);
  };

  return (
    <div style={{ position: 'relative', overflow: 'hidden' }}>
      <div onClick={() => setShow(true)}>PopUp demo</div>
      <br />
      <br />
      <br />
      <div onClick={() => setShow2(true)}>PopUp demo2</div>
      <br />
      <div>内容区域宽高，占据何方由div自己定义</div>
      <br />
      <div>
        示例div：
        {`  position: absolute;
  top: 0;
  right: 0;
  width: 50%;
  height: 100%;
  background-color: #102947;`}
      </div>
      <br />
      <br />
      <br />
      <div>pop层暂提供以下动画效果 可点击查看</div>
      <br />
      <div onClick={() => toshow('rtg-zoom')}>rtg-zoom</div>
      <br />
      <div onClick={() => toshow('rtg-slide-up')}>rtg-slide-up</div>
      <br />
      <div onClick={() => toshow('rtg-slide-down')}>rtg-slide-down</div>
      <br />
      <div onClick={() => toshow('rtg-slide-right')}>rtg-slide-right</div>
      <br />
      <PopUp visible={show} popupTransition={popupTransition} mask onCancel={() => setShow(false)}>
        <div className={styles.taskinfo}>123</div>
      </PopUp>

      <PopUp
        visible={show2}
        popupTransition="rtg-zoom"
        onCancel={() => setShow2(false)}
        mask
        maskclosable
        // popStyle={{ top: '50px', height: '50%' }}
      >
        <div className={styles.taskpop}>
          <div className={styles.taskcontent}>123</div>
        </div>
      </PopUp>
    </div>
  );
}

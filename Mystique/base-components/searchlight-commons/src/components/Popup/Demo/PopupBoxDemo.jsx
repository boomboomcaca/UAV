/* eslint-disable react/jsx-wrap-multilines */
import React, { useState } from 'react';
import { Button } from 'dui';
import Popup, { PopupModal, PopupDrawer } from '../index';
import styles from './index.module.less';

const PopupBoxDemo = () => {
  const [showPopup1, setShowPopup1] = useState(false);
  const [showPopup2, setShowPopup2] = useState(false);
  const [showPopup3, setShowPopup3] = useState(false);

  const [showModal, setShowModal] = useState(false);
  const [showDrawer, setShowDrawer] = useState(false);

  const [showDrawer2, setShowDrawer2] = useState(false);

  return (
    <div className={styles.root}>
      <div className={styles.content}>
        <Button
          onClick={() => {
            setShowPopup1(true);
          }}
        >
          第一种弹出层
        </Button>
        <div>hello world!</div>
        <Button
          onClick={() => {
            setShowPopup2(true);
          }}
        >
          第二种弹出层
        </Button>
        <div>hello world!</div>
        <Button
          onClick={() => {
            setShowPopup3(true);
          }}
        >
          第三种弹出层
        </Button>
        <div>hello world!</div>
        <Button
          onClick={() => {
            setShowModal(!showModal);
          }}
        >
          PopupModal
        </Button>
        <div>hello world!</div>
        <Button
          onClick={() => {
            setShowDrawer(!showDrawer);
          }}
        >
          PopupDrawer
        </Button>

        <Button
          onClick={() => {
            setShowDrawer2(!showDrawer2);
          }}
        >
          PopupDrawer2
        </Button>

        <Popup
          style={{ backgroundColor: 'transparent' }}
          popStyle={{
            width: '30%',
            height: '100%',
            backgroundColor: '#80802080',
          }}
          visible={showPopup1}
          onClose={() => {
            setShowPopup1(false);
          }}
        >
          hello wolrd!
        </Popup>

        <Popup
          style={{ backgroundColor: 'transparent' }}
          popStyle={{
            width: '30%',
            height: '100%',
            backgroundColor: '#80802080',
          }}
          getContainer={false}
          visible={showPopup3}
          onClose={() => {
            setShowPopup3(false);
          }}
        >
          <Button
            onClick={() => {
              setShowPopup1(true);
            }}
          >
            第一种弹出层
          </Button>
        </Popup>
      </div>
      <PopupModal
        visible={showModal}
        title="保存模板"
        popStyle={{ width: '80%', height: '80%' }}
        closeOnMask={false}
        popClassName={styles.pop1}
        onClose={() => {
          setShowModal((prev) => {
            return !prev;
          });
        }}
      >
        <div>hello world!</div>
      </PopupModal>
      <PopupDrawer
        visible={showDrawer}
        title="选择模板"
        popClassName={styles.pop2}
        onClose={() => {
          setShowDrawer((prev) => {
            return !prev;
          });
        }}
        titleAttach={
          <Button
            onClick={() => {
              setShowDrawer2(!showDrawer2);
            }}
          >
            PopupDrawer2
          </Button>
        }
        onTitleAttachClick={() => {
          window.console.log('collecting');
        }}
      >
        <div>hello world!</div>
      </PopupDrawer>
      <Popup
        style={{ backgroundColor: 'transparent' }}
        popStyle={{
          width: '30%',
          height: '100%',
          backgroundColor: '#80802080',
        }}
        destoryOnClose
        visible={showPopup2}
        onClose={() => {
          setShowPopup2(false);
        }}
      >
        hello wolrd!
      </Popup>

      <PopupDrawer
        visible={showDrawer2}
        title="选择模板"
        popStyle={{ width: 300 }}
        onClose={() => {
          setShowDrawer2((prev) => {
            return !prev;
          });
        }}
        titleAttach="采集当前模板"
        onTitleAttachClick={() => {
          window.console.log('collecting');
        }}
        getContainer="otherContainer"
      >
        <div>hello world!</div>
      </PopupDrawer>

      <div id="otherContainer" className={styles.container}>
        showPopupDrawer2
      </div>
    </div>
  );
};

PopupBoxDemo.defaultProps = {};

PopupBoxDemo.propTypes = {};

export default PopupBoxDemo;

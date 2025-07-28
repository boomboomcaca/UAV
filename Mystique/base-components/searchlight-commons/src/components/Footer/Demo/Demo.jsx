import React, { useRef, useState } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import { message } from 'dui';
import Footer from '..';
import styles from './index.module.less';

const { Audio, Capture, RunState, Record, RecordIQ, Data, Station } = Footer;

const Demo = (props) => {
  const { className } = props;

  const footer = useRef(null);

  const [test, setTest] = useState(0);

  const [msg, setMsg] = useState({
    type: 'error',
    msg: '2022-03-22 打开二阶三阶谐波，进入mkr列表点击删除按钮没有删除mkr（偶现）测试继续观察复现',
  });

  // const [chkDef, setChkDef] = useState({ [RunState]: true, [Record]: false });

  const chkDef = useRef({ [RunState]: false, [Record]: false }).current;

  return (
    <div className={classnames(styles.root, className)}>
      <Footer
        ref={footer}
        message={msg}
        disableStatesDef={{ [Audio]: true, [Data]: true, [RunState]: false, [Record]: false, [Station]: true }}
        checkedStatesDef={chkDef}
        visibleStatesDef={{ [Station]: true, [Audio]: false, [Record]: true, [RecordIQ]: false }}
        hasAudio
        popupContent={<div>123</div>}
        showPopup
        edgeInfo={{
          edgeId: '40002',
          featureId: 'e7f24f60-5405-11ed-b38a-3ff40986c160',
          mfid: '40002',
          deviceId: '9bb2f6e0-6256-11ed-b5df-a5953a71f56c',
          edgeName: '40002',
          deviceName: '演示接收机',
          featureName: '单频测量',
          address: null,
          moduleState: 'idle',
          isActive: true,
          type: 'stationaryCategory',
          longitude: 104.07274,
          latitude: 30.578993,
          category: '1',
          frequency: {
            minimum: 0.3,
            maximum: 8000,
          },
          ifBandwidth: 40000,
        }}
        // antennaInfo={{
        //   id: 'a7fb7a07-912e-2da1-5547-7b75fec9102b',
        //   displayName: '天线2',
        //   model: 'DH8911',
        //   type: 'monitoring',
        //   startFrequency: 20,
        //   stopFrequency: 1300,
        //   polarization: 'horizontal',
        //   isActive: 3,
        //   height: 0,
        //   gain: 0,
        //   passiveCode: '0x21',
        //   activeCode: '0x22',
        //   tag: '',
        //   lowisActive: true,
        // }}
        // avaliable={false}
        onClick={(before, tag, e) => {
          window.console.log(before, tag, e);
          if (tag === Audio) {
            footer.current.updateCheckedStates(tag, !before);
          }
          if (tag === Capture) {
            footer.current.updateLoadingStates(tag, true);
            setTimeout(() => {
              footer.current.updateLoadingStates(tag, false);
            }, 1000);
          }
          if (tag === RunState) {
            footer.current.updateLoadingStates(tag, true);
            footer.current.updateCheckedStates(Record, false);
            setTimeout(() => {
              footer.current.updateLoadingStates(tag, false);
              footer.current.updateCheckedStates(tag, !before);
            }, 1000);
          }
          if (tag === Record) {
            footer.current.updateCheckedStates(tag, !before);
          }
          if (tag === Data) {
            window.console.log(tag);
            message.info(tag);
          }
          if (tag === Station) {
            window.console.log(tag);
            message.info(tag);
          }
        }}
        canSpace={false}
      >
        啥也没有
      </Footer>
      <div
        className={styles.test}
        onClick={() => {
          setTest(test + 1);
        }}
      >
        {`test:${test}`}
      </div>
    </div>
  );
};

Demo.defaultProps = {
  className: null,
};

Demo.propTypes = {
  className: PropTypes.any,
};

export default Demo;

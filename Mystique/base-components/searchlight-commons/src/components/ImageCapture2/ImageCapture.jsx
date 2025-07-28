import React, { memo, forwardRef, useImperativeHandle, useEffect, useState, useRef } from 'react';
import PropTypes from 'prop-types';
import { useKeyPress } from 'ahooks';
import Icon from '@ant-design/icons';
import { PopUp } from 'dui';
import { CloseIcon } from 'dc-icon';
import Popup from '../Popup';
import showImage, { downloadImage, getFormatedDate } from './capture';
import FinishSVG from './FinishSVG.jsx';
import CloseSVG from './CloseSVG.jsx';
import styles from './index.module.less';

const ImageCapture = forwardRef((props, ref) => {
  const { timeout, axios, selectedModule, feature } = props;

  const [imgUrl, setImgUrl] = useState(null);

  useImperativeHandle(ref, () => ({
    onCapture: (domID, callback) => {
      showImage(domID, (imgUri) => {
        callback({ tag: 'captureResult', value: true, url: imgUri });
        const item = { url: imgUri, timestamp: new Date().getTime() };
        setImgUrl(item);
      });
    },
    onCaptureTest: (url, callback) => {
      callback({ tag: 'captureResult', value: true, url });
      const item = { url, timestamp: new Date().getTime() };
      setImgUrl(item);
    },
  }));

  const [printscreens, savePrintscreens] = useState([]);

  const excludeRef = useRef(null);
  const [showMax, setShowMax] = useState(false);
  useKeyPress('esc', () => {
    setShowMax?.(false);
  });

  const removeItem = (timestamp) => {
    savePrintscreens(
      printscreens.filter((i) => {
        if (timestamp === i.timestamp) {
          clearTimeout(i.timer);
          return false;
        }
        return true;
      }),
    );
  };

  const down = ({ url, timestamp }) => {
    const name = `${feature?.toUpperCase()}${getFormatedDate(timestamp)}.jpg`;
    window.console.log(name);
    downloadImage(url, name, timeout);
    if (axios && feature) {
      const img = {
        edgeId: selectedModule?.edgeId || 'unknown',
        functionName: feature,
        name,
        image: url,
      };
      axios({
        url: `sys/screenshot/add`,
        method: 'post',
        data: img,
      }).then((res) => {
        // TODO 暂不提醒
        window.console.log(res);
      });
    }
    removeItem(timestamp);
  };

  useEffect(() => {
    if (imgUrl && imgUrl.url !== null) {
      savePrintscreens([
        ...printscreens,
        {
          ...imgUrl,
          timestamp: new Date().getTime(),
        },
      ]);
    }
  }, [imgUrl]);

  useEffect(() => {
    let interval = null;
    if (printscreens.length > 0 && !showMax) {
      interval = setInterval(() => {
        const now = new Date().getTime();
        printscreens.forEach((i) => {
          const { timestamp } = i;
          if (now - timestamp >= timeout) {
            down(i);
          }
        });
      }, 100);
    }
    return () => clearInterval(interval);
  }, [printscreens, showMax]);

  return (
    <Popup
      popStyle={{
        width: 'auto',
        height: 'auto',
        bottom: 0,
        pointerEvents: 'all',
      }}
      ghost
      getContainer={false}
      closeOnMask={false}
      visible
    >
      <div className={styles.root}>
        {printscreens.map((i) => {
          return (
            <div
              key={i.timestamp}
              className={styles.img}
              onClick={() => {
                excludeRef.current = i;
                setShowMax(true);
              }}
              title="查看大图"
            >
              <img alt="img" src={i.url} />
              <div className={styles.btn} style={{ pointerEvents: 'all' }}>
                <div
                  className={styles.down}
                  onClick={(e) => {
                    e.stopPropagation();
                    down(i);
                  }}
                >
                  <Icon title="立即保存" component={FinishSVG} />
                </div>
                <div
                  className={styles.close}
                  onClick={(e) => {
                    e.stopPropagation();
                    removeItem(i.timestamp);
                  }}
                >
                  <Icon title="取消保存" component={CloseSVG} />
                </div>
              </div>
            </div>
          );
        })}
        <PopUp visible={showMax} popupTransition="rtg-fade" usePortal mask={false} popStyle={{ positive: 'relative' }}>
          <div className={styles.imgMax}>
            <div className={styles.image}>
              <img alt="" src={excludeRef.current?.url} />
            </div>
            <div className={styles.btnMax}>
              <div
                className={styles.closeMax}
                onClick={() => {
                  excludeRef.current = null;
                  setShowMax(false);
                }}
              >
                <CloseIcon />
              </div>
            </div>
          </div>
        </PopUp>
      </div>
    </Popup>
  );
});

ImageCapture.defaultProps = {
  timeout: 6000,
  axios: null,
  selectedModule: null,
  feature: null,
};

ImageCapture.propTypes = {
  timeout: PropTypes.number,
  axios: PropTypes.any,
  selectedModule: PropTypes.any,
  feature: PropTypes.any,
};

const areEquals = (prev, next) => {
  return prev.imgURL === next.imgURL;
};

export default memo(ImageCapture, areEquals);

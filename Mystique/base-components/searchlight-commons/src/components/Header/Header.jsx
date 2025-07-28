/* eslint-disable max-len */
import React, { useEffect, useState, useRef, useMemo, useImperativeHandle } from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import NotiFilter from 'notifilter';
import Icon from '@ant-design/icons';
import {
  BackIcon,
  /* HomeIcon, */
  RoundInfoIcon,
  /* TaskIcon, */
  SettingIcon,
  NewReplayListIcon,
} from 'dc-icon';
import langT from 'dc-intl';
import Srm from './srm/Index.jsx';
import { MenuType } from './header';
import styles from './index.module.less';

const Header = React.forwardRef((props, ref) => {
  const {
    title,
    pdfUrl,
    showIcon,
    hideIcon,
    // showRightLogo,
    children,
    disabledState,
    visible,
    className,
    style,
    // taskNumber,
    dcAxios,
    onMenuItemClick,
    edgeId,
    wsNotiUrl,
  } = props;

  const [systemTime, setSystemTime] = useState('');
  const [srmData, setSrmData] = useState({
    srmCPU: { percent: 0 },
    srmMemory: { percent: 0, used: 0, total: 0 },
    srmHDD: { percent: 0, used: 0, total: 0 },
  });

  const srmDataRef = useRef({
    srmCPU: { percent: 0 },
    srmMemory: { percent: 0, used: 0, total: 0 },
    srmHDD: { percent: 0, used: 0, total: 0 },
  });
  const sysTimeDifferenceRef = useRef(0);
  let timeInterval = null;
  useEffect(() => {
    // 时钟函数
    initClock();
    return () => {
      if (timeInterval) {
        clearInterval(timeInterval);
        timeInterval = null;
      }
    };
  }, []);
  const notiFilterFn = (noti) => {
    if (noti.type === 'srmCPU') {
      updateSrmData({
        ...srmDataRef.current,
        srmCPU: { percent: noti.useage || 0 },
      });
    }
    if (noti.type === 'srmMemory') {
      updateSrmData({
        ...srmDataRef.current,
        srmMemory: {
          percent: noti.total === 0 ? 0 : (noti.used / noti.total) * 100,
          used: noti.used,
          total: noti.total,
        },
      });
    }
    if (noti.type === 'srmHDD') {
      updateSrmData({
        ...srmDataRef.current,
        srmHDD: {
          percent: noti.total === 0 ? 0 : (noti.used / noti.total) * 100,
          used: noti.used,
          total: noti.total,
        },
      });
    }
  };
  const initClock = () => {
    // 时钟函数
    if (timeInterval) {
      clearInterval(timeInterval);
      timeInterval = null;
    }
    // 获取系统时间
    const sysTimePro = new Promise((resolve, reject) => {
      // 获取系统时间
      if (dcAxios) {
        dcAxios({
          url: '/manager/runtime/getDate',
          method: 'get',
        }).then((res) => {
          const { result } = res;
          if (result) {
            const sysDate = new Date(result);
            const nowDate = new Date();
            const timeDifference = sysDate.getTime() - nowDate.getTime();
            window.sessionStorage.setItem('sysTimeDifference', timeDifference);
            // 系统时间戳差值
            sysTimeDifferenceRef.current = timeDifference;
            resolve(result);
          }
          resolve(null);
        });
      } else {
        resolve(null);
      }
    });
    Promise.all([sysTimePro]).then((res) => {
      let sysTime = res[0];
      if (!sysTime && window.sessionStorage.getItem('sysTimeDifference')) {
        sysTime = new Date().getTime() + Number(window.sessionStorage.getItem('sysTimeDifference'));
      }
      timeInterval = setInterval(() => {
        if (sysTime) {
          // 更新系统时间
          sysTime += 1000;
        }
        const nowDate = sysTime ? new Date(sysTime) : new Date();
        const dayWords = ['天', '一', '二', '三', '四', '五', '六'];
        const year = nowDate.getFullYear();
        const day = nowDate.getDay();
        let month = nowDate.getMonth() + 1;
        month = month > 9 ? month : `0${month}`;
        let days = nowDate.getDate();
        days = days > 9 ? days : `0${days}`;
        let hours = nowDate.getHours();
        hours = hours > 9 ? hours : `0${hours}`;
        let minutes = nowDate.getMinutes();
        minutes = minutes > 9 ? minutes : `0${minutes}`;
        let seconds = nowDate.getSeconds();
        seconds = seconds > 9 ? seconds : `0${seconds}`;
        // const timeStr = `${year}.${month}.${days} 星期${dayWords[day]} ${hours}:${minutes}:${seconds}`;
        const timeStr = `${year}.${month}.${days} ${langT('commons', `HeaderTime${day}`)} ${hours}:${minutes}`;
        setSystemTime(timeStr);
      }, 1000);
    });
  };
  const showIconStyle = (type) => {
    const bo1 = showIcon === null || showIcon === undefined || showIcon.includes(type);
    const s1 = bo1 ? null : { display: 'none' };
    const bo2 = hideIcon !== null && hideIcon !== undefined && hideIcon.includes(type);
    const s2 = bo2 ? { display: 'none' } : null;
    let enable = true;
    if (typeof disabledState === 'boolean') {
      enable = !disabledState;
    }
    if (typeof disabledState === 'string') {
      if (disabledState === 'left') {
        if (type === MenuType.HOME || type === MenuType.RETURN) {
          enable = false;
        } else {
          enable = true;
        }
      }
      if (disabledState === 'right') {
        if (
          type === MenuType.MESSAGE ||
          type === MenuType.MORE ||
          type === MenuType.INFO ||
          type === MenuType.TOOL ||
          type === MenuType.REPLAY
        ) {
          enable = false;
        } else {
          enable = true;
        }
      }
    }
    if (disabledState instanceof Array) {
      if (disabledState.indexOf(type) > -1) {
        enable = false;
      }
    }
    const s3 = {
      pointerEvents: enable ? 'all' : 'none',
      opacity: enable ? 1 : 0.5,
    };
    return { ...s1, ...s2, ...s3 };
  };
  const updateSrmData = (obj) => {
    setSrmData(obj);
    srmDataRef.current = obj;
  };
  useMemo(() => {
    updateSrmData({
      srmCPU: { percent: 0 },
      srmMemory: { percent: 0, used: 0, total: 0 },
      srmHDD: { percent: 0, used: 0, total: 0 },
    });
    let unregister;
    if (edgeId && wsNotiUrl) {
      unregister = NotiFilter.register({
        url: wsNotiUrl,
        onmessage: (ret) => {
          const { result } = ret;
          if (result.dataCollection) {
            for (let i = 0; i < result.dataCollection.length; i += 1) {
              const noti = result.dataCollection[i];
              notiFilterFn && notiFilterFn(noti);
            }
          }
        },
        edgeId: [edgeId],
        dataType: ['srmCPU', 'srmMemory', 'srmHDD'],
      });
    }
    return () => {
      if (unregister) {
        unregister();
      }
    };
  }, [edgeId]);

  // 自定义暴露给父组件的方法或者变量
  useImperativeHandle(
    ref,
    () => ({
      getSysDate: () => {
        // 系统时间Date
        if (sysTimeDifferenceRef.current) {
          const nowDateTime = new Date().getTime();
          return new Date(sysTimeDifferenceRef.current + nowDateTime);
        }
        return new Date();
      },
    }),
    [],
  );

  return (
    <div className={classnames(styles.header, className, !visible ? styles.hide : null)} style={style}>
      <div className={styles.fuck} />
      <div className={styles.back}>
        <div className={styles.title2}>{title}</div>
      </div>
      <div className={styles.action}>
        <div className={styles.icon1} />
        <div className={styles.actleft}>
          {edgeId && (
            <div className={styles.srm}>
              <div className={styles.content}>
                <Srm type="ssd" data={srmData.srmHDD} />
                <Srm type="memory" data={srmData.srmMemory} />
                <Srm type="cpu" data={srmData.srmCPU} />
              </div>
            </div>
          )}
          <div
            className={styles.actionleftitem}
            style={showIconStyle(MenuType.RETURN)}
            onClick={() => {
              onMenuItemClick(MenuType.RETURN);
            }}
          >
            <div>
              <BackIcon color="var(--theme-font-80)" />
            </div>
          </div>
          <div
            className={styles.actionleftitem}
            style={showIconStyle(MenuType.HOME)}
            onClick={() => {
              onMenuItemClick(MenuType.HOME);
            }}
          >
            <div>
              {/* <HomeIcon color="var(--theme-font-80)" /> */}
              {/* 2022-04-11 下次修改子应用修改，到时候此处图标再该回去 */}
              <BackIcon color="var(--theme-font-80)" />
            </div>
          </div>
        </div>
        <div className={styles.actright}>
          <div className={styles.actrightInner}>
            <div
              className={styles.actrightitem}
              style={showIconStyle(MenuType.INFO)}
              onClick={() => {
                onMenuItemClick(MenuType.INFO);
              }}
            >
              <div>
                <RoundInfoIcon color="var(--theme-font-80)" />
              </div>
            </div>
            <div
              className={styles.actrightitem}
              style={showIconStyle(MenuType.REPLAY)}
              onClick={() => {
                onMenuItemClick(MenuType.REPLAY);
              }}
            >
              <div>
                <NewReplayListIcon color="var(--theme-font-80)" />
              </div>
            </div>
            <div
              className={styles.actrightitem}
              style={showIconStyle(MenuType.MORE)}
              onClick={() => {
                onMenuItemClick(MenuType.MORE);
              }}
            >
              <div>
                <SettingIcon color="var(--theme-font-80)" />
              </div>
            </div>
            <div
              className={styles.actrightitem}
              style={showIconStyle(MenuType.HELP)}
              onClick={() => {
                onMenuItemClick(MenuType.HELP);
              }}
            >
              <div>
                {pdfUrl && pdfUrl !== '' && pdfUrl.includes?.('http') ? (
                  <a href={pdfUrl} target="_blank" rel="noreferrer">
                    <Icon component={HELPSVG} />
                  </a>
                ) : (
                  <Icon component={HELPSVG} />
                )}
              </div>
            </div>
            {(children && toString.call(children) === '[object Array]' ? children : [children]).map((child) => {
              return child ? (
                <div key={child.key} className={styles.actrightitem}>
                  {child}
                </div>
              ) : null;
            })}
          </div>
        </div>
        <div className={styles.righttime}>
          <span>{systemTime}</span>
        </div>
      </div>
    </div>
  );
});

Header.defaultProps = {
  title: '',
  pdfUrl: '',
  showIcon: null,
  hideIcon: null,
  // showRightLogo: true,
  children: null,
  disabledState: false,
  visible: true,
  className: null,
  style: null,
  // taskNumber: null,
  dcAxios: null,
  edgeId: '',
  wsNotiUrl: '',
  onMenuItemClick: () => {},
};

Header.propTypes = {
  title: PropTypes.any,
  pdfUrl: PropTypes.string,
  showIcon: PropTypes.array,
  hideIcon: PropTypes.array,
  // showRightLogo: PropTypes.bool,
  children: PropTypes.any,
  disabledState: PropTypes.any,
  visible: PropTypes.bool,
  className: PropTypes.any,
  style: PropTypes.any,
  // taskNumber: PropTypes.any,
  dcAxios: PropTypes.func,
  edgeId: PropTypes.string,
  wsNotiUrl: PropTypes.string,
  onMenuItemClick: PropTypes.func,
};

const HELPSVG = () => (
  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      fillRule="evenodd"
      clipRule="evenodd"
      d="M20.5 12C20.5 16.6944 16.6944 20.5 12 20.5C7.30558 20.5 3.5 16.6944 3.5 12C3.5 7.30558 7.30558 3.5 12 3.5C16.6944 3.5 20.5 7.30558 20.5 12ZM22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2C17.5228 2 22 6.47715 22 12ZM12 6.74988C10.7574 6.74988 9.75002 7.75724 9.75002 8.99988C9.75002 9.41409 9.41423 9.74988 9.00002 9.74988C8.5858 9.74988 8.25002 9.41409 8.25002 8.99988C8.25002 6.92881 9.92895 5.24988 12 5.24988C14.0711 5.24988 15.75 6.92881 15.75 8.99988C15.75 10.1959 15.1892 11.2614 14.3193 11.9468C13.8871 12.2873 13.4822 12.6275 13.1841 12.9941C12.8894 13.3566 12.75 13.6833 12.75 13.9999V15.4999C12.75 15.9141 12.4142 16.2499 12 16.2499C11.5858 16.2499 11.25 15.9141 11.25 15.4999V13.9999C11.25 13.2119 11.6044 12.5593 12.0203 12.0478C12.4329 11.5403 12.9555 11.1117 13.391 10.7686C13.9155 10.3553 14.25 9.71689 14.25 8.99988C14.25 7.75724 13.2427 6.74988 12 6.74988ZM13 17.9999C13 18.5522 12.5523 18.9999 12 18.9999C11.4477 18.9999 11 18.5522 11 17.9999C11 17.4477 11.4477 16.9999 12 16.9999C12.5523 16.9999 13 17.4477 13 17.9999Z"
      fill="var(--theme-font-80)"
    />
  </svg>
);
export default Header;

import React, { useState, useRef } from 'react';
// import PropTypes from 'prop-types';
import { CompassIcon, PlaybackIcon, CalculateIcon } from 'dc-icon';
import Header from '../index';
import dcAxios from '../../../api/request';
import ssd from '../ssd.jsx';
import styles from './index.module.less';

const { MenuType } = Header;

const HeaderDemo = () => {
  const [menuItem, setMenuItem] = useState(null);
  const headerRef = useRef();
  const onMenuItemClick = (type) => {
    // window.console.log(type);
    const sysDate = headerRef.current.getSysDate();
    // console.log('sysDate--->', sysDate);
    setMenuItem(type);
    switch (type) {
      case MenuType.RETURN:
        break;
      case MenuType.HOME:
        break;
      case MenuType.MESSAGE:
        break;
      case MenuType.MORE:
        break;
      case MenuType.INFO:
        break;
      case MenuType.REPLAY:
        break;
      default:
        break;
    }
  };

  const title = (
    <>
      <PlaybackIcon style={{ marginRight: 8 }} />
      单车场强定位
    </>
  );

  return (
    <div className={styles.root}>
      {/* <span>按钮点击：{menuItem}</span> */}
      <Header
        ref={headerRef}
        title={title}
        edgeId="40009"
        wsNotiUrl="ws://192.168.102.16:12001/notify"
        showIcon={[MenuType.HOME, MenuType.MESSAGE]}
        hideIcon={[MenuType.MESSAGE]}
        onMenuItemClick={onMenuItemClick}
        dcAxios={dcAxios}
      >
        <CompassIcon
          key={1}
          onClick={() => {
            window.console.log('click');
          }}
        />
        <CalculateIcon key={2} />
      </Header>
      <Header
        title={title}
        hideIcon={[MenuType.RETURN]}
        disabledState
        onMenuItemClick={onMenuItemClick}
        taskNumber={9}
      />
      <Header title={title} disabledState="left" onMenuItemClick={onMenuItemClick} taskNumber={88} />
      <Header
        title={title}
        pdfUrl="http://192.168.102.103:6066/public/ffm.pdf"
        disabledState="right"
        style={{ marginBottom: 8 }}
        onMenuItemClick={onMenuItemClick}
        taskNumber={0}
      />
      <Header
        title={title}
        disabledState={[MenuType.HOME, MenuType.MORE]}
        onMenuItemClick={onMenuItemClick}
        taskNumber={3}
      />
      <div>{ssd(100)}</div>
    </div>
  );
};

HeaderDemo.defaultProps = {};

HeaderDemo.propTypes = {};

export default HeaderDemo;

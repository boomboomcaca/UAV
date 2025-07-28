import React, { useContext, useState, useEffect, useRef } from 'react';
import classnames from 'classnames';
import { Header } from 'searchlight-commons';
import { Input, message, Modal, Drawer } from 'dui';
import { restartEdge, getList } from '@/api/cloud';
import { deviceUrl } from '@/api/path';
import AppContext from '@/store/context';
import Preview from '@/views/Preview';
import Station from '@/views/Station';
import Edit from '@/views/Edit';
import StationMonitor from '@/views/StationMonitor';
import Template from '@/views/Template';
import { HEADER_RETURN } from '@/store/reducer';
import styles from './styles.module.less';

const { MenuType } = Header;

export default () => {
  const {
    state: {
      actions: { master, options },
    },
    dispatch,
  } = useContext(AppContext);

  window.console.log('master', master);

  // const [selectedID, setSelectedID] = useState('4444444444');
  // const [isOpenSelf, setIsOpenSelf] = useState(true);
  // const [showPage, setShowPage] = useState('detail');
  const [selectedID, setSelectedID] = useState(null);
  const [isOpenSelf, setIsOpenSelf] = useState(true);
  const fromSpecialRef = useRef(false);
  // const [showPage, setShowPage] = useState('preview');
  const [showPage, setShowPage] = useState('station');

  const [showEdit, setShowEdit] = useState(false);
  const [editType, setEditType] = useState(null);
  const [editParam, setEditParam] = useState(null);

  const [showTemplate, setShowTemplate] = useState(false);

  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    if (master && options) {
      const { edgeId, action } = options;
      if (edgeId) {
        // 从其它应用跳转过来
        window.console.log('-------------options-------------');
        window.console.log({ edgeId });
        setSelectedID(edgeId === undefined ? null : edgeId);
        setIsOpenSelf(false);
        fromSpecialRef.current = false;
        setShowPage('detail');
      }
      if (action) {
        // 从其它应用跳转过来：morscr
        setIsOpenSelf(false);
        fromSpecialRef.current = true;
        setShowPage('preview');
      }
    }
  }, []);

  const onMenuItemClick = (type) => {
    switch (type) {
      case MenuType.RETURN:
        if (!isOpenSelf) {
          if (showPage === 'detail' && fromSpecialRef.current) {
            setShowPage('preview');
            return;
          }
          master?.back();
          return;
        }
        if (showTemplate) {
          setShowTemplate(false);
          return;
        }
        if (showEdit) {
          dispatch({ type: HEADER_RETURN });
        } else {
          setShowPage('preview');
        }
        break;
      case MenuType.MORE:
        setShowTemplate(true);
        break;
      case MenuType.HOME:
        master?.backHome('runningOnce');
        break;
      default:
        break;
    }
  };

  const onPreviewClick = (e) => {
    if (e.tag === 'edit') {
      // setShowPassword(true);
      setShowPage('station');
    }
    if (e.tag === 'detail') {
      setSelectedID(e.id);
      setShowPage('detail');
    }
    if (e.tag === 'rest') {
      Modal.confirm({
        title: '提示',
        closable: false,
        content: '确定要重启环境监控系统？',
        onOk: async () => {
          // TODO 重启环境监控
          const p = {};
          const res1 = await getList(deviceUrl, {
            page: 1,
            rows: 1000,
            sort: 'desc',
            order: 'createTime',
            moduleType: 'device',
          });
          if (res1.result) {
            const find = res1.result.find((r) => {
              return r.moduleCategory[0] === 'control';
            });
            if (find) {
              p.deviceId = find.id;
              const res2 = await restartEdge(p);
              if (res2) {
                message.success({ key: 'tip', content: '发送重启指令成功' });
              } else {
                message.success({ key: 'tip', content: '发送重启指令失败' });
              }
            } else {
              message.success({ key: 'tip', content: '无相关环境监控设备' });
            }
          }
        },
      });
    }
  };

  const onStationClick = (e) => {
    setShowEdit(true);
    const { tag, args } = e;
    setEditType(tag);
    setEditParam(args);
  };

  const getTitle = () => {
    // if (showTemplate) {
    //   return '站点模块模板管理';
    // }
    // if (showPage === 'preview') {
    //   return '监测网管控';
    // }
    // if (showPage === 'station') {
    //   if (showEdit) {
    //     return editType === 'new' ? '站点配置/添加站点' : '站点配置/编辑站点';
    //   }
    //   return '站点配置/站点列表';
    // }
    // if (showPage === 'detail') {
    //   return '站点详细信息';
    // }
    return '站点配置';
  };

  return (
    <div className={styles.app}>
      {/* <Header
        title={getTitle()}
        // showIcon={
        //   isOpenSelf
        //     ? [
        //         showPage === 'preview' ? MenuType.HOME : null,
        //         showPage !== 'preview' || showTemplate ? MenuType.RETURN : null,
        //         showPage === 'station' ? MenuType.MORE : null,
        //       ]
        //     : [MenuType.RETURN]
        // }
        showIcon={
          isOpenSelf
            ? [
                showPage === 'station' && !showTemplate && !showEdit ? MenuType.HOME : null,
                showPage !== 'station' || showTemplate || showEdit ? MenuType.RETURN : null,
                showPage === 'station' ? MenuType.MORE : null,
              ]
            : [MenuType.RETURN]
        }
        onMenuItemClick={onMenuItemClick}
      /> */}
      <div className={classnames(styles.body, (showEdit || showTemplate) && styles.hide)}>
        {showPage === 'preview' ? <Preview hasRight={isOpenSelf} onPreviewClick={onPreviewClick} /> : null}
        {showPage === 'station' ? (
          <Station onStationClick={onStationClick} refreshKey={showEdit} onShowTemplate={() => setShowTemplate(true)} />
        ) : null}
        {showPage === 'detail' ? <StationMonitor edgeID={selectedID} disabled={!isOpenSelf} /> : null}
      </div>
      <div className={classnames(styles.popup, showEdit && !showTemplate ? null : styles.hide)}>
        <Edit
          editype={editType}
          editParam={editParam}
          visible={showEdit}
          onReturn={() => {
            setShowEdit(false);
            setEditType(null);
            setEditParam(null);
          }}
        />
      </div>
      {/* <div className={classnames(styles.popup, !showTemplate && styles.hide)}>
        <Template />
      </div> */}
      <Drawer title="模板管理" visible={showTemplate} onCancel={() => setShowTemplate(false)} width="1024px">
        <Template />
      </Drawer>

      <Modal
        visible={showPassword}
        title="输入密码"
        style={{ width: 420 }}
        onCancel={() => {
          setShowPassword(false);
        }}
        onOk={() => {
          if (password === '142857') {
            setShowPassword(false);
            setShowPage('station');
            setPassword('');
          } else {
            message.error({ key: 'tip', content: '没有权限！' });
          }
        }}
      >
        <Input
          type="password"
          placeholder="请输入配置密码"
          style={{ width: '100%' }}
          value={password}
          onChange={(val) => {
            setPassword(val);
          }}
        />
      </Modal>
    </div>
  );
};

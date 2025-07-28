import { useState, useEffect, useRef } from 'react';
import { Modal, message } from 'dui';
import dayjs from 'dayjs';
import langT from 'dc-intl';
import { gotoSync, gotoDownload } from '../utils/helper';

function useReplayList(type, wsTaskUrl, appid, request, setSyncData, onSelectChange) {
  const pageInfoRef = useRef({ page: 1, rows: 20, sort: 'desc', order: 'updateTime' });

  const timeRangeRef = useRef(null);

  const edgeNameRef = useRef('');

  const [replayList, setReplayList] = useState({ result: [], total: 0 });

  const [replayRefresh, setReplayRefresh] = useState(0);

  const [standardSegments, setStandardSegments] = useState([]);

  const getReplayList = (pinfo) => {
    const param = {
      frontFuncId: appid,
      ...pinfo,
      ...timeRangeRef.current,
    };
    if (edgeNameRef.current !== '') {
      param['edgeName.lk'] = edgeNameRef.current;
    }
    let paramStr = '';
    Object.keys(param).forEach((prop) => {
      paramStr += `${prop}=${param[prop]}&`;
    });
    paramStr = paramStr.substring(0, paramStr.length - 1);
    request({
      url: `log/logFileInfo/getList${paramStr !== '' ? '?' : ''}${paramStr}`,
      method: 'get',
    }).then((res) => {
      setReplayList(res);
    });
  };

  const delReplayList = (items) => {
    request({
      url: `log/logFileInfo/delList`,
      method: 'post',
      data: {
        id: items.map((i) => {
          return i.id;
        }),
      },
    }).then(() => {
      setReplayRefresh((p) => {
        return p + 1;
      });
      getReplayList(pageInfoRef.current);
    });
  };

  const onDeleteReplayItem = (items) => {
    Modal.confirm({
      title: langT('commons', 'tip'),
      content: langT('commons', 'sureToDeleteReplayData'),
      onOk: () => {
        delReplayList(items);
      },
    });
  };

  const onPageChange = (page /* , pagesize */) => {
    getReplayList({ ...pageInfoRef.current, page });
  };

  const onPlayback = (item) => {
    // playback
    onSelectChange(item);
  };

  const onPlaysync = (item /* , setSync */) => {
    gotoSync(item, wsTaskUrl, (bo) => {
      if (bo) {
        message.loading({ key: 'commons-main', content: langT('commons', 'dataSyncing') });
        setSyncData({ sourceFile: item.sourceFile, rate: '0%' /* , syncCode: 200 */ });
      } else {
        message.error({ key: 'commons-main', content: langT('commons', 'dataSyncFailure') });
        setSyncData({ sourceFile: item.sourceFile, rate: '-1%' /* , syncCode: 200 */ });
      }
    });
  };

  const onSearchChanged = (str) => {
    window.console.log(str);
    edgeNameRef.current = str;
    setReplayRefresh((p) => {
      return p + 1;
    });
    const param = pageInfoRef.current;
    getReplayList({ ...param });
  };

  const onTimeChange = (d) => {
    if (d.length === 2) {
      timeRangeRef.current = {
        'updateTime.gt': dayjs(d[0]).format('YYYY-MM-DD'),
        'updateTime.lt': dayjs(d[1]).format('YYYY-MM-DD'),
      };
    }
    setReplayRefresh((p) => {
      return p + 1;
    });
    const param = pageInfoRef.current;
    getReplayList({ ...param });
  };

  const updateRemark = (e) => {
    request({
      url: `log/logFileInfo/update`,
      method: 'post',
      data: { sourceFile: e.sourceFile, remark: e.val },
    }).then(() => {
      setReplayRefresh((p) => {
        return p + 1;
      });
    });
  };

  const onDownload = (tag, dat) => {
    gotoDownload(dat.id, tag, wsTaskUrl, (res) => {
      window.console.log(res);
      if (res.error) {
        const { message: msg } = res.error;
        message.error({ key: 'commons-main', content: msg });
      }
      if (res.result) {
        const name = res.result.replace('/public/replay/', '');
        window.console.log(name);
        // download(`/download${res.result}`).then((ret) => {
        //   convertRes2Blob(ret, name);
        // });
      }
    });
  };

  useEffect(() => {
    if (type === 'segment') {
      request({
        url: `segment/scanSegment/getList`,
        method: 'get',
      }).then((res) => {
        setStandardSegments(res.result);
      });
    }

    const param = pageInfoRef.current;
    getReplayList({ ...param });
  }, []);

  return {
    replayRefresh,
    standardSegments,
    replayList,
    onDeleteReplayItem,
    onPageChange,
    onPlayback,
    onPlaysync,
    onSearchChanged,
    onTimeChange,
    updateRemark,
    onDownload,
  };
}

export default useReplayList;

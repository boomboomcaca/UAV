import { useState, useEffect, useRef } from 'react';
import { useRequest } from 'ahooks';
import { message, Modal } from 'dui';
import langT from 'dc-intl';

const defaultParam = { page: 1, rows: 20, sort: 'desc', order: 'createTime', functionName: 'ffm' };

function useCaptureList(request) {
  const hasRef = useRef(0);
  const totalRef = useRef(0);
  const paramRef = useRef(defaultParam);

  const [captures, setCaptures] = useState([]);

  const { run } = useRequest(
    () => {
      let paramStr = '';
      Object.keys(paramRef.current).forEach((prop) => {
        paramStr += `${prop}=${paramRef.current[prop]}&`;
      });
      paramStr = paramStr.substring(0, paramStr.length - 1);
      return request({
        url: `sys/screenshot/getList${paramStr !== '' ? '?' : ''}${paramStr}`,
        method: 'get',
      });
    },
    {
      manual: true,
      onSuccess: (ret) => {
        window.console.log(ret.result);
        const all = [...captures, ...ret.result];
        setCaptures(all);
        hasRef.current = all.length;
        totalRef.current = ret.total;
      },
    },
  );

  const { run: delCaptures } = useRequest(
    (id) =>
      request({
        url: `sys/screenshot/delList`,
        method: 'post',
        data: {
          id,
        },
      }),
    {
      manual: true,
      onSuccess: (res) => {
        window.console.log(res);
        message.success(langT('commons', 'screenShootFinish'));
        ReloadCaptures();
      },
      onError: (res) => {
        window.console.log(res);
        message.error(langT('commons', 'screenShootFailure'));
      },
    },
  );

  // const { run: editCaptures } = useRequest((param) => update('sys/screenshot', param), {
  //   manual: true,
  //   onSuccess: (res) => {
  //     window.console.log(res);
  //     ReloadCaptures();
  //   },
  // });

  useEffect(() => {
    run();
  }, []);

  const ReloadCaptures = (param) => {
    hasRef.current = 0;
    totalRef.current = 0;
    paramRef.current = { ...paramRef.current, ...param, page: 1 };
    setCaptures([]);
    run();
  };

  const onLoadMore = (times, callback) => {
    const pros = [];
    for (let i = 0; i < times; i += 1) {
      if (hasRef.current < totalRef.current && totalRef.current > 20) {
        callback?.(true);
        paramRef.current.page += 1;

        let paramStr = '';
        Object.keys(paramRef.current).forEach((prop) => {
          paramStr += `${prop}=${paramRef.current[prop]}&`;
        });
        paramStr = paramStr.substring(0, paramStr.length - 1);
        pros.push(
          request({
            url: `sys/screenshot/getList${paramStr !== '' ? '?' : ''}${paramStr}`,
            method: 'get',
          }),
        );
      } else {
        callback?.(false);
      }
    }
    if (pros.length > 0) {
      Promise.all(pros).then((r) => {
        r.forEach((ret) => {
          const all = [...captures, ...ret.result];
          setCaptures(all);
          hasRef.current = all.length;
          totalRef.current = ret.total;
        });
      });
    }
  };

  const onDelete = (items) => {
    window.console.log(items);
    Modal.confirm({
      title: langT('commons', 'tip'),
      closable: false,
      content: langT('commons', 'sureToDeletScreenShoot'),
      onOk: () => {
        delCaptures(
          items.map((i) => {
            return i.id;
          }),
        );
      },
    });
  };

  const onDownload = (items) => {
    items.forEach((itm) => {
      window.console.log(itm);
      if (window.App && itm.path2) {
        window.App.downloadAndSaveFile(`${itm.path2}`, 'screenshot').catch((re) => window.console.log(re));
      }
    });
  };

  return { captures, onLoadMore, onDelete, onDownload };
}

export default useCaptureList;

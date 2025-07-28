import { useState, useEffect, useRef } from 'react';
import { useRequest } from 'ahooks';
import { message, Modal } from 'dui';
import langT from 'dc-intl';

function useCaptureList(request, functionName) {
  const hasRef = useRef(0);
  const totalRef = useRef(0);
  const paramRef = useRef({ page: 1, rows: 20, sort: 'desc', order: 'createTime', functionName });

  const [captures, setCaptures] = useState([]);

  const [downloading, setDownloading] = useState(false);
  const [deleting, setDeleting] = useState(false);

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
        setDeleting(false);
      },
      onError: (res) => {
        window.console.log(res);
        message.error(langT('commons', 'screenShootFailure'));
        setDeleting(false);
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
        setDeleting(true);
        delCaptures(
          items.map((i) => {
            return i.id;
          }),
        );
      },
    });
  };

  const onDownload = (items) => {
    window.console.log('downloading');
    const pros = [];
    const names = [];
    items.forEach((itm) => {
      window.console.log(itm);
      if (window.App && itm.path2) {
        let n = itm.name;
        const idx = itm.name.lastIndexOf('.');
        if (idx > 0) {
          n = itm.name.substring(0, idx);
        }
        pros.push(window.App.downloadAndSaveFile(`${itm.path2}`, 'screenshot', n));
        names.push(itm.name);
      }
    });
    if (pros.length > 0) {
      setDownloading(true);
      Promise.allSettled(pros)
        .then((res) => {
          window.console.log('downloaded');
          res.forEach((r, i) => {
            const { status, value, reason } = r;
            if (status === 'rejected') {
              message.error({ key: 'faild', content: `下载截图失败！(${names[i]})` });
            }
            if (status === 'fulfilled') {
              // TODO 暂不处理
            }
          });
          setTimeout(() => {
            setDownloading(false);
          }, 1000);
        })
        .catch((rej) => {
          window.console.log('failed');
          message.error('下载截图失败！');
          setTimeout(() => {
            setDownloading(false);
          }, 1000);
        });
    }
  };

  return { captures, onLoadMore, onDelete, onDownload, deleting, downloading };
}

export default useCaptureList;

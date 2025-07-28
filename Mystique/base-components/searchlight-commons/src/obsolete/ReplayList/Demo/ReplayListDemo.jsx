import React, { useState, useRef, useEffect } from 'react';
import { message } from 'dui';
import ReplayList from '..';
import dataTemp from './data';

let ids = [];
const data = { result: [], total: 0 };

const ReplayListDemo = () => {
  useEffect(() => {
    for (let i = 0; i < 20; i += 1) {
      const dt = dataTemp.result.map((x) => {
        return { ...x, id: `${x.id}-${i}` };
      });
      data.result = [...data.result, ...dt];
    }
    data.total = data.result.length;

    setListData({
      result: data.result.slice(0, 20).map((x, idx) => {
        return { ...x, test: `xyz-${idx}` };
      }),
      total: data.total,
    });
  }, []);

  const pageRef = useRef(1);
  const pageSizeRef = useRef(10);
  const [listData, setListData] = useState({ result: [] });
  const [syncData, setSyncData] = useState(null);
  return (
    <>
      <div
        style={{
          width: 64,
          height: 4,
          borderRadius: 2,
          border: '1px solid rgba(60, 229, 211, 0.2)',
          position: 'relative',
          boxSizing: 'border-box',
        }}
      >
        <div
          style={{
            width: (64 * 100) / 100,
            height: 4,
            borderRadius: 2,
            background: 'linear-gradient(180deg, #ADFFF6 0%, #3DE7D5 100%)',
            position: 'absolute',
            top: '-1px',
            left: '-1px',
            transition: 'width ease 0.5s',
          }}
        />
      </div>
      <ReplayList
        pageSize={10}
        data={listData /* { result: [] } */}
        syncData={syncData}
        onDeleteItems={(items) => {
          message.info({
            content: `删除 ${items.map((i) => {
              return `${i.id},`;
            })}`,
            key: 'info',
            duration: 1,
          });
          ids = [
            ...ids,
            ...items.map((i) => {
              return i.id;
            }),
          ];
          setListData({
            result: data.result
              .filter((dr) => {
                return !ids.includes(dr.id);
              })
              .slice((pageRef.current - 1) * pageSizeRef.current, pageRef.current * pageSizeRef.current),
            total: data.total,
          });
        }}
        onPageChange={(page, pagesize) => {
          message.info({
            content: `翻页 ${page} ${pagesize}`,
            key: 'info',
            duration: 1,
          });
          pageRef.current = page;
          pageSizeRef.current = pagesize;
          setListData({
            result: data.result.slice((page - 1) * pagesize, page * pagesize),
            total: data.total,
          });
        }}
        onPlayback={(item) => {
          if (item.percentage)
            message.info({
              content: item.id,
              key: 'info',
              duration: 1,
            });
        }}
        onPlaysync={(item, setSync) => {
          if (item.percentage)
            message.info({
              content: item.id,
              key: 'info',
              duration: 1,
            });
          if (setSync) {
            // setSync(true);
            // setTimeout(() => {
            //   setSync(false);
            // }, 2000);
            setTimeout(() => {
              setSyncData({ rate: '20%', sourceFile: item.sourceFile });
            }, 200);
            setTimeout(() => {
              setSyncData({ rate: '30%', sourceFile: item.sourceFile });
            }, 500);
            setTimeout(() => {
              setSyncData({ rate: '50%', sourceFile: item.sourceFile });
            }, 800);
            setTimeout(() => {
              setSyncData({ rate: '75%', sourceFile: item.sourceFile });
            }, 800);
            // setTimeout(() => {
            //   setSyncData({ rate: '0%', sourceFile: item.sourceFile });
            // }, 0);
            setTimeout(() => {
              setSyncData({ rate: '100%', sourceFile: item.sourceFile });
            }, 2000);
          }
        }}
        onSearchChanged={(str) => {
          window.console.log(str);
        }}
      />
    </>
  );
};

ReplayListDemo.defaultProps = {};

ReplayListDemo.propTypes = {};

export default ReplayListDemo;

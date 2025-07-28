import React from 'react';
import icons from './icons';

const getColumns = (type, canOperate, onOperate) => {
  const cols = [];
  if (type === 'single') {
    cols.push({
      key: 'frequency',
      name: (
        <div>
          <span style={{ fontSize: 14, fontWeight: 'bold' }}>频率</span>
          <span style={{ fontSize: 12, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>（MHz）</span>
        </div>
      ),
      sort: true,
    });
    cols.push({
      key: 'bandwidth',
      name: (
        <div>
          <span style={{ fontSize: 14, fontWeight: 'bold' }}>中频带宽</span>
          <span style={{ fontSize: 10, fontWeight: 'normal', color: 'rgba(255, 255, 255, 0.8)' }}>（kHz）</span>
        </div>
      ),
      sort: true,
    });
  } else if (type === 'segment') {
    cols.push({
      key: 'segmentInfo',
      name: <div style={{ fontSize: 14, fontWeight: 'bold' }}>频段信息</div>,
      sort: true,
    });
  }
  return [
    ...cols,
    {
      key: 'updateTime',
      name: <div style={{ fontSize: 14, fontWeight: 'bold' }}>录制时间</div>,
      sort: true,
    },
    // {
    //   key: 'test',
    //   name: '测试',
    //   sort: true,
    // },
    {
      key: 'timeSpan',
      name: <div style={{ fontSize: 14, fontWeight: 'bold' }}>持续时长</div>,
      sort: true,
    },
    {
      key: 'percentage',
      name: <div style={{ fontSize: 14, fontWeight: 'bold' }}>数据同步</div>,
      render: (dat) => {
        const { percentage } = dat;
        return (
          <div style={{ fontSize: 14 }}>
            {percentage === 101 ? (
              icons.compelete
            ) : percentage < 0 ? (
              '未同步'
            ) : percentage !== 200 ? (
              <div>
                <div style={{ color: 'rgba(255, 255, 255, 0.8)', fontSize: 12, marginBottom: 2 }}>
                  {`${percentage}%`}
                </div>
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
                      width: (64 * percentage) / 100,
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
              </div>
            ) : (
              '同步中...'
            )}
          </div>
        );
      },
    },
    {
      key: 'operate',
      name: <div style={{ fontSize: 14, fontWeight: 'bold' }}>操作</div>,
      render: (dat) => {
        const { percentage } = dat;
        return (
          <div
            onClick={() => {
              if (canOperate) onOperate(dat);
            }}
            style={{ fontSize: 24, cursor: canOperate ? 'pointer' : 'not-allowed', opacity: canOperate ? 1 : 0.5 }}
          >
            {percentage === 101 ? icons.play : percentage < 0 ? icons.upload : icons.waiting}
          </div>
        );
      },
    },
  ];
};

export default getColumns;

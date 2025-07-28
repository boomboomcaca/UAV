import React, { useState } from 'react';
import DfChart from '../DfChart.jsx';
import { Button } from 'dui';

const DfChartDemo = () => {
  const [demoBearings, setDemoBearings] = useState(() => [
    {
      bearing: 40,
      id: 1,
    },
    {
      bearing: 60,
      id: 2,
    },
    {
      bearing: 90,
      id: 3,
    },
    {
      bearing: 170,
      id: 4,
    },
    {
      bearing: 260,
      id: 5,
    },
  ]);

  const [chartType, setChartType] = useState(['chart', 'table']);

  return (
    <div style={{ width: '100%', height: '100%', display: 'flex', justifyContent: 'center', alignContent: 'center' }}>
      <div style={{ width: '25%' }}>
        <code>
          props.pointerColors : '传入自定义颜色' ['white', 'black', '#00a1d6']
          <br />
          props.data : '数据'. <br />[ &#123; id: 1, frequency: 101.7, bearing: 90 &#125;, &#123; id: 2, frequency: 88,
          bearing: 180 &#125;]
          <br />
          props.displayType : '显示表盘或者表格' ['chart', 'table'] .<br />
          props.onDelete : '删除数据事件' <br />
          props.defaultSelectedId: 初始化时选中的频率id
          <br />
          props.tableHeight (string) '50%', '100px', 自由组合高度
          <br />
          props.chartHeight (string) <br />
        </code>
        <Button
          onClick={() => {
            setChartType(['chart']);
          }}
        >
          只看表盘
        </Button>
        <Button
          onClick={() => {
            setChartType(['table']);
          }}
        >
          只看表格
        </Button>
        <Button
          onClick={() => {
            setChartType(['chart', 'table']);
          }}
        >
          都看
        </Button>
        <Button
          onClick={() => {
            setDemoBearings([
              {
                bearing: 40,
                id: 1,
              },
              {
                bearing: 0,
                id: 2,
              },
              {
                bearing: 90,
                id: 3,
              },
              {
                bearing: 170,
                id: 4,
              },
              {
                bearing: 260,
                id: 5,
              },
            ]);
          }}
        >
          数据变化
        </Button>
      </div>
      <div style={{ width: '74%' }}>
        <DfChart
          data={demoBearings}
          displayType={chartType}
          defaultSelectedId={1}
          onDelete={(data) => {
            const temp = [...demoBearings];
            const idx = temp.findIndex((t) => t.id === data.id);
            if (idx >= 0) {
              temp.splice(idx, 1);
              setDemoBearings(temp);
            }
          }}
        />
      </div>
    </div>
  );
};

export default DfChartDemo;

import React, { useState, useEffect } from 'react';
import { Button } from 'dui';
import businessSegmentPng from '../assets/businessSegment-icon.png';
import freSettingPng from '../assets/freSetting-icon.png';
import StepTabs from '../index';
const { StepTabPane } = StepTabs;
export default () => {
  const tabOption = [
    {
      title: '新建采集任务',
      describe: '标准任务创建',
      imgBox: <img alt="" src={businessSegmentPng} />,
    },
    {
      title: '已有任务',
      imgBox: <img alt="" src={freSettingPng} />,
    },
    {
      title: '数据分析',
    },
    {
      title: '报告查看',
    },
  ];
  const [hideMenu, setHideMenu] = useState(false);
  useEffect(() => {
    console.log('demo--->StepTabPane', StepTabPane);
  }, []);
  const onTabChange = (currentTap) => {
    console.log('currentTap--->', currentTap);
  };
  return (
    <div style={{ width: '100%', height: '100%' }}>
      <Button onClick={() => setHideMenu(!hideMenu)}>显示/隐藏</Button>
      <StepTabs currentTabNum={-1} hideMenu={hideMenu} onTabChange={onTabChange}>
        {/* 循环生成的tab项 */}

        {tabOption.map((item, index) => (
          <StepTabPane
            key={`StepTabPane-${index + 1}`}
            tab={index}
            title={item.title || null}
            describe={item.describe || null}
            imgBox={item.imgBox || null}
          >
            <span>{`StepTabPane-${index + 1}`}</span>
          </StepTabPane>
        ))}

        {/* 非循环生成的tab项 */}

        {/* <StepTabPane key={'StepTabPane-0'} tab="1" title="test" imgBox={null}>
          <span>StepTabPane0</span>
        </StepTabPane>
        <StepTabPane key={'StepTabPane-1'} tab="2" title="test1" imgBox={null}>
          <span>StepTabPane1</span>
        </StepTabPane> */}
      </StepTabs>
    </div>
  );
};

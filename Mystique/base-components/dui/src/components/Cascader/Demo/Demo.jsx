import React, { useState } from 'react';
import Cascader from '../index';
import styles from './index.module.less';

export default function Demo() {
  const data = [
    {
      id: 1,
      name: '四川',
      children: [
        {
          id: 12,
          name: '成都',
          children: [
            { id: 122, name: '高新区' },
            { id: 123, name: '天府新区' },
            { id: 124, name: '金牛区' },
          ],
        },
        {
          id: 13,
          name: '乐山',
          children: [
            { id: 132, name: '市中区' },
            { id: 133, name: '五通桥区' },
            { id: 134, name: '沙湾区' },
          ],
        },
        { id: 14, name: '绵阳' },
      ],
    },
    {
      id: 2,
      name: '未知区域1',
      children: [
        {
          id: 22,
          name: '地月系',
          children: [
            { id: 222, name: '百慕大' },
            { id: 223, name: '南极' },
            { id: 224, name: '马里亚纳海沟' },
            { id: 225, name: '月球背面' },
            { id: 226, name: '姆大陆' },
            { id: 227, name: '亚特兰蒂斯' },
            { id: 228, name: '苏美尔' },
            { id: 229, name: '山海经' },
          ],
        },
        {
          id: 23,
          name: '上古文明',
          children: [
            { id: 232, name: '亿年前' },
            { id: 233, name: '玛雅星' },
          ],
        },
      ],
    },
    { id: 3, name: '未知区域2' },
    { id: 4, name: '未知区域3' },
  ];

  const [values, setValues] = useState([{ name: '四川省' }, { name: '成都市' }, { name: '天府新区' }]);

  const onSelectValue = (val) => {
    setValues(val);
  };
  return (
    <div className={styles.root}>
      <Cascader
        options={data}
        values={values}
        keyMap={{ LABEL: 'name' }}
        splitter=" # "
        onSelectValue={onSelectValue}
      />
      {/* <div
        className={styles.test}
        onWheel={(e) => {
          const dom = e.currentTarget;
          const scrollWidth = 50;
          e.deltaY > 0 ? (dom.scrollLeft += scrollWidth) : (dom.scrollLeft -= scrollWidth);
        }}
      >
        <div>a</div>
        <div>b</div>
        <div>c</div>
        <div>d</div>
        <div>e</div>
        <div>f</div>
        <div>g</div>
        <div>h</div>
        <div>i</div>
        <div>j</div>
        <div>k</div>
      </div> */}
    </div>
  );
}

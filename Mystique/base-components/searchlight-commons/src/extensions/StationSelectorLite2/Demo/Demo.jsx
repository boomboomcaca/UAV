/*
 * @Author: XYQ
 * @Date: 2022-01-24 18:09:27
 * @LastEditors: wangXueDong
 * @LastEditTime: 2022-12-09 16:35:36
 * @Description: file content
 */
import React, { useEffect, useState } from 'react';
import { Button } from 'dui';

import res from './res';
import StationSelectorLite from '../index';

export default function StationViewDemo() {
  const [showSelector, setShowSelector] = useState(false);

  const [modules, setModules] = useState([
    {
      id: '40003',
      moduleIds: [
        {
          ffm: 'd339bd20-024c-11ed-b977-658efbacac67',
        },
        {
          fdf: 'd3396f00-024c-11ed-b977-658efbacac67',
        },
        {
          scan: 'd33920e0-024c-11ed-b977-658efbacac67',
        },
      ],
    },
    {
      id: '40004',
      moduleIds: [
        {
          ffm: 'ee96e570-024c-11ed-b977-658efbacac67',
        },
        {
          fdf: 'ee973390-024c-11ed-b977-658efbacac67',
        },
        {
          scan: 'ee964930-024c-11ed-b977-658efbacac67',
        },
      ],
    },
    {
      id: '40002',
      moduleIds: [
        {
          ffm: 'bae93890-024c-11ed-b977-658efbacac67',
        },
        {
          fdf: 'bae986b0-024c-11ed-b977-658efbacac67',
        },
        {
          scan: 'bae8ea70-024c-11ed-b977-658efbacac67',
        },
      ],
    },
    {
      id: '40001',
      moduleIds: [
        {
          ffm: '93292270-024c-11ed-b977-658efbacac67',
        },
        {
          fdf: '93297090-024c-11ed-b977-658efbacac67',
        },
        {
          scan: '98ad07c0-024c-11ed-b977-658efbacac67',
        },
      ],
    },
  ]); //

  return (
    <div>
      <Button onClick={() => setShowSelector(true)}>选择功能</Button>
      <StationSelectorLite
        visible={showSelector}
        stations={res.result}
        onClose={() => {
          setShowSelector(false);
        }}
        mapOptions={{
          mapType: 'amap',
          customUrl: undefined,
          fontUrl: undefined,
        }}
        selectType={['ffm', 'fdf', 'scan']}
        selectEdgeId={modules}
        onSelect={(x) => {
          console.log(x);
          setModules(x);
          setShowSelector(false);
        }}
      />
    </div>
  );
}

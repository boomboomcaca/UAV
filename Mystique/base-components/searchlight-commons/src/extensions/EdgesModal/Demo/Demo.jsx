/*
 * @Author: XYQ
 * @Date: 2022-01-24 18:09:27
 * @LastEditors: XYQ
 * @LastEditTime: 2022-10-18 09:42:08
 * @Description: file content
 */
import React, { useEffect, useState } from 'react';
import { Button } from 'dui';
import Axios from 'axios';
import testData from './testData.json';
import EdgesModal from '../EdgesModal';

export default function StationViewDemo() {
  const [showSelector, setShowSelector] = useState(false);

  const [modules, setModules] = useState(null); //
  const [listinfo, setListInfo] = useState([]);

  const onEdgesChange = (e) => {
    console.log(e);
    setModules(e);
  };
  return (
    <div>
      <Button onClick={() => setShowSelector(true)}>选择功能</Button>
      <EdgesModal
        mapOptions={{
          apiBaseUrl: '',
          mapType: 'amap',
          customUrl: undefined,
          fontUrl: undefined,
        }}
        stations={testData}
        selectEdges={modules}
        number={2}
        onEdgesChange={onEdgesChange}
        onCancel={() => {
          setShowSelector(false);
        }}
        visible={showSelector}
      />
    </div>
  );
}

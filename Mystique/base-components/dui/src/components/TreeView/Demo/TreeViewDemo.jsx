import React, { useState } from 'react';
import TreeView from '../index.jsx';

export default function TreeViewDemo() {
  const [state, setstate] = useState(false);
  const onChange = (val) => {
    setstate(val);
  };

  return (
    <>
      <TreeView
        rootName="test"
        nodes={[
          {
            name: 'level1-1',
            code: 'level1-1',
            cities: [
              {
                name: 'level2-1',
                code: 'level2-1',
                status: 1,
              },
              {
                name: 'level2-1',
                code: 'level2-1',
                status: 1,
              },
              {
                name: 'level2-1',
                code: 'level2-1',
              },
            ],
          },
          {
            name: 'level1-1',
            code: 'level1-1',
            cities: [
              {
                name: 'level2-1',
                code: 'level2-1',
              },
              {
                name: 'level2-1',
                code: 'level2-1',
                status: 1,
              },
              {
                name: 'level2-1',
                code: 'level2-1',
              },
            ],
          },
          {
            name: 'level1-1',
            code: 'level1-1',
            cities: [
              {
                name: 'level2-1',
                code: 'level2-1',
              },
              {
                name: 'level2-1',
                code: 'level2-1',
              },
              {
                name: 'level2-1',
                code: 'level2-1',
              },
            ],
          },
        ]}
      />
    </>
  );
}

import React, { useState } from 'react';
import Table from '../index';
import Button from '../../Button';
import get from './columns.jsx';
import styles from './index.module.less';

const { getColumns, getData } = get;

export default function Demo() {
  const onColumnSort = (sort) => {
    window.console.log(sort);
  };

  const [selectRow, setSelectRow] = useState(-1);
  const onRowSelected = (item, index, toSelect) => {
    window.console.log({ item, index, toSelect });

    // setSelectRow(toSelect ? item.kid : null);
    setSelectRow(toSelect ? index : null);
  };

  const [selections, setSelections] = useState(() => {
    const rows = getData();
    return [rows[2]];
  });

  const onSelectionChanged = (items) => {
    window.console.log(items);
    setSelections(items);
  };

  const [data2, setData2] = useState([{ kid: 1, col1: '????????', test: '!!!!!!!', col2: '########' }]);

  return (
    <div className={styles.root}>
      <Table
        rowKey="kid"
        columns={getColumns()}
        options={{ canRowSelect: true, rowCheckColor: '#3CE5D3' }}
        data={getData()}
        currentSelections={selections}
        onColumnSort={onColumnSort}
        // selectRowKey={selectRow}
        // selectRowIndex={selectRow}
        onRowSelected={onRowSelected}
        onSelectionChanged={onSelectionChanged}
      />
      <Table
        rowKey="kid"
        columns={getColumns()}
        options={{ bordered: { inline: true, outline: true }, rowHeight: 100 }}
        data={getData()}
      />
      <Table
        rowKey="kid"
        columns={getColumns()}
        options={{ bordered: { inline: false, outline: true } }}
        data={getData()}
      />
      <Table
        // rowKey="kid"
        columns={getColumns()}
        showSelection={false}
        // selectRowIndex={1}
        options={{ canRowSelect: true, bordered: false }}
        data={data2}
      />
      <Button
        style={{ position: 'absolute', left: '50%', top: '50%', transform: 'translate(-50%,-50%)' }}
        disabled={!selections || selections.length === 0}
        onClick={() => {
          setSelections(null);
        }}
      >
        取消
      </Button>
      <Button
        style={{ position: 'absolute', left: '60%', top: '60%', transform: 'translate(-50%,-50%)' }}
        onClick={() => {
          setData2([
            { kid: '1', col1: '????????', test: '!!!!!!!', col2: '!!!!!!!' },
            { kid: '2', col1: '????????', test: '########', col2: '########' },
            { kid: '3', col1: '????????', test: '########', col2: '########' },
          ]);
        }}
      >
        ??????????????????12
      </Button>
      <Button
        style={{ position: 'absolute', left: '70%', top: '70%', transform: 'translate(-50%,-50%)' }}
        onClick={() => {
          setData2([
            { kid: '1', col1: '???', test: '!!!!!!!', col2: '!!!!!!!' },
            { kid: '2', col1: '??', test: '########', col2: '########' },
          ]);
        }}
      >
        ??????????????????1
      </Button>
      <Button
        style={{ position: 'absolute', left: '80%', top: '80%', transform: 'translate(-50%,-50%)' }}
        onClick={() => {
          setData2([]);
        }}
      >
        ??????????????????
      </Button>
      <div className={styles.xx} />
    </div>
  );
}

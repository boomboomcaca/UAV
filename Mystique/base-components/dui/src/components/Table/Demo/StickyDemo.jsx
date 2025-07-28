import React from 'react';
import StickyTable from '../StickyTable.jsx';
import { getColumns, getData } from './stickyColumns.jsx';
import styles from './sticky.module.less';

export default function Demo() {
  return (
    <div className={styles.root}>
      <StickyTable rowKey="id" columns={getColumns(false)} border={{ type: 'outer' }} />
      <StickyTable
        rowKey="id"
        columns={getColumns(true)}
        data={getData()}
        border={{ type: 'all', style: { border: '1px solid var(--theme-font-50)' } }}
      />
      <StickyTable
        rowKey="id"
        columns={getColumns(false)}
        data={getData()}
        border={{ type: 'inner', style: { border: '1px solid var(--theme-font-50)' } }}
      />
    </div>
  );
}

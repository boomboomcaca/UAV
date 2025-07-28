import React, { memo } from 'react';
import langT from 'dc-intl';
import styles from './styles.module.less';

const Calendar = () => {
  return (
    <div className={styles.week}>
      <div className={styles.weekitem}>{langT('dui', 'Mon')}</div>
      <div className={styles.weekitem}>{langT('dui', 'Tues')}</div>
      <div className={styles.weekitem}>{langT('dui', 'Wed')}</div>
      <div className={styles.weekitem}>{langT('dui', 'Thur')}</div>
      <div className={styles.weekitem}>{langT('dui', 'Fri')}</div>
      <div className={styles.weekitem}>{langT('dui', 'Sat')}</div>
      <div className={styles.weekitem}>{langT('dui', 'Sun')}</div>
    </div>
  );
};
export default memo(Calendar);

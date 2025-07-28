import React from 'react';
import PropTypes from 'prop-types';
import { Checkbox } from 'dui';
import styles from './index.module.less';

const Filter = (props) => {
  const { title, filters, values, onChanged } = props;

  return (
    <div className={styles.root}>
      <div className={styles.title}>{title}</div>
      <div className={styles.group}>
        {filters?.map((f) => {
          return (
            <Checkbox
              key={f.id}
              className={styles.checkbox}
              checked={values?.find((v) => v.id === f.id)}
              onChange={(chk) => onChanged(f, chk, title)}
            >
              {f.value === '' || f.value === undefined ? `${title}-无` : f.value}
            </Checkbox>
          );
        })}
      </div>
    </div>
  );
};

Filter.defaultProps = {
  title: '',
  filters: null,
  values: null,
  onChanged: () => {},
};

Filter.propTypes = {
  title: PropTypes.string,
  filters: PropTypes.any,
  values: PropTypes.any,
  onChanged: PropTypes.func,
};

export default Filter;

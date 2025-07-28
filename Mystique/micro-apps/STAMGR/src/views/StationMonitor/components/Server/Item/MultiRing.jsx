import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import Ring from '@/components/Ring';
import styles from './multiRing.module.less';

const MultiRing = (props) => {
  const { className, title, value, unit, color } = props;

  return (
    <div className={classnames(styles.root, className)}>
      <div className={styles.title}>{title}</div>
      <Ring borderColor={color}>
        <span className={styles.value}>{value}</span>
        <span className={styles.unit}>{unit}</span>
      </Ring>
    </div>
  );
};

MultiRing.defaultProps = {
  className: null,
  title: '占用率',
  value: null,
  unit: '',
  color: 'rgba(60, 229, 211, 0.2)',
};

MultiRing.propTypes = {
  className: PropTypes.any,
  title: PropTypes.string,
  value: PropTypes.any,
  unit: PropTypes.string,
  color: PropTypes.string,
};

export default MultiRing;

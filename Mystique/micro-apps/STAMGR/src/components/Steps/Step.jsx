import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './step.module.less';

const Step = (props) => {
  const { tag, title, checked, onClick } = props;
  return (
    <div key={tag} onClick={onClick} className={styles.step}>
      <div className={classnames(styles.circle1, checked && styles.circle1checked)} />
      <div className={classnames(styles.circle2, checked && styles.circle2checked)} />
      <div className={classnames(styles.title, checked && styles.titlechecked)}>{title}</div>
    </div>
  );
};

Step.defaultProps = {
  tag: null,
  title: '',
  checked: false,
  onClick: () => {},
};

Step.propTypes = {
  tag: PropTypes.any,
  title: PropTypes.string,
  checked: PropTypes.bool,
  onClick: PropTypes.func,
};

export default Step;

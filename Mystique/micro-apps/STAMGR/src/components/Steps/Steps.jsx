import React from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';

const Steps = (props) => {
  const { step, children, onClick } = props;

  const element = (toString.call(children) === '[object Array]' ? children : [children]).map((child) => {
    const { tag } = child.props;
    return React.cloneElement(child, {
      ...child.props,
      onClick: () => {
        onClick(tag);
      },
      checked: JSON.stringify(step) === JSON.stringify(tag),
    });
  });

  return <div className={styles.steps}>{element}</div>;
};

Steps.defaultProps = {
  step: null,
  children: null,
  onClick: null,
};

Steps.propTypes = {
  step: PropTypes.any,
  children: PropTypes.any,
  onClick: PropTypes.func,
};

export default Steps;

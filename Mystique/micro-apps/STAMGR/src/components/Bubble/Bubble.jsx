import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import shadows from './icons/shadows.jsx';
import styles from './index.module.less';

const Bubble = (props) => {
  const { className, bubble, baseColor, children } = props;

  return (
    <div
      className={classnames(styles.root, className)}
      style={bubble ? null : { boxShadow: `inset -4px -4px 4px ${baseColor}1E, inset 0px 0px 20px ${baseColor}7F` }}
    >
      {bubble || shadows(baseColor)}
      <div className={styles.children}>{children}</div>
    </div>
  );
};

Bubble.defaultProps = {
  className: null,
  bubble: null,
  baseColor: '#2DB3FF',
  children: null,
};

Bubble.propTypes = {
  className: PropTypes.any,
  bubble: PropTypes.any,
  baseColor: PropTypes.string,
  children: PropTypes.any,
};

export default Bubble;

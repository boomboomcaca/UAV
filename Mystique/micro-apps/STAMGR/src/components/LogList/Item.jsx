import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Item = (props) => {
  const { className, children } = props;

  return <div className={classnames(styles.item, className)}>{children}</div>;
};

Item.defaultProps = {
  className: null,
  children: null,
};

Item.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
};

export default Item;

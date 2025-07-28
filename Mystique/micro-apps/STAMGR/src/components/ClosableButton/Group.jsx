import React from 'react';
import PropTypes from 'prop-types';
import classnames from 'classnames';
import styles from './index.module.less';

const Group = (props) => {
  const { className, children } = props;
  return (
    <div
      className={classnames(styles.group, className)}
      onWheel={(e) => {
        const dom = e.currentTarget;
        const scrollWidth = 50;
        e.deltaY > 0 ? (dom.scrollLeft += scrollWidth) : (dom.scrollLeft -= scrollWidth);
      }}
    >
      {children}
    </div>
  );
};

Group.defaultProps = {
  className: null,
  children: null,
};

Group.propTypes = {
  className: PropTypes.any,
  children: PropTypes.any,
};

export default Group;

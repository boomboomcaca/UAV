import React from 'react';
import PropTypes from 'prop-types';
import Spliter from './Spliter.jsx';
// import styles from './index.module.less';

const getKey = () => {
  return Math.trunc(Math.random() * 0x1000000).toString();
};

const Action = (props) => {
  const { children } = props;
  const childrenArr = (toString.call(children) === '[object Array]' ? children : [children]).filter((c) => {
    return c ? c.type?.typeTag !== 'Null' : false;
  });
  return (
    <>
      {childrenArr.map((child, index) => {
        return index + 1 === childrenArr.length || !child.props.visible
          ? child
          : [child, <Spliter key={getKey()} width={16} />];
      })}
    </>
  );
};

Action.defaultProps = {
  children: null,
};

Action.propTypes = {
  children: PropTypes.any,
};

Action.typeTag = 'Action';

export default Action;

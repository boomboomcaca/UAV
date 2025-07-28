import React from 'react';
import PropTypes from 'prop-types';
// import styles from './index.module.less';

const Message = (props) => {
  const { children } = props;
  return <>{children}</>;
};

Message.defaultProps = {
  children: null,
};

Message.propTypes = {
  children: PropTypes.any,
};

Message.typeTag = 'Message';

export default Message;

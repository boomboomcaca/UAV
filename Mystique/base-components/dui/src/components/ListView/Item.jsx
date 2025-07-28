import React from 'react';
import PropTypes from 'prop-types';
import styles from './index.module.less';

const Item = (props) => {
  const { content, style } = props;
  return (
    <div className={styles.item} style={style}>
      {content}
    </div>
  );
};

Item.defaultProps = {
  content: null,
  style: null,
};

Item.propTypes = {
  content: PropTypes.any,
  style: PropTypes.any,
};

export default Item;
